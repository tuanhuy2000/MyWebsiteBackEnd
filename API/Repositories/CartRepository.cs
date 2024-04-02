using API.Data;
using API.Model;
using MySqlConnector;

namespace API.Repositories
{
    public class CartRepository
    {
        private readonly DBConnection conn;
        private readonly IConfiguration _configuration;

        public CartRepository(DBConnection conn, IConfiguration configuration)
        {
            this.conn = conn;
            _configuration = configuration;
        }

        public async Task<string> GetIdCartByIdUser(string uId)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                string idCart = "";
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT Id FROM tbl_cart WHERE IdUser = @IdUser";
                command.Parameters.AddWithValue("@IdUser", uId);
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        idCart = reader.GetString(0);
                        break;
                    }
                }
                else
                {
                    connect.Close();
                    return null;
                }
                connect.Close();
                return idCart;
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
        }

        public async Task<int> AddToCart(string uId, string pId, int quantity)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string idCart = await GetIdCartByIdUser(uId);
                connect1.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect1;
                //sql.CommandText = "INSERT INTO tbl_cart_product (IdCart, IdProduct, Quantity) VALUES (@cId, @pId, @quantity)";
                sql.CommandText = "INSERT INTO tbl_cart_product (IdCart, IdProduct, Quantity) SELECT * FROM (SELECT @cId AS IdCart, @pId AS IdProduct, @quantity AS Quantity) AS tmp WHERE NOT EXISTS (SELECT IdCart, IdProduct FROM tbl_cart_product WHERE IdCart = @cId AND IdProduct = @pId) LIMIT 1";
                sql.Parameters.AddWithValue("@cId", idCart);
                sql.Parameters.AddWithValue("@pId", pId);
                sql.Parameters.AddWithValue("@quantity", quantity);
                int result = sql.ExecuteNonQuery();
                connect1.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect1.Close();
                return 0;
            }
        }

        public async Task<List<Product>> GetProductInCart(string uId)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            List<Product> list = new List<Product>();
            try
            {
                string idCart = await GetIdCartByIdUser(uId);
                connect1.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect1;
                sql.CommandText = "SELECT p.*, cp.Quantity FROM tbl_cart_product AS cp, tbl_product AS p WHERE cp.IdCart = @cId AND p.Id = cp.IdProduct";
                sql.Parameters.AddWithValue("@cId", idCart);
                await using var reader1 = sql.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(7);
                        var infor = reader1.GetString(4);
                        var type = reader1.GetString(5);
                        ProductRepository _productRepository = new ProductRepository(new DBConnection());
                        List<string> img = await _productRepository.GetImgByIdProduct(id);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = type, Img = img };
                        list.Add(product);
                    }
                }
                else
                {
                    connect1.Close();
                    return list;
                }
                connect1.Close();
                return list;
            }
            catch (Exception ex)
            {
                connect1.Close();
                return null;
            }
        }

        public async Task<int> ChangeQuantity(string uId, string pId, int quantity)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string idCart = await GetIdCartByIdUser(uId);
                connect1.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect1;
                sql.CommandText = "UPDATE tbl_cart_product SET Quantity = @quantity WHERE IdCart = @cId AND IdProduct = @pId";
                sql.Parameters.AddWithValue("@cId", idCart);
                sql.Parameters.AddWithValue("@pId", pId);
                sql.Parameters.AddWithValue("@quantity", quantity);
                int result = sql.ExecuteNonQuery();
                connect1.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect1.Close();
                return 0;
            }
        }

        public async Task<int> DeleteProductInCart(string uId, string pId)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string idCart = await GetIdCartByIdUser(uId);
                connect1.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect1;
                sql.CommandText = "DELETE FROM tbl_cart_product WHERE IdCart = @cId AND IdProduct = @pId";
                sql.Parameters.AddWithValue("@cId", idCart);
                sql.Parameters.AddWithValue("@pId", pId);
                int result = sql.ExecuteNonQuery();
                connect1.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect1.Close();
                return 0;
            }
        }
    }
}
