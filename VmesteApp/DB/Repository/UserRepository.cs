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
        public bool RegisterUser(Users user)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO \"VmesteDB\".users (full_name, email, phone, password_hash, user_role, family_id) " +
                    "VALUES (@n, @e, @ph, @ps, @r, @fc)", conn))
                {
                    cmd.Parameters.AddWithValue("n", user.name);
                    cmd.Parameters.AddWithValue("e", user.email);
                    cmd.Parameters.AddWithValue("ph", user.phone);
                    cmd.Parameters.AddWithValue("ps", user.password);
                    cmd.Parameters.AddWithValue("r", user.role);
                    cmd.Parameters.AddWithValue("fc", (object)user.familyId ?? DBNull.Value);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public Users Login(string identifier, string password)
        {
            string hashedPassword = PasswordHasher.HashPassword(password);

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Сравниваем с колонкой password_hash в базе
                string sql = "SELECT * FROM \"VmesteDB\".users WHERE (email = @id OR phone = @id) AND password_hash = @ps";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", identifier);
                    cmd.Parameters.AddWithValue("ps", hashedPassword); // Сравниваем хэши

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Users
                            {
                                userId = reader.GetInt32(reader.GetOrdinal("user_id")),
                                name = reader.GetString(reader.GetOrdinal("full_name")),
                                email = reader.GetString(reader.GetOrdinal("email")),
                                role = reader.GetString(reader.GetOrdinal("user_role")),
                                familyId = reader.IsDBNull(reader.GetOrdinal("family_id"))
                                           ? (int?)null
                                           : reader.GetInt32(reader.GetOrdinal("family_id"))
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
