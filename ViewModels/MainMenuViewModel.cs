using CommunityToolkit.Mvvm.Input;

namespace PacmanAvalonia.ViewModels;

public partial class MainMenuViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindow;

    public MainMenuViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
    }
    [RelayCommand]
    public void StartGame()
    {
        _mainWindow.NavigateTo(new GameViewModel(_mainWindow));
    }
}