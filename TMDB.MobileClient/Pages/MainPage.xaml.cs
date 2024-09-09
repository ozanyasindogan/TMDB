using TMDB.MobileClient.ViewModels;

namespace TMDB.MobileClient.Pages;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = MauiProgram.ServiceProvider!.GetService<MainPageViewModel>()!;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeDataAsync();
    }
}