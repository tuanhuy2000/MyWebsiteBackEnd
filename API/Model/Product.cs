namespace API.Model
{
    public class Product
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

        private int _Price;

        public int Price
        {
            get { return _Price; }
            set { _Price = value; }
        }

        private int _Quantity;

        public int Quantity
        {
            get { return _Quantity; }
            set { _Quantity = value; }
        }

        private string _Information;

        public string Information
        {
            get { return _Information; }
            set { _Information = value; }
        }

        private string _Address;

        public string Address
        {
            get { return _Address; }
            set { _Address = value; }
        }

        private string _Type;

        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        private List<string> _Img;

        public List<string> Img
        {
            get { return _Img; }
            set { _Img = value; }
        }

    }
}
