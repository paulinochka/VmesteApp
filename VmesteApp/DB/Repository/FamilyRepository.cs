using Npgsql;
using System;
using System.Collections.Generic;
using VmesteApp.DB.Models;

namespace VmesteApp.DB.Repository
{
    public class FamilyRepository
    {
        // 1. Получение списка всех участников семьи по ID семьи
        public List<Users> GetFamilyMembers(int familyId)
        {
            List<Users> members = new List<Users>();
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM \"VmesteDB\".users WHERE family_id = @fid";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("fid", familyId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(new Users
                            {
                                userId = reader.GetInt32(reader.GetOrdinal("user_id")),
                                name = reader.GetString(reader.GetOrdinal("full_name")),
                                email = reader.GetString(reader.GetOrdinal("email")),
                                phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                                role = reader.GetString(reader.GetOrdinal("user_role")),
                                familyId = reader.GetInt32(reader.GetOrdinal("family_id"))
                            });
                        }
                    }
                }
            }
            return members;
        }

        // 2. Получение данных о семье по ID
        public Families GetFamilyById(int familyId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM \"VmesteDB\".families WHERE family_id = @id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", familyId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Families
                            {
                                familyID = reader.GetInt32(reader.GetOrdinal("family_id")),
                                familyCode = reader.GetString(reader.GetOrdinal("invite_code"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        // 3. Выход из семьи (удаление связи у пользователя)
        public void LeaveFamily(int userId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Устанавливаем family_id в NULL, чтобы отвязать пользователя
                string sql = "UPDATE \"VmesteDB\".users SET family_id = NULL WHERE user_id = @uid";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("uid", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 4. Удаление семьи (только если в ней нет других участников, кроме главы)
        public void DeleteFamily(int familyId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Сначала отвязываем всех пользователей
                        string updateUsers = "UPDATE \"VmesteDB\".users SET family_id = NULL WHERE family_id = @fid";
                        using (var cmd = new NpgsqlCommand(updateUsers, conn))
                        {
                            cmd.Parameters.AddWithValue("fid", familyId);
                            cmd.ExecuteNonQuery();
                        }

                        // Затем удаляем саму запись семьи
                        string deleteFam = "DELETE FROM \"VmesteDB\".families WHERE family_id = @fid";
                        using (var cmd = new NpgsqlCommand(deleteFam, conn))
                        {
                            cmd.Parameters.AddWithValue("fid", familyId);
                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}