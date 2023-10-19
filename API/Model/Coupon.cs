namespace API.Model
{
    public class Coupon
    {
		private string _Id;

		public string Id
		{
			get { return _Id; }
			set { _Id = value; }
		}

		private string _Code;

		public string Code
		{
			get { return _Code; }
			set { _Code = value; }
		}

		private int _Quantity;

		public int Quantity
		{
			get { return _Quantity; }
			set { _Quantity = value; }
		}

		private string _Worth;

		public string Worth
		{
			get { return _Worth; }
			set { _Worth = value; }
		}

		private string _Describe;

		public string Describe
		{
			get { return _Describe; }
			set { _Describe = value; }
		}

		private DateTime _From;

		public DateTime From
		{
			get { return _From; }
			set { _From = value; }
		}

		private DateTime _To	;

		public  DateTime To		
		{
			get { return _To; }
			set { _To = value; }
		}

        private string _Type;

        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        private string _ProductType;

		public string ProductType
		{
			get { return _ProductType; }
			set { _ProductType = value; }
		}
    }
}
