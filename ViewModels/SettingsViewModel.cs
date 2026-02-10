using CommunityToolkit.Mvvm.Input;
using PacmanAvalonia.Services;

namespace PacmanAvalonia.ViewModels;

/// <summary>
/// This class provide settings for main menu, the user can clear the record of scores.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindow;

    public SettingsViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    /// <summary>
    /// Command in the view, call the service and clear the records.
    /// </summary>
    [RelayCommand]
    public void ClearHighScores()
    {
        ScoreService.ClearScores();
    }
    
    /// <summary>
    /// Go to main menu
    /// </summary>
    [RelayCommand]
    public void GoBack()
    {
        _mainWindow.NavigateTo(new MainMenuViewModel(_mainWindow));
    }
}