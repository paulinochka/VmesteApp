using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VmesteApp.DB.Models;
using VmesteApp.Security;

namespace VmesteApp.DB.Repository
{
    public class UserRepository
    {
        public bool RegisterUser(Users user, string inviteCode = null)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int? finalFamilyId = null;

                        if (user.role == "Глава семьи")
                        {
                            // 1. Генерируем новый код (например, VMESTE-XXXX)
                            string newCode = "VM-" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper();

                            // 2. Создаем новую семью
                            using (var cmdFam = new NpgsqlCommand(
                                "INSERT INTO \"VmesteDB\".families (invite_code) VALUES (@c) RETURNING family_id", conn))
                            {
                                cmdFam.Parameters.AddWithValue("c", newCode);
                                finalFamilyId = (int)cmdFam.ExecuteScalar();
                            }

                        }
                        else if (user.role == "Участник семьи")
                        {
                            // 1. Ищем ID семьи по введенному коду
                            using (var cmdCheck = new NpgsqlCommand(
                                "SELECT family_id FROM \"VmesteDB\".families WHERE invite_code = @c", conn))
                            {
                                cmdCheck.Parameters.AddWithValue("c", inviteCode ?? "");
                                var result = cmdCheck.ExecuteScalar();

                                if (result == null) throw new Exception("Код семьи не найден!");
                                finalFamilyId = (int)result;
                            }
                        }

                        // 3. Регистрируем пользователя с полученным family_id
                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO \"VmesteDB\".users (full_name, email, phone, password_hash, user_role, family_id) " +
                            "VALUES (@n, @e, @ph, @ps, @r, @fid)", conn))
                        {
                            cmd.Parameters.AddWithValue("n", user.name);
                            cmd.Parameters.AddWithValue("e", user.email);
                            cmd.Parameters.AddWithValue("ph", user.phone ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("ps", user.password);
                            cmd.Parameters.AddWithValue("r", user.role);
                            cmd.Parameters.AddWithValue("fid", (object)finalFamilyId ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        // Тут можно пробросить ошибку дальше, чтобы показать её в UI
                        throw ex;
                    }
                }
            }
        }
        public void UpdateUserProfile(int userId, string name, string email, string phone)
        {
            try
            {
                using (var connection = DbConnection.GetConnection())
                {
                    connection.Open();
                    var sql = "UPDATE \"VmesteDB\".users SET full_name = @name, email = @email, phone = @phone WHERE user_id = @id";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("name", name);
                        cmd.Parameters.AddWithValue("email", email);
                        cmd.Parameters.AddWithValue("phone", (object)phone ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new Exception("Пользователь с такой почтой или номером телефона уже зарегистрирован.");
            }
        }
        public void UpdatePassword(int userId, string hashedPassword)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Используем двойные кавычки для схемы и таблицы, как в вашем Login
                string sql = "UPDATE \"VmesteDB\".users SET password_hash = @ps WHERE user_id = @id";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("ps", hashedPassword);
                    cmd.Parameters.AddWithValue("id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void UpdateAvatarPath(int userId, string fileName)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE \"VmesteDB\".users SET avatar_path = @p WHERE user_id = @id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("p", fileName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public Users Login(string identifier, string password)
        {
            string hashedPassword = PasswordHasher.HashPassword(password);

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
            SELECT u.*, f.invite_code 
            FROM ""VmesteDB"".users u
            LEFT JOIN ""VmesteDB"".families f ON u.family_id = f.family_id
            WHERE (u.email = @id OR u.phone = @id) AND u.password_hash = @ps";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", identifier);
                    cmd.Parameters.AddWithValue("ps", hashedPassword);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Users
                            {
                                userId = reader.GetInt32(reader.GetOrdinal("user_id")),
                                name = reader.GetString(reader.GetOrdinal("full_name")),
                                email = reader.GetString(reader.GetOrdinal("email")),
                                phone = reader.IsDBNull(reader.GetOrdinal("phone"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("phone")),
                                role = reader.GetString(reader.GetOrdinal("user_role")),
                                familyId = reader.IsDBNull(reader.GetOrdinal("family_id"))
                                           ? (int?)null
                                           : reader.GetInt32(reader.GetOrdinal("family_id")),

                                familyInviteCode = reader.IsDBNull(reader.GetOrdinal("invite_code"))
                                                   ? "Нет кода"
                                                   : reader.GetString(reader.GetOrdinal("invite_code"))
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
