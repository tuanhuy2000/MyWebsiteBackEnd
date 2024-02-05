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

        private string _Status;

		public string Status
		{
			get { return _Status; }
			set { _Status = value; }
		}

		private string _ShippingWay;

		public string ShippingWay
		{
			get { return _ShippingWay; }
			set { _ShippingWay = value; }
		}

		private DateTime _OrderDate;

		public DateTime OrderDate
		{
			get { return _OrderDate; }
			set { _OrderDate = value; }
		}

		private DateTime _CompletionDate;

		public DateTime CompletionDate
		{
			get { return _CompletionDate; }
			set { _CompletionDate = value; }
		}

	}
}
