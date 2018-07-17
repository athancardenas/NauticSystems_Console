using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    public class EnquiryRegistryFolderPathHelper
    {
        private string _InboundQueue = string.Empty;
        private string _OutboundQueue = string.Empty;
        private string _OutboundArchive = string.Empty;
        private string _FailedItems = string.Empty;
        private string _UATLog = string.Empty;
        private string _rmsDocs = string.Empty;
        private string _temp = string.Empty;
        private string _ProcessedFiles = string.Empty;
        private string _exMsg = string.Empty;
        private string _ReturnStr = string.Empty;

        public EnquiryRegistryFolderPathHelper()
        {
            try
            {
                /*
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Nautic Systems" + TempStr + @"\nsReplication", true);

                switch (RegistryKeyStr)
                {
                    case "InboundQueue":
                        return key.GetValue("InboundQueue").ToString();
                    case "OutboundQueue":
                        return key.GetValue("OutboundQueue").ToString();
                    case "OutboundArchive":
                        return key.GetValue("OutboundArchive").ToString();
                    case "FailedItems":
                        return key.GetValue("FailedItems").ToString();
                    case "UATLog":
                        return key.GetValue("UATLog").ToString();
                    case "rmsDocs":
                        return key.GetValue("rmsDocs").ToString();
                    case "temp":
                        return key.GetValue("temp").ToString();
                    case "ProcessedFiles":
                        return key.GetValue("ProcessedFiles").ToString();

                }
                */

                using (SqlConnection conn = new SqlConnection())
                {

                    //conn.ConnectionString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                    using (SqlCommand cmd = new SqlCommand("spGetRepNSRepConfig", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            if (dr.HasRows)
                            {
                                
                                _InboundQueue = dr["InboundQueue"].ToString();
                                _OutboundQueue = dr["OutboundQueue"].ToString();
                                _OutboundArchive = dr["OutboundArchive"].ToString();
                                _FailedItems = dr["FailedItems"].ToString();
                                _UATLog = dr["UATLog"].ToString();
                                _rmsDocs = dr["rmsDocs"].ToString();
                                _temp = dr["temp"].ToString();
                                _ProcessedFiles = dr["ProcessedFiles"].ToString();
                            }

                        }

                        dr.Close();

                    }

                    conn.Close();

                }

            }
            catch (System.Exception ex)
            {
                //Console.WriteLine(ex.ToString());

                //return ex.Message;
                _exMsg = ex.Message;
                //ReturnKeyStr = "Nautic Systems Replication Registry does not have the Key = '" + RegistryKeyStr + "'.\r\nPlease contact System Administrator!";
                //return ReturnKeyStr;

                //return @"C:\AE IT Project\NauticSystems" + TempStr + @"\NauticSystems\";
            }
        }

        public string get(string RegistryKeyStr)
        {
            if (string.IsNullOrEmpty(_exMsg))
            {
                switch (RegistryKeyStr)
                {
                    case "InboundQueue":
                        _ReturnStr = _InboundQueue;
                        break;
                    case "OutboundQueue":
                        _ReturnStr = _OutboundQueue;
                        break;
                    case "OutboundArchive":
                        _ReturnStr = _OutboundArchive;
                        break;
                    case "FailedItems":
                        _ReturnStr = _FailedItems;
                        break;
                    case "UATLog":
                        _ReturnStr = _UATLog;
                        break;
                    case "rmsDocs":
                        _ReturnStr = _rmsDocs;
                        break;
                    case "temp":
                        _ReturnStr = _temp;
                        break;
                    case "ProcessedFiles":
                        _ReturnStr = _ProcessedFiles;
                        break;
                }

                if (!System.IO.Directory.Exists(_ReturnStr))
                    System.IO.Directory.CreateDirectory(_ReturnStr);
            }
            
            else
            {
                _ReturnStr = _exMsg;
            }            

            return _ReturnStr;
        }

        public string getInboundDir()
        {
            return _InboundQueue;
        }
        public string getOutboundDir()
        {
            return _OutboundQueue;
        }
        public string getOutboundArchiveDir()
        {
            return _OutboundArchive;
        }
        public string getFailedItemsDir()
        {
            return _FailedItems;
        }
        public string geUATLogDir()
        {
            return _UATLog;
        }
        public string getRMSDocsDir()
        {
            return _rmsDocs;
        }
        public string getTempDir()
        {
            return _temp;
        }
        public string getProcessedFilesDir()
        {
            return _ProcessedFiles;
        }

    }
}
