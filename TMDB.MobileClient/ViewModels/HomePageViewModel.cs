using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TMDB.MobileClient.Models;
using TMDB.MobileClient.Models.Messages;

namespace TMDB.MobileClient.ViewModels;

public partial class HomePageViewModel : BaseViewModel, IRecipient<ApiClientErrorMessage>
{
    [ObservableProperty] 
    private ObservableCollection<MovieItem> _upcomingMoviesCollection = new();

    [ObservableProperty]
    private int _pageIndex;

    [ObservableProperty]
    private int _lastKnownPage;

    private MovieItem? _selectedMovieItem;

    public MovieItem? SelectedMovieItem
    {
        get => _selectedMovieItem;
        set
        {
            if (_selectedMovieItem == value)
            {
                return;
            }

            _selectedMovieItem = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty] 
    private bool _isRefreshing;

    public bool CanFetchMore => PageIndex < LastKnownPage;

    public HomePageViewModel()
    {
        WeakReferenceMessenger.Default.Register<ApiClientErrorMessage>(this);
    }

    [RelayCommand]
    public async Task RefreshDataAsync()
    {
        UpcomingMoviesCollection.Clear();
        PageIndex = 0;
        LastKnownPage = 1;
        await FetchMoreDataAsync();
        IsRefreshing = false;
    }

    [RelayCommand(CanExecute = nameof(CanFetchMore))]
    private async Task FetchMoreDataAsync()
    {
        if (!CanFetchMore)
        {
            return;
        }
        
        IsBusy = true;

        // you may want to change the region here
        var upcomingMovies = await RestApiClient.GetUpcomingMoviesAsync(PageIndex + 1);

        if (upcomingMovies is null)
        {
            IsBusy = false;
            return;
        }

        LastKnownPage = upcomingMovies.TotalPages;

        if (upcomingMovies.Results is null)
        {
            IsBusy = false;
            return;
        }

        var baseUrl = RestApiClient.TmdbConfiguration?.Images?.SecureBaseUrl;
        string? posterUrl = null;
        string? backdropUrl = null;

        if (baseUrl is not null)
        {
            posterUrl = baseUrl + "w500";       // 500px width for posters
            //backdropUrl = baseUrl + "w1280";    // 1280px width for backdrop
            backdropUrl = baseUrl + "original";    // original backdrop image (full size
        }

        PageIndex = upcomingMovies.Page;
        
        foreach (var result in upcomingMovies.Results)
        {
            var newItem = new MovieItem
            {
                Page = PageIndex,
                Id = result.Id,
                OriginalTitle = result.OriginalTitle,
                Title = result.Title,
                Overview = result.Overview,
                VoteAverage = (int)(result.VoteAverage * 10),
                ReleaseDate = result.ReleaseDate,
                ReleaseDateString = $"{result.ReleaseDate:MMM dd, yyyy}"
            };
            
            // set poster url
            if (result.PosterPath is not null && posterUrl is not null)
            {
                newItem.PosterPath = posterUrl + result.PosterPath;
            }

            // set backdrop url
            if (result.BackdropPath is not null && backdropUrl is not null )
            {
                newItem.BackdropPath = backdropUrl + result.BackdropPath;
            }

            if (string.IsNullOrEmpty(newItem.BackdropPath))
            {
                newItem.BackdropPath = newItem.PosterPath;
            }

            // map genres
            if (result.GenreIds is not null && RestApiClient.MovieGenres?.Genres is not null)
            {
                foreach (var genreId in result.GenreIds)
                {
                    var genre = RestApiClient.MovieGenres.Genres.FirstOrDefault(x => x.Id == genreId);
                    if (genre?.Name != null)
                    {
                        newItem.Genres.Add(genre.Name);
                    }
                }
            }

            // map language iso to language name
            if (result.OriginalLanguage is not null && RestApiClient.Languages is not null)
            {
                var language = RestApiClient.Languages.FirstOrDefault(x => x.Iso == result.OriginalLanguage);
                if (language is not null)
                {
                    newItem.OriginalLanguage = language.EnglishName;
                }
            }

            UpcomingMoviesCollection.Add(newItem);
        }

        IsBusy = false;
    }

    public void Receive(ApiClientErrorMessage message)
    {
        IsBusy = false;
        IsRefreshing = false;
        Shell.Current.CurrentPage.DisplayAlert(message.Value.Title, message.Value.Message, "OK");
    }
}