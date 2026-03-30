using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        SqlDataAdapter adapter;
        DataTable table;

        string connStr = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CompanyDB;Integrated Security=True;Pooling=true";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            adapter = new SqlDataAdapter("SELECT * FROM Employees", connStr);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            table = new DataTable();
            adapter.Fill(table);
            EmployeeGrid.ItemsSource = table.DefaultView;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            adapter.Update(table);
        }
    }
}
