using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using GymManager.Models;

namespace GymManager.DB
{
    public class VisitRepo
    {
        private readonly string _connectionString;

        public VisitRepo(IOptions<DbConfig> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public List<Visit> GetAll()
        {
            var list = new List<Visit>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT v.id, v.member_id, m.full_name, v.visit_date 
                    FROM visits v
                    JOIN members m ON v.member_id = m.id
                    ORDER BY v.visit_date DESC LIMIT 100";

                using (var command = new SqliteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Visit
                        {
                            Id = reader.GetInt32(0),
                            ClientId = reader.GetInt32(1),
                            ClientName = reader.GetString(2),
                            VisitDate = DateHelper.ToDisplayDateTime(reader.GetString(3))
                        });
                    }
                }
            }

            return list;
        }

        public string AddVisit(int clientId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string checkQuery = @"
                    SELECT id, visits_left, end_date
                    FROM member_subscriptions
                    WHERE member_id = @clientId 
                      AND date(end_date) >= date('now')
                      AND (visits_left IS NULL OR visits_left > 0)
                    ORDER BY end_date ASC LIMIT 1";

                int subId = -1;
                object? visitsLeftObj = null;

                using (var cmd = new SqliteCommand(checkQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            subId = reader.GetInt32(0);
                            visitsLeftObj = reader.GetValue(1);
                        }
                    }
                }

                if (subId == -1)
                {
                    return "Нет активного абонемента или закончились занятия!";
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertVisit = "INSERT INTO visits (member_id, visit_date) VALUES (@clientId, @visitDate)";
                        using (var cmd = new SqliteCommand(insertVisit, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            cmd.Parameters.AddWithValue("@visitDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.ExecuteNonQuery();
                        }

                        if (visitsLeftObj != DBNull.Value)
                        {
                            int currentVisits = Convert.ToInt32(visitsLeftObj);
                            int nextVisits = currentVisits - 1;

                            string updateSub = "UPDATE member_subscriptions SET visits_left = @visitsLeft WHERE id = @subId";
                            using (var cmd = new SqliteCommand(updateSub, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@visitsLeft", nextVisits);
                                cmd.Parameters.AddWithValue("@subId", subId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return "Успешно";
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return "Ошибка при записи посещения";
                    }
                }
            }
        }

        public List<Client> GetEndingSubs()
        {
            var list = new List<Client>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT m.id, m.full_name, m.phone, ms.end_date, s.name
                    FROM member_subscriptions ms
                    JOIN members m ON ms.member_id = m.id
                    JOIN subscriptions s ON ms.subscription_id = s.id
                    WHERE date(ms.end_date) >= date('now') 
                      AND date(ms.end_date) <= date('now', '+3 days')
                      AND (ms.visits_left IS NULL OR ms.visits_left > 0)
                    ORDER BY ms.end_date ASC";

                using (var command = new SqliteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Phone = reader.GetString(2),
                            SubscriptionEndDate = DateHelper.ToDisplayDate(reader.GetString(3)),
                            ActiveSubscriptionName = reader.GetString(4)
                        });
                    }
                }
            }

            return list;
        }

        public Dictionary<string, double> GetStats()
        {
            var stats = new Dictionary<string, double>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM members", connection))
                {
                    stats["TotalMembers"] = Convert.ToDouble(cmd.ExecuteScalar());
                }

                string activeSubsQuery = @"
                    SELECT COUNT(DISTINCT member_id) 
                    FROM member_subscriptions 
                    WHERE date(end_date) >= date('now') 
                      AND (visits_left IS NULL OR visits_left > 0)";
                using (var cmd = new SqliteCommand(activeSubsQuery, connection))
                {
                    stats["ActiveSubscriptions"] = Convert.ToDouble(cmd.ExecuteScalar());
                }

                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM trainers", connection))
                {
                    stats["TotalTrainers"] = Convert.ToDouble(cmd.ExecuteScalar());
                }

                string todayVisitsQuery = "SELECT COUNT(*) FROM visits WHERE date(visit_date) = date('now')";
                using (var cmd = new SqliteCommand(todayVisitsQuery, connection))
                {
                    stats["TodayVisits"] = Convert.ToDouble(cmd.ExecuteScalar());
                }

                string revenueQuery = @"
                    SELECT COALESCE(SUM(s.price), 0) 
                    FROM member_subscriptions ms 
                    JOIN subscriptions s ON ms.subscription_id = s.id";
                using (var cmd = new SqliteCommand(revenueQuery, connection))
                {
                    stats["TotalRevenue"] = Convert.ToDouble(cmd.ExecuteScalar());
                }
            }

            return stats;
        }

        public Dictionary<string, int> GetSubStats()
        {
            var dist = new Dictionary<string, int>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT s.name, COUNT(ms.id) 
                    FROM subscriptions s
                    LEFT JOIN member_subscriptions ms ON s.id = ms.subscription_id
                        AND date(ms.end_date) >= date('now')
                        AND (ms.visits_left IS NULL OR ms.visits_left > 0)
                    GROUP BY s.name";

                using (var command = new SqliteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dist[reader.GetString(0)] = reader.GetInt32(1);
                    }
                }
            }

            return dist;
        }
    }
}
