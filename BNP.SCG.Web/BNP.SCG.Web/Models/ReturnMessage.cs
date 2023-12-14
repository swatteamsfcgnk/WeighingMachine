namespace BNP.SCG.Web.Models
{
    public class ReturnMessage
    {
        private List<String> _message = new List<String>();
        public List<String> message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value == null)
                    _message = new List<String>();
                else
                    _message = value;
            }
        }

        public bool isCompleted { get; set; }
    }
    public class ReturnObject<T> : ReturnMessage
    {
        public T data { get; set; }
    }
    public class ReturnList<T> : ReturnObject<List<T>>
    {

    }
}
