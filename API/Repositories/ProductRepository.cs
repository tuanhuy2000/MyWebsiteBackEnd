using API.Data;
using API.Model;
using MySqlConnector;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace API.Repositories
{
    public class ProductRepository
    {
        private readonly DBConnection conn;
        private readonly IConfiguration _configuration;

        public ProductRepository(DBConnection conn, IConfiguration configuration)
        {
            this.conn = conn;
            _configuration = configuration;
        }

        public ProductRepository(DBConnection conn)
        {
            this.conn = conn;
        }

        public async Task<List<string>> GetAllTypeProduct()
        {
            MySqlConnection connect = conn.ConnectDB();
            List<string> list = new List<string>();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT * FROM tbl_product_type";
                command.CommandText = queryString;
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string type = reader.GetString(0);
                        list.Add(type);
                    }
                }
                else
                {
                    connect.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            connect.Close();
            return list;
        }

        public int CreateProduct(Product product, string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string idShop = "";
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT Id FROM tbl_shop WHERE IdUser = @IdUser";
                command.Parameters.AddWithValue("@IdUser", id);
                command.CommandText = queryString;
                using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        idShop = reader.GetString(0);
                        break;
                    }
                }
                else
                {
                    connect.Close();
                    return 0;
                }
                connect.Close();
                connect1.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect1;
                sql.CommandText = "INSERT INTO tbl_product (Id, Name, Price, Quantity, Information, Type, IdShop) VALUES (@id, @name, @price, @quantity, @infor, @type, @idShop)";
                sql.Parameters.AddWithValue("@id", product.Id);
                sql.Parameters.AddWithValue("@name", product.Name);
                sql.Parameters.AddWithValue("@price", product.Price);
                sql.Parameters.AddWithValue("@quantity", product.Quantity);
                sql.Parameters.AddWithValue("@infor", product.Information);
                sql.Parameters.AddWithValue("@type", product.Type);
                sql.Parameters.AddWithValue("@idShop", idShop);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                foreach (var item in product.Img)
                {
                    int res = CreateImgProduct(item, product.Id);
                    if (res == 0)
                    {
                        return 0;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                connect.Close();
                return 0;
            }
        }

        public int CreateImgProduct(string img, string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                Byte[] bitmapData = new Byte[img.Length];
                bitmapData = Convert.FromBase64String(img);
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_img (Data, IdProduct) VALUES (@data, @IdProduct)";
                sql.Parameters.AddWithValue("@data", bitmapData);
                sql.Parameters.AddWithValue("@IdProduct", id);
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

        public async Task<Page> GetPageProductAsync(string col, int pageNum, int perPage, string direction, string uId)
        {
            List<Product> products = new List<Product>();
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
                command.CommandText = "SELECT COUNT(p.id) FROM tbl_product AS p, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = p.IdShop AND u.Id = @id";
                command.Parameters.AddWithValue("@id", uId);
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
                command1.CommandText = "SELECT p.* FROM tbl_product AS p, tbl_shop AS s, tbl_user AS u WHERE u.Id = s.IdUser AND s.Id = p.IdShop AND u.Id = @id ORDER BY p." + col + " " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@id", uId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(3);
                        var infor = reader1.GetString(4);
                        var type = reader1.GetString(5);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = type };
                        products.Add(product);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = products };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchProductOfUserByName(int pageNum, int perPage, string direction, string key, string uId)
        {
            List<Product> products = new List<Product>();
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
                command.CommandText = "SELECT COUNT(p.id) FROM tbl_product AS p, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = p.IdShop AND u.Id = @id AND p.Name LIKE @key";
                command.Parameters.AddWithValue("@key", "%" + key + "%");
                command.Parameters.AddWithValue("@id", uId);
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
                command1.CommandText = "SELECT p.* FROM tbl_product AS p, tbl_shop AS s, tbl_user AS u WHERE u.Id = s.IdUser AND s.Id = p.IdShop AND u.Id = @id AND p.Name LIKE @key ORDER BY Name " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", "%" + key + "%");
                command1.Parameters.AddWithValue("@id", uId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(3);
                        var infor = reader1.GetString(4);
                        var type = reader1.GetString(5);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = type };
                        products.Add(product);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = products };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public int DeleteProductById(string id)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            MySqlConnection connect2 = conn.ConnectDB();
            MySqlConnection connect4 = conn.ConnectDB();
            try
            {
                // delete from tbl_cart_product when product in cart
                connect1.Open();
                var sql1 = new MySqlCommand();
                sql1.Connection = connect1;
                string queryString1 = "DELETE FROM tbl_cart_product WHERE IdProduct = @id";
                sql1.Parameters.AddWithValue("@id", id);
                sql1.CommandText = queryString1;
                sql1.ExecuteNonQuery();
                connect1.Close();
                // delete img when img in product
                connect4.Open();
                var sql4 = new MySqlCommand();
                sql4.Connection = connect4;
                string queryString4 = "DELETE FROM tbl_img WHERE IdProduct = @id";
                sql4.Parameters.AddWithValue("@id", id);
                sql4.CommandText = queryString4;
                sql4.ExecuteNonQuery();
                connect4.Close();
                // delete product
                connect2.Open();
                var sql2 = new MySqlCommand();
                sql2.Connection = connect2;
                string queryString2 = "DELETE FROM tbl_product WHERE Id = @id";
                sql2.Parameters.AddWithValue("@id", id);
                sql2.CommandText = queryString2;
                int result = sql2.ExecuteNonQuery();
                connect2.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect1.Close();
                connect2.Close();
                connect4.Close();
                return 0;
            }
        }

        public async Task<List<string>> GetImgByIdProduct(string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            List<string> imgs = new List<string>();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT * FROM tbl_img WHERE IdProduct = @id";
                command.Parameters.AddWithValue("@id", id);
                command.CommandText = queryString;
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        byte[] imageBytes = (byte[])reader["Data"];
                        string img = Convert.ToBase64String(imageBytes);
                        imgs.Add(img);
                    }
                }
                else
                {
                    connect.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            connect.Close();
            return imgs;
        }

        public int ChangeProductById(Product product)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            MySqlConnection connect4 = conn.ConnectDB();
            try
            {
                // delete img when img in product
                connect4.Open();
                var sql4 = new MySqlCommand();
                sql4.Connection = connect4;
                string queryString4 = "DELETE FROM tbl_img WHERE IdProduct = @id";
                sql4.Parameters.AddWithValue("@id", product.Id);
                sql4.CommandText = queryString4;
                sql4.ExecuteNonQuery();
                connect4.Close();
                // change img product
                foreach (var item in product.Img)
                {
                    int res = CreateImgProduct(item, product.Id);
                    if (res == 0)
                    {
                        return 0;
                    }
                }
                //update product
                connect1.Open();
                var sql1 = new MySqlCommand();
                sql1.Connection = connect1;
                string queryString1 = "UPDATE tbl_product SET Name = @name, Price = @price, Quantity = @quantity, Information = @infor, Type = @type WHERE Id = @id";
                sql1.Parameters.AddWithValue("@id", product.Id);
                sql1.Parameters.AddWithValue("@name", product.Name);
                sql1.Parameters.AddWithValue("@price", product.Price);
                sql1.Parameters.AddWithValue("@quantity", product.Quantity);
                sql1.Parameters.AddWithValue("@infor", product.Information);
                sql1.Parameters.AddWithValue("@type", product.Type);
                sql1.CommandText = queryString1;
                int result = sql1.ExecuteNonQuery();
                connect1.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect1.Close();
                connect4.Close();
                return 0;
            }
        }

        public async Task<Page> SearchProductOfUserByType(int pageNum, int perPage, string direction, string key, string uId)
        {
            List<Product> products = new List<Product>();
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
                command.CommandText = "SELECT COUNT(p.id) FROM tbl_product AS p, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = p.IdShop AND u.Id = @id AND p.Type = @key";
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@id", uId);
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
                command1.CommandText = "SELECT p.* FROM tbl_product AS p, tbl_shop AS s, tbl_user AS u WHERE u.Id = s.IdUser AND s.Id = p.IdShop AND u.Id = @id AND p.Type = @key ORDER BY p.Name " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", key);
                command1.Parameters.AddWithValue("@id", uId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(3);
                        var infor = reader1.GetString(4);
                        var type = reader1.GetString(5);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = type };
                        products.Add(product);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = products };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<int> GetCountProductOfUser(string id)
        {
            int total = 0;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(p.id) FROM tbl_product AS p, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = p.IdShop AND u.Id = @id";
                command.Parameters.AddWithValue("@id", id);
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        total = reader.GetInt32(0);
                    }
                }
                connect.Close();

            }
            catch (Exception ex)
            {
                connect.Close();
                return 0;
            }
            return total;
        }

        public async Task<Page> GetPageProduct(int pageNum, int perPage)
        {
            List<Product> products = new List<Product>();
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
                command.CommandText = "SELECT COUNT(Id) FROM tbl_product WHERE Quantity > 0";
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
                command1.CommandText = "SELECT * FROM tbl_product WHERE Quantity > 0 ORDER BY Id LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(3);
                        var infor = reader1.GetString(4);
                        var type = reader1.GetString(5);
                        List<string> img = await GetImgByIdProduct(id);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = type, Img = img };
                        products.Add(product);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = products };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchPageProduct(int pageNum, int perPage, string keyWord, string type, string direction)
        {
            List<Product> products = new List<Product>();
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
                command.CommandText = "SELECT COUNT(Id) FROM tbl_product WHERE Name LIKE @key AND Type LIKE @type";
                command.Parameters.AddWithValue("@key", "%" + keyWord + "%");
                command.Parameters.AddWithValue("@type", "%" + type + "%");
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
                command1.CommandText = "SELECT * FROM tbl_product WHERE Name LIKE @key AND Type LIKE @type ORDER BY Price " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", "%" + keyWord + "%");
                command1.Parameters.AddWithValue("@type", "%" + type + "%");
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(3);
                        var infor = reader1.GetString(4);
                        var ty = reader1.GetString(5);
                        List<string> img = await GetImgByIdProduct(id);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = ty, Img = img };
                        products.Add(product);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = products };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<int> CountProductOfShop(string id)
        {
            int total = 0;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(Id) FROM tbl_product WHERE IdShop = @id";
                command.Parameters.AddWithValue("@id", id);
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        total = reader.GetInt32(0);
                    }
                }
                connect.Close();

            }
            catch (Exception ex)
            {
                connect.Close();
                return 0;
            }
            return total;
        }

        public async Task<Page> GetPageProductOfShop(int pageNum, int perPage, string idShop)
        {
            List<Product> products = new List<Product>();
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
                command.CommandText = "SELECT COUNT(Id) FROM tbl_product WHERE IdShop = @id";
                command.Parameters.AddWithValue("@id", idShop);
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
                command1.CommandText = "SELECT * FROM tbl_product WHERE IdShop = @id ORDER BY Id LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@id", idShop);
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(3);
                        var infor = reader1.GetString(4);
                        var type = reader1.GetString(5);
                        List<string> img = await GetImgByIdProduct(id);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = type, Img = img };
                        products.Add(product);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = products };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchPageProductOfShop(int pageNum, int perPage, string keyWord, string type, string direction, string idShop)
        {
            List<Product> products = new List<Product>();
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
                command.CommandText = "SELECT COUNT(Id) FROM tbl_product WHERE Name LIKE @key AND Type LIKE @type AND IdShop = @id";
                command.Parameters.AddWithValue("@key", "%" + keyWord + "%");
                command.Parameters.AddWithValue("@type", "%" + type + "%");
                command.Parameters.AddWithValue("@id", idShop);
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
                command1.CommandText = "SELECT * FROM tbl_product WHERE Name LIKE @key AND Type LIKE @type AND IdShop = @id ORDER BY Price " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", "%" + keyWord + "%");
                command1.Parameters.AddWithValue("@type", "%" + type + "%");
                command1.Parameters.AddWithValue("@id", idShop);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var price = reader1.GetDouble(2);
                        var quantity = reader1.GetInt32(3);
                        var infor = reader1.GetString(4);
                        var ty = reader1.GetString(5);
                        List<string> img = await GetImgByIdProduct(id);
                        Product product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = ty, Img = img };
                        products.Add(product);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = products };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Product> GetProductById(string id)
        {
            MySqlConnection connect = conn.ConnectDB();
            Product product = null;
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT * FROM tbl_product WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(1);
                        var price = reader.GetDouble(2);
                        var quantity = reader.GetInt32(3);
                        var infor = reader.GetString(4);
                        var type = reader.GetString(5);
                        List<string> img = await GetImgByIdProduct(id);
                        product = new Product { Id = id, Name = name, Price = price, Quantity = quantity, Information = infor, Type = type, Img = img };
                    }
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            return product;
        }
    }
}
