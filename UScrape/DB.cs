
namespace UScrape
{
    public static class DB
    {
        public static string ConnectionString { get; private set; } = string.Empty;

        public static void Set(string host, string port, string database, string username, string password)
        {
            string DATA_SOURCE = host;
            string PORT = port;
            string DATABASE = database;
            string USER_ID = username;
            string USER_PW = password;

            ConnectionString = $"Data source={DATA_SOURCE}; Port = {PORT}; Initial Catalog={DATABASE}; User Id={USER_ID}; Password={USER_PW};";
        }
    }
}
