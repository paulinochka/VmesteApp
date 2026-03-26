using Npgsql;
using System;
using System.Collections.Generic;
using VmesteApp.DB.Models;
using VmesteApp.EventsModels;

namespace VmesteApp.DB.Repository
{
    public class EventRepository
    {
        // Метод для получения всех событий семьи (с учетом приватности)
        public List<Events> GetFamilyEvents(int familyId, int currentUserId)
        {
            List<Events> eventsList = new List<Events>();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Логика: выбираем все публичные события семьи ИЛИ приватные события текущего пользователя
                string sql = @"SELECT * FROM ""VmesteDB"".events 
                               WHERE family_id = @fid 
                               AND (is_private = false OR user_id = @uid)
                               ORDER BY event_date, event_time";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("fid", familyId);
                    cmd.Parameters.AddWithValue("uid", currentUserId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            eventsList.Add(new Events
                            {
                                eventId = (int)reader["event_id"],
                                familyId = (int)reader["family_id"],
                                userId = (int)reader["user_id"],
                                name = reader["title"].ToString(),
                                description = reader["description"]?.ToString(),
                                eventDate = (DateTime)reader["event_date"],
                                eventTime = (DateTime)reader["event_time"],
                                category = reader["category"]?.ToString(),
                                isPrivate = (bool)reader["is_private"]
                            });
                        }
                    }
                }
            }
            return eventsList;
        }

        // Метод добавления нового события
        public void AddEvent(Events newEvent)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO ""VmesteDB"".events (family_id, user_id, title, 
                            description, event_date, event_time, category, is_private) 
                               VALUES (@fid, @uid, @name, @desc, @edate, @etime, @cat, @priv)";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("fid", newEvent.familyId);
                    cmd.Parameters.AddWithValue("uid", newEvent.userId);
                    cmd.Parameters.AddWithValue("name", newEvent.name);
                    cmd.Parameters.AddWithValue("desc", (object)newEvent.description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("edate", newEvent.eventDate);
                    cmd.Parameters.AddWithValue("etime", newEvent.eventTime);
                    cmd.Parameters.AddWithValue("cat", (object)newEvent.category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("priv", newEvent.isPrivate);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Удаление события (только создателем)
        public bool DeleteEvent(int eventId, int userId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM \"VmesteDB\".events WHERE event_id = @eid AND user_id = @uid";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("eid", eventId);
                    cmd.Parameters.AddWithValue("uid", userId);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        // Метод для получения ЗАДАЧ (события на сегодня)
        public List<TaskItem> GetTasksForToday(int familyId, int currentUserId)
        {
            var tasks = new List<TaskItem>();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Выбираем события, где дата равна сегодняшней
                string sql = @"SELECT title FROM ""VmesteDB"".events 
                       WHERE family_id = @fid 
                       AND (is_private = false OR user_id = @uid)
                       AND event_date = CURRENT_DATE
                       ORDER BY event_time";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("fid", familyId);
                    cmd.Parameters.AddWithValue("uid", currentUserId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                TaskName = reader["title"].ToString()
                            });
                        }
                    }
                }
            }
            return tasks;
        }

        // Метод для получения БЛИЖАЙШИХ СОБЫТИЙ (все что после сегодня)
        public List<EventItem> GetUpcomingEvents(int familyId, int currentUserId)
        {
            var events = new List<EventItem>();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Выбираем события, дата которых больше сегодняшней
                string sql = @"SELECT title, event_date, event_time, category FROM ""VmesteDB"".events 
                       WHERE family_id = @fid 
                       AND (is_private = false OR user_id = @uid)
                       AND event_date > CURRENT_DATE
                       ORDER BY event_date, event_time 
                       LIMIT 5"; // Берем только ближайшие 5

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("fid", familyId);
                    cmd.Parameters.AddWithValue("uid", currentUserId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime date = reader.GetDateTime(reader.GetOrdinal("event_date"));
                            TimeSpan timeSpan = reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("event_time"));
                            DateTime time = DateTime.Today.Add(timeSpan);

                            string category = reader["category"]?.ToString();

                            events.Add(new EventItem
                            {
                                Title = reader["title"].ToString(),
                                DateTimeString = $"{date:dd MMMM}, {time:HH:mm}",
                                Icon = GetIconByCategory(category)
                            });
                        }
                    }
                }
            }
            return events;
        }

        private string GetIconByCategory(string category)
{
    if (category == null) return "📅";

    switch (category.ToLower())
    {
        case "семья":
            return "👨‍👩‍👧‍👦";
        case "работа":
            return "💼";
        case "личное":
            return "👤";
        case "путешествие":
            return "🌍";
        default:
            return "📅";
    }
}
    }
}
