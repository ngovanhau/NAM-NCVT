using System.Net;

namespace BE.Common.Exceptions
{
    public class NotFoundException: ApplicationException
    {
        protected override string ErrorCode { get; } = "not found";

        public NotFoundException(string message) : base(message) { }

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.BadRequest;
    }
}
