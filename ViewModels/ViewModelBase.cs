using CommunityToolkit.Mvvm.ComponentModel;

namespace PacmanAvalonia.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Inherits from ObservableObject to provide built-in support for property change notifications (INotifyPropertyChanged).
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
}