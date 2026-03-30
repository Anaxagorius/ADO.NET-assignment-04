using System.Diagnostics;
using System.Text;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace ADONetDemo;

/// <summary>
/// Demonstrates connection pooling in ADO.NET by comparing the elapsed time
/// when opening many connections with pooling enabled vs. disabled.
/// </summary>
public partial class ConnectionPoolingWindow : Window
{
    private const string SimpleQuery = "SELECT 1;";

    // Pooling-enabled connection string (default behaviour)
    private static string PooledConnStr =>
        new SqlConnectionStringBuilder(App.ConnectionString)
        {
            Pooling  = true,
            MinPoolSize = 2,
            MaxPoolSize = 20
        }.ConnectionString;

    // Non-pooled connection string – creates a new physical connection every time
    private static string NonPooledConnStr =>
        new SqlConnectionStringBuilder(App.ConnectionString)
        {
            Pooling = false
        }.ConnectionString;

    public ConnectionPoolingWindow()
    {
        InitializeComponent();
    }

    // -------------------------------------------------------------------------
    // With pooling
    // -------------------------------------------------------------------------
    private async void BtnPooled_Click(object sender, RoutedEventArgs e)
        => await RunBenchmark(pooled: true);

    // -------------------------------------------------------------------------
    // Without pooling
    // -------------------------------------------------------------------------
    private async void BtnNoPool_Click(object sender, RoutedEventArgs e)
        => await RunBenchmark(pooled: false);

    // -------------------------------------------------------------------------
    // Core benchmark
    // -------------------------------------------------------------------------
    private async Task RunBenchmark(bool pooled)
    {
        if (!int.TryParse(TxtConnCount.Text.Trim(), out int connCount) || connCount < 1)
        {
            MessageBox.Show("Enter a valid connection count (>= 1).", "Input Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(TxtIterations.Text.Trim(), out int iterations) || iterations < 1)
        {
            MessageBox.Show("Enter a valid iteration count (>= 1).", "Input Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string connStr = pooled ? PooledConnStr : NonPooledConnStr;
        string mode    = pooled ? "WITH Pooling" : "WITHOUT Pooling";

        TxtStatus.Text = $"Running {mode}…";
        SetButtonsEnabled(false);

        var sb = new StringBuilder();
        sb.AppendLine($"=== {mode} | {connCount} connections × {iterations} iterations ===");

        try
        {
            // Warm-up for pooled mode: pre-populate the pool
            if (pooled)
            {
                sb.AppendLine("[Warm-up] pre-opening pool connections…");
                await Task.Run(() =>
                {
                    for (int w = 0; w < Math.Min(connCount, 5); w++)
                    {
                        using var c = new SqlConnection(connStr);
                        c.Open();
                    }
                });
                sb.AppendLine("[Warm-up complete]");
            }

            long totalMs = 0;

            for (int iter = 1; iter <= iterations; iter++)
            {
                var sw = Stopwatch.StartNew();
                long iterMs = await Task.Run(() => OpenAndQueryConnections(connStr, connCount));
                sw.Stop();

                sb.AppendLine($"  Iteration {iter}: {connCount} connections opened+closed in {iterMs} ms");
                totalMs += iterMs;
            }

            double avg = (double)totalMs / iterations;
            sb.AppendLine($"  Average: {avg:F1} ms per iteration");
            sb.AppendLine();

            TxtStatus.Text = $"Done ({mode}) – avg {avg:F1} ms";
        }
        catch (Exception ex)
        {
            sb.AppendLine($"[ERROR] {ex.Message}");
            TxtStatus.Text = "Error – see log";
        }
        finally
        {
            SetButtonsEnabled(true);
        }

        TxtLog.AppendText(sb.ToString());
        TxtLog.ScrollToEnd();
    }

    /// <summary>
    /// Opens <paramref name="count"/> connections, executes a trivial query on each,
    /// then closes them. Returns elapsed milliseconds.
    /// </summary>
    private static long OpenAndQueryConnections(string connStr, int count)
    {
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
        {
            // Using block ensures connection is returned to pool (or truly closed)
            using var conn = new SqlConnection(connStr);
            conn.Open();
            using var cmd = new SqlCommand(SimpleQuery, conn);
            cmd.ExecuteScalar();
        }   // Dispose returns connection to pool
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    // -------------------------------------------------------------------------
    // Clear pool
    // -------------------------------------------------------------------------
    private void BtnClearPool_Click(object sender, RoutedEventArgs e)
    {
        SqlConnection.ClearAllPools();
        TxtLog.AppendText("[Pool cleared – next run will create new physical connections]\n");
        TxtLog.ScrollToEnd();
        TxtStatus.Text = "Pool cleared.";
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private void SetButtonsEnabled(bool enabled)
    {
        BtnPooled.IsEnabled    = enabled;
        BtnNoPool.IsEnabled    = enabled;
        BtnClearPool.IsEnabled = enabled;
    }
}
