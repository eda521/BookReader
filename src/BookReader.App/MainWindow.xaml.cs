using System.Windows;
using BookReader.App.ViewModels; // 👈 důležité

namespace BookReader.App;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(); // 👈 vytvoření VM tady
    }
}
