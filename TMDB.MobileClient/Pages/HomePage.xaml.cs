using TMDB.MobileClient.ViewModels;

namespace TMDB.MobileClient.Pages;

public partial class HomePage : ContentPage
{
    private readonly HomePageViewModel _viewModel;

	public HomePage()
	{
		InitializeComponent();
        
        _viewModel = MauiProgram.ServiceProvider!.GetService<HomePageViewModel>()!;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshDataAsync();
    }
}