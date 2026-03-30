using System.Windows;

namespace ADONetDemo;

/// <summary>
/// Navigation hub – opens each demo window.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenDataAdapterReader_Click(object sender, RoutedEventArgs e)
        => new DataAdapterReaderWindow().Show();

    private void OpenCommandBuilder_Click(object sender, RoutedEventArgs e)
        => new CommandBuilderWindow().Show();

    private void OpenConnectionPooling_Click(object sender, RoutedEventArgs e)
        => new ConnectionPoolingWindow().Show();
}
