using Avalonia.Controls;
using System.Threading.Tasks;
using TrailMealsPlanner.Desktop.ViewModels;

namespace TrailMealsPlanner.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();
        this.viewModel = null!;
    }

    public MainWindow(MainWindowViewModel viewModel)
        : this()
    {
        this.viewModel = viewModel;
        DataContext = viewModel;
    }

    public Task InitializeAsync()
    {
        return viewModel?.InitializeAsync() ?? Task.CompletedTask;
    }
}
