using API.Data;
using API.Model;
using MySqlConnector;
using System;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace API.Repositories
{
    public class OrderRepository
    {
        private readonly DBConnection conn;
        private readonly IConfiguration _configuration;

        public OrderRepository(DBConnection conn, IConfiguration configuration)
        {
            this.conn = conn;
            _configuration = configuration;
        }

        public bool CreateTransport(Transport transport)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_transport (Id, ShippingCode, ShippingUnit, ShippingWay) VALUES (@id, @code, @unit, @way)";
                sql.Parameters.AddWithValue("@id", transport.Id);
                sql.Parameters.AddWithValue("@code", transport.ShippingCode);
                sql.Parameters.AddWithValue("@unit", transport.ShippingUnit);
                sql.Parameters.AddWithValue("@way", transport.ShippingWay);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result == 1;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
        }

        public bool CreateOrder(Order order, string uId, string sId, string aId)
        {
            MySqlConnection connect = conn.ConnectDB();
            string orderDate = DateTime.UtcNow.ToString("yyyy/MM/dd");
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_order (Id, Orderer, IdShop, Address, PaymentType, TotalCost, Discount, Status, ShippingWay, OrderDate) VALUES (@id, @uId, @sId, @address, @payType, @total, @discount, @status, @shippingWay, @orderDate)";
                sql.Parameters.AddWithValue("@id", order.Id);
                sql.Parameters.AddWithValue("@uId", uId);
                sql.Parameters.AddWithValue("@sId", sId);
                sql.Parameters.AddWithValue("@address", aId);
                sql.Parameters.AddWithValue("@payType", order.PaymentType);
                sql.Parameters.AddWithValue("@total", order.TotalCost);
                sql.Parameters.AddWithValue("@discount", order.Discount);
                sql.Parameters.AddWithValue("@status", order.Status);
                sql.Parameters.AddWithValue("@shippingWay", order.ShippingWay);
                sql.Parameters.AddWithValue("@orderDate", orderDate);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result == 1;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
        }

        public bool AddCouponToOrder(string oId, string cId)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_order_coupon (IdOrder, IdCoupon) VALUES (@oId, @cId)";
                sql.Parameters.AddWithValue("@oId", oId);
                sql.Parameters.AddWithValue("@cId", cId);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result == 1;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
        }

        public bool AddProductToOrder(string oId, string pId, int quantity)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_order_product (IdOrder, IdProduct, Quantity) VALUES (@oId, @pId, @quantity)";
                sql.Parameters.AddWithValue("@oId", oId);
                sql.Parameters.AddWithValue("@pId", pId);
                sql.Parameters.AddWithValue("@quantity", quantity);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result == 1;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
        }

        public async Task<Page> GetOrderByIdShopOrUser(int pageNum, int perPage, string direction, string tId, string type)
        {
            List<Order> orders = new List<Order>();
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
                command.CommandText = "SELECT COUNT(*) FROM tbl_order WHERE " + type + " = @tId";
                command.Parameters.AddWithValue("@tId", tId);
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
                command1.CommandText = "SELECT * FROM tbl_order WHERE " + type + " = @tId ORDER BY OrderDate " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@tId", tId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var paymentType = reader1.GetString(4);
                        var totalCost = reader1.GetDouble(5);
                        var discount = reader1.GetDouble(6);
                        var status = reader1.GetString(7);
                        var shippingWay = reader1.GetString(8);
                        var orderDate = reader1.GetDateTime(9);
                        DateTime completionDate = new DateTime();
                        if (!reader1.IsDBNull(10))
                        {
                            completionDate = reader1.GetDateTime(10);
                        }
                        Order order = new Order
                        {
                            Id = id,
                            PaymentType = paymentType,
                            TotalCost = totalCost,
                            Discount = discount,
                            Status = status,
                            ShippingWay = shippingWay,
                            OrderDate = orderDate,
                            CompletionDate = completionDate
                        };
                        orders.Add(order);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = orders };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchOrderOfShopOrUser(int pageNum, int perPage, string direction, string key, string tId, string type)
        {
            List<Order> orders = new List<Order>();
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
                command.CommandText = "SELECT COUNT(*) FROM tbl_order WHERE " + type + " = @tId AND Id LIKE @key";
                command.Parameters.AddWithValue("@key", "%" + key + "%");
                command.Parameters.AddWithValue("@tId", tId);
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
                command1.CommandText = "SELECT * FROM tbl_order WHERE " + type + " = @tId AND Id LIKE @key ORDER BY OrderDate " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", "%" + key + "%");
                command1.Parameters.AddWithValue("@tId", tId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var paymentType = reader1.GetString(4);
                        var totalCost = reader1.GetDouble(5);
                        var discount = reader1.GetDouble(6);
                        var status = reader1.GetString(7);
                        var shippingWay = reader1.GetString(8);
                        var orderDate = reader1.GetDateTime(9);
                        DateTime completionDate = new DateTime();
                        if (!reader1.IsDBNull(10))
                        {
                            completionDate = reader1.GetDateTime(10);
                        }
                        Order order = new Order
                        {
                            Id = id,
                            PaymentType = paymentType,
                            TotalCost = totalCost,
                            Discount = discount,
                            Status = status,
                            ShippingWay = shippingWay,
                            OrderDate = orderDate,
                            CompletionDate = completionDate
                        };
                        orders.Add(order);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = orders };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<int> CountOrderOfShop(string id)
        {
            int total = 0;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(Id) FROM tbl_Order WHERE IdShop = @id";
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

        public bool AddTransportToOrder(string tId, string oId)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "UPDATE tbl_order SET Transport = @tId, Status = 'Shipping' WHERE Id = @oId";
                sql.Parameters.AddWithValue("@oId", oId);
                sql.Parameters.AddWithValue("@tId", tId);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result == 1;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
        }

        public async Task<Address> GetAddressOfOrderById(string oId)
        {
            Address address = null;
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect1.Open();
                var command1 = new MySqlCommand();
                command1.Connection = connect1;
                command1.CommandText = "SELECT a.* FROM tbl_order AS o, tbl_address AS a WHERE o.Id = @oId AND o.Address = a.Id";
                command1.Parameters.AddWithValue("@oId", oId);
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
                        address = new Address { Id = id, Name = name, Phone = phone, FullAddress = fullAddress, Type = type };
                    }
                }
                connect1.Close();
            }
            catch (Exception ex)
            {
                connect1.Close();
                return address;
            }
            return address;
        }

        public async Task<Transport> GetTransportOfOrderById(string oId)
        {
            Transport transport = null;
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect1.Open();
                var command1 = new MySqlCommand();
                command1.Connection = connect1;
                command1.CommandText = "SELECT t.* FROM tbl_order AS o, tbl_transport AS t WHERE o.Id = @oId AND o.Transport = t.Id";
                command1.Parameters.AddWithValue("@oId", oId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var code = reader1.GetString(1);
                        var unit = reader1.GetString(2);
                        var way = reader1.GetString(3);
                        transport = new Transport { Id = id, ShippingCode = code, ShippingUnit = unit, ShippingWay = way };
                    }
                }
                connect1.Close();
            }
            catch (Exception ex)
            {
                connect1.Close();
                return transport;
            }
            return transport;
        }

        public async Task<List<Product>> GetProductOfOrderById(string oId)
        {
            List<Product> products = new List<Product>();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect1.Open();
                var command1 = new MySqlCommand();
                command1.Connection = connect1;
                command1.CommandText = "SELECT p.*, op.Quantity FROM tbl_order_product AS op, tbl_product AS p WHERE op.IdOrder = @oId AND op.IdProduct = p.Id";
                command1.Parameters.AddWithValue("@oId", oId);
                await using var reader1 = command1.ExecuteReader();
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
                        products.Add(product);
                    }
                }
                connect1.Close();
            }
            catch (Exception ex)
            {
                connect1.Close();
                return null;
            }
            return products;
        }

        public async Task<Shop> GetShopOfOrderById(string oId)
        {
            Shop shop = null;
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect1.Open();
                var command1 = new MySqlCommand();
                command1.Connection = connect1;
                command1.CommandText = "SELECT s.* FROM tbl_order AS o, tbl_shop AS s WHERE o.Id = @oId AND o.IdShop = s.Id";
                command1.Parameters.AddWithValue("@oId", oId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var idShop = reader1.GetString(0);
                        var name = reader1.GetString(1);
                        var address = reader1.GetString(2);
                        byte[] imageBytes = (byte[])reader1["Avatar"];
                        string avt = Convert.ToBase64String(imageBytes);
                        shop = new Shop { Id = idShop, Name = name, Address = address, Avatar = avt };
                        break;
                    }
                }
                connect1.Close();
            }
            catch (Exception ex)
            {
                connect1.Close();
                return shop;
            }
            return shop;
        }

        public bool DeleteOrderById(string oId)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                connect1.Open();
                var command1 = new MySqlCommand();
                command1.Connection = connect1;
                command1.CommandText = "DELETE FROM tbl_order WHERE Id = @oId";
                command1.Parameters.AddWithValue("@oId", oId);
                int result = command1.ExecuteNonQuery();
                if (result == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                connect1.Close();
            }
            catch (Exception ex)
            {
                connect1.Close();
                return false;
            }
        }

        public async Task<bool> CheckBeforeOrder(string uId, string pId)
        {
            int result = 0;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(u.Id) FROM tbl_user AS u, tbl_shop AS s, tbl_product AS p WHERE p.Id = @pId AND p.IdShop = s.Id AND s.IdUser = u.Id AND u.Id = @uId";
                command.Parameters.AddWithValue("@uId", uId);
                command.Parameters.AddWithValue("@pId", pId);
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result = reader.GetInt32(0);
                    }
                }
                if (result == 0)
                {
                    return true;
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
            return false;
        }

        public bool FinishOrder(string oId)
        {
            MySqlConnection connect = conn.ConnectDB();
            string completionDate = DateTime.UtcNow.ToString("yyyy/MM/dd");
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "UPDATE tbl_order SET Status = 'Completed', CompletionDate = @date WHERE Id = @oId";
                sql.Parameters.AddWithValue("@oId", oId);
                sql.Parameters.AddWithValue("@date", completionDate);
                int result = sql.ExecuteNonQuery();
                connect.Close();
                return result == 1;
            }
            catch (Exception ex)
            {
                connect.Close();
                return false;
            }
        }

        public async Task<Page> SearchOrderOfShopOrUserByStatus(int pageNum, int perPage, string direction, string status, string tId, string type)
        {
            List<Order> orders = new List<Order>();
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
                command.CommandText = "SELECT COUNT(*) FROM tbl_order WHERE " + type + " = @tId AND Status = @status";
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@tId", tId);
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
                command1.CommandText = "SELECT * FROM tbl_order WHERE " + type + " = @tId AND Status = @status ORDER BY OrderDate " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@status", status);
                command1.Parameters.AddWithValue("@tId", tId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var paymentType = reader1.GetString(4);
                        var totalCost = reader1.GetDouble(5);
                        var discount = reader1.GetDouble(6);
                        var sta = reader1.GetString(7);
                        var shippingWay = reader1.GetString(8);
                        var orderDate = reader1.GetDateTime(9);
                        DateTime completionDate = new DateTime();
                        if (!reader1.IsDBNull(10))
                        {
                            completionDate = reader1.GetDateTime(10);
                        }
                        Order order = new Order
                        {
                            Id = id,
                            PaymentType = paymentType,
                            TotalCost = totalCost,
                            Discount = discount,
                            Status = sta,
                            ShippingWay = shippingWay,
                            OrderDate = orderDate,
                            CompletionDate = completionDate
                        };
                        orders.Add(order);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = orders };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }
    }
}
