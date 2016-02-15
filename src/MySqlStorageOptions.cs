using System;

namespace Hangfire.MySql
{
    public class MySqlStorageOptions
    {

        private TimeSpan _queuePollInterval;


        public MySqlStorageOptions()
        {
            QueuePollInterval = TimeSpan.FromSeconds(15);
            EnsureDatabase = true;
        }

        /// <summary>
        /// Should the database schema be checked for missing hangfire tables and updated if any are missing? Default==true.
        /// </summary>
        public bool EnsureDatabase { get; set; }

        public TimeSpan QueuePollInterval
        {
            get { return _queuePollInterval; }
            set
            {
                var message = String.Format(
                    "The QueuePollInterval property value should be positive. Given: {0}.",
                    value);

                if (value == TimeSpan.Zero)
                {
                    throw new ArgumentException(message, "value");
                }
                if (value != value.Duration())
                {
                    throw new ArgumentException(message, "value");
                }

                _queuePollInterval = value;
            }
        }

    }

}