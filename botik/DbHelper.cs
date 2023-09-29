using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QC = Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace botik
{
    internal class DbHelper
    {
        public QC.SqlConnection? connection_;
        public bool connectAsync(string cs)
        {
            // Создание подключения
            connection_ = new QC.SqlConnection(cs);
            try
            {
                // Открываем подключение
                connection_.Open();
                Console.WriteLine("Connection opened");
            }
            catch (QC.SqlException ex)
            {
                Console.WriteLine("Cannot connect to db: " + ex.Message);
            }

            return connection_.State == ConnectionState.Open;
        }
        public void closeConnection()
        {
            if (connection_ != null)
            {
                connection_.Close();
            }
        }

        public void executeCommand(string command)
        {
            QC.SqlCommand cmd = new QC.SqlCommand();
            cmd.CommandText = command;
            try 
            {
                cmd.Connection = connection_;
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL ERROR: " + ex.Message);
            }
            
        }

        public int executeSELECTCommand(string command)
        {
            QC.SqlCommand cmd = new QC.SqlCommand(command, connection_);
            cmd.CommandText = command;
            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) 
                {
                    object id = reader.GetValue(0);
                    return Convert.ToInt32(id);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL ERROR: " + ex.Message);
            }

            return 0;
        }
    }
}
