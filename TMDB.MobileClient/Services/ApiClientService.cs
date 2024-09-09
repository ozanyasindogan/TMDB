using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using FFImageLoading;
using MonkeyCache;
using MonkeyCache.FileStore;
using TMDB.MobileClient.Models.Messages;
using TMDB.MobileClient.Models.Tmdb;

namespace TMDB.MobileClient.Services;

public class ApiClientService
{
    // api token to be used during querying the service, which can be located at:
    // https://www.themoviedb.org/settings/api
    private const string ApiReadAccessToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJkYTdlYjY2YTRiOGQyZjYzMzFmMzFiZDMyZjc1OTZjYiIsIm5iZiI6MTcyNTgwMzczNy45ODI4MSwic3ViIjoiNjZkZDk1YzExZjJmZDA0ZTBhOWE5Y2IwIiwic2NvcGVzIjpbImFwaV9yZWFkIl0sInZlcnNpb24iOjF9.bQiZT3xouBWtHPvsvH1e2qn-6_jYTkTVh_UaVmxUWeA";

    // this address will be used as a base address for the calls
    private const string ApiBaseAddress = "https://api.themoviedb.org/3/";

    // requests will timeout if no response received in 20 seconds
    private const double HttpTimeoutInSeconds = 20;

    private const double ConfigurationCacheExpiresInDays = 7;
    private const double MovieGenresCacheExpiresInDays = 7;
    private const double UpcomingMoviesCacheExpiresInSeconds = 60;
    private const double LanguagesCacheExpiresInDays = 30;

    // application id for monkey cache
    private const string ApplicationId = "tmdb_mobile_client";

    // our http client for api calls
    private readonly HttpClient _httpClient;

    // http client to fetch and cache images
    private readonly HttpClient _imageClient;

    // we will be using an instance of json serializer options which internally caches
    // and builds json schemas for small performance/memory gains, and we're forcing
    // it to use camelCase serialization
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    // TMDB configuration which is received/cached from TMDB service
    public Configuration? TmdbConfiguration { get; set; }

    public MovieGenres? MovieGenres { get; set; }

    public List<MovieLanguage>? Languages { get; set; }

