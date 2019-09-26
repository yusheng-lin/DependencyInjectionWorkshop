using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class UserDal
    {
        public string GetPasswordFromDb(string account)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
}