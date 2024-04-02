using MySqlConnector;

namespace API.Data
{
    public class DBConnection
    {
        private string Server = "localhost";

        private string DatabaseName = "my-website";

        private string UserName = "root";

        private string Password = "";
        public MySqlConnection ConnectDB()
        {
            string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DatabaseName, UserName, Password);
            MySqlConnection connect = new MySqlConnection(connstring);
            return connect;
        }
    }
}
