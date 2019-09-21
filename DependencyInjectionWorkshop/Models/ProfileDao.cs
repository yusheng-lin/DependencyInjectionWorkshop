using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPassword(string account);
    }

    public class ProfileDao : IProfile
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
