using System.Data;
using System.Diagnostics;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace ADONetDemo;

/// <summary>
/// Demonstrates and compares SqlDataReader (connected, forward-only)
/// and SqlDataAdapter (disconnected, DataSet-based) for reading product data.
/// </summary>
public partial class DataAdapterReaderWindow : Window
{
    private const string Query = "SELECT ProductId, Name, Category, Price, Stock FROM Products;";

    public DataAdapterReaderWindow()
    {
        InitializeComponent();
    }

    // -------------------------------------------------------------------------
    // SqlDataReader approach
    // -------------------------------------------------------------------------
    private void BtnReader_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            // Build an in-memory DataTable manually so we can bind to DataGrid
            var table = new DataTable();
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name",      typeof(string));
            table.Columns.Add("Category",  typeof(string));
            table.Columns.Add("Price",     typeof(decimal));
            table.Columns.Add("Stock",     typeof(int));

            // SqlDataReader keeps the connection open while reading rows
            using var connection = new SqlConnection(App.ConnectionString);
            using var command    = new SqlCommand(Query, connection);

            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                table.Rows.Add(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDecimal(3),
                    reader.GetInt32(4));
            }
            // Connection is closed when 'connection' is disposed (end of using block)

            sw.Stop();
            ResultGrid.ItemsSource = table.DefaultView;
            TxtStatus.Text = $"SqlDataReader – {table.Rows.Count} rows in {sw.ElapsedMilliseconds} ms";
        }
        catch (Exception ex)
        {
            ShowError("SqlDataReader error", ex);
        }
    }

    // -------------------------------------------------------------------------
    // SqlDataAdapter approach
    // -------------------------------------------------------------------------
    private void BtnAdapter_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var table = new DataTable();

            // SqlDataAdapter opens, fills DataTable, then closes the connection
            using var connection = new SqlConnection(App.ConnectionString);
            using var adapter    = new SqlDataAdapter(Query, connection);

            adapter.Fill(table);   // internally opens and closes the connection

            sw.Stop();
            ResultGrid.ItemsSource = table.DefaultView;
            TxtStatus.Text = $"SqlDataAdapter – {table.Rows.Count} rows in {sw.ElapsedMilliseconds} ms";
        }
        catch (Exception ex)
        {
            ShowError("SqlDataAdapter error", ex);
        }
    }

    private static void ShowError(string title, Exception ex)
        => MessageBox.Show(
            $"{ex.Message}\n\nMake sure the database is set up and the connection string is correct.",
            title, MessageBoxButton.OK, MessageBoxImage.Warning);
}
