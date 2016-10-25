using System;

namespace Wki.EventSourcing.Util
{
    /// <summary>
    /// A fakable replacement for System.DateTime
    /// </summary>
    /// <example>
    /// var dt = SystemTime.Now;
    /// 
    /// // fake before calling
    /// SystemTime.Fake( () => new DateTime(1964,5,3) );
    /// var fakedDt = SystemTime.Now;
    /// </example>
    public static class SystemTime
    {
        private static Func<DateTime> dateTimeProvider = () => DateTime.Now;

        /// <summary>
        /// add a fake provider
        /// </summary>
        /// <param name="provider">Provider.</param>
        public static void Fake(Func<DateTime> provider) =>
            dateTimeProvider = provider;

        /// <summary>
        /// return time given from provider
        /// </summary>
        /// <value>The now.</value>
        public static DateTime Now { get { return dateTimeProvider(); } }
    }
}
