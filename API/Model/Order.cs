namespace API.Model
{
    public class Order
    {
		private string _Id;

		public string Id
		{
			get { return _Id; }
			set { _Id = value; }
		}

		private string _PaymentType;

		public string PaymentType
		{
			get { return _PaymentType; }
			set { _PaymentType = value; }
		}

		private double _TotalCost;

		public double TotalCost
		{
			get { return _TotalCost; }
			set { _TotalCost = value; }
		}

		private double _Discount;

		public double Discount
		{
			get { return _Discount; }
			set { _Discount = value; }
		}
	}
}
