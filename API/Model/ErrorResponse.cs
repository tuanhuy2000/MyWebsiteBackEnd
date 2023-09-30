namespace API.Model
{
    public class ErrorResponse
    {
        private string _Message;

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        private string _Status;

        public string Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
    }
}
