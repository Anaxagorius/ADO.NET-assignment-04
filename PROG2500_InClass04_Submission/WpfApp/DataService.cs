
using System.Data;
using System.Data.SqlClient;

namespace WpfApp
{
    public class DataService
    {
        string connStr = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CompanyDB;Integrated Security=True;Pooling=true";

        public DataTable LoadEmployees()
        {
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Employees", connStr);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }
    }
}
