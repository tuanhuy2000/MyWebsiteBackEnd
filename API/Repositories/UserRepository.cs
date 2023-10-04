using API.Data;
using API.Model;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Repositories
{
    public class UserRepository
    {
        private readonly DBConnection conn;
        private readonly IConfiguration _configuration;

        public UserRepository(DBConnection conn, IConfiguration configuration)
        {
            this.conn = conn;
            _configuration = configuration;
        }

        //public async Task<List<User>> GetAllUsersAsync()
        //{
        //    List<User> users = new List<User>();
        //    MySqlConnection connect = conn.ConnectDB();
        //    try
        //    {
        //        connect.Open();

        //        var command = new MySqlCommand();
        //        command.Connection = connect;

        //        string queryString = "SELECT * FROM tbl_user";
        //        command.CommandText = queryString;

        //        await using var reader = command.ExecuteReader();

        //        if (reader.HasRows)
        //        {
        //            while (reader.Read())
        //            {
        //                var id = reader.GetString(0);
        //                //var name = reader["Name"];
        //                var name = reader.GetString(1);
        //                var phoneNumber = reader.GetString("PhoneNumber");
        //                var email = reader.GetString(3);
        //                var role = reader.GetString(6);
        //                User user = new User { Id = id, Name = name, PhoneNumber = phoneNumber, Email = email, Role = role };
        //                users.Add(user);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        connect.Close();
        //        return users;
        //    }
        //    connect.Close();
        //    return users;
        //}

        public int Signin(User user)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_user (Id, Name, PhoneNumber, Email, UserName, Password, Role) VALUES (@Id, @Name, @PhoneNumber, @Email, @UserName, @Password, @Role)";
                sql.Parameters.AddWithValue("@Id", user.Id);
                sql.Parameters.AddWithValue("@Name", user.Name);
                sql.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                sql.Parameters.AddWithValue("@Email", user.Email);
                sql.Parameters.AddWithValue("@UserName", user.UserName);
                sql.Parameters.AddWithValue("@Password", user.Password);
                sql.Parameters.AddWithValue("@Role", user.Role);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect.Close();
                return 0;
            }
        }

        public async Task<User> Login(Login login)
        {
            User userLogin = null;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();

                var command = new MySqlCommand();
                command.Connection = connect;

                string queryString = "SELECT * FROM tbl_user WHERE UserName = @username AND Password = @password";
                command.Parameters.AddWithValue("@username", login.UserName);
                command.Parameters.AddWithValue("@password", login.Password);
                command.CommandText = queryString;

                await using var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var id = reader.GetString(0);
                        var name = reader.GetString(1);
                        var phoneNumber = reader.GetString("PhoneNumber");
                        var email = reader.GetString(3);
                        var Role = reader.GetString(6);
                        userLogin = new User { Id = id, Name = name, PhoneNumber = phoneNumber, Email = email, Role = Role };
                        break;
                    }
                }
                else
                {
                    connect.Close();
                    return userLogin;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            connect.Close();
            return userLogin;
        }

        public string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }

        public int SaveRefreshToken(RefreshToken refreshToken)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var sql = new MySqlCommand();
                sql.Connection = connect;
                string queryString = "SELECT * FROM tbl_refreshtoken WHERE IdUser = @IdUser";
                sql.Parameters.AddWithValue("@IdUser", refreshToken.User.Id);
                sql.CommandText = queryString;
                using var reader = sql.ExecuteReader();
                if (reader.HasRows)
                {
                    connect.Close();
                    connect.Open();
                    sql.CommandText = "UPDATE tbl_refreshtoken SET RefreshToken = @RefreshToken , Expire = @Expire WHERE IdUser = @IdU";
                    sql.Parameters.AddWithValue("@IdU", refreshToken.User.Id);
                    sql.Parameters.AddWithValue("@RefreshToken", refreshToken.RefreshTokenn);
                    sql.Parameters.AddWithValue("@Expire", refreshToken.Expire.ToString("yyyy-MM-dd HH:mm:ss"));
                    int result = sql.ExecuteNonQuery();
                    connect.Close();
                    return result;
                }
                else
                {
                    connect.Close();
                    connect.Open();
                    sql.CommandText = "INSERT INTO tbl_refreshtoken (Id, IdUser, RefreshToken, Expire) VALUES (@Id, @UserId, @RefreshToken, @Expire)";
                    sql.Parameters.AddWithValue("@Id", refreshToken.Id);
                    sql.Parameters.AddWithValue("@UserId", refreshToken.User.Id);
                    sql.Parameters.AddWithValue("@RefreshToken", refreshToken.RefreshTokenn);
                    sql.Parameters.AddWithValue("@Expire", refreshToken.Expire.ToString("yyyy-MM-dd HH:mm:ss"));
                    int result = sql.ExecuteNonQuery();
                    connect.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return 0;
            }
        }

        public string GetToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString() ),
                new Claim("UserId",user.Id.ToString()),
                new Claim("Name",user.Name),
                new Claim("Email",user.Email),
                //role
                new Claim(ClaimTypes.Role,user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddSeconds(10),
                signingCredentials: signIn
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GetRefreshToken(string IdUser)
        {
            MySqlConnection connect = conn.ConnectDB();
            string token = null;
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT RefreshToken FROM tbl_refreshtoken WHERE IdUser = @IdUser";
                command.Parameters.AddWithValue("@IdUser", IdUser);
                command.CommandText = queryString;
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        token = reader.GetString(0);
                        break;
                    }
                }
                else
                {
                    connect.Close();
                    return token;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            connect.Close();
            return token;
        }

        public async Task<Page> GetPageUsersAsync(int pageNum, int perPage, string direction)
        {
            List<User> users = new List<User>();
            int total = 0;
            int totalPages = 0;
            Page page = null;
            MySqlConnection connect = conn.ConnectDB();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(*) FROM tbl_user";
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        total = reader.GetInt32(0);
                    }
                }
                connect.Close();
                connect1.Open();
                var command1 = new MySqlCommand();
                command1.Connection = connect1;
                command1.CommandText = "SELECT * FROM tbl_user ORDER BY Name " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var phoneNumber = reader1.GetString("PhoneNumber");
                        var email = reader1.GetString(3);
                        var role = reader1.GetString(6);
                        User user = new User { Id = id, Name = name, PhoneNumber = phoneNumber, Email = email, Role = role };
                        users.Add(user);
                    }
                }
                connect1.Close();
                if (((double)total / (double)perPage) % 1 == 0)
                {
                    totalPages = total / perPage;
                }
                else
                {
                    totalPages = (total / perPage) + 1;
                }
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = users };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<string> RenewToken(string RefreshToken)
        {
            MySqlConnection connect = conn.ConnectDB();
            DateTime expire = new DateTime(1, 1, 1);
            string IdUser = null;
            string newToken = null;
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT IdUser, Expire FROM tbl_refreshtoken WHERE RefreshToken = @RefreshToken";
                command.Parameters.AddWithValue("@RefreshToken", RefreshToken);
                command.CommandText = queryString;
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        IdUser = reader.GetString(0);
                        expire = reader.GetDateTime(1);
                        break;
                    }
                }
                if (DateTime.UtcNow < expire)
                {
                    User user = null;
                    try
                    {
                        connect.Close();
                        connect.Open();
                        string query = "SELECT * FROM tbl_user WHERE Id = @Id";
                        command.Parameters.AddWithValue("@Id", IdUser);
                        command.CommandText = query;
                        await using var read = command.ExecuteReader();
                        if (read.HasRows)
                        {
                            while (read.Read())
                            {
                                var id = read.GetString(0);
                                //var name = reader["Name"];
                                var name = read.GetString(1);
                                var phoneNumber = read.GetString("PhoneNumber");
                                var email = read.GetString(3);
                                var role = read.GetString(6);
                                user = new User { Id = id, Name = name, PhoneNumber = phoneNumber, Email = email, Role = role };
                            }
                        }
                        newToken = GetToken(user);
                    }
                    catch (Exception ex)
                    {
                        connect.Close();
                        return null;
                    }
                }
                connect.Close();
                return newToken;
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
        }

        public async Task<User> GetUserByRefreshToken(string refreshToken)
        {
            User userLogin = null;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT u.Id, u.Name, u.PhoneNumber, u.Email, u.Role FROM tbl_refreshtoken As r, tbl_user AS u WHERE r.IdUser = u.Id AND r.RefreshToken = @refreshToken";
                command.Parameters.AddWithValue("@refreshToken", refreshToken);
                command.CommandText = queryString;

                await using var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var id = reader.GetString(0);
                        var name = reader.GetString(1);
                        var phoneNumber = reader.GetString("PhoneNumber");
                        var email = reader.GetString(3);
                        var Role = reader.GetString(4);
                        userLogin = new User { Id = id, Name = name, PhoneNumber = phoneNumber, Email = email, Role = Role };
                        break;
                    }
                }
                else
                {
                    connect.Close();
                    return userLogin;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            connect.Close();
            return userLogin;
        }

        public int DeleteUserById(string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect.Open();
                var sql = new MySqlCommand();
                sql.Connection = connect;
                string queryString = "DELETE r.* FROM tbl_refreshtoken As r, tbl_user AS u WHERE r.IdUser = u.Id AND u.Id = @Id";
                sql.Parameters.AddWithValue("@Id", id);
                sql.CommandText = queryString;
                sql.ExecuteNonQuery();
                connect.Close();
                connect1.Open();
                var sql1 = new MySqlCommand();
                sql1.Connection = connect1;
                string queryString1 = "DELETE FROM tbl_user WHERE Id = @Id";
                sql1.Parameters.AddWithValue("@Id", id);
                sql1.CommandText = queryString1;
                int result = sql1.ExecuteNonQuery();
                connect1.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return 0;
            }
        }

        public int ChangePasswordById(string id, string password, string newPassword)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var sql = new MySqlCommand();
                sql.Connection = connect;
                string queryString = "UPDATE tbl_user SET Password = @newPassword WHERE Id = @Id AND Password = @password";
                sql.Parameters.AddWithValue("@Id", id);
                sql.Parameters.AddWithValue("@newPassword", newPassword);
                sql.Parameters.AddWithValue("@password", password);
                sql.CommandText = queryString;
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect.Close();
                return 0;
            }
        }

        public int ChangeUserInfor(User user)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var sql = new MySqlCommand();
                sql.Connection = connect;
                string queryString = "UPDATE tbl_user SET Name = @name, PhoneNumber = @phoneNumber, Email = @email WHERE Id = @Id";
                sql.Parameters.AddWithValue("@name", user.Name);
                sql.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
                sql.Parameters.AddWithValue("@email", user.Email);
                sql.Parameters.AddWithValue("@Id", user.Id);
                sql.CommandText = queryString;
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect.Close();
                return 0;
            }
        }
    }
}
