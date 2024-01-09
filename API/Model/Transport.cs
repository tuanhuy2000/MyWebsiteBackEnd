namespace API.Model
{
    public class Transport
    {
		private string _Id;

		public string Id
		{
			get { return _Id; }
			set { _Id = value; }
		}

        private string _ShippingCode;

        public string ShippingCode
        {
            get { return _ShippingCode; }
            set { _ShippingCode = value; }
        }

        private string _ShippingUnit;

		public string ShippingUnit
		{
			get { return _ShippingUnit; }
			set { _ShippingUnit = value; }
		}

		private string _ShippingWay;

		public string ShippingWay
		{
			get { return _ShippingWay; }
			set { _ShippingWay = value; }
		}
	}
}
