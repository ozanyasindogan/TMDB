using System.Net.Http.Headers;
using System.Text.Json;


const string apiReadAccessToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJkYTdlYjY2YTRiOGQyZjYzMzFmMzFiZDMyZjc1OTZjYiIsIm5iZiI6MTcyNTgwMzczNy45ODI4MSwic3ViIjoiNjZkZDk1YzExZjJmZDA0ZTBhOWE5Y2IwIiwic2NvcGVzIjpbImFwaV9yZWFkIl0sInZlcnNpb24iOjF9.bQiZT3xouBWtHPvsvH1e2qn-6_jYTkTVh_UaVmxUWeA";

var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.themoviedb.org/3/"),
    Timeout = TimeSpan.FromSeconds(20)
};

httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiReadAccessToken);

var now = DateTime.UtcNow.Date;
var lastDate = DateTime.MaxValue.Date;

// 2024-03-19
var minDate = $"{now:yyyy}-{now:MM}-{now:dd}";
var maxDate = $"{lastDate:yyyy}-{lastDate:MM}-{lastDate:dd}";

var url = $"movie/upcoming?region=tr&page=1";

var response = await httpClient.GetStringAsync(url);

Console.Clear();
Console.WriteLine(response);
Console.ReadLine();