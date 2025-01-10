using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TucLoggbok
{
    public class DatabaseConfig
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["LibraryManagement"].ConnectionString;
    }
}
