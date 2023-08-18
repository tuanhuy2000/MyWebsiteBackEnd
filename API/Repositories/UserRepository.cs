using API.Data;
using API.Model;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();

                var command = new MySqlCommand();
                command.Connection = connect;

                string queryString = "SELECT * FROM tbl_user";
                command.CommandText = queryString;

                using var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        //var name = reader["Name"];
                        var name = reader.GetString(1);
                        var phoneNumber = reader.GetString("PhoneNumber");
                        var email = reader.GetString(3);
                        var userName = reader.GetString(4);
                        var password = reader.GetString(5);
                        User user = new User { Id = id, Name = name, PhoneNumber = phoneNumber, Email = email, UserName = userName, Password = password };
                        users.Add(user);
                    }
                }
                else
                {
                    connect.Close();
                    return users;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return users;
            }
            connect.Close();
            return users;
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

                using var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        //var name = reader["Name"];
                        var name = reader.GetString(1);
                        var phoneNumber = reader.GetString("PhoneNumber");
                        var email = reader.GetString(3);
                        var userName = reader.GetString(4);
                        var Password = reader.GetString(5);
                        userLogin = new User { Id = id, Name = name, PhoneNumber = phoneNumber, Email = email, UserName = userName, Password = Password };
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
                return userLogin;
            }
            connect.Close();
            return userLogin;
        }

        public async Task<string> GetToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString() ),
                new Claim("UserId",user.Id.ToString()),
                new Claim("Name",user.Name),
                new Claim("Email",user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(3),
                signingCredentials: signIn
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
