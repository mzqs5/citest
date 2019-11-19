using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DbHelper
{
    public class SqlHelper : ISqlHelper, IDisposable
    {
        public SqlHelper(IDbConnection connection)
        {
            Connection = connection;
        }

        public IDbConnection Connection { get; set; }

        public void Dispose()
        {
            Connection.Dispose();
        }


        private IDbCommand buildCommand(string sqlText, CommandType commandType, params IDbDataParameter[] parms)
        {
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");

            IDbCommand command = Connection.CreateCommand();
            command.CommandText = sqlText;
            command.CommandType = commandType;
            if (parms != null && parms.Length > 0)
            {
                foreach (var item in parms)
                {
                    command.Parameters.Add(item);
                }
            }

            return command;
        }

        public int Execute(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms)
        {
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");

            ConnectionState originalState = Connection.State;

            if (originalState != ConnectionState.Open)
                Connection.Open();
            try
            {
                return buildCommand(sqlText, commandType, parms).ExecuteNonQuery();
            }
            finally
            {
                if (originalState == ConnectionState.Closed)
                    Connection.Close();
            }
        }



        public int Execute(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text)
        {
            return Execute(sqlText, commandType, func());
        }

        public IEnumerable<T> Query<T>(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms) where T : class
        {
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");

            ConnectionState originalState = Connection.State;

            if (originalState != ConnectionState.Open)
                Connection.Open();
            try
            {
                var reader = buildCommand(sqlText, commandType, parms).ExecuteReader();
                var builder = EntityBuilder<T>.CreateBuilder(reader);
                while (reader.Read())
                    yield return builder.Build(reader);
            }
            finally
            {
                if (originalState == ConnectionState.Closed)
                    Connection.Close();
            }
        }

        public IEnumerable<T> Query<T>(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text) where T : class
        {
            return Query<T>(sqlText, commandType, func());
        }

        public object Scalar(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms)
        {
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");

            ConnectionState originalState = Connection.State;

            if (originalState != ConnectionState.Open)
                Connection.Open();
            try
            {
                return buildCommand(sqlText, commandType, parms).ExecuteScalar();
            }
            finally
            {
                if (originalState == ConnectionState.Closed)
                    Connection.Close();
            }
        }

        public object Scalar(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text)
        {
            return Scalar(sqlText, commandType, func());
        }

        public T Scalar<T>(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms) where T : struct
        {
            return (T)Scalar(sqlText, commandType, parms);
        }

        public T Scalar<T>(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text) where T : struct
        {
            return Scalar<T>(sqlText, commandType, func());
        }
    }
}
