using System;
using System.Collections.Generic;
using System.Text;

namespace DbHelper
{
    public class MSSqlHelper : SqlHelper
    {
        public MSSqlHelper(string connectionString) : base(new System.Data.SqlClient.SqlConnection(connectionString)) { }
    }
}
