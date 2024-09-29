using MySql.Data.MySqlClient;
using System.Data;

namespace UScrape
{
    public static class SQL
    {
        ///<summary> Crée un paramètre SQL </summary>
        public static MySqlParameter newParameter(string paramName, SqlDbType type, ParameterDirection direction, object? value = null)
        {
            MySqlParameter param = new MySqlParameter(paramName, type);
            param.Direction = direction;
            if (value is not null) param.Value = value;

            return param;
        }

        ///<summary> Crée une commande SQL. </summary>
        public static MySqlCommand newCommand(MySqlConnection connection, string procName, params MySqlParameter[] parameters)
        {
            MySqlCommand cmd = new MySqlCommand(procName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (var param in parameters) cmd.Parameters.Add(param);

            return cmd;
        }

        ///<summary> Exécute une commande SQL et retourne une collection de la première valeur de chaque enregistrements. </summary>
        public static IEnumerable<string> ExecuteReader(MySqlCommand command)
        {
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return reader.GetString(0);
            }
        }

        ///<summary> Exécute une commande SQL et retourne une collection de chaque enregistrements. </summary>
        public static IEnumerable<string[]> ExecuteReader(MySqlCommand command, params string[] colNames)
        {
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string[] record = new string[colNames.Length];

                for (int i = 0; i < colNames.Length; i++)
                {
                    record[i] = reader[colNames[i]].ToString();
                }

                yield return record;
            }
        }

        ///<summary> Essaie d'ouvrir une connexion SQL et retourne vrai si réussi. </summary>
        public static bool TryOpen(MySqlConnection connection, out Exception? exception)
        {
            exception = null;

            if (connection.State == ConnectionState.Open) return true;

            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                connection.Close();
                exception = e;
                return false;
            }

            return true;
        }

        ///<summary> Essaie d'ouvrir une connexion SQL, fait une action et retourne vrai si réussi. </summary>
        public static bool TryExecute(MySqlConnection connection, Action action, out Exception? exception)
        {
            exception = null;

            try
            {
                connection.Open();
                action();
            }
            catch (Exception e)
            {
                connection.Close();
                exception = e;
                return false;
            }

            return true;
        }
    }
}
