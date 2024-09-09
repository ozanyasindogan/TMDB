using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TMDB.MobileClient.Models.Messages;

namespace TMDB.MobileClient.ViewModels;

public partial class MainPageViewModel : BaseViewModel, IRecipient<ApiClientErrorMessage>
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty] 
    private bool _hasErrors;

    [ObservableProperty] 
    private string? _statusMessage;

    public MainPageViewModel()
    {
        WeakReferenceMessenger.Default.Register<ApiClientErrorMessage>(this);
    }

    public void Receive(ApiClientErrorMessage message)
    {
        IsBusy = false;
        HasErrors = true;
        
        var msg = message.Value;
        StatusMessage = msg.Message;
    }

    [RelayCommand]
    public async Task InitializeDataAsync()
    {
        HasErrors = false;
        IsBusy = true;

        StatusMessage = "Fetching configuration...";
        var result = await RestApiClient.GetConfigurationAsync();

        if (result)
        {
            StatusMessage = "Fetching languages...";
            result = await RestApiClient.GetLanguagesAsync();

            if (result)
            {
                StatusMessage = "Fetching movie genres...";
                result = await RestApiClient.GetMovieGenresAsync();
                
                if (result)
                {
                    StatusMessage = "Fetching upcoming movies...";
                    await RestApiClient.GetUpcomingMoviesAsync();

                    if (result)
                    {
                        await Navigation.NavigateToAsync<HomePageViewModel>(true);
                    }
                }
            }
        }
    }
}