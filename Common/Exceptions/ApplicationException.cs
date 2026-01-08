using System.Net;
using BE.Common.Models;

namespace BE.Common.Exceptions
{
    public abstract class ApplicationException: Exception
    {
        public abstract HttpStatusCode HttpStatusCode { get;  }
        protected virtual string ErrorCode { get; } = string.Empty;
        public ApplicationException(string message)
            : base(message)
        {
        }
        public ResponseModel GetErrorResponse()
        {
               return ResponseModel.Error(this.Message, this.HttpStatusCode, this.ErrorCode);
        }
    }
}
