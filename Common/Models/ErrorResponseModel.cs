using System.Net;

namespace BE.Common.Models
{
    public class ErrorResponseModel
    {
        public HttpStatusCode HttpStatus { get; set; } = HttpStatusCode.InternalServerError;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = "System Error";
    }
}
