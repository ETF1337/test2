using Npgsql;

public class PostgreDataAccess
{
    private readonly string _connectionString;

    public PostgreDataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void AddContact(string name, string email)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand("INSERT INTO my_table (name, email) VALUES (@name, @email)", connection))
            {
                command.Parameters.AddWithValue("name", name);
                command.Parameters.AddWithValue("email", email);
                command.ExecuteNonQuery();
            }
        }
    }

    public List<(int id, string name, string email)> GetContacts()
    {
        var contacts = new List<(int, string, string)>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand("SELECT * FROM my_table", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    contacts.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                }
            }
        }

        return contacts;
    }
}