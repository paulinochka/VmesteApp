using Npgsql;
using System;
using System.Collections.Generic;
using VmesteApp.DB.Models;

namespace VmesteApp.DB.Repository
{
    public class EventRepository
    {
        // Метод для получения всех событий семьи (с учетом приватности)
        public List<Events> GetFamilyEvents(int familyId, int currentUserId)
        {
            List<Events> eventsList = new List<Events>();

            // Используйте ваш метод получения соединения (например, Database.GetConnection())
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
    }
}
