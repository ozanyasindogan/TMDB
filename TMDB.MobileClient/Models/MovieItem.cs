using CommunityToolkit.Mvvm.ComponentModel;

namespace TMDB.MobileClient.Models;

public partial class MovieItem : ObservableObject
{
    public int Page { get; set; }

    public int Id { get; set; }

    public List<string> Genres = new(6);

    public string? OriginalLanguage { get; set; }

    public string? OriginalTitle { get; set; }

    public string? Title { get; set; }

    public string? Overview { get; set; }

    public int VoteAverage { get; set; }

    public DateTimeOffset ReleaseDate { get; set; }

    public string? ReleaseDateString { get; set; }

    public string? PosterPath { get; set; }

    public string? BackdropPath { get; set; }

    [ObservableProperty] private ImageSource? _posterImageSource;
    [ObservableProperty] private ImageSource? _backdropImageSource;
}