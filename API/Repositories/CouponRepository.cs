using API.Data;
using API.Model;
using MySqlConnector;

namespace API.Repositories
{
    public class CouponRepository
    {
        private readonly DBConnection conn;
        private readonly IConfiguration _configuration;

        public CouponRepository(DBConnection conn, IConfiguration configuration)
        {
            this.conn = conn;
            _configuration = configuration;
        }

        public async Task<List<string>> GetAllTypeCoupon()
        {
            MySqlConnection connect = conn.ConnectDB();
            List<string> list = new List<string>();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                string queryString = "SELECT * FROM tbl_coupon_type";
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

        public int CreateShopCoupon(Coupon coupon, string uId)
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
                command.Parameters.AddWithValue("@IdUser", uId);
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
                string fr = coupon.From.ToString("yyyy/MM/dd");
                string t = coupon.To.ToString("yyyy/MM/dd");
                connect1.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect1;
                //sql.CommandText = "INSERT INTO tbl_coupon (Id, Code, Quantity, Worth, DescribeInfor, CouponFrom, CouponTo, CouponType, ProductType, IdShop) VALUES (@id, @code, @quantity, @worth, @describe, @from, @to, @cType, @pType, @sId)";
                sql.CommandText = "INSERT INTO tbl_coupon (Id, Code, Quantity, Worth, Minimum, Maximum, DescribeInfor, CouponFrom, CouponTo, CouponType, ProductType, IdShop) " +
                    "SELECT * FROM (SELECT @id AS Id, @code AS Code, @quantity AS Quantity, @worth AS Worth, @minimum AS Minimum, @maximum AS Maximum, @describe AS DescribeInfor, @from AS CouponFrom, @to AS CouponTo, @cType AS CouponType, @pType AS ProductType, @sId AS IdShop) AS tmp" +
                    " WHERE NOT EXISTS (SELECT Code FROM tbl_coupon WHERE Code = @code) LIMIT 1";
                sql.Parameters.AddWithValue("@id", coupon.Id);
                sql.Parameters.AddWithValue("@code", coupon.Code);
                sql.Parameters.AddWithValue("@quantity", coupon.Quantity);
                sql.Parameters.AddWithValue("@worth", coupon.Worth);
                sql.Parameters.AddWithValue("@minimum", coupon.Minimum);
                sql.Parameters.AddWithValue("@maximum", coupon.Maximum);
                sql.Parameters.AddWithValue("@describe", coupon.Describe);
                sql.Parameters.AddWithValue("@from", fr);
                sql.Parameters.AddWithValue("@to", t);
                sql.Parameters.AddWithValue("@cType", coupon.Type);
                sql.Parameters.AddWithValue("@pType", coupon.ProductType);
                sql.Parameters.AddWithValue("@sId", idShop);
                int result = sql.ExecuteNonQuery();
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

        public async Task<Page> GetPageCouponAsync(int pageNum, int perPage, string direction, string uId)
        {
            List<Coupon> coupons = new List<Coupon>();
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
                command.CommandText = "SELECT COUNT(c.Id) FROM tbl_coupon AS c, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id";
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
                command1.CommandText = "SELECT c.* FROM tbl_coupon AS c, tbl_shop AS s, tbl_user AS u WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id ORDER BY c.CouponFrom " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@id", uId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var from = reader1.GetDateTime(7);
                        var to = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = from, To = to, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public int DeleteCouponById(string id)
        {
            MySqlConnection connect2 = conn.ConnectDB();
            try
            {
                connect2.Open();
                var sql2 = new MySqlCommand();
                sql2.Connection = connect2;
                string queryString2 = "DELETE FROM tbl_coupon WHERE Id = @id";
                sql2.Parameters.AddWithValue("@id", id);
                sql2.CommandText = queryString2;
                int result = sql2.ExecuteNonQuery();
                connect2.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect2.Close();
                return 0;
            }
        }

        public int ChangeCouponById(Coupon coupon)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string fr = coupon.From.ToString("yyyy/MM/dd");
                string t = coupon.To.ToString("yyyy/MM/dd");
                connect1.Open();
                var sql1 = new MySqlCommand();
                sql1.Connection = connect1;
                string queryString1 = "UPDATE tbl_coupon SET Code = @code, Quantity = @quantity, Worth = @worth, Minimum = @minimum, Maximum = @maximum, DescribeInfor = @describe, CouponFrom = @from, CouponTo = @to, CouponType = @type, ProductType = @productType WHERE Id = @id";
                sql1.Parameters.AddWithValue("@id", coupon.Id);
                sql1.Parameters.AddWithValue("@code", coupon.Code);
                sql1.Parameters.AddWithValue("@quantity", coupon.Quantity);
                sql1.Parameters.AddWithValue("@worth", coupon.Worth);
                sql1.Parameters.AddWithValue("@minimum", coupon.Minimum);
                sql1.Parameters.AddWithValue("@maximum", coupon.Maximum);
                sql1.Parameters.AddWithValue("@describe", coupon.Describe);
                sql1.Parameters.AddWithValue("@from", fr);
                sql1.Parameters.AddWithValue("@to", t);
                sql1.Parameters.AddWithValue("@type", coupon.Type);
                sql1.Parameters.AddWithValue("@productType", coupon.ProductType);
                sql1.CommandText = queryString1;
                int result = sql1.ExecuteNonQuery();
                connect1.Close();
                return result;
            }
            catch (Exception ex)
            {
                connect1.Close();
                return 0;
            }
        }

