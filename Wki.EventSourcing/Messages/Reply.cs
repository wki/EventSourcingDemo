namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Reply to a command: either OK or Error(Message)
    /// </summary>
    public class Reply
    {
        public bool IsOk { get; private set; }
        public string Message { get; private set; }

        private Reply(bool isOk, string message = null)
        {
            IsOk = isOk;
            Message = message;
        }

        /// <summary>
        /// Construct and return an Ok-reply
        /// </summary>
        public static Reply Ok() => 
            new Reply(true);

        /// <summary>
        /// Construct and returns an Error-reply with a message
        /// </summary>
        /// <param name="message">Message.</param>
        public static Reply Error(string message) =>
            new Reply(false, message);
        
        public static Reply<T> Value(T value) => 
            new Reply<T>(value);
    }

    public class Reply<TResult> : Reply
    {
        public TResult Value { get; private set; }

        public Reply<TResult>(TResult value) : base(true)
        {
            Value = value;    
        }
    }
}
