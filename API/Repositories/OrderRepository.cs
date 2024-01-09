using API.Data;
using API.Model;
using MySqlConnector;
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

        public bool CreateOrder(Order order,string uId, string tId, string aId)
        {
            MySqlConnection connect = conn.ConnectDB();
            try
            {
                connect.Open();
                MySqlCommand sql = new MySqlCommand();
                sql.Connection = connect;
                sql.CommandText = "INSERT INTO tbl_order (Id, Orderer, Address, PaymentType, TotalCost, Discount, Transport) VALUES (@id, @uId, @address, @payType, @total, @discount, @transport)";
                sql.Parameters.AddWithValue("@id", order.Id);
                sql.Parameters.AddWithValue("@uId", uId);
                sql.Parameters.AddWithValue("@address", aId);
                sql.Parameters.AddWithValue("@payType", order.PaymentType);
                sql.Parameters.AddWithValue("@total", order.TotalCost);
                sql.Parameters.AddWithValue("@discount", order.Discount);
                sql.Parameters.AddWithValue("@transport", tId);
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
    }
}