    public static bool HasInternetAccess => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public ApiClientService()
    {
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiBaseAddress),
            Timeout = TimeSpan.FromSeconds(HttpTimeoutInSeconds)
        };

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiReadAccessToken);

        _imageClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(HttpTimeoutInSeconds)
        };
        
        _imageClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        
        ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration()
        {
            HttpClient = _imageClient
        });

        Barrel.ApplicationId = ApplicationId;
        BarrelUtils.SetBaseCachePath(FileSystem.CacheDirectory);
        Barrel.Current.AutoExpire = true;
        Barrel.Current.EmptyExpired();

        // Barrel.Current.EmptyAll(); // this will clear all cached results, for testing purposes only.
    }

    // https://developer.themoviedb.org/reference/configuration-details
    // The data returned here in the configuration endpoint is designed to provide some of
    // the required information you'll need as you integrate our API.
    // For example, you can get a list of valid image sizes and the valid image address.
    public async Task<bool> GetConfigurationAsync()
    {
        TmdbConfiguration = await GetAsync<Configuration>("configuration", TimeSpan.FromDays(ConfigurationCacheExpiresInDays));
        return TmdbConfiguration is not null;
    }

    public async Task<bool> GetLanguagesAsync()
    {
        Languages = await GetAsync<List<MovieLanguage>>("configuration/languages", TimeSpan.FromDays(LanguagesCacheExpiresInDays));
        return Languages is not null;
    }

    // https://developer.themoviedb.org/reference/genre-movie-list
    // Get the list of official genres for movies.
    public async Task<bool> GetMovieGenresAsync()
    {
        MovieGenres = await GetAsync<MovieGenres>("genre/movie/list", TimeSpan.FromDays(MovieGenresCacheExpiresInDays));
        return MovieGenres is not null;
    }

    // https://developer.themoviedb.org/reference/movie-upcoming-list
    // Get a list of movies that are being released soon.
    // Region: ISO-3166-1 code, default to ca (Canada)
    // --url 'https://api.themoviedb.org/3/discover/movie?include_adult=false&include_video=false&language=en-US&page=1&sort_by=popularity.desc&with_release_type=2|3&release_date.gte={min_date}&release_date.lte={max_date}' \
    public async Task<UpcomingMovies?> GetUpcomingMoviesAsync(int page = 1, string language = "en-US", string? region = null)
    {
        var regionParam = region is not null ? "&region=" + region : string.Empty;
        return await GetAsync<UpcomingMovies>($"movie/upcoming?page={page}&language={language}{regionParam}", 
            TimeSpan.FromSeconds(UpcomingMoviesCacheExpiresInSeconds));
    }

    private void ReportException(Exception? exception = null)
    {
        WeakReferenceMessenger.Default.Send(new ApiClientErrorMessage(new ErrorMessage("Api Error",
            "An unhandled exception occured while trying to perform an Api request, please check your internet connection.", 
            exception)));
    }

    private void ReportNoInternetConnection()
    {
        WeakReferenceMessenger.Default.Send(new ApiClientErrorMessage(new ErrorMessage("Connection Error",
            "Your device is not connected to the internet and requested resource is not available in the cache.")));
    }

    private void ReportDeserializationError(string url)
    {
        WeakReferenceMessenger.Default.Send(new ApiClientErrorMessage(new ErrorMessage("Serialization Error",
            $"Could not deserialize received response from url: {url}, probably the schema has changed.")));
    }

    private async Task ReportUnsuccesfulStatusCode(HttpResponseMessage response)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                WeakReferenceMessenger.Default.Send(new ApiClientErrorMessage(new ErrorMessage("Authorization Error",
                    "Provided api token was not authorized, please check.")));
                break;
            case HttpStatusCode.TooManyRequests:
                WeakReferenceMessenger.Default.Send(new ApiClientErrorMessage(new ErrorMessage("Too Many Requests",
                    "TMDB service does not allow to make more requests at the moment.")));
                break;
            default:
                var content = await response.Content.ReadAsStringAsync();
                WeakReferenceMessenger.Default.Send(new ApiClientErrorMessage(new ErrorMessage("Unexpected Status Code",
                    $"TMDB service returned an unexpected http status code: {response.StatusCode}. Response content: {content}")));
                break;
        }
    }
    
    public async Task<T?> GetAsync<T>(string url, TimeSpan cacheExpiryTimeSpan)
    {
        try
        {
            T? cachedValue = default;

            if (Barrel.Current.Exists(url))
            {
                cachedValue = Barrel.Current.Get<T>(url, _jsonSerializerOptions);
                if (!Barrel.Current.IsExpired(url))
                {
                    return cachedValue;
                }
            }

            if (!HasInternetAccess)
            {
                if (cachedValue is not null)
                {
                    return cachedValue;
                }

                ReportNoInternetConnection();
                return default;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var etag = Barrel.Current.GetETag(url);
            if (cachedValue is not null && etag is not null)
            {
                request.Headers.IfNoneMatch.TryParseAdd(etag);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return cachedValue;
            }

            if (response.IsSuccessStatusCode)
            {
                cachedValue = await response.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions);

                if (cachedValue is not null)
                {
                    Barrel.Current.Add(url, 
                        cachedValue, 
                        cacheExpiryTimeSpan,
                        _jsonSerializerOptions,
                        response.Headers.ETag?.Tag);

                    return cachedValue;
                }

                ReportDeserializationError(url);
                return default;
            }

            await ReportUnsuccesfulStatusCode(response);
            return default;
        }
        catch (Exception e)
        {
            ReportException(e);
            return default;
        }
    }
}