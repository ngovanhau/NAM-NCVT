using BE.Repositorys;
using BE.Common.Services;
using BE.Common.Exceptions;
using System.Text.RegularExpressions;
using System.Text;
using BE.Common.Application.CustomAttributes;
using BE.Domains.Entities;

namespace BE.Service
{
    [ScopedService]
    public class R5EVENTSService : BaseService
    {
        private readonly R5EVENTSRepository _repo;

        public R5EVENTSService(
            R5EVENTSRepository repo,
            IServiceProvider services
            ) : base(services)
        {
            _repo = repo;
        }

        public async Task<R5EVENT> GetByWo(string WO_CODE)
        {
            var result = await _repo.GetByWo(WO_CODE);
            return result;
        }
    }
}
