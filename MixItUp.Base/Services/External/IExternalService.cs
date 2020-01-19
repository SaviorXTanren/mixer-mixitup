using System;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public interface IExternalService
    {
        string Name { get; }

        bool IsConnected { get; }

        Task<ExternalServiceResult> Connect();

        Task Disconnect();
    }

    public class ExternalServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public ExternalServiceResult() : this(true) { }

        public ExternalServiceResult(bool success)
        {
            this.Success = success;
        }

        public ExternalServiceResult(bool success, string message)
            : this(success)
        {
            this.Message = message;
        }

        public ExternalServiceResult(string message) : this(false, message) { }

        public ExternalServiceResult(Exception exception)
            : this(exception.Message)
        {
            this.Exception = exception;
        }

        public ExternalServiceResult(string message, Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine();
            stringBuilder.Append(exception.Message);

            this.Exception = exception;
        }
    }

    public class ExternalServiceResult<T> : ExternalServiceResult
    {
        public T Result { get; set; }

        public ExternalServiceResult(T result)
        {
            this.Result = result;
            this.Success = !object.Equals(this.Result, default(T));
        }

        public ExternalServiceResult(bool success, T result)
        {
            this.Success = success;
            this.Result = result;
        }

        public ExternalServiceResult(bool success, string message)
            : this(success, default(T))
        {
            this.Message = message;
        }

        public ExternalServiceResult(string message) : this(false, message) { }

        public ExternalServiceResult(Exception exception)
            : this(exception.Message)
        {
            this.Exception = exception;
        }

        public ExternalServiceResult(string message, Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine();
            stringBuilder.Append(exception.Message);

            this.Exception = exception;
        }
    }
}