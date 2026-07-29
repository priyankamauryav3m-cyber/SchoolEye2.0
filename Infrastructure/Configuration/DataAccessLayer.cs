using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Configuration
{
    public class DataAccessLayer
    {
        private readonly string strV3MConn;
        private SqlDataAdapter v3mDataAdapter;
        private SqlTransaction v3mTransaction;
        private DataSet v3mDataSet;

        // Constructor to pass in the connection string
        public DataAccessLayer(string strConn)
        {
            if (string.IsNullOrEmpty(strConn))
                throw new InvalidOperationException("Connection string cannot be null or empty.");
            strV3MConn = strConn;
        }

        // Method to get a SqlConnection
        public SqlConnection GetV3MConnection()
        {
            var v3mConn = new SqlConnection(strV3MConn);
            return v3mConn;
        }

        // Get DataSet
        public DataSet GetV3MSyncDataSet(string sqlQry, SqlCommand? mCommand = null)
        {
            v3mDataSet = new DataSet();

            using (var connection = GetV3MConnection())
            {
                connection.Open();

                if (mCommand == null)
                {
                    mCommand = new SqlCommand(sqlQry, connection);
                }

                mCommand.Connection = connection;
                mCommand.CommandText = sqlQry;
                mCommand.CommandType = CommandType.Text;

                v3mDataAdapter = new SqlDataAdapter(mCommand);
                v3mDataAdapter.Fill(v3mDataSet);

                return v3mDataSet;
            }
        }

        // Get DataTable
        public DataTable GetV3MSyncDataTable(string sqlQry, SqlCommand? mCommand = null)
        {
            v3mDataSet = new DataSet();

            using (var connection = GetV3MConnection())
            {
                connection.Open();

                if (mCommand == null)
                {
                    mCommand = new SqlCommand(sqlQry, connection);
                }

                mCommand.Connection = connection;
                mCommand.CommandText = sqlQry;
                mCommand.CommandType = CommandType.Text;

                v3mDataAdapter = new SqlDataAdapter(mCommand);
                v3mDataAdapter.Fill(v3mDataSet);

                return v3mDataSet.Tables[0];
            }
        }

        // Get Scalar Value
        public string? GetV3MScalerValue(string sqlQry, SqlCommand? mCommand = null)
        {
            using (var connection = GetV3MConnection())
            {
                connection.Open();

                if (mCommand == null)
                {
                    mCommand = new SqlCommand(sqlQry, connection);
                }
                mCommand.Connection = connection;
                mCommand.CommandText = sqlQry;
                mCommand.CommandType = CommandType.Text;

                var result = mCommand.ExecuteScalar();
                return result?.ToString();
            }
        }

        // Execute DML Query (Insert, Update, Delete)
        public void V3MDMLQuery(string sqlQry, SqlCommand? mCommand = null)
        {
            using (var connection = GetV3MConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (mCommand == null)
                        {
                            mCommand = new SqlCommand(sqlQry, connection, transaction);
                        }
                        mCommand.Connection = connection;
                        mCommand.Transaction = transaction;
                        mCommand.CommandText = sqlQry;
                        mCommand.CommandType = CommandType.Text;

                        mCommand.ExecuteNonQuery();
                        transaction.Commit(); // Commit the transaction
                    }
                    catch (Exception)
                    {
                        transaction.Rollback(); // Rollback in case of error
                        throw;
                    }
                }
            }
        }
    }
}
