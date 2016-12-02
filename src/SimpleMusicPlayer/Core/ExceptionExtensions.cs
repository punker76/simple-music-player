using System;
using System.Threading;

namespace SimpleMusicPlayer.Core
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Convience function for those that are not availability aware to quickly test if they should rethrow.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>Returns true if the exception is fatal for t he current thread.</returns>
        /// <remarks>The question is where in the catch clause to put the test, at the beginning of the catch or at the end. 
        /// Sometimes we do important cleaning in the catch clause plus notification of other threads. In doubt do the throw at 
        /// the end of the exception handling.
        /// </remarks>
        public static bool IsFatalException(Exception ex)
        {
            return ContainsException(ex, typeof(ThreadAbortException),
                                     typeof(StackOverflowException),
                                     typeof(OutOfMemoryException));
        }

        // recursively looks for the exceptionTypes (they might be hiding in inner exceptions)
        private static bool ContainsException(Exception ex, params Type[] exceptionTypes)
        {
            foreach (var exceptionType in exceptionTypes)
            {
                if (exceptionType.IsInstanceOfType(ex))
                {
                    return true;
                }
            }
            var aex = ex as AggregateException;
            if (aex != null)
            {
                foreach (var innerException in aex.InnerExceptions)
                {
                    return ContainsException(innerException, exceptionTypes);
                }
            }
            else if (ex.InnerException != null)
            {
                return ContainsException(ex.InnerException);
            }
            return false;
        }
    }
}