        public async Task<Page> SearchCouponOfUserByType(int pageNum, int perPage, string direction, string key, string uId)
        {
            List<Coupon> coupons = new List<Coupon>();
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
                command.CommandText = "SELECT COUNT(c.id) FROM tbl_coupon AS c, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id AND c.CouponType = @key";
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
                command1.CommandText = "SELECT c.* FROM tbl_coupon AS c, tbl_shop AS s, tbl_user AS u WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id AND c.CouponType = @key ORDER BY c.CouponFrom " + direction + " LIMIT @pageNum, @perPage";
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
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var from = reader1.GetDateTime(7);
                        var to = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = from, To = to, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchCouponOfUserByProductType(int pageNum, int perPage, string direction, string key, string uId)
        {
            List<Coupon> coupons = new List<Coupon>();
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
                command.CommandText = "SELECT COUNT(c.id) FROM tbl_coupon AS c, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id AND c.ProductType = @key";
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
                command1.CommandText = "SELECT c.* FROM tbl_coupon AS c, tbl_shop AS s, tbl_user AS u WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id AND c.ProductType = @key ORDER BY c.CouponFrom " + direction + " LIMIT @pageNum, @perPage";
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
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var from = reader1.GetDateTime(7);
                        var to = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = from, To = to, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchCouponOfUserByDate(int pageNum, int perPage, string direction, DateTime from, DateTime to, string uId)
        {
            List<Coupon> coupons = new List<Coupon>();
            int total = 0;
            int totalPages = 0;
            Page page = null;
            MySqlConnection connect = conn.ConnectDB();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string fr = from.ToString("yyyy-MM-dd");
                string t = to.ToString("yyyy-MM-dd");
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(c.id) FROM tbl_coupon AS c, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id AND (( CouponTo >= @to AND CouponFrom <= @to ) OR ( CouponTo <= @to AND CouponTo >= @from ))";
                command.Parameters.AddWithValue("@id", uId);
                command.Parameters.AddWithValue("@from", fr);
                command.Parameters.AddWithValue("@to", t);
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
                command1.CommandText = "SELECT DISTINCT c.* FROM tbl_coupon AS c, tbl_shop AS s, tbl_user AS u WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id AND (( CouponTo >= @to AND CouponFrom <= @to ) OR ( CouponTo <= @to AND CouponTo >= @from )) ORDER BY c.CouponFrom " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@from", fr);
                command1.Parameters.AddWithValue("@to", t);
                command1.Parameters.AddWithValue("@id", uId);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var fromm = reader1.GetDateTime(7);
                        var too = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = fromm, To = too, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public int CreateAdminCoupon(Coupon coupon)
        {
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string fr = coupon.From.ToString("yyyy/MM/dd");
                string t = coupon.To.ToString("yyyy/MM/dd");
                connect1.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect1;
                //sql.CommandText = "INSERT INTO tbl_coupon (Id, Code, Quantity, Worth, DescribeInfor, CouponFrom, CouponTo, CouponType, ProductType) VALUES (@id, @code, @quantity, @worth, @describe, @from, @to, @cType, @pType)";
                sql.CommandText = "INSERT INTO tbl_coupon (Id, Code, Quantity, Worth, Minimum, Maximum, DescribeInfor, CouponFrom, CouponTo, CouponType, ProductType) " +
                    "SELECT * FROM (SELECT @id AS Id, @code AS Code, @quantity AS Quantity, @worth AS Worth, @minimum AS Minimum, @maximum AS Maximum, @describe AS DescribeInfor, @from AS CouponFrom, @to AS CouponTo, @cType AS CouponType, @pType AS ProductType) AS tmp" +
                    " WHERE NOT EXISTS (SELECT Code FROM tbl_coupon WHERE Code = @code) LIMIT 1";
                sql.Parameters.AddWithValue("@id", coupon.Id);
                sql.Parameters.AddWithValue("@code", coupon.Code);
                sql.Parameters.AddWithValue("@quantity", coupon.Quantity);
                sql.Parameters.AddWithValue("@worth", coupon.Worth);
                sql.Parameters.AddWithValue("@minimum", coupon.Minimum);
                sql.Parameters.AddWithValue("@maximum", coupon.Maximum);
                sql.Parameters.AddWithValue("@describe", coupon.Describe);
                sql.Parameters.AddWithValue("@from", fr);
                sql.Parameters.AddWithValue("@to", t);
                sql.Parameters.AddWithValue("@cType", coupon.Type);
                sql.Parameters.AddWithValue("@pType", coupon.ProductType);
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

        public async Task<Page> GetPageAdminCouponAsync(int pageNum, int perPage, string direction)
        {
            List<Coupon> coupons = new List<Coupon>();
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
                command.CommandText = "SELECT COUNT(Id) FROM tbl_coupon WHERE IdShop IS NULL";
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
                command1.CommandText = "SELECT * FROM tbl_coupon WHERE IdShop IS NULL ORDER BY CouponFrom " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var from = reader1.GetDateTime(7);
                        var to = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = from, To = to, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchAdminCouponByType(int pageNum, int perPage, string direction, string key)
        {
            List<Coupon> coupons = new List<Coupon>();
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
                command.CommandText = "SELECT COUNT(Id) FROM tbl_coupon WHERE CouponType = @key AND IdShop IS NULL";
                command.Parameters.AddWithValue("@key", key);
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
                command1.CommandText = "SELECT * FROM tbl_coupon WHERE IdShop IS NULL AND CouponType = @key ORDER BY CouponFrom " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", key);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var from = reader1.GetDateTime(7);
                        var to = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = from, To = to, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchAdminCouponByProductType(int pageNum, int perPage, string direction, string key)
        {
            List<Coupon> coupons = new List<Coupon>();
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
                command.CommandText = "SELECT COUNT(Id) FROM tbl_coupon WHERE ProductType = @key AND IdShop IS NULL";
                command.Parameters.AddWithValue("@key", key);
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
                command1.CommandText = "SELECT * FROM tbl_coupon WHERE IdShop IS NULL AND ProductType = @key ORDER BY CouponFrom " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@key", key);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var from = reader1.GetDateTime(7);
                        var to = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = from, To = to, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<Page> SearchAdminCouponByDate(int pageNum, int perPage, string direction, DateTime from, DateTime to)
        {
            List<Coupon> coupons = new List<Coupon>();
            int total = 0;
            int totalPages = 0;
            Page page = null;
            MySqlConnection connect = conn.ConnectDB();
            MySqlConnection connect1 = conn.ConnectDB();
            try
            {
                string fr = from.ToString("yyyy-MM-dd");
                string t = to.ToString("yyyy-MM-dd");
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(Id) FROM tbl_coupon WHERE (( CouponTo >= @to AND CouponFrom <= @to ) OR ( CouponTo <= @to AND CouponTo >= @from )) AND IdShop IS NULL";
                command.Parameters.AddWithValue("@from", fr);
                command.Parameters.AddWithValue("@to", t);
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
                command1.CommandText = "SELECT DISTINCT * FROM tbl_coupon WHERE (( CouponTo >= @to AND CouponFrom <= @to ) OR ( CouponTo <= @to AND CouponTo >= @from )) AND IdShop IS NULL ORDER BY CouponFrom " + direction + " LIMIT @pageNum, @perPage";
                command1.Parameters.AddWithValue("@pageNum", (int)(pageNum * perPage) - perPage);
                command1.Parameters.AddWithValue("@perPage", (int)perPage);
                command1.Parameters.AddWithValue("@from", fr);
                command1.Parameters.AddWithValue("@to", t);
                await using var reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetString(0);
                        var code = reader1.GetString(1);
                        var quantity = reader1.GetInt32(2);
                        var worth = reader1.GetString(3);
                        var minimum = reader1.GetString(4);
                        var maximum = reader1.GetString(5);
                        var describe = reader1.GetString(6);
                        var fromm = reader1.GetDateTime(7);
                        var too = reader1.GetDateTime(8);
                        var type = reader1.GetString(9);
                        var typeProduct = reader1.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = fromm, To = too, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
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
                page = new Page { PageNum = pageNum, PerPage = perPage, Total = total, TotalPages = totalPages, Data = coupons };
            }
            catch (Exception ex)
            {
                connect.Close();
                connect1.Close();
                return null;
            }
            return page;
        }

        public async Task<int> GetCountCouponOfUser(string id)
        {
            int total = 0;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(c.Id) FROM tbl_coupon AS c, tbl_user AS u, tbl_shop AS s WHERE u.Id = s.IdUser AND s.Id = c.IdShop AND u.Id = @id";
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

        public async Task<int> GetCountAdminCoupon()
        {
            int total = 0;
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT COUNT(Id) FROM tbl_coupon WHERE IdShop IS NULL";
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

        public async Task<List<Coupon>> GetCouponOfShopAndProductType(string sId, string pType, string cost)
        {
            MySqlConnection connect = conn.ConnectDB();
            List<Coupon> coupons = new List<Coupon>();
            try
            {
                DateTime n = DateTime.UtcNow;
                string now = n.ToString("yyyy-MM-dd");
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT * FROM tbl_coupon WHERE IdShop = @sId AND ProductType = @pType AND CouponFrom <= @now AND CouponTo >= @now AND Minimum <= @cost";
                command.Parameters.AddWithValue("@sId", sId);
                command.Parameters.AddWithValue("@pType", pType);
                command.Parameters.AddWithValue("@now", now);
                command.Parameters.AddWithValue("@cost", Int32.Parse(cost));
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var id = reader.GetString(0);
                        var code = reader.GetString(1);
                        var quantity = reader.GetInt32(2);
                        var worth = reader.GetString(3);
                        var minimum = reader.GetString(4);
                        var maximum = reader.GetString(5);
                        var describe = reader.GetString(6);
                        var fromm = reader.GetDateTime(7);
                        var too = reader.GetDateTime(8);
                        var type = reader.GetString(9);
                        var typeProduct = reader.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = fromm, To = too, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
                    }
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            return coupons;
        }

        public async Task<List<Coupon>> GetCouponByProductType(string pType, string cType, string cost)
        {
            MySqlConnection connect = conn.ConnectDB();
            List<Coupon> coupons = new List<Coupon>();
            try
            {
                DateTime n = DateTime.UtcNow;
                string now = n.ToString("yyyy-MM-dd");
                connect.Open();
                var command = new MySqlCommand();
                command.Connection = connect;
                command.CommandText = "SELECT * FROM tbl_coupon WHERE IdShop IS NULL AND ProductType = @pType AND CouponFrom <= @now AND CouponTo >= @now AND Minimum <= @cost AND CouponType = @cType";
                command.Parameters.AddWithValue("@pType", pType);
                command.Parameters.AddWithValue("@cType", cType);
                command.Parameters.AddWithValue("@now", now);
                command.Parameters.AddWithValue("@cost", Int32.Parse(cost));
                await using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var id = reader.GetString(0);
                        var code = reader.GetString(1);
                        var quantity = reader.GetInt32(2);
                        var worth = reader.GetString(3);
                        var minimum = reader.GetString(4);
                        var maximum = reader.GetString(5);
                        var describe = reader.GetString(6);
                        var fromm = reader.GetDateTime(7);
                        var too = reader.GetDateTime(8);
                        var type = reader.GetString(9);
                        var typeProduct = reader.GetString(10);
                        Coupon coupon = new Coupon { Id = id, Code = code, Quantity = quantity, Worth = worth, Minimum = minimum, Maximum = maximum, Describe = describe, From = fromm, To = too, Type = type, ProductType = typeProduct };
                        coupons.Add(coupon);
                    }
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                connect.Close();
                return null;
            }
            return coupons;
        }
    }
}
