using Npgsql;

namespace CardGeneratorBackend.Environment
{
    public class PostgresConnectionParameters
    {
        public const string ENV_OBJECT_KEY = "PostgreSQL";
        private const int DEFAULT_PORT = 5432;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;

        public string Database { get; set; } = string.Empty;

        public string Port { get; set; } = string.Empty;

        public string ConnectionString
        {
            get
            {
                var connStringBuilder = new NpgsqlConnectionStringBuilder
                {
                    Host = Host,
                    Database = Database,
                    Username = Username,
                    Password = Password
                };

                if (int.TryParse(Port, out int portNum)) {
                    connStringBuilder.Port = portNum;
                }
                else
                {
                    connStringBuilder.Port = DEFAULT_PORT;
                }

                return connStringBuilder.ConnectionString;
            }
        }
    }
}
