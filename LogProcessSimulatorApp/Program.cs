using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Threading;

namespace LogProcessSimulatorApp
{
    class Program
    {
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

                int _LogSeq;

                #region 情境：進行 WebService 呼叫建立呼叫紀錄

                _cmd.CommandText = @"BEGIN
SELECT SYSTEM.EVENT_LOGS_SEQ.NEXTVAL INTO :EVENT_NO FROM DUAL;

INSERT INTO SYSTEM.EVENT_LOGS (
    EVENT_NO
    ,REQUEST_XML
) VALUES (
    :EVENT_NO
    ,:REQUEST_XML
);

END;";

                _cmd.Parameters.Clear();
                _cmd.Parameters.Add(":EVENT_NO", OracleDbType.Int32);
                _cmd.Parameters.Add(":REQUEST_XML", OracleDbType.Clob);

                _cmd.Parameters[":EVENT_NO"].Direction = ParameterDirection.Output;
                _cmd.Parameters[":REQUEST_XML"].Direction = ParameterDirection.Input;

                _cmd.Parameters[":EVENT_NO"].Value = DBNull.Value;
                _cmd.Parameters[":REQUEST_XML"].Value = GetRequestXml();

                _conn.Open();
                _cmd.ExecuteNonQuery();
                _conn.Close();

                //直接轉型出現 "System.InvalidCastException"
                //_LogSeq = Convert.ToInt32(_cmd.Parameters[":EVENT_NO"].Value);

                _LogSeq = Convert.ToInt32(_cmd.Parameters[":EVENT_NO"].Value.ToString());

                #endregion

                Console.WriteLine("Event No: {0}", _LogSeq);

                //模擬 WebService 存取資料的時間
                Thread.Sleep(TimeSpan.FromSeconds(1));

                #region 情境：取得 WebService 呼叫結果並更新事件紀錄

                _cmd.CommandText = @"UPDATE SYSTEM.EVENT_LOGS SET
    RESPONSE_TIME = SYSDATE
    ,RESPONSE_XML = :RESPONSE_XML
    ,STATUS_CODE = :STATUS_CODE
    ,ERROR_MESSAGE = :ERROR_MESSAGE
WHERE EVENT_NO = :EVENT_NO";

                _cmd.Parameters.Clear();
                _cmd.Parameters.Add(":RESPONSE_XML", OracleDbType.Clob);
                _cmd.Parameters.Add(":STATUS_CODE", OracleDbType.Int32);
                _cmd.Parameters.Add(":ERROR_MESSAGE", OracleDbType.NVarchar2, 150);
                _cmd.Parameters.Add(":EVENT_NO", OracleDbType.Int32);

                _cmd.Parameters[":RESPONSE_XML"].Value = GetResponseXml();
                _cmd.Parameters[":STATUS_CODE"].Value = 200;
                _cmd.Parameters[":ERROR_MESSAGE"].Value = DBNull.Value;
                _cmd.Parameters[":EVENT_NO"].Value = _LogSeq;

                _conn.Open();
                _cmd.ExecuteNonQuery();
                _conn.Close();

                #endregion

            }

            Console.WriteLine();
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }

        /*
         * 範例 XML 資料來源
         *  https://www.w3schools.com/xml/xml_soap.asp
         */
        static string GetRequestXml()
        {
            return @"<?xml version=""1.0""?>
<soap:Envelope
    xmlns:soap=""http://www.w3.org/2003/05/soap-envelope/""
    soap:encodingStyle=""http://www.w3.org/2003/05/soap-encoding"">
    <soap:Body>
      <m:GetPrice xmlns:m=""https://www.w3schools.com/prices"">
        <m:Item>Apples</m:Item>
      </m:GetPrice>
    </soap:Body>
</soap:Envelope>";
        }

        static string GetResponseXml()
        {
            return @"<?xml version=""1.0""?>
<soap:Envelope
xmlns:soap=""http://www.w3.org/2003/05/soap-envelope/""
soap:encodingStyle=""http://www.w3.org/2003/05/soap-encoding"">
    <soap:Body>
      <m:GetPriceResponse xmlns:m=""https://www.w3schools.com/prices"">
        <m:Price>1.90</m:Price>
      </m:GetPriceResponse>
    </soap:Body>
</soap:Envelope>";
        }
    }
}
