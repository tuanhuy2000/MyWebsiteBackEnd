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

        public bool Signin(User user)
        {
            MySqlConnection connect = conn.ConnectDB();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                //sql.CommandText = "INSERT INTO tbl_user (Id, Name, PhoneNumber, Email, UserName, Password, Role) VALUES (@Id, @Name, @PhoneNumber, @Email, @UserName, @Password, @Role)";
                sql.CommandText = "INSERT INTO tbl_user (Id, Name, PhoneNumber, Email, UserName, Password, Role) SELECT * FROM (SELECT @Id AS Id, @Name AS Name, @PhoneNumber AS PhoneNumber, @Email AS Email, @UserName AS UserName, @Password AS Password, @Role AS Role) AS tmp WHERE NOT EXISTS (SELECT UserName FROM tbl_user WHERE UserName = @UserName) LIMIT 1";
                sql.Parameters.AddWithValue("@Id", user.Id);
                sql.Parameters.AddWithValue("@Name", user.Name);
                sql.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                sql.Parameters.AddWithValue("@Email", user.Email);
                sql.Parameters.AddWithValue("@UserName", user.UserName);
                sql.Parameters.AddWithValue("@Password", user.Password);
                sql.Parameters.AddWithValue("@Role", user.Role);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                connect1.Open();
                var sql1 = new MySqlCommand();
                sql1.Connection = connect1;
                string queryString1 = "INSERT INTO tbl_cart(Id, IdUser) VALUES (@Id, @IdUser)";
                sql1.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                sql1.Parameters.AddWithValue("@IdUser", user.Id);
                sql1.CommandText = queryString1;
                int result1 = sql1.ExecuteNonQuery();
                connect1.Close();
                if (result1 == 1 && result == 1)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
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
                expires: DateTime.UtcNow.AddMinutes(1),
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

        public bool DeleteUserById(string id)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            MySqlConnection connect2 = conn.ConnectDB();
            MySqlConnection connect3 = conn.ConnectDB();
            MySqlConnection connect4 = conn.ConnectDB();
            MySqlConnection connect5 = conn.ConnectDB();
            MySqlConnection connect6 = conn.ConnectDB();
            MySqlConnection connect7 = conn.ConnectDB();
            MySqlConnection connect8 = conn.ConnectDB();
            MySqlConnection connect9 = conn.ConnectDB();
            MySqlConnection connect10 = conn.ConnectDB();
            MySqlConnection connect11 = conn.ConnectDB();
            try
            {
                //delete from tbl_order when order in user's shop
                connect11.Open();
                var sql11 = new MySqlCommand();
                sql11.Connection = connect11;
                string queryString11 = "DELETE o.* FROM tbl_order As o, tbl_user AS u, tbl_shop AS s WHERE s.IdUser = u.Id AND u.Id = @Id AND o.IdShop = s.Id";
                sql11.Parameters.AddWithValue("@Id", id);
                sql11.CommandText = queryString11;
                sql11.ExecuteNonQuery();
                connect11.Close();
                // delete from tbl_cart_product when product in user's shop or user's cart
                connect1.Open();
                var sql1 = new MySqlCommand();
                sql1.Connection = connect1;
                string queryString1 = "DELETE cp.* FROM tbl_cart_product As cp, tbl_user AS u, tbl_shop AS s, tbl_product AS p, tbl_cart AS c " +
                    "WHERE ( s.IdUser = u.Id AND s.Id = p.IdShop AND u.Id = @Id AND cp.IdProduct = p.Id )" +
                    " OR ( u.Id = @Id AND u.Id = c.IdUser AND c.Id = cp.IdCart )";
                sql1.Parameters.AddWithValue("@Id", id);
                sql1.CommandText = queryString1;
                sql1.ExecuteNonQuery();
                connect1.Close();
                // delete img when img in user's shop's product
                connect8.Open();
                var sql8 = new MySqlCommand();
                sql8.Connection = connect8;
                string queryString8 = "DELETE i.* FROM tbl_img As i, tbl_user AS u, tbl_shop AS s, tbl_product AS p " +
                    "WHERE s.IdUser = u.Id AND s.Id = p.IdShop AND u.Id = @Id AND i.IdProduct = p.Id";
                sql8.Parameters.AddWithValue("@Id", id);
                sql8.CommandText = queryString8;
                sql8.ExecuteNonQuery();
                connect8.Close();
                // delete product when product in user's shop
                connect2.Open();
                var sql2 = new MySqlCommand();
                sql2.Connection = connect2;
                string queryString2 = "DELETE p.* FROM tbl_product As p, tbl_user AS u, tbl_shop AS s " +
                    "WHERE s.IdUser = u.Id AND s.Id = p.IdShop AND u.Id = @Id";
                sql2.Parameters.AddWithValue("@Id", id);
                sql2.CommandText = queryString2;
                sql2.ExecuteNonQuery();
                connect2.Close();
                // delete coupon when coupon in user's shop 
                connect3.Open();
                var sql3 = new MySqlCommand();
                sql3.Connection = connect3;
                string queryString3 = "DELETE c.* FROM tbl_coupon As c, tbl_user AS u, tbl_shop AS s WHERE" +
                    " s.IdUser = u.Id AND s.Id = c.IdShop AND u.Id = @Id";
                sql3.Parameters.AddWithValue("@Id", id);
                sql3.CommandText = queryString3;
                sql3.ExecuteNonQuery();
                connect3.Close();
                // delete user's refresh token 
                connect4.Open();
                var sql4 = new MySqlCommand();
                sql4.Connection = connect4;
                string queryString4 = "DELETE r.* FROM tbl_refreshtoken As r, tbl_user AS u WHERE r.IdUser = u.Id AND u.Id = @Id";
                sql4.Parameters.AddWithValue("@Id", id);
                sql4.CommandText = queryString4;
                sql4.ExecuteNonQuery();
                connect4.Close();
                // delete user's cart
                connect5.Open();
                var sql5 = new MySqlCommand();
                sql5.Connection = connect5;
                string queryString5 = "DELETE r.* FROM tbl_cart As r, tbl_user AS u WHERE r.IdUser = u.Id AND u.Id = @Id";
                sql5.Parameters.AddWithValue("@Id", id);
                sql5.CommandText = queryString5;
                int result5 = sql5.ExecuteNonQuery();
                connect5.Close();
                // delete user's shop
                connect6.Open();
                var sql6 = new MySqlCommand();
                sql6.Connection = connect6;
                string queryString6 = "DELETE s.* FROM tbl_shop As s, tbl_user AS u WHERE s.IdUser = u.Id AND u.Id = @Id";
                sql6.Parameters.AddWithValue("@Id", id);
                sql6.CommandText = queryString6;
                sql6.ExecuteNonQuery();
                connect6.Close();
                // delete user's order
                connect10.Open();
                var sql10 = new MySqlCommand();
                sql10.Connection = connect10;
                string queryString10 = "DELETE FROM tbl_order WHERE Orderer = @Id";
                sql10.Parameters.AddWithValue("@Id", id);
                sql10.CommandText = queryString10;
                sql10.ExecuteNonQuery();
                connect10.Close();
                // delete user's address
                connect9.Open();
                var sql9 = new MySqlCommand();
                sql9.Connection = connect9;
                string queryString9 = "DELETE a.* FROM tbl_address AS a, tbl_user AS u WHERE a.UserId = u.Id AND u.Id = @Id";
                sql9.Parameters.AddWithValue("@Id", id);
                sql9.CommandText = queryString9;
                sql9.ExecuteNonQuery();
                connect9.Close();
                // delete user
                connect7.Open();
                var sql7 = new MySqlCommand();
                sql7.Connection = connect7;
                string queryString7 = "DELETE FROM tbl_user WHERE Id = @Id";
                sql7.Parameters.AddWithValue("@Id", id);
                sql7.CommandText = queryString7;
                int result7 = sql7.ExecuteNonQuery();
                connect7.Close();
                if (result5 == 1 && result7 == 1)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                connect1.Close();
                connect2.Close();
                connect3.Close();
                connect4.Close();
                connect5.Close();
                connect6.Close();
                connect7.Close();
                return false;
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

        public async Task<Page> SearchUser(int pageNum, int perPage, string direction, string key)
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
                command.CommandText = "SELECT COUNT(*) FROM tbl_user WHERE Name LIKE @key OR PhoneNumber LIKE @key OR Email LIKE @key";
                command.Parameters.AddWithValue("@key", "%" + key + "%");
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
                command1.CommandText = "SELECT * FROM tbl_user WHERE Name LIKE @key OR PhoneNumber LIKE @key OR Email LIKE @key ORDER BY Name " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", "%" + key + "%");
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

        public bool AddAddress(string id, Address address)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_address (Id, Name, Phone, Address, Type, UserId) VALUES (@Id, @Name, @Phone, @Address, @Type, @UserId)";
                sql.Parameters.AddWithValue("@Id", address.Id);
                sql.Parameters.AddWithValue("@Name", address.Name);
                sql.Parameters.AddWithValue("@Phone", address.Phone);
                sql.Parameters.AddWithValue("@Address", address.FullAddress);
                sql.Parameters.AddWithValue("@Type", address.Type);
                sql.Parameters.AddWithValue("@UserId", id);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result == 1 ? true : false;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
        }

        public async Task<List<Address>> GetAllAddress(string uId)
        {
            List<Address> list = new List<Address>();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect1.Open();
                var command1 = new MySqlCommand();
                command1.Connection = connect1;
                command1.CommandText = "SELECT * FROM tbl_address WHERE UserId = @id";
                command1.Parameters.AddWithValue("@id", uId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var phone = reader1.GetString("Phone");
                        var fullAddress = reader1.GetString(3);
                        var type = reader1.GetString(4);
                        Address address = new Address { Id = id, Name = name, Phone = phone, FullAddress = fullAddress, Type = type };
                        list.Add(address);
                    }
                }
                connect1.Close();
            }
            catch (Exception ex)
            {
                connect1.Close();
                return null;
            }
            return list;
        }

        public int ChangeAddress(Address address)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var sql = new MySqlCommand();
                sql.Connection = connect;
                string queryString = "UPDATE tbl_address SET Name = @name, Phone = @phone, Address = @fullAddress, Type = @type WHERE Id = @Id";
                sql.Parameters.AddWithValue("@name", address.Name);
                sql.Parameters.AddWithValue("@phone", address.Phone);
                sql.Parameters.AddWithValue("@fullAddress", address.FullAddress);
                sql.Parameters.AddWithValue("@type", address.Type);
                sql.Parameters.AddWithValue("@Id", address.Id);
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

        public int DeleteAddress(string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var sql = new MySqlCommand();
                sql.Connection = connect;
                string queryString = "DELETE FROM tbl_address WHERE Id = @Id";
                sql.Parameters.AddWithValue("@Id", id);
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
