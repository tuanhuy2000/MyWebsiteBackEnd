namespace WebApplication1.Models
{
    public class APIResponse
    {
        private bool _Success;

        public bool Success
        {
            get { return _Success; }
            set { _Success = value; }
        }

        private string _Message;

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        private object _Data;

        public object Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
    }
}
