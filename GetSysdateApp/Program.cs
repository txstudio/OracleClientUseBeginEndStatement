using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace GetSysdateApp
{
    class Program
    {
        /* 
         * 使用 PL/SQL 與 Output OracleParameter 取得 Oracle 資料庫的系統時間 (SYSDATE) 
         */

        static void Main(string[] args)
        {
            var _ipAddress = "(your database location)";
            var _userId = "system";
            var _password = "oracle";

            var connectionString = string.Format("Data Source={0};User Id={1};Password={2};"
                                                , _ipAddress
                                                , _userId
                                                , _password);

            using (OracleConnection _conn = new OracleConnection())
            {
                _conn.ConnectionString = connectionString;

                OracleCommand _cmd;

                _cmd = new OracleCommand();
                _cmd.Connection = _conn;
                _cmd.CommandType = CommandType.Text;
                _cmd.BindByName = true;

                _cmd.Parameters.Add(":DATE", OracleDbType.Date);
                _cmd.Parameters[":DATE"].Direction = ParameterDirection.Output;
                _cmd.Parameters[":DATE"].Value = DBNull.Value;

                _cmd.CommandText = @"BEGIN 
                    SELECT SYSDATE 
                    INTO :DATE 
                    FROM DUAL;
                END;";

                _conn.Open();
                _cmd.ExecuteNonQuery();
                _conn.Close();

                var _oracleDate = (_cmd.Parameters[":DATE"].Value);

                //直接轉型出現 "System.InvalidCastException"
                //var _dateTime = Convert.ToDateTime(_oracleDate);

                var _dateTime = Convert.ToDateTime(_oracleDate.ToString());

                Console.WriteLine("Oracle Database SYSDATE");
                Console.WriteLine("{0:yyyy/MM/dd HH:mm:ss}", _dateTime);
            }

            Console.WriteLine();
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }
    }
}
