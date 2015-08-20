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
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        public LogLevel Level { get; set; }

        public void Write(string message, LogLevel logLevel)
        {
            logger.Log(NLogLogLevelToSplatLogLevel(logLevel), message);
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
                    throw new ArgumentOutOfRangeException("logLevel", logLevel, "This Splat.LogLevel isn't implemented!");
            }
        }
    }
}