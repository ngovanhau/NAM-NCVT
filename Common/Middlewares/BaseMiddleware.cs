namespace BE.Common.Middlewares
{
    public abstract class BaseMiddleware
    {
        protected readonly RequestDelegate next;
        public BaseMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public abstract Task Invoke(HttpContext context, IServiceProvider services, IConfiguration configuration);
    }
}
