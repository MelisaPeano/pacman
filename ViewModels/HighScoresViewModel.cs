using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PacmanAvalonia.Services;

namespace PacmanAvalonia.ViewModels;

/// <summary>
/// Manages the logic for the High Scores leaderboard screen.
/// It is responsible for loading and displaying the best player results.
/// </summary>
public partial class HighScoresViewModel : ViewModelBase
{
    /// <summary>
    /// Reference to the main window view model to handle navigation between views.
    /// </summary>
    private readonly MainWindowViewModel _mainWindow;

    /// <summary>
    /// Gets the collection of high score entries to be displayed in the UI.
    /// Uses ObservableCollection so the View automatically updates when items are added.
    /// </summary>
    public ObservableCollection<ScoreEntry> HighScores { get; } = new();

    /// <summary>
    /// Initializes a new instance of the HighScoresViewModel class.
    /// </summary>
    /// <param name="mainWindow">The main application navigator.</param>
    public HighScoresViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
        LoadScores();
    }

    /// <summary>
    /// Clears the current list and fetches the top scores from the persistence service.
    /// </summary>
    private void LoadScores()
    {
        HighScores.Clear();
        
        var scores = ScoreService.GetBestScores();
        
        foreach (var s in scores)
        {
            HighScores.Add(s);
        }
    }

    /// <summary>
    /// Navigates the user back to the Main Menu.
    /// </summary>
    [RelayCommand]
    public void GoBack()
    {
        _mainWindow.NavigateTo(new MainMenuViewModel(_mainWindow));
    }
}