namespace BE.Common.Services
{
    public abstract class BaseService(IServiceProvider services)
    {
        protected IServiceProvider _services = services;
    }
}
