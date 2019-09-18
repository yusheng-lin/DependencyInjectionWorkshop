using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            throw new NotImplementedException();
        }

        public string GetPassword(string accountId)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                var password = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                                                        commandType: CommandType.StoredProcedure).SingleOrDefault();

                return password;
            }
        }
    }
}