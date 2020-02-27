using System;
using System.Text;

namespace MixItUp.Base.Util
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public Result() : this(true) { }

        public Result(bool success)
        {
            this.Success = success;
        }

        public Result(bool success, string message)
            : this(success)
        {
            this.Message = message;
        }

        public Result(string message) : this(false, message) { }

        public Result(Exception exception)
            : this(exception.Message)
        {
            this.Exception = exception;
        }

        public Result(string message, Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine();
            stringBuilder.Append(exception.Message);

            this.Exception = exception;
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; set; }

        public Result(T result)
        {
            this.Value = result;
            this.Success = !object.Equals(this.Value, default(T));
        }

        public Result(bool success, T value)
        {
            this.Success = success;
            this.Value = value;
        }

        public Result(bool success, string message)
            : this(success, default(T))
        {
            this.Message = message;
        }

        public Result(string message) : this(false, message) { }

        public Result(Exception exception)
            : this(exception.Message)
        {
            this.Exception = exception;
        }

        public Result(string message, Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine();
            stringBuilder.Append(exception.Message);

            this.Exception = exception;
        }
    }
}
