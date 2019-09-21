using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPassword(string account)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                return connection.Query<string>("spGetUserPassword", new { Id = account },
                        commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }
        }
    }
}
