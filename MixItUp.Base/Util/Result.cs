using System;
using System.Collections.Generic;
using System.Text;

namespace MixItUp.Base.Util
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public bool DisplayMessage { get; set; } = true;

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
            this.Append(message);
            this.Exception = exception;
        }

        public Result(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                this.Combine(result);
            }
        }

        public void Combine(Result other)
        {
            this.Success = this.Success && other.Success;

            if (this.Exception == null)
            {
                this.Exception = other.Exception;
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(this.Message))
            {
                stringBuilder.AppendLine(this.Message);
                stringBuilder.AppendLine();
            }
            if (!string.IsNullOrEmpty(other.Message))
            {
                stringBuilder.Append(other.Message);
            }
            this.Message = stringBuilder.ToString();
        }

        public override string ToString() { return (!string.IsNullOrEmpty(this.Message)) ? this.Message : string.Empty; }

        public void Append(string message)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(this.Message);
            stringBuilder.AppendLine();
            stringBuilder.Append(message);
            this.Message = stringBuilder.ToString();
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
