using API.Data;
using API.Model;
using MySqlConnector;

namespace API.Repositories
{
    public class ShopRepository
    {
        private readonly DBConnection conn;
        private readonly IConfiguration _configuration;

        public ShopRepository(DBConnection conn, IConfiguration configuration)
        {
            this.conn = conn;
            _configuration = configuration;
        }

        public async Task<Shop> GetShopByUserId(string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            Shop shop = null;
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT * FROM tbl_shop WHERE IdUser = @IdUser";
                command.Parameters.AddWithValue("@IdUser", id);
                command.CommandText = queryString;
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var idShop = reader.GetString(0);
                        var name = reader.GetString(1);
                        var address = reader.GetString(2);
                        byte[] imageBytes = (byte[])reader["Avatar"];
                        string avt = Convert.ToBase64String(imageBytes);
                        shop = new Shop { Id = idShop, Name = name, Address = address, Avatar = avt };
                        break;
                    }
                }
                else
                {
                    connect.Close();
                    return shop;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            connect.Close();
            return shop;
        }

        public int CreateShop(Shop shop, string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                Byte[] bitmapData = new Byte[shop.Avatar.Length];
                bitmapData = Convert.FromBase64String(shop.Avatar);
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_shop (Id, Name, Address, Avatar, IdUser) VALUES (@Id, @Name, @Address, @Avatar, @IdUser)";
                sql.Parameters.AddWithValue("@Id", shop.Id);
                sql.Parameters.AddWithValue("@Name", shop.Name);
                sql.Parameters.AddWithValue("@Address", shop.Address);
                sql.Parameters.AddWithValue("@Avatar", bitmapData);
                sql.Parameters.AddWithValue("@IdUser", id);
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

        public int ChangeShopInfor(Shop shop, string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                Byte[] bitmapData = new Byte[shop.Avatar.Length];
                bitmapData = Convert.FromBase64String(shop.Avatar);
                connect.Open();
                var sql = new MySqlCommand();
                sql.Connection = connect;
                string queryString = "UPDATE tbl_shop SET Name = @name, Address = @address, Avatar = @avatar WHERE IdUser = @Id";
                sql.Parameters.AddWithValue("@name", shop.Name);
                sql.Parameters.AddWithValue("@address", shop.Address);
                sql.Parameters.AddWithValue("@avatar", bitmapData);
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

        public int DeleteShopByIdUser(string id)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            MySqlConnection connect2 = conn.ConnectDB();
            MySqlConnection connect3 = conn.ConnectDB();
            MySqlConnection connect4 = conn.ConnectDB();
            MySqlConnection connect6 = conn.ConnectDB();
            try
            {
                // delete from tbl_cart_product when product in user's shop or user's cart
                connect1.Open();
                var sql1 = new MySqlCommand();
                sql1.Connection = connect1;
                string queryString1 = "DELETE cp.* FROM tbl_cart_product As cp, tbl_user AS u, tbl_shop AS s, tbl_product AS p, tbl_cart AS c " +
                    "WHERE s.IdUser = u.Id AND s.Id = p.IdShop AND u.Id = @Id AND cp.IdProduct = p.Id";
                sql1.Parameters.AddWithValue("@Id", id);
                sql1.CommandText = queryString1;
                sql1.ExecuteNonQuery();
                connect1.Close();
                // delete img when img in user's shop's product
                connect4.Open();
                var sql4 = new MySqlCommand();
                sql4.Connection = connect4;
                string queryString4 = "DELETE i.* FROM tbl_img As i, tbl_user AS u, tbl_shop AS s, tbl_product AS p " +
                    "WHERE s.IdUser = u.Id AND s.Id = p.IdShop AND u.Id = @Id AND i.IdProduct = p.Id";
                sql4.Parameters.AddWithValue("@Id", id);
                sql4.CommandText = queryString4;
                sql4.ExecuteNonQuery();
                connect4.Close();
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
                // delete user's shop
                connect6.Open();
                var sql6 = new MySqlCommand();
                sql6.Connection = connect6;
                string queryString6 = "DELETE s.* FROM tbl_shop As s, tbl_user AS u WHERE s.IdUser = u.Id AND u.Id = @Id";
                sql6.Parameters.AddWithValue("@Id", id);
                sql6.CommandText = queryString6;
                int result = sql6.ExecuteNonQuery();
                connect6.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect1.Close();
                connect2.Close();
                connect3.Close();
                connect4.Close();
                connect6.Close();
                return 0;
            }
        }

        public async Task<Shop> GetShopByProductId(string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            Shop shop = null;
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT s.* FROM tbl_shop AS s, tbl_product AS p WHERE p.IdShop = s.Id AND p.Id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.CommandText = queryString;
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var idShop = reader.GetString(0);
                        var name = reader.GetString(1);
                        var address = reader.GetString(2);
                        byte[] imageBytes = (byte[])reader["Avatar"];
                        string avt = Convert.ToBase64String(imageBytes);
                        shop = new Shop { Id = idShop, Name = name, Address = address, Avatar = avt };
                        break;
                    }
                }
                else
                {
                    connect.Close();
                    return shop;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            connect.Close();
            return shop;
        }
    }
}
