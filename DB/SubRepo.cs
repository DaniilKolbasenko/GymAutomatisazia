using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using GymManager.Models;

namespace GymManager.DB
{
    public class SubRepo
    {
        private readonly string _connectionString;

        public SubRepo(IOptions<DbConfig> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public List<Subscription> GetAll()
        {
            var list = new List<Subscription>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT id, name, duration_days, price FROM subscriptions ORDER BY price ASC";
                using (var cmd = new SqliteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Subscription
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            DurationDays = reader.GetInt32(2),
                            Price = reader.GetDouble(3)
                        });
                    }
                }
            }
            return list;
        }

        public void Add(Subscription sub)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO subscriptions (name, duration_days, price) VALUES (@name, @duration, @price)";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", sub.Name);
                    cmd.Parameters.AddWithValue("@duration", sub.DurationDays);
                    cmd.Parameters.AddWithValue("@price", sub.Price);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(Subscription sub)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE subscriptions SET name = @name, duration_days = @duration, price = @price WHERE id = @id";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", sub.Id);
                    cmd.Parameters.AddWithValue("@name", sub.Name);
                    cmd.Parameters.AddWithValue("@duration", sub.DurationDays);
                    cmd.Parameters.AddWithValue("@price", sub.Price);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM subscriptions WHERE id = @id";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
