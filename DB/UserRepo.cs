using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using GymManager.Models;

namespace GymManager.DB
{
    public class UserRepo
    {
        private readonly string _connectionString;

        public UserRepo(IOptions<DbConfig> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public User? ValidateUser(string username, string password)
        {
            string hash = HashHelper.HashPassword(password);

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT id, username, role FROM users WHERE username = @username AND password_hash = @hash LIMIT 1";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@hash", hash);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Role = reader.GetString(2)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<User> GetAll()
        {
            var list = new List<User>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT id, username, role FROM users ORDER BY username ASC";
                using (var cmd = new SqliteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Role = reader.GetString(2)
                        });
                    }
                }
            }
            return list;
        }

        public void Add(User user, string password)
        {
            string hash = HashHelper.HashPassword(password);
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO users (username, password_hash, role) VALUES (@username, @hash, @role)";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM users WHERE id = @id";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
