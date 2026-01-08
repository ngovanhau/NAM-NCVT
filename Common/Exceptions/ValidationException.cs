using System.Net;

namespace BE.Common.Exceptions
{
    public class ValidationException : ApplicationException
    {
        protected override string ErrorCode { get; } = "validation failed";

        public ValidationException(string message) : base(message) { }

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.BadRequest;
    }
}
