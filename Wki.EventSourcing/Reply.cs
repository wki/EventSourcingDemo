namespace Wki.EventSourcing
{
    /// <summary>
    /// Reply to a command: either OK or Error(Message)
    /// </summary>
    public class Reply
    {
        public bool IsOk { get; private set; }
        public string Message { get; private set; }

        protected Reply(bool isOk, string message = null)
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
        /// Construct and return an Error-reply with a message
        /// </summary>
        /// <param name="message">Message.</param>
        public static Reply Error(string message) =>
            new Reply(false, message);

        /// <summary>
        /// Construct and return an Error-reply with a formatted message
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Reply Error(string format, params object[] args) =>
            new Reply(false, string.Format(format, args));

        /// <summary>
        /// Construct and return a typed reply with a given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Reply<T> Value<T>(T value) =>
            new Reply<T>(value);
    }
    /// <summary>
    /// Typed reply with a value of a given type
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class Reply<TResult> : Reply
    {
        public TResult Value { get; private set; }

        public Reply(TResult value) : base(true)
        {
            Value = value;
        }
    }
}
