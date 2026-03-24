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
                string sql = @"SELECT * FROM events 
                               WHERE familyid = @fid 
                               AND (isprivate = false OR userid = @uid)
                               ORDER BY eventdate, eventtime";

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
                                eventId = (int)reader["eventid"],
                                familyId = (int)reader["familyid"],
                                userId = (int)reader["userid"],
                                name = reader["name"].ToString(),
                                description = reader["description"]?.ToString(),
                                eventDate = (DateTime)reader["eventdate"],
                                eventTime = (DateTime)reader["eventtime"],
                                category = reader["category"]?.ToString(),
                                isPrivate = (bool)reader["isprivate"]
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
                string sql = @"INSERT INTO events (familyid, userid, name, description, eventdate, eventtime, category, isprivate) 
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
                string sql = "DELETE FROM events WHERE eventid = @eid AND userid = @uid";
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
