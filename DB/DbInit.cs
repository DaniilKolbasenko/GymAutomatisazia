using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace GymManager.DB
{
    public class DbInit
    {
        private readonly string _connectionString;

        public DbInit(IOptions<DbConfig> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public void Initialize()
        {
            var builder = new SqliteConnectionStringBuilder(_connectionString);
            string dbPath = builder.DataSource;

            bool dbExists = File.Exists(dbPath);
            
            try
            {
                string absDbPath = Path.GetFullPath(dbPath);
                string workingDir = Directory.GetCurrentDirectory();
                string debugInfo = $"Working Dir: {workingDir}\nDatabase Path: {absDbPath}\nTime: {DateTime.Now}\nDbExists on check: {dbExists}\n";
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "db_debug.txt"), debugInfo);
            }
            catch { }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                if (!dbExists)
                {
                    string schemaSql = "";
                    if (File.Exists("schema.sql"))
                    {
                        schemaSql = File.ReadAllText("schema.sql");
                    }
                    else
                    {
                        schemaSql = GetDefaultSchemaSql();
                    }

                    using (var command = new SqliteCommand(schemaSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    string insertDefaults = @"
                        INSERT OR IGNORE INTO subscriptions (id, name, duration_days, price) VALUES 
                        (1, 'Месяц Безлимит', 30, 2500.0),
                        (2, '3 Месяца Безлимит', 90, 6500.0),
                        (3, '12 занятий (30 дней)', 30, 1800.0),
                        (4, 'Разовое посещение', 1, 300.0),
                        (5, 'Годовой абонемент', 365, 20000.0),
                        (6, 'Заморозка (7 дней)', 7, 300.0),
                        (7, 'Заморозка (14 дней)', 14, 500.0);";
                    using (var command = new SqliteCommand(insertDefaults, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Check if database needs population (missing new trainers or no members at all)
                bool needsPopulation = false;
                
                try
                {
                    using (var checkTrainersCmd = new SqliteCommand("SELECT COUNT(*) FROM trainers WHERE id = 6", connection))
                    {
                        long trainer6Count = Convert.ToInt64(checkTrainersCmd.ExecuteScalar());
                        if (trainer6Count == 0)
                        {
                            needsPopulation = true;
                        }
                    }

                    using (var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM members", connection))
                    {
                        long memberCount = Convert.ToInt64(checkCmd.ExecuteScalar());
                        if (memberCount == 0)
                        {
                            needsPopulation = true;
                        }
                    }
                }
                catch
                {
                    needsPopulation = true;
                }

                if (needsPopulation)
                {
                    try
                    {
                        string clearSql = @"
                            DELETE FROM visits;
                            DELETE FROM member_subscriptions;
                            DELETE FROM members;
                            DELETE FROM trainers;";
                        using (var clearCmd = new SqliteCommand(clearSql, connection))
                        {
                            clearCmd.ExecuteNonQuery();
                        }
                    }
                    catch { }

                    string populateSql = @"
                        INSERT OR REPLACE INTO trainers (id, full_name, specialization, phone) VALUES 
                        (1, 'Иванов Иван Иванович', 'Силовой тренинг, бодибилдинг', '+7 (999) 111-22-33'),
                        (2, 'Петрова Анна Сергеевна', 'Йога, пилатес, растяжка', '+7 (999) 444-55-66'),
                        (3, 'Сидоров Алексей Петрович', 'Кроссфит, функциональный тренинг', '+7 (999) 777-88-99'),
                        (4, 'Козлов Дмитрий Васильевич', 'Бокс, единоборства', '+7 (999) 222-33-44'),
                        (5, 'Смирнова Елена Игоревна', 'Кардио, снижение веса', '+7 (999) 555-66-77'),
                        (6, 'Морозов Сергей Александрович', 'Плавание, аквааэробика', '+7 (999) 888-99-00');

                        INSERT OR REPLACE INTO members (id, full_name, phone, birth_date, join_date, trainer_id) VALUES
                        (1, 'Фомин Артем Дмитриевич', '+7 (911) 123-45-67', '1995-03-12', date('now', '-30 days'), 1),
                        (2, 'Кузнецова Мария Андреевна', '+7 (911) 234-56-78', '1998-07-22', date('now', '-29 days'), 2),
                        (3, 'Лебедев Сергей Васильевич', '+7 (911) 345-67-89', '1992-11-05', date('now', '-28 days'), 3),
                        (4, 'Козлова Ольга Николаевна', '+7 (911) 456-78-90', '1990-05-15', date('now', '-10 days'), 1),
                        (5, 'Тарасов Вадим Михайлович', '+7 (911) 567-89-01', '1988-09-30', date('now', '-15 days'), 5),
                        (6, 'Морозова Екатерина Владимировна', '+7 (911) 678-90-12', '1996-01-25', date('now', '-45 days'), 2),
                        (7, 'Павлов Андрей Петрович', '+7 (911) 789-01-23', '1993-04-18', date('now', '-88 days'), 3),
                        (8, 'Новикова Татьяна Сергеевна', '+7 (911) 890-12-34', '1997-08-14', date('now', '-10 days'), 5),
                        (9, 'Соколов Игорь Святославович', '+7 (911) 901-23-45', '1991-12-08', date('now', '-20 days'), 1),
                        (10, 'Воробьева Анастасия Денисовна', '+7 (922) 111-22-33', '1999-02-17', date('now', '-30 days'), 2),
                        (11, 'Федоров Даниил Андреевич', '+7 (922) 222-33-44', '1994-06-03', date('now', '-15 days'), 3),
                        (12, 'Михайлова Вера Сергеевна', '+7 (922) 333-44-55', '1990-10-27', date('now', '-29 days'), 2),
                        (13, 'Романов Кирилл Валерьевич', '+7 (922) 444-55-66', '1995-05-19', date('now', '-10 days'), 1),
                        (14, 'Степанова Анна Алексеевна', '+7 (922) 555-66-77', '1998-09-11', date('now', '-5 days'), 5),
                        (15, 'Белов Денис Игоревич', '+7 (922) 666-77-88', '1992-01-04', date('now', '-25 days'), 6),
                        (16, 'Яковлева Ольга Петровна', '+7 (922) 777-88-99', '1997-03-28', date('now', '-20 days'), 2),
                        (17, 'Григорьев Никита Александрович', '+7 (922) 888-99-00', '1993-07-21', date('now', '-8 days'), 3),
                        (18, 'Родионова Мария Викторовна', '+7 (933) 111-22-33', '1996-11-14', date('now', '-12 days'), 1),
                        (19, 'Егоров Павел Сергеевич', '+7 (933) 222-33-44', '1991-04-07', date('now'), 5),
                        (20, 'Ковалева Татьяна Анатольевна', '+7 (933) 333-44-55', '1999-08-31', date('now', '-1 day'), 6),
                        (21, 'Кудрявцев Александр Владимирович', '+7 (933) 444-55-66', '1994-12-24', date('now'), 1),
                        (22, 'Александров Михаил Юрьевич', '+7 (933) 555-66-77', '1990-02-15', date('now', '-180 days'), 6),
                        (23, 'Захарова Елена Николаевна', '+7 (933) 666-77-88', '1995-06-09', date('now', '-363 days'), 2),
                        (24, 'Борисов Сергей Алексеевич', '+7 (933) 777-88-99', '1992-10-02', date('now', '-10 days'), 3),
                        (25, 'Дмитриев Илья Валерьевич', '+7 (944) 111-22-33', '1997-01-26', date('now', '-10 days'), 1),
                        (26, 'Ильина Ксения Сергеевна', '+7 (944) 222-33-44', '1993-05-18', date('now', '-10 days'), 2);

                        INSERT OR REPLACE INTO member_subscriptions (id, member_id, subscription_id, purchase_date, end_date, visits_left) VALUES
                        (1, 1, 1, date('now', '-5 days'), date('now', '+25 days'), NULL),
                        (2, 2, 1, date('now', '-29 days'), date('now', '+1 day'), NULL),
                        (3, 3, 1, date('now', '-28 days'), date('now', '+2 days'), NULL),
                        (4, 4, 1, date('now', '-10 days'), date('now', '+20 days'), NULL),
                        (5, 5, 1, date('now', '-15 days'), date('now', '+15 days'), NULL),
                        (6, 6, 2, date('now', '-45 days'), date('now', '+45 days'), NULL),
                        (7, 7, 2, date('now', '-88 days'), date('now', '+2 days'), NULL),
                        (8, 8, 2, date('now', '-10 days'), date('now', '+80 days'), NULL),
                        (9, 9, 2, date('now', '-20 days'), date('now', '+70 days'), NULL),
                        (10, 10, 2, date('now', '-30 days'), date('now', '+60 days'), NULL),
                        (11, 11, 3, date('now', '-15 days'), date('now', '+15 days'), 6),
                        (12, 12, 3, date('now', '-29 days'), date('now', '+1 day'), 2),
                        (13, 13, 3, date('now', '-10 days'), date('now', '+20 days'), 12),
                        (14, 14, 3, date('now', '-5 days'), date('now', '+25 days'), 10),
                        (15, 15, 3, date('now', '-25 days'), date('now', '+5 days'), 0),
                        (16, 16, 3, date('now', '-20 days'), date('now', '+10 days'), 1),
                        (17, 17, 3, date('now', '-8 days'), date('now', '+22 days'), 8),
                        (18, 18, 3, date('now', '-12 days'), date('now', '+18 days'), 5),
                        (19, 19, 4, date('now'), date('now'), 1),
                        (20, 20, 4, date('now', '-1 day'), date('now', '-1 day'), 0),
                        (21, 21, 4, date('now'), date('now'), 1),
                        (22, 22, 5, date('now', '-180 days'), date('now', '+185 days'), NULL),
                        (23, 23, 5, date('now', '-363 days'), date('now', '+2 days'), NULL),
                        (24, 24, 5, date('now', '-10 days'), date('now', '+355 days'), NULL),
                        (25, 25, 1, date('now', '-10 days'), date('now', '+27 days'), NULL),
                        (26, 25, 6, date('now', '-5 days'), date('now', '+2 days'), 0),
                        (27, 26, 2, date('now', '-10 days'), date('now', '+94 days'), NULL),
                        (28, 26, 7, date('now', '-1 day'), date('now', '+13 days'), 0);

                        INSERT OR REPLACE INTO visits (id, member_id, visit_date) VALUES
                        (1, 1, date('now', '-1 hour')),
                        (2, 4, date('now', '-2 hours')),
                        (3, 11, date('now', '-3 hours')),
                        (4, 16, date('now', '-4 hours')),
                        (5, 22, date('now', '-5 hours'));";
                    using (var popCmd = new SqliteCommand(populateSql, connection))
                    {
                        popCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private string GetDefaultSchemaSql()
        {
            return @"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE NOT NULL,
                password_hash TEXT NOT NULL,
                role TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS trainers (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                full_name TEXT NOT NULL,
                specialization TEXT,
                phone TEXT
            );

            CREATE TABLE IF NOT EXISTS members (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                full_name TEXT NOT NULL,
                phone TEXT NOT NULL,
                birth_date TEXT,
                join_date TEXT NOT NULL,
                trainer_id INTEGER,
                FOREIGN KEY (trainer_id) REFERENCES trainers(id) ON DELETE SET NULL
            );

            CREATE TABLE IF NOT EXISTS subscriptions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                duration_days INTEGER NOT NULL,
                price REAL NOT NULL
            );

            CREATE TABLE IF NOT EXISTS member_subscriptions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                member_id INTEGER NOT NULL,
                subscription_id INTEGER NOT NULL,
                purchase_date TEXT NOT NULL,
                end_date TEXT NOT NULL,
                visits_left INTEGER,
                FOREIGN KEY (member_id) REFERENCES members(id) ON DELETE CASCADE,
                FOREIGN KEY (subscription_id) REFERENCES subscriptions(id) ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS visits (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                member_id INTEGER NOT NULL,
                visit_date TEXT NOT NULL,
                FOREIGN KEY (member_id) REFERENCES members(id) ON DELETE CASCADE
            );

            INSERT OR IGNORE INTO users (id, username, password_hash, role) VALUES 
            (1, 'admin', '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918', 'Администратор');

            INSERT OR IGNORE INTO subscriptions (id, name, duration_days, price) VALUES 
            (1, 'Месяц Безлимит', 30, 2500.0),
            (2, '3 Месяца Безлимит', 90, 6500.0),
            (3, '12 занятий (30 дней)', 30, 1800.0),
            (4, 'Разовое посещение', 1, 300.0),
            (5, 'Годовой абонемент', 365, 20000.0),
            (6, 'Заморозка (7 дней)', 7, 300.0),
            (7, 'Заморозка (14 дней)', 14, 500.0);

            INSERT OR IGNORE INTO trainers (id, full_name, specialization, phone) VALUES 
            (1, 'Иванов Иван Иванович', 'Силовой тренинг, пауэрлифтинг', '+7 (999) 111-22-33'),
            (2, 'Петрова Анна Сергеевна', 'Кардио, пилатес, растяжка', '+7 (999) 444-55-66'),
            (3, 'Сидоров Алексей Петрович', 'Кроссфит, functional-тренинг', '+7 (999) 777-88-99');
            ";
        }
    }
}
