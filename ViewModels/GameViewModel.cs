using CommunityToolkit.Mvvm.Input;

namespace PacmanAvalonia.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindow;

    public GameViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
    }

    [RelayCommand]
    private void BackToMenu()
    {
        _mainWindow.CurrentViewModel = new MainMenuViewModel(_mainWindow);
    }
}