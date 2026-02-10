using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PacmanAvalonia.Models.enums;

namespace PacmanAvalonia.ViewModels;

/// <summary>
/// Represents the logic for the main menu screen, handling navigation to different game modes.
/// </summary>
public partial class MainMenuViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindow;
    public List<int> Levels { get; } = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    
    [ObservableProperty]
    private int _selectedLevel = 1;
    
    [RelayCommand]
    public void GoToHighScores()
    {
        _mainWindow.NavigateTo(new HighScoresViewModel(_mainWindow));
    }

    [RelayCommand]
    public void GoToSettings()
    {
        _mainWindow.NavigateTo(new SettingsViewModel(_mainWindow));
    }
    /// <summary>
    /// Initializes a new instance of the MainMenuViewModel class.
    /// </summary>
    /// <param name="mainWindow">The main window view model used for navigation.</param>
    public MainMenuViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
    }

    /// <summary>
    /// Starts a new game in Story Mode (Level 1 to 10 progression).
    /// </summary>
    [RelayCommand]
    public void StartStoryMode()
    {
        _mainWindow.NavigateTo(new LevelSelectionViewModel(_mainWindow));
    }

    /// <summary>
    /// Starts a new game in Survivor Mode (Infinite random levels).
    /// </summary>
    [RelayCommand]
    public void StartSurvivorMode()
    {
        _mainWindow.NavigateTo(new GameViewModel(_mainWindow, GameMode.Survivor));
    }
}