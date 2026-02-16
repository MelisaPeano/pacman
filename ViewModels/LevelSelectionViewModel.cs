using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using PacmanAvalonia.Models.enums;

namespace PacmanAvalonia.ViewModels;

/// <summary>
/// Manages the level selection screen logic, allowing the player to choose a starting level for Story Mode.
/// </summary>
public partial class LevelSelectionViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindow;

    /// <summary>
    /// Gets the list of available levels to display in the grid (1 to 10).
    /// </summary>
    public List<int> Levels { get; } = new() { 1, 2, 3, 4, 5};

    /// <summary>
    /// Initializes a new instance of the LevelSelectionViewModel class.
    /// </summary>
    /// <param name="mainWindow">The main window view model used for navigation.</param>
    public LevelSelectionViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
    }

    /// <summary>
    /// Starts the game at the specific level selected by the user.
    /// </summary>
    /// <param name="level">The level number to start at.</param>
    [RelayCommand]
    public void SelectLevel(int level)
    {
        _mainWindow.NavigateTo(new GameViewModel(_mainWindow, GameMode.Story, level));
    }

    /// <summary>
    /// Returns the user to the main menu.
    /// </summary>
    [RelayCommand]
    public void GoBack()
    {
        _mainWindow.NavigateTo(new MainMenuViewModel(_mainWindow));
    }
}