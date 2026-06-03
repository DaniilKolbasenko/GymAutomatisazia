using System.Collections.Generic;
using MySqlConnector;
using Microsoft.Extensions.Options;
using GymManager.Models;

namespace GymManager.DB
{
    public class TrainerRepo
    {
        private readonly string _connectionString;

        public TrainerRepo(IOptions<DbConfig> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public List<Trainer> GetAll()
        {
            var list = new List<Trainer>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT id, full_name, specialization, phone FROM trainers ORDER BY full_name ASC";
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Trainer
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Specialization = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Phone = reader.IsDBNull(3) ? "" : reader.GetString(3)
                        });
                    }
                }
            }
            return list;
        }

        public void Add(Trainer trainer)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO trainers (full_name, specialization, phone) VALUES (@fullName, @specialization, @phone)";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@fullName", trainer.FullName);
                    cmd.Parameters.AddWithValue("@specialization", trainer.Specialization);
                    cmd.Parameters.AddWithValue("@phone", trainer.Phone);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(Trainer trainer)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE trainers SET full_name = @fullName, specialization = @specialization, phone = @phone WHERE id = @id";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", trainer.Id);
                    cmd.Parameters.AddWithValue("@fullName", trainer.FullName);
                    cmd.Parameters.AddWithValue("@specialization", trainer.Specialization);
                    cmd.Parameters.AddWithValue("@phone", trainer.Phone);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM trainers WHERE id = @id";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Client> GetClientsForTrainer(int trainerId)
        {
            var list = new List<Client>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT id, full_name, phone FROM members WHERE trainer_id = @trainerId ORDER BY full_name ASC";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@trainerId", trainerId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Client
                            {
                                Id = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Phone = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}
