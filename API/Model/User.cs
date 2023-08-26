namespace API.Model
{
    public class User
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

		private string _PhoneNumber;

		public string PhoneNumber
		{
			get { return _PhoneNumber; }
			set { _PhoneNumber = value; }
		}
		private string _Email;

		public string Email
		{
			get { return _Email; }
			set { _Email = value; }
		}

		private string _UserName;

		public string UserName
		{
			get { return _UserName; }
			set { _UserName = value; }
		}

		private string _Password;

		public string Password
		{
			get { return _Password; }
			set { _Password = value; }
		}

		private string _AccessToken;

		public string AccessToken
		{
			get { return _AccessToken; }
			set { _AccessToken = value; }
		}

		private string _Role;

		public string Role
		{
			get { return _Role; }
			set { _Role = value; }
		}

	}
}
