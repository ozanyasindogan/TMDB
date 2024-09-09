using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDB.MobileClient.ViewModels;

namespace TMDB.MobileClient.Services;

public class NavigationService
{
    public async Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel 
        => await InternalNavigateToAsync(typeof(TViewModel), null);

    public async Task NavigateToAsync<TViewModel>(bool isAbsoluteRoute) where TViewModel : BaseViewModel 
        => await InternalNavigateToAsync(typeof(TViewModel), null, isAbsoluteRoute);

    public async Task NavigateToAsync<TViewModel>(ShellNavigationQueryParameters parameters) where TViewModel : BaseViewModel 
        => await InternalNavigateToAsync(typeof(TViewModel), parameters);

    public async Task GoBackAsync() 
        => await Shell.Current.GoToAsync("..");

    static async Task InternalNavigateToAsync(Type viewModelType, ShellNavigationQueryParameters? parameters, bool isAbsoluteRoute = false) 
    {
        var viewName = viewModelType.FullName?.Replace("ViewModels", "Pages").Replace("PageViewModel", "Page");
        var absolutePrefix = isAbsoluteRoute ? "///" : string.Empty;

        if (parameters != null) 
        {
            await Shell.Current.GoToAsync($"{absolutePrefix}{viewName}", parameters);
        }
        else 
        {
            await Shell.Current.GoToAsync($"{absolutePrefix}{viewName}");
        }
    }
}