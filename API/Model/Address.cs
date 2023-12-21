namespace API.Model
{
    public class Address
    {
		private string _Id;

		public string Id
		{
			get { return _Id; }
			set { _Id = value; }
		}

		private string _Name;

		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		private string _Phone;

		public string Phone
		{
			get { return _Phone; }
			set { _Phone = value; }
		}

		private string _FullAddress;

		public string FullAddress
		{
			get { return _FullAddress; }
			set { _FullAddress = value; }
		}

		private string _Type;

		public string Type
		{
			get { return _Type; }
			set { _Type = value; }
		}

	}
}
