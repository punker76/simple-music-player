using System;
using NLog;
using LogLevel = Splat.LogLevel;

namespace SimpleMusicPlayer.Core
{
    internal class NLogLogger : Splat.ILogger
    {
        private readonly Logger logger;

        public NLogLogger(Logger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public LogLevel Level { get; set; }

        /// <summary>Writes a message to the target.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="logLevel">The severity level of the log message.</param>
        public void Write(string message, LogLevel logLevel)
        {
            logger.Log(NLogLogLevelToSplatLogLevel(logLevel), message);
        }

        /// <summary>Writes a messge to the target.</summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        /// <param name="logLevel">The log level.</param>
        public void Write(string message, Type type, LogLevel logLevel)
        {
            logger.Log(NLogLogLevelToSplatLogLevel(logLevel), message, type);
        }

        private static NLog.LogLevel NLogLogLevelToSplatLogLevel(Splat.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;

                case LogLevel.Info:
                    return NLog.LogLevel.Info;

                case LogLevel.Warn:
                    return NLog.LogLevel.Warn;

                case LogLevel.Error:
                    return NLog.LogLevel.Error;

                case LogLevel.Fatal:
                    return NLog.LogLevel.Fatal;
                default:
                    return NLog.LogLevel.Off;
            }
        }
    }
}