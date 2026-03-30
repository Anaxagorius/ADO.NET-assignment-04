using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace ADONetDemo;

/// <summary>
/// Demonstrates SqlCommandBuilder: loads a DataTable into a DataGrid,
/// then uses SqlCommandBuilder to auto-generate INSERT/UPDATE/DELETE
/// commands when saving changes back to the database.
/// </summary>
public partial class CommandBuilderWindow : Window, IDisposable
{
    private const string SelectQuery =
        "SELECT ProductId, Name, Category, Price, Stock FROM Products;";

    private SqlDataAdapter?    _adapter;
    private SqlCommandBuilder? _builder;
    private SqlConnection?     _connection;
    private DataTable?         _table;
    private bool               _disposed;

    public CommandBuilderWindow()
    {
        InitializeComponent();
        Closed += (_, _) => Dispose();
    }

    // -------------------------------------------------------------------------
    // Load data
    // -------------------------------------------------------------------------
    private void BtnLoad_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Dispose any previous connection/adapter before creating new ones
            Dispose();

            _table      = new DataTable();
            _connection = new SqlConnection(App.ConnectionString);
            _adapter    = new SqlDataAdapter(SelectQuery, _connection);

            // SqlCommandBuilder automatically generates INSERT/UPDATE/DELETE
            // commands based on the SELECT statement above.
            _builder = new SqlCommandBuilder(_adapter);

            // Optional: log the auto-generated commands so the user can see them
            _adapter.Fill(_table);

            LogGeneratedCommands();

            ProductGrid.ItemsSource = _table.DefaultView;
            BtnSave.IsEnabled       = true;
            BtnAddRow.IsEnabled     = true;
            TxtStatus.Text          = $"Loaded {_table.Rows.Count} rows.";
        }
        catch (Exception ex)
        {
            ShowError("Load error", ex);
        }
    }

    // -------------------------------------------------------------------------
    // Add a blank row
    // -------------------------------------------------------------------------
    private void BtnAddRow_Click(object sender, RoutedEventArgs e)
    {
        if (_table is null) return;

        DataRow newRow = _table.NewRow();
        newRow["Name"]     = "New Product";
        newRow["Category"] = "General";
        newRow["Price"]    = 0.00m;
        newRow["Stock"]    = 0;
        _table.Rows.Add(newRow);

        // Scroll to the new row
        ProductGrid.ScrollIntoView(ProductGrid.Items[^1]);
        TxtStatus.Text = "New row added – edit then click Save Changes.";
    }

    // -------------------------------------------------------------------------
    // Save all pending changes using SqlDataAdapter.Update()
    // -------------------------------------------------------------------------
    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (_adapter is null || _table is null) return;

        // Commit any in-progress cell edits
        ProductGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

        try
        {
            int affected = _adapter.Update(_table);
            _table.AcceptChanges();
            TxtStatus.Text = $"Saved – {affected} row(s) affected.";
            AppendLog($"\n[Update complete] {affected} row(s) affected.");
        }
        catch (Exception ex)
        {
            ShowError("Save error", ex);
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retrieves the auto-generated SQL from SqlCommandBuilder and shows it in
    /// the log pane so the user can see what will be executed.
    /// </summary>
    private void LogGeneratedCommands()
    {
        if (_builder is null) return;

        try
        {
            string insert = _builder.GetInsertCommand().CommandText;
            string update = _builder.GetUpdateCommand().CommandText;
            string delete = _builder.GetDeleteCommand().CommandText;

            AppendLog("=== Auto-Generated SQL Commands (SqlCommandBuilder) ===");
            AppendLog($"INSERT: {insert}");
            AppendLog($"UPDATE: {update}");
            AppendLog($"DELETE: {delete}");
        }
        catch (Exception ex)
        {
            AppendLog($"[Could not retrieve generated commands: {ex.Message}]");
        }
    }

    private void AppendLog(string message)
    {
        TxtLog.AppendText(message + "\n");
        TxtLog.ScrollToEnd();
    }

    private static void ShowError(string title, Exception ex)
        => MessageBox.Show(
            $"{ex.Message}\n\nMake sure the database is set up and the connection string is correct.",
            title, MessageBoxButton.OK, MessageBoxImage.Warning);

    // -------------------------------------------------------------------------
    // IDisposable
    // -------------------------------------------------------------------------
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _builder?.Dispose();
        _adapter?.Dispose();
        _connection?.Dispose();
    }
}
