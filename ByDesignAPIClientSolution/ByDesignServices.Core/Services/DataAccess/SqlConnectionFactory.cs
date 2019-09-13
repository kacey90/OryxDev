using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ByDesignServices.Core.Services.DataAccess
{
    public class SqlConnectionFactory : ISqlConnectionFactory, IDisposable
    {
        private readonly string _connectionString;
        private IDbConnection _dbConnection;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Dispose()
        {
            if (this._dbConnection != null && this._dbConnection.State == ConnectionState.Open)
            {
                this._dbConnection.Dispose();
            }
        }

        public IDbConnection GetOpenConnection()
        {
            if (this._dbConnection == null || this._dbConnection.State != ConnectionState.Open)
            {
                this._dbConnection = new SqlConnection(_connectionString);
                this._dbConnection.Open();
            }

            return this._dbConnection;
        }
    }
}
