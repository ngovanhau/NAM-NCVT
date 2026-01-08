using BE.Common.Exceptions;
using Newtonsoft.Json;
using ApplicationException = BE.Common.Exceptions.ApplicationException;
using BE.Common.Exceptions;


namespace BE.Common.Middlewares
{
    public class GlobalExceptionHandlerMiddleware(RequestDelegate next) : BaseMiddleware(next)
    {
        public override async Task Invoke(HttpContext context, IServiceProvider services, IConfiguration configuration)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var appException = GetApplicationException(ex);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)appException.HttpStatusCode;

                await context.Response.WriteAsync(JsonConvert.SerializeObject(appException.GetErrorResponse()));
            }
        }
        private static ApplicationException GetApplicationException(Exception ex)
        {
            return ex.GetType().IsSubclassOf(typeof(ApplicationException))
                ? (ApplicationException)ex : new InternalErrorException(ex.Message);
        }
    }
}
