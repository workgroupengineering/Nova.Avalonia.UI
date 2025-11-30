namespace Nova.Avalonia.UI.Gallery.ViewModels;

public abstract class PageViewModel : ViewModelBase
{
    protected PageViewModel(string title)
    {
        Title = title;
    }

    public string Title { get; }
}
