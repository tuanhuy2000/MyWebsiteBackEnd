namespace API.Model
{
    public class RefreshToken
    {
		private string _Id;

		public string Id
		{
			get { return _Id; }
			set { _Id = value; }
		}

		private User _User;

		public User User
		{
			get { return _User; }
			set { _User = value; }
		}

		private string _RefreshTokenn;

		public string RefreshTokenn
		{
			get { return _RefreshTokenn; }
			set { _RefreshTokenn = value; }
		}

		private DateTime _Expire;

		public DateTime Expire
		{
			get { return _Expire; }
			set { _Expire = value; }
		}

	}
}
