using System.Net;

namespace BE.Common.Models
{
    public class ResponseModel
    {
        public HttpStatusCode HttpStatus { get; set; } = HttpStatusCode.OK;
        public bool IsSuccess { get; set; } = true;
        public object? Data { get; set; }

        public static ResponseModel Success<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK) where T : class
        {
            return new ResponseModel
            {
                HttpStatus = statusCode,
                IsSuccess = true,
                Data = data,
            };
        }

        public static ResponseModel Error(String errorMsg, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string errorCode = "")
        {
            return new ResponseModel
            {
                HttpStatus = statusCode,
                IsSuccess = false,
                Data = new ErrorResponseModel
                {
                    HttpStatus = statusCode,
                    ErrorCode = errorCode,
                    ErrorMessage = errorMsg
                },
            };
        }
    }
}
