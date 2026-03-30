using System.Windows;

namespace ADONetDemo;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Central connection string used by all demo windows.
    /// Change this to point at your SQL Server LocalDB or named instance.
    /// Default: SQL Server LocalDB with a database called "ADONetDemo".
    /// </summary>
    public static string ConnectionString { get; } =
        @"Server=(localdb)\MSSQLLocalDB;Database=ADONetDemo;Integrated Security=true;" +
        "Pooling=true;Min Pool Size=2;Max Pool Size=20;Connect Timeout=30;";
}

