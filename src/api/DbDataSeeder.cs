using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace api
{
    public static class DbDataSeeder
    {
        // Astuce pour appeler une méthode de SeedData
        // TODO: Move this code when seed data is implemented in EF 7

        public static void SeedData(this IServiceScopeFactory scopeFactory)
        {
            using (var serviceScope = scopeFactory.CreateScope())
            {
                var dbcontext = serviceScope.ServiceProvider.GetService<dal.Model.MoneyboardContext>();
                dbcontext.SeedData();
            }
        }
    }
}