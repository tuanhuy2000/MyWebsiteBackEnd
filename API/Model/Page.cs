namespace API.Model
{
    public class Page
    {
		private int _PageNum;

		public int PageNum
		{
			get { return _PageNum; }
			set { _PageNum = value; }
		}

		private int _PerPage;

		public int PerPage
		{
			get { return _PerPage; }
			set { _PerPage = value; }
		}

		private int _Total;

		public int Total
		{
			get { return _Total; }
			set { _Total = value; }
		}

		private int _TotalPages;

		public int TotalPages
		{
			get { return _TotalPages; }
			set { _TotalPages = value; }
		}

        private object _Data;

        public object Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
    }
}
