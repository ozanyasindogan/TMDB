using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TMDB.MobileClient.Services;

namespace TMDB.MobileClient.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    public ApiClientService RestApiClient = MauiProgram.ServiceProvider!.GetService<ApiClientService>()!;

    public NavigationService Navigation = MauiProgram.ServiceProvider!.GetService<NavigationService>()!;
}