using System;
using System.Collections.Generic;
using MySqlConnector;
using Microsoft.Extensions.Options;
using GymManager.Models;

namespace GymManager.DB
{
    public class ClientRepo
    {
        private readonly string _connectionString;

        public ClientRepo(IOptions<DbConfig> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public List<Client> GetAll(string search = "")
        {
            var clients = new List<Client>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                
                string query = @"
                    SELECT 
                        m.id, m.full_name, m.phone, m.birth_date, m.join_date, m.trainer_id, t.full_name as trainer_name,
                        s.name AS sub_name,
                        ms.end_date,
                        ms.visits_left,
                        (ms.end_date >= CURDATE() AND (ms.visits_left IS NULL OR ms.visits_left > 0)) AS is_active
                    FROM members m
                    LEFT JOIN trainers t ON m.trainer_id = t.id
                    LEFT JOIN member_subscriptions ms ON m.id = ms.member_id 
                        AND ms.end_date >= CURDATE() 
                        AND (ms.visits_left IS NULL OR ms.visits_left > 0)
                    LEFT JOIN subscriptions s ON ms.subscription_id = s.id
                    WHERE @search = '' OR m.full_name LIKE @searchPattern OR m.phone LIKE @searchPattern
                    GROUP BY m.id
                    ORDER BY m.full_name ASC";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@search", search);
                    command.Parameters.AddWithValue("@searchPattern", $"%{search}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var client = new Client
                            {
                                Id = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Phone = reader.GetString(2),
                                BirthDate = reader.IsDBNull(3) ? "" : reader.GetDateTime(3).ToString("dd.MM.yyyy"),
                                JoinDate = reader.GetDateTime(4).ToString("dd.MM.yyyy"),
                                TrainerId = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                                TrainerName = reader.IsDBNull(6) ? "Не назначен" : reader.GetString(6)
                            };

                            bool isActive = !reader.IsDBNull(10) && reader.GetInt32(10) == 1;
                            
                            if (isActive && !reader.IsDBNull(7))
                            {
                                client.IsSubscriptionActive = true;
                                client.ActiveSubscriptionName = reader.GetString(7);
                                client.SubscriptionEndDate = reader.GetDateTime(8).ToString("dd.MM.yyyy");
                                client.VisitsLeftText = reader.IsDBNull(9) ? "Безлимит" : reader.GetInt32(9).ToString();
                            }
                            else
                            {
                                client.IsSubscriptionActive = false;
                                client.ActiveSubscriptionName = "Нет абонемента";
                                client.SubscriptionEndDate = "-";
                                client.VisitsLeftText = "-";
                                client.TrainerName = "-";
                            }

                            clients.Add(client);
                        }
                    }
                }
            }

            return clients;
        }

        public void Add(Client client)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO members (full_name, phone, birth_date, join_date, trainer_id) VALUES (@fullName, @phone, @birthDate, @joinDate, @trainerId)";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fullName", client.FullName);
                    command.Parameters.AddWithValue("@phone", client.Phone);
                    string dbBirthDate = DateHelper.ToDbDate(client.BirthDate);
                    command.Parameters.AddWithValue("@birthDate", string.IsNullOrEmpty(dbBirthDate) ? (object)DBNull.Value : dbBirthDate);
                    command.Parameters.AddWithValue("@joinDate", DateTime.Today);
                    command.Parameters.AddWithValue("@trainerId", client.TrainerId.HasValue ? (object)client.TrainerId.Value : DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Client client)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE members SET full_name = @fullName, phone = @phone, birth_date = @birthDate, trainer_id = @trainerId WHERE id = @id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", client.Id);
                    command.Parameters.AddWithValue("@fullName", client.FullName);
                    command.Parameters.AddWithValue("@phone", client.Phone);
                    string dbBirthDate = DateHelper.ToDbDate(client.BirthDate);
                    command.Parameters.AddWithValue("@birthDate", string.IsNullOrEmpty(dbBirthDate) ? (object)DBNull.Value : dbBirthDate);
                    command.Parameters.AddWithValue("@trainerId", client.TrainerId.HasValue ? (object)client.TrainerId.Value : DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM members WHERE id = @id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddSub(int clientId, int subscriptionId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                
                int durationDays = 30;
                string subName = "";

                string selectQuery = "SELECT name, duration_days FROM subscriptions WHERE id = @id";
                using (var cmd = new MySqlCommand(selectQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@id", subscriptionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            subName = reader.GetString(0);
                            durationDays = reader.GetInt32(1);
                        }
                    }
                }

                bool isFreeze = subName.Contains("Заморозка");

                if (isFreeze)
                {
                    string activeSubQuery = @"
                        SELECT id, end_date 
                        FROM member_subscriptions 
                        WHERE member_id = @clientId 
                          AND end_date >= CURDATE()
                          AND (visits_left IS NULL OR visits_left > 0)
                        ORDER BY end_date DESC LIMIT 1";

                    int activeSubId = -1;
                    DateTime? activeEndDate = null;

                    using (var cmd = new MySqlCommand(activeSubQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                activeSubId = reader.GetInt32(0);
                                activeEndDate = reader.GetDateTime(1);
                            }
                        }
                    }

                    if (activeSubId != -1 && activeEndDate.HasValue)
                    {
                        DateTime extendedEndDate = activeEndDate.Value.AddDays(durationDays);
                        string updateActiveSub = "UPDATE member_subscriptions SET end_date = @extendedEndDate WHERE id = @activeSubId";
                        using (var cmd = new MySqlCommand(updateActiveSub, connection))
                        {
                            cmd.Parameters.AddWithValue("@extendedEndDate", extendedEndDate);
                            cmd.Parameters.AddWithValue("@activeSubId", activeSubId);
                            cmd.ExecuteNonQuery();
                        }

                        DateTime purchaseDate = DateTime.Today;
                        DateTime endDate = DateTime.Today.AddDays(durationDays);

                        string insertQuery = @"
                            INSERT INTO member_subscriptions (member_id, subscription_id, purchase_date, end_date, visits_left) 
                            VALUES (@clientId, @subId, @purchaseDate, @endDate, 0)";

                        using (var cmd = new MySqlCommand(insertQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            cmd.Parameters.AddWithValue("@subId", subscriptionId);
                            cmd.Parameters.AddWithValue("@purchaseDate", purchaseDate);
                            cmd.Parameters.AddWithValue("@endDate", endDate);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    string deactivateQuery = @"
                        UPDATE member_subscriptions 
                        SET end_date = DATE_SUB(CURDATE(), INTERVAL 1 DAY), 
                            visits_left = CASE WHEN visits_left IS NOT NULL THEN 0 ELSE NULL END
                        WHERE member_id = @clientId 
                          AND end_date >= CURDATE()";

                    using (var cmd = new MySqlCommand(deactivateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        cmd.ExecuteNonQuery();
                    }

                    bool isLimitedVisits = false;
                    int visitsLimit = 0;
                    if (subName.Contains("12 занятий"))
                    {
                        isLimitedVisits = true;
                        visitsLimit = 12;
                    }
                    else if (subName.Contains("Разовое"))
                    {
                        isLimitedVisits = true;
                        visitsLimit = 1;
                    }

                    DateTime purchaseDate = DateTime.Today;
                    DateTime endDate = DateTime.Today.AddDays(durationDays);

                    string insertQuery = @"
                        INSERT INTO member_subscriptions (member_id, subscription_id, purchase_date, end_date, visits_left) 
                        VALUES (@clientId, @subId, @purchaseDate, @endDate, @visitsLeft)";

                    using (var cmd = new MySqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        cmd.Parameters.AddWithValue("@subId", subscriptionId);
                        cmd.Parameters.AddWithValue("@purchaseDate", purchaseDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                        cmd.Parameters.AddWithValue("@visitsLeft", isLimitedVisits ? (object)visitsLimit : DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
