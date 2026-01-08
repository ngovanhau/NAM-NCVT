using BE.Common.Domains.Config;
using BE.Common.Domains.Config.Interfaces;
using BE.Common.Domains.Config.Connections;
using BE.Domains.Entities;
using Dapper;

namespace BE.Repositorys
{
    public class R5EVENTSRepository: BaseRepository
    {
        public R5EVENTSRepository(IEnvResolver resolver, IConnectionFactory factory)
            : base(resolver, factory) { }

        public async Task<R5EVENT> GetByWo(string WO_CODE)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT *
                FROM R5EVENTS
                WHERE
                    EVT_CODE = @WO_CODE;
            ";

            var result = await connection.QueryFirstOrDefaultAsync<R5EVENT>(sql, new
            {
                WO_CODE
            });

            return result;
        }
    }
}
