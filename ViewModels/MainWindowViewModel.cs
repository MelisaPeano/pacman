using CommunityToolkit.Mvvm.ComponentModel;

namespace PacmanAvalonia.ViewModels;

/// <summary>
/// Serves as the root view model for the application, managing navigation between different screens.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    /// <summary>
    /// Initializes a new instance of the MainWindowViewModel class and sets the default view to the Main Menu.
    /// </summary>
    public MainWindowViewModel()
    {
        _currentViewModel = new MainMenuViewModel(this);
    }

    /// <summary>
    /// Changes the currently displayed view model to the specified one.
    /// </summary>
    /// <param name="viewModel">The new view model to navigate to.</param>
    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }
}