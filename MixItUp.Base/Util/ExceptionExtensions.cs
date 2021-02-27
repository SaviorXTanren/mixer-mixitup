using System;
using System.Collections.Generic;

namespace MixItUp.Base.Util
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> UnwrapException(this Exception exception)
        {
            List<Exception> results = new List<Exception>();
            while (exception != null)
            {
                results.Add(exception);
                exception = exception.InnerException;
            }
            return results;
        }
    }
}
