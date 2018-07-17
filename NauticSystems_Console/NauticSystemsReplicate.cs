using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Microsoft.Win32;
using Microsoft.SharePoint.Client;
using System.Xml;
using System.Text;
using System.DirectoryServices;
using System.Globalization;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    class NauticSystemsReplicate
    {//
        // Configuration of UAT Ship Servers
        //public List<class_UATConfig> UATPathConfigs;
        public bool MultipleShipTest;
        public string ProcessedFilesPath;
        public int[] ActiveIMO;
        public int SingleShipIMOInt = 9708227;
        public string OperationModeStr;
        public string inLog;

        bool useTPL = true;
        string RelocationFolder = "_AppReference";
        string DMSSRFolderSearchPattern = "*_";
        string JSONFileName = "fatt.nsf";

        EnquiryRegistryFolderPathHelper erfpHelper = null;

        public NauticSystemsReplicate()
        {
            // instantiate EnquiryRegistryFolderPathHelper to load path values from DB
            erfpHelper = new EnquiryRegistryFolderPathHelper();
        }

        public string AckFromShipReplication_test()
        {
            return "Program configuration is done!";
        }

        public string AckFromShipReplication()
        {
            string ACKFromShipReplicationStatus = string.Empty;
            int ACKFromShipReplicationCount = 0;

            int AckIMO = 0;

            // Global Variable configurations
            MultipleShipTest = false;
            //MultipleShipTest = true;
            ProcessedFilesPath = EnquiryRegistryFolderPath("ProcessedFiles");
            OperationModeStr = string.Empty;
            //OperationModeStr = "Trial";

            /*
            UATPathConfigs = new List<class_UATConfig>();
            
                if (MultipleShipTest)
                {
                    UATPathConfigs.Add(new class_UATConfig() { IMO = 9732589, IPAddress = "10.0.1.231", SQLServerPassword = "Naut1cM@ster#", SQLServerDatabase = "NauticMaster_9732589" });
                    UATPathConfigs.Add(new class_UATConfig() { IMO = 9261451, IPAddress = "10.0.1.231", SQLServerPassword = "Naut1cM@ster#", SQLServerDatabase = "NauticMaster_9261451" });
                    UATPathConfigs.Add(new class_UATConfig() { IMO = 9697832, IPAddress = "10.0.1.231", SQLServerPassword = "Naut1cM@ster#", SQLServerDatabase = "NauticMaster_9697832" });
                    UATPathConfigs.Add(new class_UATConfig() { IMO = 9699995, IPAddress = "10.0.1.231", SQLServerPassword = "Naut1cM@ster#", SQLServerDatabase = "NauticMaster_9699995" });
                    UATPathConfigs.Add(new class_UATConfig() { IMO = 9790919, IPAddress = "10.0.1.231", SQLServerPassword = "Naut1cM@ster#", SQLServerDatabase = "NauticMaster_9790919" });
                }
                else
                    UATPathConfigs.Add(new class_UATConfig() { IMO = 9732589, IPAddress = "10.0.1.231", SQLServerPassword = "Naut1cM@ster#", SQLServerDatabase = "NauticMaster_9732589" });
                
            for (int ShipLoopInt = 0; ShipLoopInt < UATPathConfigs.Count; ShipLoopInt++)
            {*/

            //SingleShipIMOInt = 9732589;
            //SingleShipIMOInt = 9708227;

            //////////

            // Log starting time tick
            long startTimeTicks = DateTime.Now.Ticks;
            Console.WriteLine("Task: DMSSR . . . ");
            Console.WriteLine("Using TPL? " + useTPL.ToString().ToUpper());
            Console.WriteLine(startTimeTicks.ToString() + " Start DMSSR . . . ");
            Console.WriteLine("Processing files for DMSSR. Please wait . . . ");

            //////////

            if (!useTPL)
            {
                #region v1.0

                if (MultipleShipTest)
                    ActiveIMO = getAllActiveVessels();
                else
                    ActiveIMO = new int[] { SingleShipIMOInt };

                // Process DMSSR Replication Folder from Ship
                for (int ShipLoopInt = 0; ShipLoopInt < ActiveIMO.Length; ShipLoopInt++)
                {
                    AckIMO = ActiveIMO[ShipLoopInt];
                    //string NMPathStr = @"\\10.0.1.231\c$";
                    //AckIMO = UATPathConfigs[ShipLoopInt].IMO;

                    /*
                    string NMPathStr = @"\\" + UATPathConfigs[ShipLoopInt].IPAddress + @"\c$";

                    string NMOutPathStr = string.Empty;
                    string NMInPathStr = string.Empty;
                    string NMOutArchivePathStr = string.Empty;

                    UATGlobals.SetUATGlobals(UATPathConfigs[ShipLoopInt].IMO, UATPathConfigs[ShipLoopInt].IPAddress, UATPathConfigs[ShipLoopInt].SQLServerPassword, UATPathConfigs[ShipLoopInt].SQLServerDatabase);
                    */

                    ACKFromShipReplicationStatus = string.Empty;

                    string RepDirectoryStr = string.Empty;
                    string RepRefStr = string.Empty;
                    string RepFolSrcStr = string.Empty;
                    string RepYYMMStr = "";
                    string RepYYMMDDStr = "";
                    string RepDOCUniqueIdStr = string.Empty;
                    string JSONFileNameStr = "fatt.nsf";
                    string RepModeStr = "";
                    int RepID = 0;

                    string RootFolderStr = string.Empty;
                    string ListNameStr = string.Empty;
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    class_nmReplicationJSONFull AckFromShipReplicationJSONFULL = new class_nmReplicationJSONFull();
                    AckFromShipReplicationJSONFULL.entries = new List<class_nmReplicationJSONFile>();
                    class_nmReplicationJSONFile JSONFile = new class_nmReplicationJSONFile();
                    List<class_UploadFile> UploadFileInfos = new List<class_UploadFile>();
                    class_UploadFile UploadFileInfo = new class_UploadFile();

                    // Create variables for Ship Document Library Infos
                    string nmstrIDPath = string.Empty;
                    string nmstrVirtual_LocalRelativePath = string.Empty;
                    string nmstrLinkFilename = string.Empty;
                    int nmRepID = -1;
                    string nmStatus = string.Empty;
                    int nmFSObjType = -1;
                    string nmServerUrl = string.Empty;
                    int nmUniqueId = -1;
                    int nmParentUniqueId = -1;
                    string nmDocumentLibrary = string.Empty;

                    bool ReadyForACK = false;
                    bool ProceedForACK = false;
                    bool IsRemoteDelete = false;
                    string folPath = string.Empty;
                    int ACKFromShipReplicationThisReplicationCount = 0;
                    string StrFileFolderExistenceCheck = string.Empty;
                    string StrFileFolderChecking = string.Empty;
                    string StrFileFolderVirtual_LocalRelativePath = string.Empty;
                    string StrFileFolderIDPath = string.Empty;
                    //string StrFolIDPath = string.Empty;
                    int MaxNSInt = 100;
                    //List<class_sharepointlist> IMPSPListInfos = new List<class_sharepointlist>();
                    List<string> ShipReplicationFolder = new List<string>();
                    List<string> ACKFromShipReplicationStatusList = new List<string>();
                    string ErrorStr = string.Empty;

                    //List<int> AckIMOInt = new List<int>();

                    //string strConnString = ConfigurationManager.ConnectionStrings["nmConnectionString"].ConnectionString;

                    string strConnString = string.Empty;
                    SqlConnection con = new SqlConnection();

                    /*
                    string strConnString = "Data Source=" + UATGlobals.UATSQLServerIP +
                    ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                    "User id=sa;" +
                    "Password=" + UATGlobals.UATSQLServerPassword + ";";

                    SqlConnection con = new SqlConnection(strConnString);

                    using (SqlCommand cmd = new SqlCommand("spGetRepOutQueuePath", con))
                    {
                        con.Open();

                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Read();

                        NMPathStr = NMPathStr + dr[0].ToString().Substring(dr[0].ToString().Split('\\').First().Length, dr[0].ToString().Length - dr[0].ToString().Split('\\').First().Length);
                        NMPathStr = NMPathStr.Substring(0, NMPathStr.Length - 1);
                        NMPathStr = NMPathStr.Substring(0, NMPathStr.Length - NMPathStr.Split('\\').Last().Length);

                        NMOutPathStr = NMPathStr + @"OutboundQueue\";
                        NMInPathStr = NMPathStr + @"InboundQueue\";
                        NMOutArchivePathStr = NMPathStr + @"Archive\Outbound\";

                        con.Close();

                    }
                    */

                    // inbound queue folder path for the ship (IMO) in current loop
                    string FromShipReplicationIMOPath = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString();

                    // create inbound queue folder path for the ship (IMO) in current loop if not exists
                    if (!Directory.Exists(FromShipReplicationIMOPath))
                        Directory.CreateDirectory(FromShipReplicationIMOPath);

                    // Check for existing folders (format: DMSSRYYMMXXX9732589YYMMDD_) and loop through them
                    foreach (string NSINFolders in Directory.GetDirectories(FromShipReplicationIMOPath, "*_", SearchOption.TopDirectoryOnly))
                    {
                        DirectoryInfo di = new DirectoryInfo(NSINFolders);
                        string TempDateStr = DateTime.Now.ToString("yyyyMMddmmss");

                        //if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last()))
                        //    System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last());

                        string AppRefNSINFolder = Path.GetDirectoryName(NSINFolders) + @"\_AppReference\" + Path.GetFileName(NSINFolders);

                        /*
                        if (!System.IO.Directory.Exists(NMOutArchivePathStr + NMOutFolders.Split('\\').Last()))
                            System.IO.Directory.CreateDirectory(NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr);
                        */

                        foreach (string NSINFiles in Directory.GetFiles(NSINFolders))
                        {
                            string TempStr = NSINFiles;

                            //if (System.IO.File.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last()))
                            //    System.IO.File.Delete(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last());

                            string AppRefNSINFile = AppRefNSINFolder + @"\" + Path.GetFileName(NSINFiles);

                            if (System.IO.File.Exists(AppRefNSINFile))
                                System.IO.File.Delete(AppRefNSINFile);

                            /*
                            if (System.IO.File.Exists(NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr + @"\" + NMOutFiles.Split('\\').Last()))
                                System.IO.File.Delete(NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr + @"\" + NMOutFiles.Split('\\').Last());
                            */

                            //if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last()))
                            //    System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last());

                            if (!Directory.Exists(AppRefNSINFolder))
                                Directory.CreateDirectory(AppRefNSINFolder);

                            // System.IO.File.Copy(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last(), EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last());
                            System.IO.File.Copy(NSINFiles, AppRefNSINFile);

                            /*
                            System.IO.File.Copy(NMOutPathStr + NMOutFolders.Split('\\').Last() + @"\" + NMOutFiles.Split('\\').Last(), NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr + @"\" + NMOutFiles.Split('\\').Last());
                            */

                        }

                        //di.Delete(true);
                        DeleteDirectory(NSINFolders);

                    }

                    // End of Configuration of UAT Ship Servers

                    // if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference"))
                    if (!System.IO.Directory.Exists(FromShipReplicationIMOPath + @"\_AppReference"))
                        continue;

                    // if (System.IO.Directory.GetDirectories(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference").Count() == 0)
                    if (System.IO.Directory.GetDirectories(FromShipReplicationIMOPath + @"\_AppReference").Count() == 0)
                        continue;

                    FromShipReplicationIMOPath = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference";

                    foreach (string StrIMOName in Directory.GetDirectories(FromShipReplicationIMOPath))
                    {
                        string TempStr = string.Empty;

                        if (StrIMOName.IndexOf('/') >= 0)
                            TempStr = StrIMOName.Split('/').Last();
                        else if (StrIMOName.IndexOf('\\') >= 0)
                            TempStr = StrIMOName.Split('\\').Last();

                        if ((TempStr.Substring(12, 7) == SingleShipIMOInt.ToString()) || MultipleShipTest)
                        {
                            /*
                            string FromShipReplicationFolderPath = StrIMOName + "\\_AppReference";
                            if (!System.IO.Directory.Exists(FromShipReplicationFolderPath))
                                System.IO.Directory.CreateDirectory(FromShipReplicationFolderPath);

                            foreach (string StrFolderName in Directory.GetDirectories(FromShipReplicationFolderPath))
                            {
                                if (System.IO.File.Exists(StrFolderName + "\\" + JSONFileNameStr))
                                    ShipReplicationFolder.Add(StrFolderName.Split('\\').Last());

                            }
                            */

                            if (System.IO.File.Exists(StrIMOName + "\\" + JSONFileNameStr))
                                ShipReplicationFolder.Add(StrIMOName.Split('\\').Last());

                        }

                    }

                    //ShipReplicationFolder.Add("DMSSR17090079732589170904_");

                    ACKFromShipReplicationThisReplicationCount = 0;

                    foreach (string ShipReplicationFolders in ShipReplicationFolder)
                    {
                        ACKFromShipReplicationThisReplicationCount = ACKFromShipReplicationCount;

                        string ShipReplicationFolderNameStr = string.Empty;

                        if (ShipReplicationFolders.IndexOf('/') >= 0)
                            ShipReplicationFolderNameStr = ShipReplicationFolders.Split('/').Last();
                        else
                            ShipReplicationFolderNameStr = ShipReplicationFolders;

                        RepModeStr = ShipReplicationFolderNameStr.Substring(0, 5);
                        RepYYMMStr = ShipReplicationFolderNameStr.Substring(5, 4);
                        RepID = Convert.ToInt32(ShipReplicationFolderNameStr.Substring(9, 3));
                        RepRefStr = RepModeStr + RepYYMMStr + ConvertIntToString(RepID, 3);
                        //AckIMOInt.Add(Convert.ToInt32(ShipReplicationFolders.Substring(12,7)));
                        AckIMO = Convert.ToInt32(ShipReplicationFolderNameStr.Substring(12, 7));
                        RepYYMMDDStr = ShipReplicationFolderNameStr.Substring(19, 6);
                        //} // End of foreach (string ShipReplicationFolders in ShipReplicationFolder)

                        //foreach (int AckIMO in AckIMOInt)
                        //{
                        List<class_sharepointlist> TempsIMPSPListInfos = new List<class_sharepointlist>();
                        TempsIMPSPListInfos = SearchVesselSPInfos(AckIMO);

                        foreach (class_sharepointlist TempsIMPSPList in TempsIMPSPListInfos)
                        {
                            for (int j = 1; j < (MaxNSInt + 1); j++)
                            {
                                if (TempsIMPSPList.strBaseFileName == "NS" + ConvertIntToString(j, 4) + "-" + AckIMO.ToString())
                                {
                                    RootFolderStr = "/" + "NS" + ConvertIntToString(j, 4) + "-" + AckIMO.ToString();
                                    ListNameStr = TempsIMPSPList.strFileDirRef.Split('/').Last();
                                    //IMPSPListInfos.Add(TempsIMPSPList);
                                    break;
                                }
                                else
                                    continue;

                            }

                        }

                        ////
                        Console.WriteLine("ReplicationFolder: " + ShipReplicationFolderNameStr + " - " + ListNameStr);

                        if (ListNameStr != string.Empty)
                        {

                            ProceedForACK = false;

                            RepDirectoryStr = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + "\\_AppReference\\" + RepModeStr + RepYYMMStr + ConvertIntToString(RepID, 3) + AckIMO.ToString() + RepYYMMDDStr + "_";

                            if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                            {
                                System.IO.StreamReader myFile = new System.IO.StreamReader(RepDirectoryStr + "\\" + JSONFileNameStr);
                                string myString = string.Empty;

                                while ((myString = myFile.ReadLine()) != null)
                                {
                                    class_nmReplicationJSONFull ReplicationJSONFileFromFile = new class_nmReplicationJSONFull();
                                    ReplicationJSONFileFromFile = js.Deserialize<class_nmReplicationJSONFull>(myString);

                                    foreach (class_nmReplicationJSONFile JSONFiles in ReplicationJSONFileFromFile.entries)
                                    {
                                        // Only File will be proceesed
                                        if (JSONFiles.RepSPDocsInfo.intFSObjType == 0)
                                        {

                                            AckFromShipReplicationJSONFULL.RepHeader = ReplicationJSONFileFromFile.RepHeader;
                                            //AckFromShipReplicationJSONFULL.RepHeader.RepIMO = AckIMO;
                                            //JSONFiles.RepDistributionStatus = "RC";

                                            nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                            nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                            nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                            nmRepID = JSONFiles.RepID;
                                            nmStatus = JSONFiles.RepDistributionStatus;
                                            nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                            nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                            nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                            nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                            nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                            //Enquiry from SQL
                                            //strConnString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

                                            strConnString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
                                            con = new SqlConnection(strConnString);

                                            SqlCommand cmd1 = new SqlCommand();
                                            SqlCommand cmd2 = new SqlCommand();

                                            cmd1.CommandType = CommandType.StoredProcedure;
                                            cmd2.CommandType = CommandType.StoredProcedure;

                                            switch (JSONFiles.UpdateType)
                                            {

                                                case "RemoteDelete":
                                                    cmd1.CommandText = "sprepAckShipToShoreRDCheck";
                                                    cmd2.CommandText = "sprepAckShipToShoreRDAdd";
                                                    IsRemoteDelete = true;
                                                    break;
                                                default:
                                                    cmd1.CommandText = "sprepAckShipToShoreCheck";
                                                    cmd2.CommandText = "sprepAckShipToShoreAdd";
                                                    IsRemoteDelete = false;
                                                    break;

                                            }

                                            cmd1.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                            cmd1.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                            cmd1.Connection = con;

                                            cmd2.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                            cmd2.Parameters.Add("@pnmRepRef", SqlDbType.VarChar).Value = RepRefStr;
                                            cmd2.Parameters.Add("@pnmRepYyMmDd", SqlDbType.Int).Value = Convert.ToInt32(RepYYMMDDStr);
                                            cmd2.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                            cmd2.Parameters.Add("@pnmStatus", SqlDbType.VarChar).Value = nmStatus;
                                            cmd2.Parameters.Add("@pnmFSObjType", SqlDbType.Int).Value = nmFSObjType;
                                            cmd2.Parameters.Add("@pnmServerUrl", SqlDbType.VarChar).Value = nmServerUrl;
                                            cmd2.Parameters.Add("@pnmUniqueId", SqlDbType.Int).Value = nmUniqueId;
                                            cmd2.Parameters.Add("@pnmParentUniqueId", SqlDbType.Int).Value = nmParentUniqueId;
                                            cmd2.Parameters.Add("@pnmVirtual_LocalRelativePath", SqlDbType.VarChar).Value = nmstrIDPath;
                                            cmd2.Parameters.Add("@pnmDocumentLibrary", SqlDbType.VarChar).Value = nmDocumentLibrary;
                                            cmd2.Connection = con;

                                            try
                                            {
                                                con.Open();
                                                SqlDataReader dr = cmd1.ExecuteReader();
                                                int i = -1;

                                                while (dr.Read())
                                                {
                                                    if (dr.IsDBNull(0))
                                                        ACKFromShipReplicationStatus = "Error in SQL Enquiry! Please contact System Administrator!";
                                                    else
                                                        i = int.Parse(dr[0].ToString());

                                                }
                                                //Console.WriteLine("Record enquiried successfully");

                                                if (i == 0)
                                                {
                                                    con.Close();
                                                    ACKFromShipReplicationStatus = "Ready To Insert Replication";
                                                    con.Open();
                                                    cmd2.ExecuteNonQuery();
                                                    ProceedForACK = true;

                                                }
                                                else if (i == -1)
                                                {
                                                    ACKFromShipReplicationStatus = ResendAcknowledgement(AckIMO, nmRepID, "AC");
                                                    ProceedForACK = false;
                                                    ErrorStr = "Resent";
                                                    ACKFromShipReplicationCount++;
                                                }
                                                else
                                                {
                                                    ProceedForACK = false;
                                                    ErrorStr = "err03";

                                                }


                                            }
                                            catch (System.Exception ex)
                                            {
                                                //throw ex;
                                            }
                                            finally
                                            {
                                                con.Close();
                                                con.Dispose();
                                            }

                                            if (ProceedForACK)
                                            {
                                                JSONFile = JSONFiles;
                                                AckFromShipReplicationJSONFULL.entries.Add(JSONFiles);
                                            }
                                        }   // "Folder" entry
                                        else
                                        {

                                            ProceedForACK = false;
                                            ErrorStr = "err04";

                                        }

                                    }
                                }

                                myFile.Close();
                                myFile.Dispose();
                                //Thread.Sleep(5000);

                                /*if (ProceedForACK && System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                    System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);
                                */

                            }
                            else
                            {
                                Thread.Sleep(5000);

                                if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                {
                                    System.IO.StreamReader myFile = new System.IO.StreamReader(RepDirectoryStr + "\\" + JSONFileNameStr);
                                    string myString = string.Empty;

                                    while ((myString = myFile.ReadLine()) != null)
                                    {
                                        class_nmReplicationJSONFull ReplicationJSONFileFromFile = new class_nmReplicationJSONFull();
                                        ReplicationJSONFileFromFile = js.Deserialize<class_nmReplicationJSONFull>(myString);

                                        foreach (class_nmReplicationJSONFile JSONFiles in ReplicationJSONFileFromFile.entries)
                                        {
                                            // Only File will be proceesed
                                            if (JSONFiles.RepSPDocsInfo.intFSObjType == 0)
                                            {

                                                AckFromShipReplicationJSONFULL.RepHeader = ReplicationJSONFileFromFile.RepHeader;
                                                //AckFromShipReplicationJSONFULL.RepHeader.RepIMO = AckIMO;
                                                //JSONFiles.RepDistributionStatus = "RC";

                                                nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                                nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                                nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                                nmRepID = JSONFiles.RepID;
                                                nmStatus = JSONFiles.RepDistributionStatus;
                                                nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                                nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                                nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                                nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                                nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                                //Enquiry from SQL
                                                //strConnString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

                                                strConnString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
                                                con = new SqlConnection(strConnString);

                                                SqlCommand cmd1 = new SqlCommand();
                                                SqlCommand cmd2 = new SqlCommand();

                                                cmd1.CommandType = CommandType.StoredProcedure;
                                                cmd2.CommandType = CommandType.StoredProcedure;

                                                cmd1.CommandText = "sprepAckShipToShoreCheck";
                                                cmd2.CommandText = "sprepAckShipToShoreAdd";

                                                cmd1.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                                cmd1.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                                cmd1.Connection = con;

                                                cmd2.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                                cmd2.Parameters.Add("@pnmRepRef", SqlDbType.VarChar).Value = RepRefStr;
                                                cmd2.Parameters.Add("@pnmRepYyMmDd", SqlDbType.Int).Value = Convert.ToInt32(RepYYMMDDStr);
                                                cmd2.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                                cmd2.Parameters.Add("@pnmStatus", SqlDbType.VarChar).Value = nmStatus;
                                                cmd2.Parameters.Add("@pnmFSObjType", SqlDbType.Int).Value = nmFSObjType;
                                                cmd2.Parameters.Add("@pnmServerUrl", SqlDbType.VarChar).Value = nmServerUrl;
                                                cmd2.Parameters.Add("@pnmUniqueId", SqlDbType.Int).Value = nmUniqueId;
                                                cmd2.Parameters.Add("@pnmParentUniqueId", SqlDbType.Int).Value = nmParentUniqueId;
                                                cmd2.Parameters.Add("@pnmVirtual_LocalRelativePath", SqlDbType.VarChar).Value = nmstrIDPath;
                                                cmd2.Parameters.Add("@pnmDocumentLibrary", SqlDbType.VarChar).Value = nmDocumentLibrary;
                                                cmd2.Connection = con;

                                                try
                                                {
                                                    con.Open();
                                                    SqlDataReader dr = cmd1.ExecuteReader();
                                                    int i = -1;

                                                    while (dr.Read())
                                                    {
                                                        if (dr.IsDBNull(0))
                                                            ACKFromShipReplicationStatus = "Error in SQL Enquiry! Please contact System Administrator!";
                                                        else
                                                            i = int.Parse(dr[0].ToString());

                                                    }
                                                    //Console.WriteLine("Record enquiried successfully");

                                                    if (i == 0)
                                                    {
                                                        con.Close();
                                                        ACKFromShipReplicationStatus = "Ready To Insert Replication";
                                                        con.Open();
                                                        cmd2.ExecuteNonQuery();
                                                        ProceedForACK = true;

                                                    }
                                                    else if (i == -1)
                                                    {
                                                        ACKFromShipReplicationStatus = ResendAcknowledgement(AckIMO, nmRepID, "AC");
                                                        ProceedForACK = false;
                                                        ErrorStr = "Resent";
                                                        ACKFromShipReplicationCount++;
                                                    }
                                                    // Existing File replicated from Ship
                                                    else
                                                    {
                                                        ProceedForACK = false;
                                                        ErrorStr = "err03";

                                                    }


                                                }
                                                catch (System.Exception ex)
                                                {
                                                    //throw ex;
                                                }
                                                finally
                                                {
                                                    con.Close();
                                                    con.Dispose();
                                                }

                                                if (ProceedForACK)
                                                {
                                                    JSONFile = JSONFiles;
                                                    AckFromShipReplicationJSONFULL.entries.Add(JSONFiles);
                                                }
                                            }   // "Folder" entry
                                            else
                                            {

                                                ProceedForACK = false;
                                                ErrorStr = "err04";

                                            }

                                        }
                                    }

                                    myFile.Close();
                                    myFile.Dispose();
                                    //Thread.Sleep(5000);

                                    try
                                    {
                                        if (ProceedForACK && System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                    }
                                    catch (System.IO.IOException Ex)
                                    {
                                        if (ProceedForACK && System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                        {
                                            //Load your file using FileStream class and release an file through stream.Dispose()
                                            using (FileStream stream = new FileStream(RepDirectoryStr + "\\" + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                                            {
                                                stream.Close();
                                                stream.Dispose();
                                            }

                                            // delete the file.
                                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                        }

                                    }

                                }
                                else
                                {
                                    ACKFromShipReplicationStatus = JSONFileNameStr + " does NOT exist in this Replication Request From Ship(IMO) = " + AckIMO + " with Replication Reference = " + RepRefStr + " on Date = " + RepYYMMDDStr;
                                    ProceedForACK = false;

                                    ErrorStr = "err02";

                                }


                            }

                        }  // End of if (ListNameStr != "")
                        else
                        {
                            // Invalid Ship Information
                            ErrorStr = "err01";

                        }

                        if (ProceedForACK)
                        {
                            bool VirtualPathUpdateDone = false;
                            bool FileNameUpdateDone = false;
                            string CurrentUIDStr = string.Empty;

                            foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)
                            {
                                if (JSONFiles.UpdateType == "RemoteDelete")
                                    continue;

                                VirtualPathUpdateDone = false;
                                FileNameUpdateDone = false;

                                nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                nmRepID = JSONFiles.RepID;
                                nmStatus = JSONFiles.RepDistributionStatus;
                                nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                //StrFolIDPath = nmstrIDPath;
                                CurrentUIDStr = JSONFiles.RepSPDocsInfo.strUniqueId;

                                // Check whethere FileRenamed Updated
                                string OldFileNameStr = GetSPFileInfosByUniqueId(CurrentUIDStr, "spBaseName");

                                if ((OldFileNameStr != nmstrLinkFilename) && (OldFileNameStr != string.Empty))
                                    FileNameUpdateDone = true;

                                string TempnmstrIDPath = nmstrIDPath;
                                string TempnmstrVirtual_LocalRelativePath = nmstrVirtual_LocalRelativePath;

                                // Check whethere Path Updated
                                if (nmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                {
                                    string NewFolderNameStr = nmstrVirtual_LocalRelativePath.Split('/').Last();

                                    //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath;
                                    folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath;

                                    if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                    {
                                        do
                                        {
                                            if (TempnmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                            {
                                                NewFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;
                                                folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;

                                                if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                                {
                                                    DateTime dttspModified = GetSPFolderLMDByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    // Convert to HK Time
                                                    // string StrspModified = dttspModified.AddHours(8).ToString();

                                                    if (JSONFiles.RepSPDocsInfo.strLastModified != string.Empty)
                                                    {

                                                        //BoscoLastNight
                                                        if (dttspModified.AddHours(8).AddSeconds(-dttspModified.Second) <= Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).AddSeconds(-Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).Second))
                                                        {
                                                            string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                            //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                            folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                            if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                                VirtualPathUpdateDone = true;

                                                        }
                                                        else
                                                        {
                                                            string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                            //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                            folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                            if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                                VirtualPathUpdateDone = true;

                                                        }

                                                    }
                                                    else
                                                    {
                                                        string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                        folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                        if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                            VirtualPathUpdateDone = true;

                                                    }

                                                }
                                            }

                                            TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                            TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                        } while (TempnmstrIDPath.Split('/').Count() > 2);

                                    }
                                    else
                                    {
                                        TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                        TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                        do
                                        {
                                            if (TempnmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                            {
                                                NewFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;
                                                folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;

                                                if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                                {
                                                    DateTime dttspModified = GetSPFolderLMDByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    // Convert to HK Time
                                                    // string StrspModified = dttspModified.AddHours(8).ToString();

                                                    if (JSONFiles.RepSPDocsInfo.strLastModified == string.Empty)
                                                    {
                                                        //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                        string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                        folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                        if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                            VirtualPathUpdateDone = true;

                                                    }
                                                    else //if (dttspModified.AddHours(8) < Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified))
                                                    if (dttspModified.AddHours(8).AddSeconds(-dttspModified.Second) <= Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).AddSeconds(-Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).Second))
                                                    {
                                                        //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                        string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                        folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                        if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                            VirtualPathUpdateDone = true;

                                                    }

                                                }
                                            }

                                            TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                            TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                        } while (TempnmstrIDPath.Split('/').Count() > 2);

                                    }

                                }
                                else
                                {
                                    TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                    TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                    do
                                    {
                                        if (TempnmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                        {
                                            string NewFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                            //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;
                                            folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;

                                            if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                            {
                                                DateTime dttspModified = GetSPFolderLMDByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                // Convert to HK Time
                                                // string StrspModified = dttspModified.AddHours(8).ToString();

                                                if (JSONFiles.RepSPDocsInfo.strLastModified == string.Empty)
                                                {
                                                    //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                    string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                    folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                    ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                    if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                        VirtualPathUpdateDone = true;

                                                }
                                                else// if (dttspModified.AddHours(8) < Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified))
                                                if (dttspModified.AddHours(8).AddSeconds(-dttspModified.Second) <= Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).AddSeconds(-Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).Second))
                                                {
                                                    //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                    string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                    folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                    ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                    if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                        VirtualPathUpdateDone = true;

                                                }

                                            }
                                        }

                                        TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                        TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                    } while (TempnmstrIDPath.Split('/').Count() > 2);

                                }


                                if (FileNameUpdateDone)
                                    //ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Update", "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + OldFileNameStr, ListNameStr, JSONFiles.RepSPDocsInfo.strBaseName, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved");
                                    ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Update", "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + OldFileNameStr, ListNameStr, JSONFiles.RepSPDocsInfo.strBaseName, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                // "File"
                                if (nmFSObjType == 0)
                                {
                                    //public class_replicationlist GetUpdatedSPDocInfos(string folPath, string SPUniquedIDStr, string DocumentType, string UpdateType)

                                    ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Check", folPath, ListNameStr, nmstrLinkFilename, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0];

                                    if (ACKFromShipReplicationStatus == "File NOT exists.")
                                    {
                                        CurrentUIDStr = nmstrIDPath;
                                        ReadyForACK = false;

                                        do
                                        {

                                            if (StrFileFolderVirtual_LocalRelativePath != string.Empty)
                                            {
                                                StrFileFolderVirtual_LocalRelativePath = folPath.Split('/').Last() + "/" + StrFileFolderVirtual_LocalRelativePath;
                                                CurrentUIDStr = StrFileFolderIDPath.Split('/').Last();
                                                StrFileFolderIDPath = StrFileFolderIDPath.Replace("/" + CurrentUIDStr, "");
                                            }
                                            else
                                            {
                                                StrFileFolderVirtual_LocalRelativePath = folPath.Split('/').Last();
                                                StrFileFolderIDPath = CurrentUIDStr;
                                            }
                                            StrFileFolderChecking = folPath.Split('/').Last();
                                            if (StrFileFolderChecking == nmstrLinkFilename)
                                                StrFileFolderExistenceCheck = "File";
                                            else
                                                StrFileFolderExistenceCheck = "Folder";
                                            folPath = folPath.Substring(0, folPath.Length - StrFileFolderChecking.Length - 1);

                                            //if (StrFileFolderExistenceCheck == "Folder")
                                            //StrFolIDPath = StrFolIDPath.Substring(0, StrFolIDPath.Length - StrFileFolderChecking.Length - 1);

                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0];
                                            if (ACKFromShipReplicationStatus != "Folder NOT exists.")
                                            {
                                                do
                                                {
                                                    switch (StrFileFolderExistenceCheck)
                                                    {
                                                        case "File":

                                                            //public string SPUploadMultipleFile(List<class_UploadFile> UploadFileInfos, string RepModCodeStr, bool SingleFileUpload)
                                                            UploadFileInfos = new List<class_UploadFile>();
                                                            UploadFileInfo = new class_UploadFile();

                                                            UploadFileInfo.SourceUrl = nmstrLinkFilename;
                                                            //UploadFileInfo.DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                            UploadFileInfo.DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                            UploadFileInfos.Add(UploadFileInfo);

                                                            if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl))
                                                                System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl);

                                                            //Thread.Sleep(5000);

                                                            if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                            else
                                                            {
                                                                Thread.Sleep(5000);
                                                                if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                    //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                    System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                else
                                                                {
                                                                    ACKFromShipReplicationStatus = "Error in proccessing the file '" + JSONFileNameStr + "' in this Replication Request From Ship(IMO) = " + AckIMO + " with Replication Reference = " + RepRefStr + " on Date = " + RepYYMMDDStr;
                                                                    return ACKFromShipReplicationStatus;
                                                                }

                                                            }

                                                            ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true, 1);
                                                            //ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true);

                                                            //if (ACKFromShipReplicationStatus != "1 file has been uploaded successfully!")
                                                            if (ACKFromShipReplicationStatus != "File '" + UploadFileInfos[0].SourceUrl + "' has been uploaded for approval!")
                                                            {
                                                                if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl))
                                                                    System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                            }

                                                            break;

                                                        case "Folder":

                                                            do
                                                            {
                                                                if (CurrentUIDStr.IndexOf('-') > 0)
                                                                {
                                                                    ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", "http://dms.angloeastern.com" + GetSPFolderInfoByUniqueId(CurrentUIDStr).strFileDirRef, ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                                    folPath = folPath + "/" + StrFileFolderChecking;

                                                                }
                                                                else
                                                                {
                                                                    if (SBFSPFolderCheck(AckIMO, Convert.ToInt32(CurrentUIDStr)) != string.Empty)
                                                                    {
                                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath + "/" + SBFSPFolderCheck(AckIMO, Convert.ToInt32(CurrentUIDStr)), ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                                        folPath = folPath + "/" + StrFileFolderChecking;

                                                                    }
                                                                    else
                                                                    {
                                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "New", folPath, ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                                        folPath = folPath + "/" + StrFileFolderChecking;

                                                                        SBFSPFolderAdd(AckIMO, Convert.ToInt32(CurrentUIDStr), GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")), "Folder", "Existing").strspUniqueId);

                                                                    }

                                                                }

                                                                StrFileFolderChecking = StrFileFolderVirtual_LocalRelativePath.Split('/').First();
                                                                StrFileFolderVirtual_LocalRelativePath = StrFileFolderVirtual_LocalRelativePath.Replace(StrFileFolderChecking + "/", "");
                                                                if (StrFileFolderVirtual_LocalRelativePath.IndexOf('/') >= 0)
                                                                {
                                                                    StrFileFolderChecking = StrFileFolderVirtual_LocalRelativePath.Split('/').First();
                                                                    //StrFileFolderVirtual_LocalRelativePath = StrFileFolderVirtual_LocalRelativePath.Replace(StrFileFolderChecking + "/", "");

                                                                    ReadyForACK = false;

                                                                }
                                                                else
                                                                {
                                                                    UploadFileInfos = new List<class_UploadFile>();
                                                                    UploadFileInfo = new class_UploadFile();

                                                                    UploadFileInfo.SourceUrl = nmstrLinkFilename;
                                                                    //UploadFileInfo.DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                                    UploadFileInfo.DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                                    UploadFileInfos.Add(UploadFileInfo);

                                                                    if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl))
                                                                        System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl);

                                                                    //Thread.Sleep(5000);

                                                                    if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                        //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, Server.MapPath("~/App_Data/Temp") + "\\" + UploadFileInfo.SourceUrl);
                                                                        System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                    else
                                                                    {
                                                                        Thread.Sleep(5000);
                                                                        if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                            //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, Server.MapPath("~/App_Data/Temp") + "\\" + UploadFileInfo.SourceUrl);
                                                                            System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                        else
                                                                        {
                                                                            ACKFromShipReplicationStatus = "Error in proccessing the file '" + JSONFileNameStr + "' in this Replication Request From Ship(IMO) = " + AckIMO + " with Replication Reference = " + RepRefStr + " on Date = " + RepYYMMDDStr;
                                                                            return ACKFromShipReplicationStatus;
                                                                        }

                                                                    }

                                                                    ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true, 1);
                                                                    //ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true);

                                                                    //if (ACKFromShipReplicationStatus != "1 file has been uploaded successfully!")
                                                                    if (ACKFromShipReplicationStatus != "File '" + UploadFileInfos[0].SourceUrl.Split('\\').Last() + "' has been uploaded for approval!")
                                                                    {
                                                                        if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl.Split('\\').Last()))
                                                                            System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl.Split('\\').Last());
                                                                    }

                                                                    ReadyForACK = true;

                                                                }

                                                            } while (!ReadyForACK);

                                                            break;
                                                        default:
                                                            break;
                                                    }

                                                } //while (folPath.Replace(@"http://oneview.angloeastern.com/sites/oneview/nsdl/", "") != (ListNameStr + nmstrVirtual_LocalRelativePath));
                                                while (folPath.Replace(@"http://dms.angloeastern.com/sites/nsdl/", "") != (ListNameStr + nmstrVirtual_LocalRelativePath));

                                                ReadyForACK = true;
                                                ACKFromShipReplicationCount++;

                                            }
                                            else
                                                ReadyForACK = false;

                                        } while (!ReadyForACK);

                                        int[] IMOArray = new int[] { AckIMO };

                                        ACKFromShipReplicationStatus = CreateReplication(UploadFileInfo.DestinationUrl, RepModeStr, IMOArray);

                                        if (ACKFromShipReplicationStatus == "\r\nReplication Acknowledgement to Ship has been created successfully!!")
                                        {

                                            //int nmRepStatusCount = UpdatenmRepStatus(nmRepID);

                                            //if (nmRepStatusCount == 1)
                                            ACKFromShipReplicationStatus = "RC";    //When Replication Engine confirms sent out of ACK then Replication Engine will update the Replication Status = AC
                                            ACKFromShipReplicationCount++;
                                            //else
                                            // ACKFromShipReplicationStatus = "NA";

                                        }
                                        else
                                            ACKFromShipReplicationStatus = "NA";

                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                    }
                                    else
                                    {
                                        // ACK for File exists in Ship Filing
                                        //if (VirtualPathUpdateDone || FileNameUpdateDone)
                                        //{
                                        UploadFileInfos = new List<class_UploadFile>();
                                        UploadFileInfo = new class_UploadFile();

                                        UploadFileInfo.SourceUrl = nmstrLinkFilename;
                                        //UploadFileInfo.DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                        UploadFileInfo.DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                        UploadFileInfos.Add(UploadFileInfo);

                                        // Still Acknowledge if any update performed during VirtualPathCheck
                                        int[] IMOArray = new int[] { AckIMO };

                                        ACKFromShipReplicationStatus = CreateReplication(UploadFileInfo.DestinationUrl, RepModeStr, IMOArray);

                                        if (ACKFromShipReplicationStatus == "\r\nReplication Acknowledgement to Ship has been created successfully!!")
                                        {

                                            //int nmRepStatusCount = UpdatenmRepStatus(nmRepID);

                                            //if (nmRepStatusCount == 1)
                                            //{
                                            ACKFromShipReplicationStatus = "RC";    //When Replication Engine confirms sent out of ACK then Replication Engine will update the Replication Status = AC
                                            ACKFromShipReplicationCount++;
                                            //}
                                            //else
                                            //ACKFromShipReplicationStatus = "NA";

                                        }
                                        else
                                            ACKFromShipReplicationStatus = "NA";

                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                        /*}
                                        else
                                        {
                                            // Update Acknowledgement Status = NA (File exists in Ship Filing)
                                            ACKFromShipReplicationStatus = "NA";
                                            ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                        }*/

                                    }

                                }
                                else // "Folder"
                                {
                                    ErrorStr = "err04";
                                    // Update Acknowledgement Status = NA (Folder exists in JSON)
                                    ACKFromShipReplicationStatus = "NA";
                                    ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                }

                                StrFileFolderVirtual_LocalRelativePath = string.Empty;

                            }   // End of foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)

                            foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)
                            {
                                if (JSONFiles.UpdateType != "RemoteDelete")
                                    continue;

                                nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                nmRepID = JSONFiles.RepID;
                                nmStatus = JSONFiles.RepDistributionStatus;
                                nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                // Bosco - 3May2018 (Rename Folder before RD)

                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                //folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                if (nmstrIDPath.IndexOf('-') > 0)
                                {

                                    bool SPFolderLocated = false;
                                    string TempnmstrIDPath = nmstrIDPath;
                                    string TempnmstrVirtual_LocalRelativePath = nmstrVirtual_LocalRelativePath;
                                    string TempNewFolderPath = string.Empty;

                                    do
                                    {
                                        if (TempnmstrIDPath.Split('/').Last().IndexOf('-') > 0)
                                        {
                                            if (TempNewFolderPath != string.Empty)
                                                folPath = GetSPFolderInfoByUniqueId(nmstrIDPath.Split('/').Last()).strFileDirRef + "/" + TempNewFolderPath + "/" + nmstrLinkFilename;
                                            else
                                                folPath = GetSPFolderInfoByUniqueId(nmstrIDPath.Split('/').Last()).strFileDirRef + "/" + nmstrLinkFilename;

                                            if (folPath != ("/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename))
                                            {
                                                TempNewFolderPath = "/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                TempnmstrVirtual_LocalRelativePath = folPath;

                                                string NewFolderNameStr = string.Empty;
                                                string OldFolderNameStr = string.Empty;

                                                do
                                                {
                                                    TempNewFolderPath = TempNewFolderPath.Substring(0, TempNewFolderPath.LastIndexOf('/'));
                                                    TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.LastIndexOf('/'));

                                                    if (TempNewFolderPath != TempnmstrVirtual_LocalRelativePath)
                                                    {
                                                        NewFolderNameStr = TempNewFolderPath.Split('/').Last();
                                                        OldFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                        if (NewFolderNameStr != OldFolderNameStr)
                                                        {
                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", "http://dms.angloeastern.com" + TempnmstrVirtual_LocalRelativePath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];



                                                        }

                                                    }
                                                    else
                                                        SPFolderLocated = true;

                                                } while (!SPFolderLocated);

                                                folPath = "/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                            }
                                            else
                                            {
                                                //TempNewFolderPath = folPath.Substring(0, folPath.LastIndexOf('/'));
                                                TempNewFolderPath = folPath.Replace("/sites/nsdl/" + ListNameStr, "");
                                                TempnmstrVirtual_LocalRelativePath = nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                                string NewFolderNameStr = string.Empty;
                                                string OldFolderNameStr = string.Empty;

                                                do
                                                {
                                                    TempNewFolderPath = TempNewFolderPath.Substring(0, TempNewFolderPath.LastIndexOf('/'));
                                                    TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.LastIndexOf('/'));

                                                    if (TempNewFolderPath != TempnmstrVirtual_LocalRelativePath)
                                                    {
                                                        NewFolderNameStr = TempNewFolderPath.Split('/').Last();
                                                        OldFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                        if (NewFolderNameStr != OldFolderNameStr)
                                                        {
                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", "http://dms.angloeastern.com" + TempnmstrVirtual_LocalRelativePath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];



                                                        }

                                                    }
                                                    else
                                                        SPFolderLocated = true;

                                                } while (!SPFolderLocated);

                                                folPath = "/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                            }

                                        }
                                        else
                                        {
                                            TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.LastIndexOf('/'));
                                            TempNewFolderPath = TempnmstrVirtual_LocalRelativePath.Split('/').Last();
                                            TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrIDPath.LastIndexOf('/'));

                                        }

                                    } while (!SPFolderLocated);

                                }

                                // "File"
                                if (nmFSObjType == 0)
                                {
                                    ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Check", folPath, ListNameStr, nmstrLinkFilename, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0];

                                    if (ACKFromShipReplicationStatus != "File NOT exists.")
                                    {
                                        //Delete File and ACK
                                        //Bosco

                                        ACKFromShipReplicationStatus = SPListItemsUpdate("File", "RemoteDelete", folPath, ListNameStr, nmstrLinkFilename, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];
                                        ACKFromShipReplicationStatus = "CR";
                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                        //Bosco - 23Feb2018
                                        //int nmRepStatusCount = UpdatenmRepStatus(nmRepID, JSONFiles.UpdateType);

                                        //if (nmRepStatusCount == 1)
                                        //{
                                        ACKFromShipReplicationStatus = "RC";    //When Replication Engine confirms sent out of ACK then Replication Engine will update the Replication Status = AC
                                        ACKFromShipReplicationCount++;
                                        //}
                                        //else
                                        //    ACKFromShipReplicationStatus = "NA";


                                    }
                                    else // "File NOT exists."
                                    {
                                        ErrorStr = "err05";
                                        // Update Acknowledgement Status = NA (Remote Delete File NOT exists)
                                        ACKFromShipReplicationStatus = "NA";
                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                    }

                                }
                                else // "Folder"
                                {
                                    ErrorStr = "err04";
                                    // Update Acknowledgement Status = NA (Folder exists in JSON)
                                    ACKFromShipReplicationStatus = "NA";
                                    ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                }

                            }   // End of foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)

                            /*
                            if (System.IO.Directory.Exists(RepDirectoryStr))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(RepDirectoryStr);
                                di.Delete(true);

                            }
                            */

                            if (System.IO.Directory.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                            {
                                if (System.IO.File.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr))
                                {

                                    ACKFromShipReplicationThisReplicationCount = ACKFromShipReplicationCount - ACKFromShipReplicationThisReplicationCount;

                                    System.IO.File.Move(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr, EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco");

                                    System.IO.Directory.Delete(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_");

                                    // Disabled
                                    //CreateTRGFile(DirectoryShiftLevels(EnquiryRegistryFolderPath("OutboundQueue"), 1), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);
                                    CreateTRGFile(EnquiryRegistryFolderPath("OutboundQueue"), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);

                                }

                                if (System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                                {
                                    string TempStr = DateTime.Now.ToString("yyyyMMddmmss");

                                    if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_ProcessedFiles\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + @"_backup" + TempStr))
                                        System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_ProcessedFiles\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + @"_backup" + TempStr);

                                    string ARDirStr = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_";

                                    if (System.IO.Directory.Exists(ARDirStr))
                                    {
                                        foreach (string FileStr in System.IO.Directory.GetFiles(ARDirStr))
                                        {
                                            System.IO.File.Move(FileStr, EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_ProcessedFiles\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + @"_backup" + TempStr + @"\" + FileStr.Split('\\').Last());
                                        }

                                        DeleteDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_");

                                    }
                                }

                            }

                        }
                        else
                        {
                            System.IO.DirectoryInfo di = new DirectoryInfo(RepDirectoryStr);

                            switch (ErrorStr)
                            {
                                case "Resent":
                                    // This replication batch has been resent

                                    if (System.IO.Directory.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                                    {
                                        if (System.IO.File.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr))
                                        {

                                            ACKFromShipReplicationThisReplicationCount = ACKFromShipReplicationCount - ACKFromShipReplicationThisReplicationCount;

                                            System.IO.File.Move(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr, EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco");

                                            // Disabled
                                            //CreateTRGFile(DirectoryShiftLevels(EnquiryRegistryFolderPath("OutboundQueue"), 1), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);
                                            CreateTRGFile(EnquiryRegistryFolderPath("OutboundQueue"), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);

                                            System.IO.Directory.Delete(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_");

                                        }
                                    }

                                    di.Delete(true);

                                    break;
                                default:
                                    // This replication batch has error
                                    if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("FailedItems") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                                    {
                                        if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("FailedItems") + AckIMO))
                                            System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("FailedItems") + AckIMO);

                                        di.MoveTo(EnquiryRegistryFolderPath("FailedItems") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + DateTime.Now.ToString("yyMMddhhmmss") + "_" + ErrorStr);
                                    }
                                    break;
                            }

                        }

                    } // End of foreach (string ShipReplicationFolders in ShipReplicationFolder)
                      //// End of foreach (int AckIMO in AckIMOInt)

                    /*
                    //if (ACKFromShipReplicationCount != 0)
                    if (ACKFromShipReplicationThisReplicationCount > 0)
                    {
                        FromShipReplicationIMOPath = EnquiryRegistryFolderPath("OutboundQueue") + AckIMO.ToString();
                        foreach (string StrIMOName in Directory.GetFiles(FromShipReplicationIMOPath))
                        {
                            if (StrIMOName.Split('.').Last() == "aco")
                            {
                                //if (ProcessAckFromOffice(StrIMOName, StrIMOName.Split('\\').Last(), AckIMO))
                                //{
                                    if (ACKFromShipReplicationCount > 1)
                                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgements to Ship have been sent in this batch process!";
                                    else if (ACKFromShipReplicationCount == 1)
                                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgement to Ship has been sent in this batch process!";

                                //}

                                string TempDateStr = DateTime.Now.ToString("HHmmss");

                                System.IO.File.Copy(StrIMOName, EnquiryRegistryFolderPath("OutboundArchive") + StrIMOName.Split('\\').Last().Substring(0, StrIMOName.Split('\\').Last().Length - StrIMOName.Split('\\').Last().Split('.').Last().Length - 1) + "_" + TempDateStr + ".aco");

                                /*
                                System.IO.File.Copy(StrIMOName, NMInPathStr + @"_ProcessedFiles\" + StrIMOName.Split('\\').Last().Substring(0, StrIMOName.Split('\\').Last().Length - StrIMOName.Split('\\').Last().Split('.').Last().Length - 1) + "_" + TempDateStr + ".aco");
                                */
                    /*
                }
                else
                {
                    if (ACKFromShipReplicationCount > 1)
                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgements have been replied to Ship in this batch process!";
                    else if (ACKFromShipReplicationCount == 1)
                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgement has been replied to Ship in this batch process!";

                }

                System.IO.File.Delete(StrIMOName);

            }
            */
                    /*
                    FromShipReplicationIMOPath = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString();
                    DirectoryInfo di = new DirectoryInfo(FromShipReplicationIMOPath);
                    di.Delete(true);
                    */

                    //}

                } // For-loop Multiple-Ship

                #endregion
            }

            else
            {
                #region v2.0

                if (MultipleShipTest)
                    ActiveIMO = getAllActiveVessels();
                else
                    ActiveIMO = new int[] { SingleShipIMOInt };

                // initialize ships by active IMO and with existing directory in inbound folder
                List<Ship> ships = Ship.InitializeShips(ActiveIMO, erfpHelper.getInboundDir(), RelocationFolder);
                List<ReplicationFolder> shipReplicationFolders = new List<ReplicationFolder>();

                List<Task> loadShipReplicationFoldersTask = new List<Task>();
                foreach (Ship ship in ships)
                {
                    loadShipReplicationFoldersTask.Add
                    (
                        Task.Factory.StartNew
                        (
                            () =>
                            { 
                                // load replication folders and files for ships
                                ship.ReplicationFolders = ReplicationFolder.LoadShipReplicationFolders(ship, DMSSRFolderSearchPattern, ship.RelocationFolder);
                                shipReplicationFolders.AddRange(ship.ReplicationFolders);
                            }
                        )
                    );
                }
                Task.WaitAll(loadShipReplicationFoldersTask.ToArray());

                List<Task> loadShipReplicationFilesTask = new List<Task>();
                foreach (ReplicationFolder shipReplicationFolder in shipReplicationFolders)
                {
                    loadShipReplicationFilesTask.Add
                    (
                        Task.Factory.StartNew
                        (
                            () =>
                            shipReplicationFolder.ReplicationFiles = ReplicationFile.LoadShipReplicationFiles(shipReplicationFolder)
                        )
                    );                    
                }
                Task.WaitAll(loadShipReplicationFilesTask.ToArray());
                
                // relocate replication folders and files
                Ship.RelocateReplicationFiles(ships, JSONFileName);

                // get all ships with replication folders for acknowledgement
                var shipsForRepicationAcknowledgement = ships
                    .Where(ship => ship.HasReplicationFolderForAcknowledgement == true)
                    .Select(ship => ship);

                List<Task> loadReplicationFolderSharePointInfoTask = new List<Task>();
                // get list of vessel info from sharepoint
                foreach (Ship shipForRepicationAcknowledgement in shipsForRepicationAcknowledgement)
                {
                    loadReplicationFolderSharePointInfoTask.Add
                    (
                        Task.Factory.StartNew
                        (
                            () =>
                            ReplicationFolder.LoadReplicationFolderSharePointInfo(shipForRepicationAcknowledgement.ReplicationFolders, SearchVesselSPInfos(Convert.ToInt32(shipForRepicationAcknowledgement.IMO)))
                        )
                    );
                }
                Task.WaitAll(loadReplicationFolderSharePointInfoTask.ToArray());

                // -----



                //// Process DMSSR Replication Folder from Ship
                //for (int ShipLoopInt = 0; ShipLoopInt < ActiveIMO.Length; ShipLoopInt++)
                ////foreach (Ship ShipForReplication in Ships)
                foreach (Ship shipForReplicationAcknowledgement in shipsForRepicationAcknowledgement)
                {
                    // AckIMO = ActiveIMO[ShipLoopInt];
                    AckIMO = Convert.ToInt32(shipForReplicationAcknowledgement.IMO);
                    // can be replaced by --> ShipForReplication.IMO;

                    //string NMPathStr = @"\\10.0.1.231\c$";
                    //AckIMO = UATPathConfigs[ShipLoopInt].IMO;

                    /*
                    string NMPathStr = @"\\" + UATPathConfigs[ShipLoopInt].IPAddress + @"\c$";

                    string NMOutPathStr = string.Empty;
                    string NMInPathStr = string.Empty;
                    string NMOutArchivePathStr = string.Empty;

                    UATGlobals.SetUATGlobals(UATPathConfigs[ShipLoopInt].IMO, UATPathConfigs[ShipLoopInt].IPAddress, UATPathConfigs[ShipLoopInt].SQLServerPassword, UATPathConfigs[ShipLoopInt].SQLServerDatabase);
                    */

                    #region Initialize Variables

                    ACKFromShipReplicationStatus = string.Empty;

                    string RepDirectoryStr = string.Empty;
                    string RepRefStr = string.Empty;
                    string RepFolSrcStr = string.Empty;
                    string RepYYMMStr = "";
                    string RepYYMMDDStr = "";
                    string RepDOCUniqueIdStr = string.Empty;
                    string JSONFileNameStr = "fatt.nsf";
                    string RepModeStr = "";
                    int RepID = 0;

                    string RootFolderStr = string.Empty;
                    string ListNameStr = string.Empty;
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    class_nmReplicationJSONFull AckFromShipReplicationJSONFULL = new class_nmReplicationJSONFull();
                    AckFromShipReplicationJSONFULL.entries = new List<class_nmReplicationJSONFile>();
                    class_nmReplicationJSONFile JSONFile = new class_nmReplicationJSONFile();
                    List<class_UploadFile> UploadFileInfos = new List<class_UploadFile>();
                    class_UploadFile UploadFileInfo = new class_UploadFile();

                    // Create variables for Ship Document Library Infos
                    string nmstrIDPath = string.Empty;
                    string nmstrVirtual_LocalRelativePath = string.Empty;
                    string nmstrLinkFilename = string.Empty;
                    int nmRepID = -1;
                    string nmStatus = string.Empty;
                    int nmFSObjType = -1;
                    string nmServerUrl = string.Empty;
                    int nmUniqueId = -1;
                    int nmParentUniqueId = -1;
                    string nmDocumentLibrary = string.Empty;

                    bool ReadyForACK = false;
                    bool ProceedForACK = false;
                    bool IsRemoteDelete = false;
                    string folPath = string.Empty;
                    int ACKFromShipReplicationThisReplicationCount = 0;
                    string StrFileFolderExistenceCheck = string.Empty;
                    string StrFileFolderChecking = string.Empty;
                    string StrFileFolderVirtual_LocalRelativePath = string.Empty;
                    string StrFileFolderIDPath = string.Empty;
                    //string StrFolIDPath = string.Empty;
                    int MaxNSInt = 100;
                    //List<class_sharepointlist> IMPSPListInfos = new List<class_sharepointlist>();
                    List<string> ShipReplicationFolder = new List<string>();
                    List<string> ACKFromShipReplicationStatusList = new List<string>();
                    string ErrorStr = string.Empty;

                    //List<int> AckIMOInt = new List<int>();

                    //string strConnString = ConfigurationManager.ConnectionStrings["nmConnectionString"].ConnectionString;

                    string strConnString = string.Empty;
                    SqlConnection con = new SqlConnection();

                    /*
                    string strConnString = "Data Source=" + UATGlobals.UATSQLServerIP +
                    ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                    "User id=sa;" +
                    "Password=" + UATGlobals.UATSQLServerPassword + ";";

                    SqlConnection con = new SqlConnection(strConnString);

                    using (SqlCommand cmd = new SqlCommand("spGetRepOutQueuePath", con))
                    {
                        con.Open();

                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Read();

                        NMPathStr = NMPathStr + dr[0].ToString().Substring(dr[0].ToString().Split('\\').First().Length, dr[0].ToString().Length - dr[0].ToString().Split('\\').First().Length);
                        NMPathStr = NMPathStr.Substring(0, NMPathStr.Length - 1);
                        NMPathStr = NMPathStr.Substring(0, NMPathStr.Length - NMPathStr.Split('\\').Last().Length);

                        NMOutPathStr = NMPathStr + @"OutboundQueue\";
                        NMInPathStr = NMPathStr + @"InboundQueue\";
                        NMOutArchivePathStr = NMPathStr + @"Archive\Outbound\";

                        con.Close();

                    }
                    */

                    #endregion

                    #region MoveReplicationFolders

                    //// inbound queue folder path for the ship (IMO) in current loop
                    //string FromShipReplicationIMOPath = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString();

                    //// create inbound queue folder path for the ship (IMO) in current loop if not exists
                    //if (!Directory.Exists(FromShipReplicationIMOPath))
                    //    Directory.CreateDirectory(FromShipReplicationIMOPath);

                    //// Check for existing folders (format: DMSSRYYMMXXX9732589YYMMDD_) and loop through them
                    //foreach (string NSINFolders in Directory.GetDirectories(FromShipReplicationIMOPath, "*_", SearchOption.TopDirectoryOnly))
                    //{
                    //    DirectoryInfo di = new DirectoryInfo(NSINFolders);
                    //    string TempDateStr = DateTime.Now.ToString("yyyyMMddmmss");

                    //    //if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last()))
                    //    //    System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last());

                    //    string AppRefNSINFolder = Path.GetDirectoryName(NSINFolders) + @"\_AppReference\" + Path.GetFileName(NSINFolders);

                    //    /*
                    //    if (!System.IO.Directory.Exists(NMOutArchivePathStr + NMOutFolders.Split('\\').Last()))
                    //        System.IO.Directory.CreateDirectory(NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr);
                    //    */

                    //    foreach (string NSINFiles in Directory.GetFiles(NSINFolders))
                    //    {
                    //        string TempStr = NSINFiles;

                    //        //if (System.IO.File.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last()))
                    //        //    System.IO.File.Delete(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last());

                    //        string AppRefNSINFile = AppRefNSINFolder + @"\" + Path.GetFileName(NSINFiles);

                    //        if (System.IO.File.Exists(AppRefNSINFile))
                    //            System.IO.File.Delete(AppRefNSINFile);

                    //        /*
                    //        if (System.IO.File.Exists(NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr + @"\" + NMOutFiles.Split('\\').Last()))
                    //            System.IO.File.Delete(NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr + @"\" + NMOutFiles.Split('\\').Last());
                    //        */

                    //        //if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last()))
                    //        //    System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last());

                    //        if (!Directory.Exists(AppRefNSINFolder))
                    //            Directory.CreateDirectory(AppRefNSINFolder);

                    //        // System.IO.File.Copy(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last(), EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + NSINFolders.Split('\\').Last() + @"\" + NSINFiles.Split('\\').Last());
                    //        System.IO.File.Copy(NSINFiles, AppRefNSINFile);

                    //        /*
                    //        System.IO.File.Copy(NMOutPathStr + NMOutFolders.Split('\\').Last() + @"\" + NMOutFiles.Split('\\').Last(), NMOutArchivePathStr + NMOutFolders.Split('\\').Last() + "backup" + TempDateStr + @"\" + NMOutFiles.Split('\\').Last());
                    //        */

                    //    }

                    //    //di.Delete(true);
                    //    DeleteDirectory(NSINFolders);

                    //}

                    //// End of Configuration of UAT Ship Servers

                    #endregion

                    #region List Relocated Replication Folders with JSONFileNameStr inside (fatt.nsf)

                    //// if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference"))
                    //if (!System.IO.Directory.Exists(FromShipReplicationIMOPath + @"\_AppReference"))
                    //    continue;

                    //// if (System.IO.Directory.GetDirectories(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference").Count() == 0)
                    //if (System.IO.Directory.GetDirectories(FromShipReplicationIMOPath + @"\_AppReference").Count() == 0)
                    //    continue;

                    //FromShipReplicationIMOPath = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference";

                    //foreach (string StrIMOName in Directory.GetDirectories(FromShipReplicationIMOPath))
                    //{
                    //    string TempStr = string.Empty;

                    //    if (StrIMOName.IndexOf('/') >= 0)
                    //        TempStr = StrIMOName.Split('/').Last();
                    //    else if (StrIMOName.IndexOf('\\') >= 0)
                    //        TempStr = StrIMOName.Split('\\').Last();

                    //    if ((TempStr.Substring(12, 7) == SingleShipIMOInt.ToString()) || MultipleShipTest)
                    //    {
                    //        /*
                    //        string FromShipReplicationFolderPath = StrIMOName + "\\_AppReference";
                    //        if (!System.IO.Directory.Exists(FromShipReplicationFolderPath))
                    //            System.IO.Directory.CreateDirectory(FromShipReplicationFolderPath);

                    //        foreach (string StrFolderName in Directory.GetDirectories(FromShipReplicationFolderPath))
                    //        {
                    //            if (System.IO.File.Exists(StrFolderName + "\\" + JSONFileNameStr))
                    //                ShipReplicationFolder.Add(StrFolderName.Split('\\').Last());

                    //        }
                    //        */

                    //        if (System.IO.File.Exists(StrIMOName + "\\" + JSONFileNameStr))
                    //            ShipReplicationFolder.Add(StrIMOName.Split('\\').Last());

                    //    }

                    //}

                    ////ShipReplicationFolder.Add("DMSSR17090079732589170904_");

                    #endregion

                    ACKFromShipReplicationThisReplicationCount = 0;

                    //////
                    //foreach (Ship shipForRepAck in shipsForRepicationAcknowledgement)
                    //{
                    //    foreach (ReplicationFolder repFolAck in shipForRepAck.ReplicationFolders)
                    //    {
                    //        ShipReplicationFolder.Add(repFolAck.DirectoryName);
                    //    }
                    //}
                    //////
                    
                    //foreach (string ShipReplicationFolders in ShipReplicationFolder)
                    foreach(ReplicationFolder replicationFolderForAcknowledgement in shipForReplicationAcknowledgement.ReplicationFolders)
                    {
                        ////
                        string ShipReplicationFolders = replicationFolderForAcknowledgement.DirectoryName;

                        ACKFromShipReplicationThisReplicationCount = ACKFromShipReplicationCount;

                        string ShipReplicationFolderNameStr = string.Empty;

                        if (ShipReplicationFolders.IndexOf('/') >= 0)
                            ShipReplicationFolderNameStr = ShipReplicationFolders.Split('/').Last();
                        else
                            ShipReplicationFolderNameStr = ShipReplicationFolders;

                        //RepModeStr = ShipReplicationFolderNameStr.Substring(0, 5);
                        //RepYYMMStr = ShipReplicationFolderNameStr.Substring(5, 4);
                        //RepID = Convert.ToInt32(ShipReplicationFolderNameStr.Substring(9, 3));
                        //RepRefStr = RepModeStr + RepYYMMStr + ConvertIntToString(RepID, 3);
                        ////AckIMOInt.Add(Convert.ToInt32(ShipReplicationFolders.Substring(12,7)));
                        //AckIMO = Convert.ToInt32(ShipReplicationFolderNameStr.Substring(12, 7));
                        //RepYYMMDDStr = ShipReplicationFolderNameStr.Substring(19, 6);
                        RepModeStr = replicationFolderForAcknowledgement.ReplicationMode;
                        RepYYMMStr = replicationFolderForAcknowledgement.ReplicationYYMM;
                        RepID = replicationFolderForAcknowledgement.ReplicationId;
                        RepRefStr = replicationFolderForAcknowledgement.ReplicationReference;
                        //AckIMOInt.Add(Convert.ToInt32(ShipReplicationFolders.Substring(12,7)));
                        AckIMO = Convert.ToInt32(replicationFolderForAcknowledgement.ParentShip.IMO);
                        RepYYMMDDStr = replicationFolderForAcknowledgement.ReplicationYYMMDD;
                        //} // End of foreach (string ShipReplicationFolders in ShipReplicationFolder)

                        #region get list of vessel info from sharepoint

                        ////foreach (int AckIMO in AckIMOInt)
                        ////{
                        List<class_sharepointlist> TempsIMPSPListInfos = new List<class_sharepointlist>();
                        //TempsIMPSPListInfos = SearchVesselSPInfos(AckIMO);
                        TempsIMPSPListInfos = SearchVesselSPInfos(Convert.ToInt32(replicationFolderForAcknowledgement.ParentShip.IMO));

                        //foreach (class_sharepointlist TempsIMPSPList in TempsIMPSPListInfos)
                        //{
                        //    for (int j = 1; j < (MaxNSInt + 1); j++)
                        //    {
                        //        if (TempsIMPSPList.strBaseFileName == "NS" + ConvertIntToString(j, 4) + "-" + AckIMO.ToString())
                        //        {
                        //            RootFolderStr = "/" + "NS" + ConvertIntToString(j, 4) + "-" + AckIMO.ToString();
                        //            ListNameStr = TempsIMPSPList.strFileDirRef.Split('/').Last();
                        //            //IMPSPListInfos.Add(TempsIMPSPList);
                        //            break;
                        //        }
                        //        else
                        //            continue;

                        //    }

                        //}

                        #endregion

                        ////
                        ListNameStr = replicationFolderForAcknowledgement.SharePointListName;


                        ////
                        Console.WriteLine("ReplicationFolder: " + ShipReplicationFolderNameStr + " - " + ListNameStr);

                        if (ListNameStr != string.Empty)
                        {

                            ProceedForACK = false;

                            //RepDirectoryStr = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + "\\_AppReference\\" + RepModeStr + RepYYMMStr + ConvertIntToString(RepID, 3) + AckIMO.ToString() + RepYYMMDDStr + "_";
                            RepDirectoryStr = replicationFolderForAcknowledgement.RelocationFullPath;

                            if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                            {
                                System.IO.StreamReader myFile = new System.IO.StreamReader(RepDirectoryStr + "\\" + JSONFileNameStr);
                                string myString = string.Empty;

                                while ((myString = myFile.ReadLine()) != null)
                                {
                                    class_nmReplicationJSONFull ReplicationJSONFileFromFile = new class_nmReplicationJSONFull();
                                    ReplicationJSONFileFromFile = js.Deserialize<class_nmReplicationJSONFull>(myString);

                                    List<Task> loadJsonContentsToDbTask = new List<Task>();
                                    foreach (class_nmReplicationJSONFile JSONFiles in ReplicationJSONFileFromFile.entries)
                                    {
                                        loadJsonContentsToDbTask.Add
                                        (
                                            Task.Factory.StartNew
                                            (
                                                () =>
                                                {
                                                    // Only File will be proceesed
                                                    if (JSONFiles.RepSPDocsInfo.intFSObjType == 0)
                                                    {

                                                        AckFromShipReplicationJSONFULL.RepHeader = ReplicationJSONFileFromFile.RepHeader;
                                                        //AckFromShipReplicationJSONFULL.RepHeader.RepIMO = AckIMO;
                                                        //JSONFiles.RepDistributionStatus = "RC";

                                                        nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                                        nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                                        nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                                        nmRepID = JSONFiles.RepID;
                                                        nmStatus = JSONFiles.RepDistributionStatus;
                                                        nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                                        nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                                        nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                                        nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                                        nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                                        //Enquiry from SQL
                                                        //strConnString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

                                                        strConnString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
                                                        con = new SqlConnection(strConnString);

                                                        SqlCommand cmd1 = new SqlCommand();
                                                        SqlCommand cmd2 = new SqlCommand();

                                                        cmd1.CommandType = CommandType.StoredProcedure;
                                                        cmd2.CommandType = CommandType.StoredProcedure;

                                                        switch (JSONFiles.UpdateType)
                                                        {

                                                            case "RemoteDelete":
                                                                cmd1.CommandText = "sprepAckShipToShoreRDCheck";
                                                                cmd2.CommandText = "sprepAckShipToShoreRDAdd";
                                                                IsRemoteDelete = true;
                                                                break;
                                                            default:
                                                                cmd1.CommandText = "sprepAckShipToShoreCheck";
                                                                cmd2.CommandText = "sprepAckShipToShoreAdd";
                                                                IsRemoteDelete = false;
                                                                break;

                                                        }

                                                        cmd1.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                                        cmd1.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                                        cmd1.Connection = con;

                                                        cmd2.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                                        cmd2.Parameters.Add("@pnmRepRef", SqlDbType.VarChar).Value = RepRefStr;
                                                        cmd2.Parameters.Add("@pnmRepYyMmDd", SqlDbType.Int).Value = Convert.ToInt32(RepYYMMDDStr);
                                                        cmd2.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                                        cmd2.Parameters.Add("@pnmStatus", SqlDbType.VarChar).Value = nmStatus;
                                                        cmd2.Parameters.Add("@pnmFSObjType", SqlDbType.Int).Value = nmFSObjType;
                                                        cmd2.Parameters.Add("@pnmServerUrl", SqlDbType.VarChar).Value = nmServerUrl;
                                                        cmd2.Parameters.Add("@pnmUniqueId", SqlDbType.Int).Value = nmUniqueId;
                                                        cmd2.Parameters.Add("@pnmParentUniqueId", SqlDbType.Int).Value = nmParentUniqueId;
                                                        cmd2.Parameters.Add("@pnmVirtual_LocalRelativePath", SqlDbType.VarChar).Value = nmstrIDPath;
                                                        cmd2.Parameters.Add("@pnmDocumentLibrary", SqlDbType.VarChar).Value = nmDocumentLibrary;
                                                        cmd2.Connection = con;

                                                        try
                                                        {
                                                            con.Open();
                                                            SqlDataReader dr = cmd1.ExecuteReader();
                                                            int i = -1;

                                                            while (dr.Read())
                                                            {
                                                                if (dr.IsDBNull(0))
                                                                    ACKFromShipReplicationStatus = "Error in SQL Enquiry! Please contact System Administrator!";
                                                                else
                                                                    i = int.Parse(dr[0].ToString());

                                                            }
                                                            //Console.WriteLine("Record enquiried successfully");

                                                            if (i == 0)
                                                            {
                                                                con.Close();
                                                                ACKFromShipReplicationStatus = "Ready To Insert Replication";
                                                                con.Open();
                                                                cmd2.ExecuteNonQuery();
                                                                ProceedForACK = true;

                                                            }
                                                            else if (i == -1)
                                                            {
                                                                ACKFromShipReplicationStatus = ResendAcknowledgement(AckIMO, nmRepID, "AC");
                                                                ProceedForACK = false;
                                                                ErrorStr = "Resent";
                                                                ACKFromShipReplicationCount++;
                                                            }
                                                            else
                                                            {
                                                                ProceedForACK = false;
                                                                ErrorStr = "err03";

                                                            }


                                                        }
                                                        catch (System.Exception ex)
                                                        {
                                                            //throw ex;
                                                        }
                                                        finally
                                                        {
                                                            con.Close();
                                                            con.Dispose();
                                                        }

                                                        if (ProceedForACK)
                                                        {
                                                            JSONFile = JSONFiles;
                                                            AckFromShipReplicationJSONFULL.entries.Add(JSONFiles);
                                                        }
                                                    }   // "Folder" entry
                                                    else
                                                    {

                                                        ProceedForACK = false;
                                                        ErrorStr = "err04";

                                                    }

                                                }
                                            )
                                        );


                                    }
                                    Task.WaitAll(loadJsonContentsToDbTask.ToArray());
                                }

                                myFile.Close();
                                myFile.Dispose();
                                //Thread.Sleep(5000);

                                /*if (ProceedForACK && System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                    System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);
                                */

                            }

                            else
                            {
                                Thread.Sleep(5000);

                                if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                {
                                    System.IO.StreamReader myFile = new System.IO.StreamReader(RepDirectoryStr + "\\" + JSONFileNameStr);
                                    string myString = string.Empty;

                                    while ((myString = myFile.ReadLine()) != null)
                                    {
                                        class_nmReplicationJSONFull ReplicationJSONFileFromFile = new class_nmReplicationJSONFull();
                                        ReplicationJSONFileFromFile = js.Deserialize<class_nmReplicationJSONFull>(myString);

                                        List<Task> loadJsonContentsToDbAltTask = new List<Task>();
                                        foreach (class_nmReplicationJSONFile JSONFiles in ReplicationJSONFileFromFile.entries)
                                        {
                                            loadJsonContentsToDbAltTask.Add
                                            (
                                                Task.Factory.StartNew
                                                (
                                                    () =>
                                                    {
                                                        // Only File will be proceesed
                                                        if (JSONFiles.RepSPDocsInfo.intFSObjType == 0)
                                                        {

                                                            AckFromShipReplicationJSONFULL.RepHeader = ReplicationJSONFileFromFile.RepHeader;
                                                            //AckFromShipReplicationJSONFULL.RepHeader.RepIMO = AckIMO;
                                                            //JSONFiles.RepDistributionStatus = "RC";

                                                            nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                                            nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                                            nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                                            nmRepID = JSONFiles.RepID;
                                                            nmStatus = JSONFiles.RepDistributionStatus;
                                                            nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                                            nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                                            nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                                            nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                                            nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                                            //Enquiry from SQL
                                                            //strConnString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

                                                            strConnString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
                                                            con = new SqlConnection(strConnString);

                                                            SqlCommand cmd1 = new SqlCommand();
                                                            SqlCommand cmd2 = new SqlCommand();

                                                            cmd1.CommandType = CommandType.StoredProcedure;
                                                            cmd2.CommandType = CommandType.StoredProcedure;

                                                            cmd1.CommandText = "sprepAckShipToShoreCheck";
                                                            cmd2.CommandText = "sprepAckShipToShoreAdd";

                                                            cmd1.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                                            cmd1.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                                            cmd1.Connection = con;

                                                            cmd2.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = nmRepID;
                                                            cmd2.Parameters.Add("@pnmRepRef", SqlDbType.VarChar).Value = RepRefStr;
                                                            cmd2.Parameters.Add("@pnmRepYyMmDd", SqlDbType.Int).Value = Convert.ToInt32(RepYYMMDDStr);
                                                            cmd2.Parameters.Add("@pIMO", SqlDbType.Int).Value = AckIMO;
                                                            cmd2.Parameters.Add("@pnmStatus", SqlDbType.VarChar).Value = nmStatus;
                                                            cmd2.Parameters.Add("@pnmFSObjType", SqlDbType.Int).Value = nmFSObjType;
                                                            cmd2.Parameters.Add("@pnmServerUrl", SqlDbType.VarChar).Value = nmServerUrl;
                                                            cmd2.Parameters.Add("@pnmUniqueId", SqlDbType.Int).Value = nmUniqueId;
                                                            cmd2.Parameters.Add("@pnmParentUniqueId", SqlDbType.Int).Value = nmParentUniqueId;
                                                            cmd2.Parameters.Add("@pnmVirtual_LocalRelativePath", SqlDbType.VarChar).Value = nmstrIDPath;
                                                            cmd2.Parameters.Add("@pnmDocumentLibrary", SqlDbType.VarChar).Value = nmDocumentLibrary;
                                                            cmd2.Connection = con;

                                                            try
                                                            {
                                                                con.Open();
                                                                SqlDataReader dr = cmd1.ExecuteReader();
                                                                int i = -1;

                                                                while (dr.Read())
                                                                {
                                                                    if (dr.IsDBNull(0))
                                                                        ACKFromShipReplicationStatus = "Error in SQL Enquiry! Please contact System Administrator!";
                                                                    else
                                                                        i = int.Parse(dr[0].ToString());

                                                                }
                                                                //Console.WriteLine("Record enquiried successfully");

                                                                if (i == 0)
                                                                {
                                                                    con.Close();
                                                                    ACKFromShipReplicationStatus = "Ready To Insert Replication";
                                                                    con.Open();
                                                                    cmd2.ExecuteNonQuery();
                                                                    ProceedForACK = true;

                                                                }
                                                                else if (i == -1)
                                                                {
                                                                    ACKFromShipReplicationStatus = ResendAcknowledgement(AckIMO, nmRepID, "AC");
                                                                    ProceedForACK = false;
                                                                    ErrorStr = "Resent";
                                                                    ACKFromShipReplicationCount++;
                                                                }
                                                                // Existing File replicated from Ship
                                                                else
                                                                {
                                                                    ProceedForACK = false;
                                                                    ErrorStr = "err03";

                                                                }


                                                            }
                                                            catch (System.Exception ex)
                                                            {
                                                                //throw ex;
                                                            }
                                                            finally
                                                            {
                                                                con.Close();
                                                                con.Dispose();
                                                            }

                                                            if (ProceedForACK)
                                                            {
                                                                JSONFile = JSONFiles;
                                                                AckFromShipReplicationJSONFULL.entries.Add(JSONFiles);
                                                            }
                                                        }   // "Folder" entry
                                                        else
                                                        {

                                                            ProceedForACK = false;
                                                            ErrorStr = "err04";

                                                        }

                                                    }    
                                                )
                                            );
                                        }
                                        Task.WaitAll(loadJsonContentsToDbAltTask.ToArray());

                                    }

                                    myFile.Close();
                                    myFile.Dispose();
                                    //Thread.Sleep(5000);

                                    try
                                    {
                                        if (ProceedForACK && System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                    }
                                    catch (System.IO.IOException Ex)
                                    {
                                        if (ProceedForACK && System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                        {
                                            //Load your file using FileStream class and release an file through stream.Dispose()
                                            using (FileStream stream = new FileStream(RepDirectoryStr + "\\" + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                                            {
                                                stream.Close();
                                                stream.Dispose();
                                            }

                                            // delete the file.
                                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                        }

                                    }

                                }
                                else
                                {
                                    ACKFromShipReplicationStatus = JSONFileNameStr + " does NOT exist in this Replication Request From Ship(IMO) = " + AckIMO + " with Replication Reference = " + RepRefStr + " on Date = " + RepYYMMDDStr;
                                    ProceedForACK = false;

                                    ErrorStr = "err02";

                                }


                            }

                        }  // End of if (ListNameStr != "")
                        else
                        {
                            // Invalid Ship Information
                            ErrorStr = "err01";
                            Console.ReadLine();
                        }

                        if (ProceedForACK)
                        {
                            bool VirtualPathUpdateDone = false;
                            bool FileNameUpdateDone = false;
                            string CurrentUIDStr = string.Empty;

                            foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)
                            {
                                if (JSONFiles.UpdateType == "RemoteDelete")
                                    continue;

                                VirtualPathUpdateDone = false;
                                FileNameUpdateDone = false;

                                nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                nmRepID = JSONFiles.RepID;
                                nmStatus = JSONFiles.RepDistributionStatus;
                                nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                //StrFolIDPath = nmstrIDPath;
                                CurrentUIDStr = JSONFiles.RepSPDocsInfo.strUniqueId;

                                // Check whethere FileRenamed Updated
                                string OldFileNameStr = GetSPFileInfosByUniqueId(CurrentUIDStr, "spBaseName");

                                if ((OldFileNameStr != nmstrLinkFilename) && (OldFileNameStr != string.Empty))
                                    FileNameUpdateDone = true;

                                string TempnmstrIDPath = nmstrIDPath;
                                string TempnmstrVirtual_LocalRelativePath = nmstrVirtual_LocalRelativePath;

                                // Check whethere Path Updated
                                if (nmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                {
                                    string NewFolderNameStr = nmstrVirtual_LocalRelativePath.Split('/').Last();

                                    //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath;
                                    folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath;

                                    if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                    {
                                        do
                                        {
                                            if (TempnmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                            {
                                                NewFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;
                                                folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;

                                                if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                                {
                                                    DateTime dttspModified = GetSPFolderLMDByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    // Convert to HK Time
                                                    // string StrspModified = dttspModified.AddHours(8).ToString();

                                                    if (JSONFiles.RepSPDocsInfo.strLastModified != string.Empty)
                                                    {

                                                        //BoscoLastNight
                                                        if (dttspModified.AddHours(8).AddSeconds(-dttspModified.Second) <= Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).AddSeconds(-Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).Second))
                                                        {
                                                            string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                            //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                            folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                            if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                                VirtualPathUpdateDone = true;

                                                        }
                                                        else
                                                        {
                                                            string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                            //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                            folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                            if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                                VirtualPathUpdateDone = true;

                                                        }

                                                    }
                                                    else
                                                    {
                                                        string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                        folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                        if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                            VirtualPathUpdateDone = true;

                                                    }

                                                }
                                            }

                                            TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                            TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                        } while (TempnmstrIDPath.Split('/').Count() > 2);

                                    }
                                    else
                                    {
                                        TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                        TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                        do
                                        {
                                            if (TempnmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                            {
                                                NewFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;
                                                folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;

                                                if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                                {
                                                    DateTime dttspModified = GetSPFolderLMDByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    // Convert to HK Time
                                                    // string StrspModified = dttspModified.AddHours(8).ToString();

                                                    if (JSONFiles.RepSPDocsInfo.strLastModified == string.Empty)
                                                    {
                                                        //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                        string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                        folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                        if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                            VirtualPathUpdateDone = true;

                                                    }
                                                    else //if (dttspModified.AddHours(8) < Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified))
                                                    if (dttspModified.AddHours(8).AddSeconds(-dttspModified.Second) <= Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).AddSeconds(-Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).Second))
                                                    {
                                                        //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                        string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                        //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                        folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                        if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                            VirtualPathUpdateDone = true;

                                                    }

                                                }
                                            }

                                            TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                            TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                        } while (TempnmstrIDPath.Split('/').Count() > 2);

                                    }

                                }
                                else
                                {
                                    TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                    TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                    do
                                    {
                                        if (TempnmstrIDPath.Split('/').Last().IndexOf('-') >= 0)
                                        {
                                            string NewFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                            //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;
                                            folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath;

                                            if (SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0] == "Folder NOT exists.")
                                            {
                                                DateTime dttspModified = GetSPFolderLMDByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                // Convert to HK Time
                                                // string StrspModified = dttspModified.AddHours(8).ToString();

                                                if (JSONFiles.RepSPDocsInfo.strLastModified == string.Empty)
                                                {
                                                    //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                    string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                    folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                    ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                    if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                        VirtualPathUpdateDone = true;

                                                }
                                                else// if (dttspModified.AddHours(8) < Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified))
                                                if (dttspModified.AddHours(8).AddSeconds(-dttspModified.Second) <= Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).AddSeconds(-Convert.ToDateTime(JSONFiles.RepSPDocsInfo.strLastModified).Second))
                                                {
                                                    //string OldFolderNameStr = GetSPFolderNameByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length) + OldFolderNameStr;

                                                    string OldFolderURLStr = GetSPServerURLByUniqueId(TempnmstrIDPath.Split('/').Last());

                                                    //folPath = "http://oneview.angloeastern.com" + OldFolderURLStr;
                                                    folPath = "http://dms.angloeastern.com" + OldFolderURLStr;

                                                    ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                    if (ACKFromShipReplicationStatus == "\r\nFolder ' " + NewFolderNameStr + " ' has been renamed successfully.")
                                                        VirtualPathUpdateDone = true;

                                                }

                                            }
                                        }

                                        TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.Length - TempnmstrIDPath.Split('/').Last().Length - 1);

                                        TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.Length - TempnmstrVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                                    } while (TempnmstrIDPath.Split('/').Count() > 2);

                                }


                                if (FileNameUpdateDone)
                                    //ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Update", "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + OldFileNameStr, ListNameStr, JSONFiles.RepSPDocsInfo.strBaseName, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved");
                                    ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Update", "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + OldFileNameStr, ListNameStr, JSONFiles.RepSPDocsInfo.strBaseName, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                // "File"
                                if (nmFSObjType == 0)
                                {
                                    //public class_replicationlist GetUpdatedSPDocInfos(string folPath, string SPUniquedIDStr, string DocumentType, string UpdateType)

                                    ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Check", folPath, ListNameStr, nmstrLinkFilename, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0];

                                    if (ACKFromShipReplicationStatus == "File NOT exists.")
                                    {
                                        CurrentUIDStr = nmstrIDPath;
                                        ReadyForACK = false;

                                        do
                                        {

                                            if (StrFileFolderVirtual_LocalRelativePath != string.Empty)
                                            {
                                                StrFileFolderVirtual_LocalRelativePath = folPath.Split('/').Last() + "/" + StrFileFolderVirtual_LocalRelativePath;
                                                CurrentUIDStr = StrFileFolderIDPath.Split('/').Last();
                                                StrFileFolderIDPath = StrFileFolderIDPath.Replace("/" + CurrentUIDStr, "");
                                            }
                                            else
                                            {
                                                StrFileFolderVirtual_LocalRelativePath = folPath.Split('/').Last();
                                                StrFileFolderIDPath = CurrentUIDStr;
                                            }
                                            StrFileFolderChecking = folPath.Split('/').Last();
                                            if (StrFileFolderChecking == nmstrLinkFilename)
                                                StrFileFolderExistenceCheck = "File";
                                            else
                                                StrFileFolderExistenceCheck = "Folder";
                                            folPath = folPath.Substring(0, folPath.Length - StrFileFolderChecking.Length - 1);

                                            //if (StrFileFolderExistenceCheck == "Folder")
                                            //StrFolIDPath = StrFolIDPath.Substring(0, StrFolIDPath.Length - StrFileFolderChecking.Length - 1);

                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Check", folPath, ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0];
                                            if (ACKFromShipReplicationStatus != "Folder NOT exists.")
                                            {
                                                do
                                                {
                                                    switch (StrFileFolderExistenceCheck)
                                                    {
                                                        case "File":

                                                            //public string SPUploadMultipleFile(List<class_UploadFile> UploadFileInfos, string RepModCodeStr, bool SingleFileUpload)
                                                            UploadFileInfos = new List<class_UploadFile>();
                                                            UploadFileInfo = new class_UploadFile();

                                                            UploadFileInfo.SourceUrl = nmstrLinkFilename;
                                                            //UploadFileInfo.DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                            UploadFileInfo.DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                            UploadFileInfos.Add(UploadFileInfo);

                                                            if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl))
                                                                System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl);

                                                            //Thread.Sleep(5000);

                                                            if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                            else
                                                            {
                                                                Thread.Sleep(5000);
                                                                if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                    //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                    System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                else
                                                                {
                                                                    ACKFromShipReplicationStatus = "Error in proccessing the file '" + JSONFileNameStr + "' in this Replication Request From Ship(IMO) = " + AckIMO + " with Replication Reference = " + RepRefStr + " on Date = " + RepYYMMDDStr;
                                                                    return ACKFromShipReplicationStatus;
                                                                }

                                                            }

                                                            ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true, 1);
                                                            //ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true);

                                                            //if (ACKFromShipReplicationStatus != "1 file has been uploaded successfully!")
                                                            if (ACKFromShipReplicationStatus != "File '" + UploadFileInfos[0].SourceUrl + "' has been uploaded for approval!")
                                                            {
                                                                if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl))
                                                                    System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                            }

                                                            break;

                                                        case "Folder":

                                                            do
                                                            {
                                                                if (CurrentUIDStr.IndexOf('-') > 0)
                                                                {
                                                                    ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", "http://dms.angloeastern.com" + GetSPFolderInfoByUniqueId(CurrentUIDStr).strFileDirRef, ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                                    folPath = folPath + "/" + StrFileFolderChecking;

                                                                }
                                                                else
                                                                {
                                                                    if (SBFSPFolderCheck(AckIMO, Convert.ToInt32(CurrentUIDStr)) != string.Empty)
                                                                    {
                                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", folPath + "/" + SBFSPFolderCheck(AckIMO, Convert.ToInt32(CurrentUIDStr)), ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                                        folPath = folPath + "/" + StrFileFolderChecking;

                                                                    }
                                                                    else
                                                                    {
                                                                        ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "New", folPath, ListNameStr, StrFileFolderChecking, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];

                                                                        folPath = folPath + "/" + StrFileFolderChecking;

                                                                        SBFSPFolderAdd(AckIMO, Convert.ToInt32(CurrentUIDStr), GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")), "Folder", "Existing").strspUniqueId);

                                                                    }

                                                                }

                                                                StrFileFolderChecking = StrFileFolderVirtual_LocalRelativePath.Split('/').First();
                                                                StrFileFolderVirtual_LocalRelativePath = StrFileFolderVirtual_LocalRelativePath.Replace(StrFileFolderChecking + "/", "");
                                                                if (StrFileFolderVirtual_LocalRelativePath.IndexOf('/') >= 0)
                                                                {
                                                                    StrFileFolderChecking = StrFileFolderVirtual_LocalRelativePath.Split('/').First();
                                                                    //StrFileFolderVirtual_LocalRelativePath = StrFileFolderVirtual_LocalRelativePath.Replace(StrFileFolderChecking + "/", "");

                                                                    ReadyForACK = false;

                                                                }
                                                                else
                                                                {
                                                                    UploadFileInfos = new List<class_UploadFile>();
                                                                    UploadFileInfo = new class_UploadFile();

                                                                    UploadFileInfo.SourceUrl = nmstrLinkFilename;
                                                                    //UploadFileInfo.DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                                    UploadFileInfo.DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                                    UploadFileInfos.Add(UploadFileInfo);

                                                                    if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl))
                                                                        System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl);

                                                                    //Thread.Sleep(5000);

                                                                    if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                        //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, Server.MapPath("~/App_Data/Temp") + "\\" + UploadFileInfo.SourceUrl);
                                                                        System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                    else
                                                                    {
                                                                        Thread.Sleep(5000);
                                                                        if (System.IO.File.Exists(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl))
                                                                            //System.IO.File.Move(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, Server.MapPath("~/App_Data/Temp") + "\\" + UploadFileInfo.SourceUrl);
                                                                            System.IO.File.Copy(RepDirectoryStr + "\\" + nmUniqueId + "-" + UploadFileInfo.SourceUrl, ProcessedFilesPath + UploadFileInfo.SourceUrl);
                                                                        else
                                                                        {
                                                                            ACKFromShipReplicationStatus = "Error in proccessing the file '" + JSONFileNameStr + "' in this Replication Request From Ship(IMO) = " + AckIMO + " with Replication Reference = " + RepRefStr + " on Date = " + RepYYMMDDStr;
                                                                            return ACKFromShipReplicationStatus;
                                                                        }

                                                                    }

                                                                    ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true, 1);
                                                                    //ACKFromShipReplicationStatus = SPMultipleUploadFile(UploadFileInfos, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, true);

                                                                    //if (ACKFromShipReplicationStatus != "1 file has been uploaded successfully!")
                                                                    if (ACKFromShipReplicationStatus != "File '" + UploadFileInfos[0].SourceUrl.Split('\\').Last() + "' has been uploaded for approval!")
                                                                    {
                                                                        if (System.IO.File.Exists(ProcessedFilesPath + UploadFileInfo.SourceUrl.Split('\\').Last()))
                                                                            System.IO.File.Delete(ProcessedFilesPath + UploadFileInfo.SourceUrl.Split('\\').Last());
                                                                    }

                                                                    ReadyForACK = true;

                                                                }

                                                            } while (!ReadyForACK);

                                                            break;
                                                        default:
                                                            break;
                                                    }

                                                } //while (folPath.Replace(@"http://oneview.angloeastern.com/sites/oneview/nsdl/", "") != (ListNameStr + nmstrVirtual_LocalRelativePath));
                                                while (folPath.Replace(@"http://dms.angloeastern.com/sites/nsdl/", "") != (ListNameStr + nmstrVirtual_LocalRelativePath));

                                                ReadyForACK = true;
                                                ACKFromShipReplicationCount++;

                                            }
                                            else
                                                ReadyForACK = false;

                                        } while (!ReadyForACK);

                                        int[] IMOArray = new int[] { AckIMO };

                                        ACKFromShipReplicationStatus = CreateReplication(UploadFileInfo.DestinationUrl, RepModeStr, IMOArray);

                                        if (ACKFromShipReplicationStatus == "\r\nReplication Acknowledgement to Ship has been created successfully!!")
                                        {

                                            //int nmRepStatusCount = UpdatenmRepStatus(nmRepID);

                                            //if (nmRepStatusCount == 1)
                                            ACKFromShipReplicationStatus = "RC";    //When Replication Engine confirms sent out of ACK then Replication Engine will update the Replication Status = AC
                                            ACKFromShipReplicationCount++;
                                            //else
                                            // ACKFromShipReplicationStatus = "NA";

                                        }
                                        else
                                            ACKFromShipReplicationStatus = "NA";

                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                    }
                                    else
                                    {
                                        // ACK for File exists in Ship Filing
                                        //if (VirtualPathUpdateDone || FileNameUpdateDone)
                                        //{
                                        UploadFileInfos = new List<class_UploadFile>();
                                        UploadFileInfo = new class_UploadFile();

                                        UploadFileInfo.SourceUrl = nmstrLinkFilename;
                                        //UploadFileInfo.DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                        UploadFileInfo.DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                        UploadFileInfos.Add(UploadFileInfo);

                                        // Still Acknowledge if any update performed during VirtualPathCheck
                                        int[] IMOArray = new int[] { AckIMO };

                                        ACKFromShipReplicationStatus = CreateReplication(UploadFileInfo.DestinationUrl, RepModeStr, IMOArray);

                                        if (ACKFromShipReplicationStatus == "\r\nReplication Acknowledgement to Ship has been created successfully!!")
                                        {

                                            //int nmRepStatusCount = UpdatenmRepStatus(nmRepID);

                                            //if (nmRepStatusCount == 1)
                                            //{
                                            ACKFromShipReplicationStatus = "RC";    //When Replication Engine confirms sent out of ACK then Replication Engine will update the Replication Status = AC
                                            ACKFromShipReplicationCount++;
                                            //}
                                            //else
                                            //ACKFromShipReplicationStatus = "NA";

                                        }
                                        else
                                            ACKFromShipReplicationStatus = "NA";

                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                        /*}
                                        else
                                        {
                                            // Update Acknowledgement Status = NA (File exists in Ship Filing)
                                            ACKFromShipReplicationStatus = "NA";
                                            ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                        }*/

                                    }

                                }
                                else // "Folder"
                                {
                                    ErrorStr = "err04";
                                    // Update Acknowledgement Status = NA (Folder exists in JSON)
                                    ACKFromShipReplicationStatus = "NA";
                                    ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                }

                                StrFileFolderVirtual_LocalRelativePath = string.Empty;

                            }   // End of foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)

                            foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)
                            {
                                if (JSONFiles.UpdateType != "RemoteDelete")
                                    continue;

                                nmstrLinkFilename = JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmstrIDPath = JSONFiles.RepSPDocsInfo.strIDPath;
                                nmstrVirtual_LocalRelativePath = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath;

                                nmRepID = JSONFiles.RepID;
                                nmStatus = JSONFiles.RepDistributionStatus;
                                nmFSObjType = JSONFiles.RepSPDocsInfo.intFSObjType;
                                nmServerUrl = JSONFiles.RepSPDocsInfo.strVirtual_LocalRelativePath + "/" + JSONFiles.RepSPDocsInfo.strLinkFilename;
                                nmUniqueId = JSONFiles.RepSPDocsInfo.intId;
                                nmParentUniqueId = JSONFiles.RepSPDocsInfo.intPid;
                                nmDocumentLibrary = JSONFiles.RepSPDocsInfo.strDocumentLibrary;

                                // Bosco - 3May2018 (Rename Folder before RD)

                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                //folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                if (nmstrIDPath.IndexOf('-') > 0)
                                {

                                    bool SPFolderLocated = false;
                                    string TempnmstrIDPath = nmstrIDPath;
                                    string TempnmstrVirtual_LocalRelativePath = nmstrVirtual_LocalRelativePath;
                                    string TempNewFolderPath = string.Empty;

                                    do
                                    {
                                        if (TempnmstrIDPath.Split('/').Last().IndexOf('-') > 0)
                                        {
                                            if (TempNewFolderPath != string.Empty)
                                                folPath = GetSPFolderInfoByUniqueId(nmstrIDPath.Split('/').Last()).strFileDirRef + "/" + TempNewFolderPath + "/" + nmstrLinkFilename;
                                            else
                                                folPath = GetSPFolderInfoByUniqueId(nmstrIDPath.Split('/').Last()).strFileDirRef + "/" + nmstrLinkFilename;

                                            if (folPath != ("/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename))
                                            {
                                                TempNewFolderPath = "/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;
                                                TempnmstrVirtual_LocalRelativePath = folPath;

                                                string NewFolderNameStr = string.Empty;
                                                string OldFolderNameStr = string.Empty;

                                                do
                                                {
                                                    TempNewFolderPath = TempNewFolderPath.Substring(0, TempNewFolderPath.LastIndexOf('/'));
                                                    TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.LastIndexOf('/'));

                                                    if (TempNewFolderPath != TempnmstrVirtual_LocalRelativePath)
                                                    {
                                                        NewFolderNameStr = TempNewFolderPath.Split('/').Last();
                                                        OldFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                        if (NewFolderNameStr != OldFolderNameStr)
                                                        {
                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", "http://dms.angloeastern.com" + TempnmstrVirtual_LocalRelativePath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];



                                                        }

                                                    }
                                                    else
                                                        SPFolderLocated = true;

                                                } while (!SPFolderLocated);

                                                folPath = "/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                            }
                                            else
                                            {
                                                //TempNewFolderPath = folPath.Substring(0, folPath.LastIndexOf('/'));
                                                TempNewFolderPath = folPath.Replace("/sites/nsdl/" + ListNameStr, "");
                                                TempnmstrVirtual_LocalRelativePath = nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                                string NewFolderNameStr = string.Empty;
                                                string OldFolderNameStr = string.Empty;

                                                do
                                                {
                                                    TempNewFolderPath = TempNewFolderPath.Substring(0, TempNewFolderPath.LastIndexOf('/'));
                                                    TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrVirtual_LocalRelativePath.LastIndexOf('/'));

                                                    if (TempNewFolderPath != TempnmstrVirtual_LocalRelativePath)
                                                    {
                                                        NewFolderNameStr = TempNewFolderPath.Split('/').Last();
                                                        OldFolderNameStr = TempnmstrVirtual_LocalRelativePath.Split('/').Last();

                                                        if (NewFolderNameStr != OldFolderNameStr)
                                                        {
                                                            ACKFromShipReplicationStatus = SPListItemsUpdate("Folder", "Update", "http://dms.angloeastern.com" + TempnmstrVirtual_LocalRelativePath, ListNameStr, NewFolderNameStr, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];



                                                        }

                                                    }
                                                    else
                                                        SPFolderLocated = true;

                                                } while (!SPFolderLocated);

                                                folPath = "/sites/nsdl/" + ListNameStr + nmstrVirtual_LocalRelativePath + "/" + nmstrLinkFilename;

                                            }

                                        }
                                        else
                                        {
                                            TempnmstrIDPath = TempnmstrIDPath.Substring(0, TempnmstrIDPath.LastIndexOf('/'));
                                            TempNewFolderPath = TempnmstrVirtual_LocalRelativePath.Split('/').Last();
                                            TempnmstrVirtual_LocalRelativePath = TempnmstrVirtual_LocalRelativePath.Substring(0, TempnmstrIDPath.LastIndexOf('/'));

                                        }

                                    } while (!SPFolderLocated);

                                }

                                // "File"
                                if (nmFSObjType == 0)
                                {
                                    ACKFromShipReplicationStatus = SPListItemsUpdate("File", "Check", folPath, ListNameStr, nmstrLinkFilename, AckFromShipReplicationJSONFULL.RepHeader.RepModCode, "Approved", AckIMO, null)[0];

                                    if (ACKFromShipReplicationStatus != "File NOT exists.")
                                    {
                                        //Delete File and ACK
                                        //Bosco

                                        ACKFromShipReplicationStatus = SPListItemsUpdate("File", "RemoteDelete", folPath, ListNameStr, nmstrLinkFilename, AckFromShipReplicationJSONFULL.RepHeader.RepRef + AckIMO.ToString() + RepYYMMDDStr + "_" + nmRepID, "Approved", AckIMO, null)[0];
                                        ACKFromShipReplicationStatus = "CR";
                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                        //Bosco - 23Feb2018
                                        //int nmRepStatusCount = UpdatenmRepStatus(nmRepID, JSONFiles.UpdateType);

                                        //if (nmRepStatusCount == 1)
                                        //{
                                        ACKFromShipReplicationStatus = "RC";    //When Replication Engine confirms sent out of ACK then Replication Engine will update the Replication Status = AC
                                        ACKFromShipReplicationCount++;
                                        //}
                                        //else
                                        //    ACKFromShipReplicationStatus = "NA";


                                    }
                                    else // "File NOT exists."
                                    {
                                        ErrorStr = "err05";
                                        // Update Acknowledgement Status = NA (Remote Delete File NOT exists)
                                        ACKFromShipReplicationStatus = "NA";
                                        ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                    }

                                }
                                else // "Folder"
                                {
                                    ErrorStr = "err04";
                                    // Update Acknowledgement Status = NA (Folder exists in JSON)
                                    ACKFromShipReplicationStatus = "NA";
                                    ACKFromShipReplicationStatusList.Add(ACKFromShipReplicationStatus);

                                }

                            }   // End of foreach (class_nmReplicationJSONFile JSONFiles in AckFromShipReplicationJSONFULL.entries)

                            /*
                            if (System.IO.Directory.Exists(RepDirectoryStr))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(RepDirectoryStr);
                                di.Delete(true);

                            }
                            */

                            if (System.IO.Directory.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                            {
                                if (System.IO.File.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr))
                                {

                                    ACKFromShipReplicationThisReplicationCount = ACKFromShipReplicationCount - ACKFromShipReplicationThisReplicationCount;

                                    System.IO.File.Move(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr, EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco");

                                    System.IO.Directory.Delete(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_");

                                    // Disabled
                                    //CreateTRGFile(DirectoryShiftLevels(EnquiryRegistryFolderPath("OutboundQueue"), 1), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);
                                    CreateTRGFile(EnquiryRegistryFolderPath("OutboundQueue"), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);

                                }

                                if (System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                                {
                                    string TempStr = DateTime.Now.ToString("yyyyMMddmmss");

                                    if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_ProcessedFiles\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + @"_backup" + TempStr))
                                        System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_ProcessedFiles\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + @"_backup" + TempStr);

                                    string ARDirStr = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_";

                                    if (System.IO.Directory.Exists(ARDirStr))
                                    {
                                        foreach (string FileStr in System.IO.Directory.GetFiles(ARDirStr))
                                        {
                                            System.IO.File.Move(FileStr, EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_ProcessedFiles\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + @"_backup" + TempStr + @"\" + FileStr.Split('\\').Last());
                                        }

                                        DeleteDirectory(EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString() + @"\_AppReference\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_");

                                    }
                                }

                            }

                        }
                        else
                        {
                            System.IO.DirectoryInfo di = new DirectoryInfo(RepDirectoryStr);

                            switch (ErrorStr)
                            {
                                case "Resent":
                                    // This replication batch has been resent

                                    if (System.IO.Directory.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                                    {
                                        if (System.IO.File.Exists(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr))
                                        {

                                            ACKFromShipReplicationThisReplicationCount = ACKFromShipReplicationCount - ACKFromShipReplicationThisReplicationCount;

                                            System.IO.File.Move(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + @"\" + JSONFileNameStr, EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco");

                                            // Disabled
                                            //CreateTRGFile(DirectoryShiftLevels(EnquiryRegistryFolderPath("OutboundQueue"), 1), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);
                                            CreateTRGFile(EnquiryRegistryFolderPath("OutboundQueue"), "ACO" + AckIMO.ToString() + DateTime.Now.ToString("yyMMddhhmmss") + "_(" + ACKFromShipReplicationThisReplicationCount.ToString() + ").aco", 0, true);

                                            System.IO.Directory.Delete(EnquiryRegistryFolderPath("OutboundQueue") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_");

                                        }
                                    }

                                    di.Delete(true);

                                    break;
                                default:
                                    // This replication batch has error
                                    if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("FailedItems") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_"))
                                    {
                                        if (!System.IO.Directory.Exists(EnquiryRegistryFolderPath("FailedItems") + AckIMO))
                                            System.IO.Directory.CreateDirectory(EnquiryRegistryFolderPath("FailedItems") + AckIMO);

                                        di.MoveTo(EnquiryRegistryFolderPath("FailedItems") + AckIMO + @"\" + RepRefStr + AckIMO.ToString() + RepYYMMDDStr + "_" + DateTime.Now.ToString("yyMMddhhmmss") + "_" + ErrorStr);
                                    }
                                    break;
                            }

                        }

                    } // End of foreach (string ShipReplicationFolders in ShipReplicationFolder)
                      //// End of foreach (int AckIMO in AckIMOInt)

                    /*
                    //if (ACKFromShipReplicationCount != 0)
                    if (ACKFromShipReplicationThisReplicationCount > 0)
                    {
                        FromShipReplicationIMOPath = EnquiryRegistryFolderPath("OutboundQueue") + AckIMO.ToString();
                        foreach (string StrIMOName in Directory.GetFiles(FromShipReplicationIMOPath))
                        {
                            if (StrIMOName.Split('.').Last() == "aco")
                            {
                                //if (ProcessAckFromOffice(StrIMOName, StrIMOName.Split('\\').Last(), AckIMO))
                                //{
                                    if (ACKFromShipReplicationCount > 1)
                                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgements to Ship have been sent in this batch process!";
                                    else if (ACKFromShipReplicationCount == 1)
                                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgement to Ship has been sent in this batch process!";

                                //}

                                string TempDateStr = DateTime.Now.ToString("HHmmss");

                                System.IO.File.Copy(StrIMOName, EnquiryRegistryFolderPath("OutboundArchive") + StrIMOName.Split('\\').Last().Substring(0, StrIMOName.Split('\\').Last().Length - StrIMOName.Split('\\').Last().Split('.').Last().Length - 1) + "_" + TempDateStr + ".aco");

                                /*
                                System.IO.File.Copy(StrIMOName, NMInPathStr + @"_ProcessedFiles\" + StrIMOName.Split('\\').Last().Substring(0, StrIMOName.Split('\\').Last().Length - StrIMOName.Split('\\').Last().Split('.').Last().Length - 1) + "_" + TempDateStr + ".aco");
                                */
                    /*
                }
                else
                {
                    if (ACKFromShipReplicationCount > 1)
                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgements have been replied to Ship in this batch process!";
                    else if (ACKFromShipReplicationCount == 1)
                        ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgement has been replied to Ship in this batch process!";

                }

                System.IO.File.Delete(StrIMOName);

            }
            */
                    /*
                    FromShipReplicationIMOPath = EnquiryRegistryFolderPath("InboundQueue") + AckIMO.ToString();
                    DirectoryInfo di = new DirectoryInfo(FromShipReplicationIMOPath);
                    di.Delete(true);
                    */

                    //}

                } // For-loop Multiple-Ship

                #endregion
            }

            //////////

            // Log end time tick
            long endTimeTicks = DateTime.Now.Ticks;
            long totalTimeTicks = endTimeTicks - startTimeTicks;
            TimeSpan ts = TimeSpan.FromTicks(totalTimeTicks);
            double totalTimeMinutes = ts.TotalMinutes;

            Console.WriteLine(endTimeTicks.ToString() + " End DMSSR...");
            Console.WriteLine("\nFinished processing files for DMSSR.");
            Console.WriteLine("Total time ticks: " + totalTimeTicks);
            Console.WriteLine("Total time (in minutes): " + totalTimeMinutes);
            Console.ReadLine();

            //////////

            //////////

            // Log starting time tick
            startTimeTicks = DateTime.Now.Ticks;
            Console.WriteLine("Task: Receive ACK, RHS, RMS . . . ");
            Console.WriteLine("Using TPL? " + useTPL.ToString().ToUpper());
            Console.WriteLine(startTimeTicks.ToString() + " Start ReceiveACK . . . ");
            Console.WriteLine("Processing files for " + ActiveIMO.Length + " ship(s). Please wait . . . ");

            //////////
            if (!useTPL)
            {
                // non-TPL implem
                for (int ShipLoopInt = 0; ShipLoopInt < ActiveIMO.Length; ShipLoopInt++)
                {
                    AckIMO = ActiveIMO[ShipLoopInt];
                    ReceiveACK(AckIMO);
                    //ReceiveRHS(AckIMO);
                    ReceiveRMS(AckIMO);

                } // For-loop Multiple-Ship

            }


            /////////

            else
            {
                // Implement TPL on Receive functions
                Task[] ReceiveTasks = new Task[ActiveIMO.Length];

                for (int ShipLoopInt = 0; ShipLoopInt < ActiveIMO.Length; ShipLoopInt++)
                {
                    AckIMO = ActiveIMO[ShipLoopInt];

                    //ReceiveACK(AckIMO);
                    ReceiveTasks[ShipLoopInt] = Task.Factory.StartNew(
                        (Object obj) =>
                        {
                            FileHandlerObj fhObj = obj as FileHandlerObj;

                            ReceiveACK(ActiveIMO[fhObj.index]);
                            ReceiveRHS(ActiveIMO[fhObj.index]);
                            ReceiveRMS(ActiveIMO[fhObj.index]);
                        },
                        new FileHandlerObj() { index = ShipLoopInt });


                } // For-loop Multiple-Ship

                Task.WaitAll(ReceiveTasks);
            }


            //////////

            // Log end time tick
             endTimeTicks = DateTime.Now.Ticks;
             totalTimeTicks = endTimeTicks - startTimeTicks;
             ts = TimeSpan.FromTicks(totalTimeTicks);
             totalTimeMinutes = ts.TotalMinutes;

            Console.WriteLine(endTimeTicks.ToString() + " End ReceiveACK...");
            Console.WriteLine("\nFinished processing files for " + ActiveIMO.Length + " ship(s).");
            Console.WriteLine("Total time ticks: " + totalTimeTicks);
            Console.WriteLine("Total time (in minutes): " + totalTimeMinutes);
            Console.ReadLine();

            //////////

            //foreach (var task in ReceiveACKTasks)
            //{

            //    if (task != null)
            //        Console.WriteLine("Task ID: {0}", task.Id);
            //}

            //Console.ReadLine();

            if (ACKFromShipReplicationCount == 0)
                ACKFromShipReplicationStatus = "No Acknowledgement has been replied to Ship in this batch process!";
            else
            {
                if (ACKFromShipReplicationCount > 1)
                    ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgements have been replied to Ship in this batch process!";
                else if (ACKFromShipReplicationCount == 1)
                    ACKFromShipReplicationStatus = ACKFromShipReplicationCount + " Acknowledgement has been replied to Ship in this batch process!";

            }

            return ACKFromShipReplicationStatus;

        }

        public string SBFSPFolderCheck(int IMOInt, int nmUIDInt)
        {
            string ReturnStr = string.Empty;

            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("spGetShipFolderSPList", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@pIMO", SqlDbType.Int).Value = IMOInt;
                    cmd.Parameters.Add("@pnmUniqueId", SqlDbType.Int).Value = nmUIDInt;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        if (dr.HasRows) {

                            ReturnStr = dr[0].ToString();

                            if (ReturnStr == "0")
                                ReturnStr = string.Empty;

                        }

                    }

                    conn.Close();
                }

            }

            return ReturnStr;
        }

        public void SBFSPFolderAdd(int IMOInt, int nmUIDInt, string strspUniqueId)
        {
            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("spAddShipFolderSPList", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@pIMO", SqlDbType.Int).Value = IMOInt;
                    cmd.Parameters.Add("@pnmUniqueId", SqlDbType.Int).Value = nmUIDInt;
                    cmd.Parameters.Add("@pspUniqueId", SqlDbType.VarChar).Value = strspUniqueId.ToLower();
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                }

            }

        }

        public string EnquiryRegistryFolderPath(string RegistryKeyStr)
        {
            String ReturnKeyStr = string.Empty;

            //bool WorkInLocal = true;
            //bool WorkInLocal = false;

            //string TempStr = string.Empty;

            return erfpHelper.get(RegistryKeyStr);
            
            // return @"C:\AE IT Project\NauticSystems" + TempStr + @"\NauticSystems\";

        }    

        public string ConvertIntToString(int SourceInt, int MaxStrLen)
        {
            if (MaxStrLen > 4)
                return "Max String Lenght is set to 4";
            else
            {

                if (SourceInt >= Math.Pow(10, 5))
                    return "Max String Lenght is set to 4";
                else if (SourceInt >= Math.Pow(10, 4))
                    return SourceInt.ToString();
                else if (SourceInt >= Math.Pow(10, 3))
                {
                    if (MaxStrLen == 3)
                        return "Max String Lenght is set to 3";
                    else
                        return SourceInt.ToString();

                }
                else if (SourceInt >= Math.Pow(10, 2))
                {
                    if (MaxStrLen == 3)
                        return SourceInt.ToString();
                    else
                        return "0" + SourceInt.ToString();

                }
                else if (SourceInt >= Math.Pow(10, 1))
                {
                    if (MaxStrLen == 3)
                        return "0" + SourceInt.ToString();
                    else
                        return "00" + SourceInt.ToString();

                }
                else
                {
                    if (MaxStrLen == 3)
                        return "00" + SourceInt.ToString();
                    else
                        return "000" + SourceInt.ToString();
                }


                /*
                if (SourceInt >= Math.Pow(10, 4))
                    return "Max String Lenght is set to 4";
                else if (SourceInt >= Math.Pow(10, 3))
                    return "Max String Lenght is set to 3";
                else if (SourceInt >= Math.Pow(10, 2))
                    return SourceInt.ToString();
                else if (SourceInt >= Math.Pow(10, 1))
                    return "0" + SourceInt.ToString();
                else
                    return "00" + SourceInt.ToString();
                    */
            }

        }

        public List<class_sharepointlist> SearchVesselSPInfos(int IMO)
        {
            string strSearch = "-" + IMO.ToString();

            //ClientContext clientContext = new ClientContext(@"http://oneview.angloeastern.com/sites/oneview/nsdl");
            ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl");
            //clientContext.Credentials = System.Net.CredentialCache.DefaultCredentials;
            //clientContext.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
            clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");
            ListCollection collList = clientContext.Web.Lists;
            clientContext.Load(collList);
            clientContext.ExecuteQuery();


            List<class_sharepointlist> listItem = new List<class_sharepointlist>();
            foreach (List list in collList)
            {

                string strSPList = list.Title;
                strSPList = strSPList.Replace("&", "&amp;"); // Do not change the original document list NAME even it has ( or - character 
                int listctr = 0;

                if (list.BaseType.ToString() == "DocumentLibrary" && (list.Title == "NSG01" || list.Title == "NSG02" || list.Title == "NSG03"))
                {
                    //start
                    bool isControlled = list.EnableVersioning;
                    List sharedDocumentsList = clientContext.Web.Lists.GetByTitle(list.Title.ToString());
                    CamlQuery camlQuery = new CamlQuery();
                    //camlQuery.ViewXml = @"<View Scope='RecursiveAll'><Query><Where><Contains><FieldRef Name='FileLeafRef'/><Value Type='Text'>search</Value></Contains></Where></Query></View>";
                    camlQuery.ViewXml = @"<View Scope='RecursiveAll'><Query><Where><Contains><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + strSearch + @"</Value></Contains></Where></Query></View>";

                    ListItemCollection listItems = sharedDocumentsList.GetItems(camlQuery);
                    clientContext.Load(listItems);
                    clientContext.ExecuteQuery();
                    foreach (var listItemx in listItems)
                    {
                        //    var sss = item.FieldValues["Title"].ToString();
                        //    sss = item["FileLeafRef"].ToString();
                        class_sharepointlist item = new class_sharepointlist();
                        item.isControlledDocument = isControlled;
                        item.strFileName = listItemx["FileLeafRef"].ToString();
                        item.strModified = Convert.ToDateTime(listItemx["Modified"]);


                        var childIdFieldEditor = listItemx["Editor"] as FieldLookupValue;
                        if (childIdFieldEditor != null)
                        {
                            var childIdEditor_Value = childIdFieldEditor.LookupValue;
                            item.strModifiedBy = childIdEditor_Value;
                        }

                        else
                        {
                            var childIdEditor_Value = "";
                            item.strModifiedBy = childIdEditor_Value;
                        }


                        //item.strVersion = listItemx["ContentVersion"].ToString();
                        item.strVersion = listItemx["_UIVersionString"].ToString();
                        item.strFileDirRef = listItemx["FileDirRef"].ToString();
                        item.strUniqueID = "";  //listItemx["UniqueID"].ToString().Split('#')[1];
                        item.strBaseFileName = class_nsdlFunction.getBaseName(listItemx["FileLeafRef"].ToString());

                        var childIdFieldCheckOut = listItemx["CheckoutUser"] as FieldLookupValue;
                        if (childIdFieldCheckOut != null)
                        {
                            var childIdEditor_Value = childIdFieldEditor.LookupValue;
                            if (childIdEditor_Value != null)
                            {
                                item.strStatus = "Checked-Out";
                            }
                        }
                        else
                        {
                            item.strStatus = "Checked-In";
                        }

                        item.strApprovedBy = string.Empty;
                        item.strContentType = "";// listItemx["ContentType"].ToString();
                        try
                        {
                            item.strDocType = item.strFileName.Substring(item.strFileName.Length - 4, 4);

                            switch (item.strDocType.ToLower())
                            {
                                case ".doc":
                                case "docx":
                                    item.strDocType = "Microsoft Word";
                                    break;
                                case ".xls":
                                case "xlsx":
                                    item.strDocType = "Microsoft Excel";
                                    break;
                                case ".pdf":
                                    item.strDocType = "Acrobat PDF";
                                    break;
                            }
                        }
                        catch
                        {
                            item.strDocType = "";
                        }
                        item.ictr = listctr;
                        listctr++;
                        listItem.Add(item);
                    }
                    //end

                }
            }

            return listItem;

        }

        public string ResendAcknowledgement(int IMOInt, int RepIDInt, string ReplicationStatusStr)
        {
            string ResendStatusStr = string.Empty;

            int[] IMOArray = new int[] { IMOInt };
            string nmRepRefStr = string.Empty;
            string nmServerUrlStr = string.Empty;
            string nmDocumentLibraryStr = string.Empty;
            class_UploadFile UploadFileInfo = new class_UploadFile();

            //Enquiry from SQL
            //string strConnString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

            string strConnString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
            SqlConnection con = new SqlConnection(strConnString);

            SqlCommand cmd = new SqlCommand();

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "sprepAckShipToShoreResend";

            cmd.Parameters.Add("@pnmRepID", SqlDbType.Int).Value = RepIDInt;
            cmd.Parameters.Add("@pIMO", SqlDbType.Int).Value = IMOInt;
            cmd.Connection = con;

            try
            {
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.IsDBNull(0))
                        ResendStatusStr = "Error in SQL Enquiry! Please contact System Administrator!";
                    else
                    {
                        nmRepRefStr = dr["nmRepRef"].ToString();
                        nmServerUrlStr = dr["nmServerUrl"].ToString();
                        nmDocumentLibraryStr = dr["nmDocumentLibrary"].ToString();

                    }

                }
                //Console.WriteLine("Record enquiried successfully");

            }
            catch (System.Exception ex)
            {
                //throw ex;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }

            //UploadFileInfo.DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + nmDocumentLibraryStr.Split('.').First() + nmServerUrlStr;
            UploadFileInfo.DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + nmDocumentLibraryStr.Split('.').First() + nmServerUrlStr;

            ResendStatusStr = CreateReplication(UploadFileInfo.DestinationUrl, nmRepRefStr.Substring(0, 5), IMOArray);



            return ResendStatusStr;
        }

        public string GetSPFileInfosByUniqueId(string UIDStr, string AttribStr)
        {
            try
            {
                //using (ClientContext clientContext = new ClientContext(@"http://oneview.angloeastern.com/sites/oneview/nsdl"))
                using (ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl"))
                {
                    //clientContext.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    //clientContext.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                    clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");

                    Microsoft.SharePoint.Client.File FN = clientContext.Web.GetFileById(Guid.Parse(UIDStr));
                    clientContext.Load(FN);
                    clientContext.ExecuteQuery();

                    class_replicationlist CurrentDoc = new class_replicationlist();

                    switch (AttribStr)
                    {
                        case "spBaseName":
                            return FN.Name;
                        case "spServerUrl":
                            return FN.ServerRelativeUrl;
                        case "spLinkFilename":
                            return FN.ServerRelativeUrl.Split('/').Last();
                        case "DLName":
                            CurrentDoc = GetSPDocInfos(FN.ServerRelativeUrl, "File", "Existing");
                            return CurrentDoc.strnmDocumentLibrary;
                        case "PID":
                            CurrentDoc = GetSPDocInfos(FN.ServerRelativeUrl, "File", "Existing");
                            return CurrentDoc.strspParentUniqueId;
                        case "Latest Version Number":
                            return FN.MajorVersion.ToString() + ".0";
                        default:
                            return string.Empty;
                    }

                }
            }
            catch
            {
                return string.Empty;
            }

        }

        public List<string> SPListItemsUpdate(string DocumentType, string UpdateType, string folPath, string ListNameStr, string UpdateStr, string RepModCodeStr, string ApprovalStatusStr, int SelectedIMOInt, string IsControlled = null)
        {
            /*
             *  'DocumentType' = "File" / "Folder"
             *
             *  'UpdateType' =
             *      RemoteDelete -- RemoteDelete an item.
             *      Delete -- Delete an item.
             *      New -- Create an item.
             *      Update -- Modify an item.
             *      Check -- Check whether the item exists in SP
             *      Move -- Move an item.
             *
             */

            string ldappath = "LDAP://hkgsrvdc1.angloeasterngroup.com:389";

            UpdateStr = UpdateStr.Trim();
            string itemUniqueId = "";
            string SPListItemsUpdateStatus = string.Empty;
            int[] SelectedIMO = null;

            if (SelectedIMOInt < 0)
                SelectedIMO = new int[] { SelectedIMOInt };
            else
                SelectedIMO = getAllActiveVessels();

            /*
            for (int i = 0; i < SelectedIMO.Length; i++)
            {
                //DMSAR17120149732589171222_
                //RemoveTRGFile(DirectoryShiftLevels(EnquiryRegistryFolderPath("OutboundQueue"), 1), RepModCodeStr + DateTime.Today.ToString("yyMM") + ConvertIntToString(getRepSr(RepModCodeStr, DateTime.Today), 3) + SelectedIMO[i].ToString() + DateTime.Today.ToString("yyMMdd") + "_", true);
                RemoveTRGFile(EnquiryRegistryFolderPath("OutboundQueue"), RepModCodeStr + DateTime.Today.ToString("yyMM") + ConvertIntToString(getRepSr(RepModCodeStr, DateTime.Today), 3) + SelectedIMO[i].ToString() + DateTime.Today.ToString("yyMMdd") + "_", true);
            }*/

            if (string.IsNullOrEmpty(IsControlled)) { }
            else
            {   // action on document library
                //using (ClientContext clientContext = new ClientContext(@"http://oneview.angloeastern.com/sites/oneview/nsdl"))
                using (ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl"))
                {
                    bool controlled = IsControlled.ToLower() == "true";

                    switch (UpdateType)
                    {
                        case "New":
                            try
                            {
                                Microsoft.SharePoint.Client.Folder parentFolder = clientContext.Web.GetFolderByServerRelativeUrl(UpdateStr);
                                clientContext.Load(parentFolder);
                                clientContext.ExecuteQuery();

                                SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' exists.  Please choose another name for " + UpdateType + " Folder!";
                                goto doReturn;
                            }
                            catch
                            {
                                ListCreationInformation lci = new ListCreationInformation();
                                lci.Title = UpdateStr;

                                ListTemplate lt = clientContext.Site.GetCustomListTemplates(clientContext.Web).GetByName("NauticDocumentLibraryTemplate");
                                clientContext.Load(lt);
                                clientContext.ExecuteQuery();

                                lci.TemplateFeatureId = lt.FeatureId;
                                lci.TemplateType = lt.ListTemplateTypeKind;

                                List newLib = clientContext.Web.Lists.Add(lci);
                                clientContext.Load(newLib);
                                //"Nautic Folder";
                                newLib.ContentTypes.AddExistingContentType(clientContext.Web.ContentTypes.GetById("0x012000575D42E94EDED342801B316F180E4AAF"));
                                //"Nautic Document";
                                newLib.ContentTypes.AddExistingContentType(clientContext.Web.ContentTypes.GetById("0x010100DCAFE395F19D5946A19C2B88EE46132C"));
                                newLib.Update();
                                clientContext.ExecuteQuery();

                                // Remove default Content Types "Folder" and "Document"
                                ContentTypeCollection ctc = clientContext.Web.Lists.GetByTitle(UpdateStr).ContentTypes;
                                clientContext.Load(ctc);
                                clientContext.ExecuteQuery();

                                ctc.First(z => z.Name == "Document").DeleteObject();
                                ctc.First(z => z.Name == "Folder").DeleteObject();

                                itemUniqueId = newLib.Id.ToString();

                                if (controlled)
                                {
                                    clientContext.ExecuteQuery();

                                    SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' has been created successfully.";
                                    break;
                                }
                                else
                                {
                                    newLib.EnableVersioning = false;
                                    newLib.Update();
                                    clientContext.ExecuteQuery();

                                    SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' has been created successfully.";
                                    goto doReturn;
                                }
                            }
                        case "Update":
                            try
                            {
                                Microsoft.SharePoint.Client.Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(UpdateStr);
                                clientContext.Load(folder);
                                clientContext.ExecuteQuery();

                                folder = clientContext.Web.GetFolderById(folder.UniqueId);
                                clientContext.Load(folder);
                                clientContext.ExecuteQuery();

                                //return "Folder ' " + UpdateStr + " ' exists.  Please choose another name for " + UpdateType + " Folder!";

                                if (UpdateStr == folder.Name)
                                {
                                    SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' exists.  Please choose another name for " + UpdateType + " Folder!";
                                    goto doReturn;
                                }
                                else
                                {
                                    folder.MoveTo(UpdateStr + "_Temp");
                                    folder.Update();
                                    clientContext.ExecuteQuery();

                                    folder.MoveTo(UpdateStr);
                                    folder.Update();
                                    clientContext.ExecuteQuery();

                                    if (controlled)
                                    {
                                        SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' has been renamed successfully.";
                                        break;
                                    }
                                    else
                                    {
                                        SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' has been renamed successfully.";
                                        goto doReturn;
                                    }

                                }

                            }
                            catch
                            {
                                try
                                {
                                    Microsoft.SharePoint.Client.Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(ListNameStr);
                                    clientContext.Load(folder);
                                    clientContext.ExecuteQuery();

                                    folder.MoveTo(UpdateStr);
                                    folder.Update();
                                    clientContext.ExecuteQuery();

                                    if (controlled)
                                    {
                                        SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' has been renamed successfully.";
                                        break;
                                    }
                                    else
                                    {
                                        SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' has been renamed successfully.";
                                        goto doReturn;
                                    }
                                }
                                catch
                                {
                                    SPListItemsUpdateStatus = "Folder ' " + ListNameStr + " ' not found.";
                                    goto doReturn;
                                }
                            }
                        case "Delete":
                            try
                            {
                                Microsoft.SharePoint.Client.Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(ListNameStr);
                                clientContext.Load(folder);
                                clientContext.ExecuteQuery();

                                folder.DeleteObject();
                                folder.Update();
                                clientContext.ExecuteQuery();
                            }
                            catch { }

                            if (controlled)
                            {
                                SPListItemsUpdateStatus = "Folder ' " + ListNameStr + " ' has been deleted successfully.";
                                break;
                            }
                            else
                            {
                                SPListItemsUpdateStatus = "Folder ' " + ListNameStr + " ' has been deleted successfully.";
                                goto doReturn;
                            }
                    }

                    class_ReplicationFileFolderJSONFile JSONFile = AddSPDocumentLibraryInfoToJSON(UpdateStr);
                    // To be obtained after Replication Database has been updated
                    JSONFile.RepID = -1;
                    JSONFile.SourceURLStr = folPath;
                    JSONFile.UpdateType = UpdateType;
                    JSONFile.UpdateStr = UpdateStr;
                    //JSONFile.RepSPDocsInfo = .RepSPDocsInfo;
                    JSONFile.RepDistributionStatus = "CR";
                    CreateFileFolderReplication(JSONFile, RepModCodeStr, -1, false);

                    //return SPListItemsUpdateStatus;// + CreateFileFolderReplication(JSONFile, RepModCodeStr);
                    goto doReturn;
                }
            }

            string RelativeURLStr = string.Empty;
            string TempStr = string.Empty;

            switch (DocumentType)
            {

                case "Folder":

                    //RelativeURLStr = folPath.Replace("http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr, "");
                    //ListNameStr = getListName(folPath.Replace("http://oneview.angloeastern.com", ""));

                    RelativeURLStr = folPath.Replace("http://dms.angloeastern.com/sites/nsdl/" + ListNameStr, "");
                    ListNameStr = getListName(folPath.Replace("http://dms.angloeastern.com", ""));

                    ListNameStr = ListNameStr.Replace("&", "&amp;");
                    ListNameStr = ListNameStr.Replace("'", "&apos;");
                    ListNameStr = ListNameStr.Replace("(", "");
                    ListNameStr = ListNameStr.Replace(")", "");

                    //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + RelativeURLStr;
                    folPath = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + RelativeURLStr;
                    break;
                case "File":
                    /*
                    RelativeURLStr = folPath.Replace("http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr, "");
                    ListNameStr = getListName(folPath.Replace("http://oneview.angloeastern.com", ""));

                    ListNameStr = ListNameStr.Replace("&", "&amp;");
                    ListNameStr = ListNameStr.Replace("'", "&apos;");
                    ListNameStr = ListNameStr.Replace("(", "");
                    ListNameStr = ListNameStr.Replace(")", "");

                    folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + RelativeURLStr;*/
                    break;
                default:
                    break;
            }

            int SPIDInt = -1;

            //if (folPath != ("http://oneview.angloeastern.com/sites/oneview/nsdl/"+ ListNameStr))
            //{
            switch (DocumentType)
            {

                case "Folder":
                    //ListNameStr = getListName(folPath.Replace("http://oneview.angloeastern.com", "")); // Do not change the original document list NAME even it has ( or - character 
                    ListNameStr = getListName(folPath.Replace("http://dms.angloeastern.com", "")); // Do not change the original document list NAME even it has ( or - character 
                    break;
                case "File":
                    //ListNameStr = getListName(folPath.Replace("http://oneview.angloeastern.com", "")); // Do not change the original document list NAME even it has ( or - character 
                    break;
                default:
                    break;
            }

            SPIDInt = GetSPID(DocumentType, folPath, ListNameStr, UpdateType, UpdateStr);

            //}

            class_replicationlist SPDocsInfo = new class_replicationlist();

            //if (SPIDInt >= 0)
            //{

            string RootFolderStr = string.Empty;

            switch (DocumentType)
            {

                case "Folder":
                    //RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview"));
                    RootFolderStr = folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites"));
                    break;
                case "File":
                    //RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")).Split('/').Last();
                    //RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview") - RootFolderStr.Length - 1);
                    RootFolderStr = folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites")).Split('/').Last();
                    RootFolderStr = folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites") - RootFolderStr.Length - 1);
                    break;
                default:
                    break;
            }

            int MethodInt = 0;

            using (var clientContext = new nsdlWebService.Lists())
            {

                //client.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //client.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");
                //client.Url = "http://oneview.angloeastern.com/sites/oneview/nsdl/_vti_bin/lists.asmx";
                clientContext.Url = "http://dms.angloeastern.com/sites/nsdl/_vti_bin/lists.asmx";

                /*Get Name attribute values (GUIDs) for list and view. */
                System.Xml.XmlNode ndListView = clientContext.GetListAndView(ListNameStr, "");

                /*
                XmlDocument TempXMLDoc = new XmlDocument();
                TempXMLDoc.LoadXml(ndListView.OuterXml);
                TempXMLDoc.Save(ProcessedFilesPath + "Temp.xml");
                */

                string strListID = ndListView.ChildNodes[0].Attributes["Name"].Value;
                string strViewID = ndListView.ChildNodes[1].Attributes["Name"].Value;

                /*Create an XmlDocument object and construct a Batch element and its attributes. Note that an empty ViewName parameter causes the method to use the default view. */
                System.Xml.XmlDocument XMLDoc = new System.Xml.XmlDocument();
                System.Xml.XmlElement batchElement = XMLDoc.CreateElement("Batch");
                batchElement.SetAttribute("OnError", "Continue");
                batchElement.SetAttribute("PreCalc", "TRUE");
                //if (folPath != ("http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr))
                if (folPath != ("http://dms.angloeastern.com/sites/nsdl/" + ListNameStr))
                    batchElement.SetAttribute("RootFolder", RootFolderStr);
                else
                    batchElement.SetAttribute("ViewName", strViewID);
                /*
                 *  Specify methods for the batch post using CAML. To update or delete, 
                 *  specify the ID of the item, and to update or add, specify 
                 *  the value to place in the specified column.
                 *
                 */

                MethodInt++;

                switch (DocumentType)
                {
                    case "Folder":
                        switch (UpdateType)
                        {
                            case "New":
                                if (GetSPID(DocumentType, folPath + "/" + UpdateStr, ListNameStr, UpdateType, UpdateStr) == -1)
                                {
                                    if (RepModCodeStr.IndexOf("DMSSR") >= 0)
                                        batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + UpdateType + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder created by Vessel Replication</Field></Method>";
                                    /*else
                                        batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + UpdateType + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder created by " + LoginCS.GetAttribute(ldappath, Session["uID"].ToString(), "displayName") + "</Field></Method>";*/
                                }
                                else
                                {
                                    //SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' existed.  Please choose another name for " + UpdateType + " Folder!";
                                    SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' exists.  Please try again!";
                                    goto doReturn;
                                }
                                break;



                            case "Update":

                                TempStr = folPath.Split('/').Last();
                                folPath = folPath.Substring(0, folPath.Length - TempStr.Length);

                                if (GetSPID(DocumentType, folPath + UpdateStr, ListNameStr, UpdateType, UpdateStr) == -1)
                                {
                                    /*
                                    if (GetSPID(DocumentType, folPath + UpdateStr, ListNameStr, "New", UpdateStr) == -1)
                                    {
                                        if (RepModCodeStr.IndexOf("DMSSR") >= 0)
                                            batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"'" + EncodeSPStrings(UpdateStr) + "\" by Vessel Replication</Field></Method>";
                                        else
                                            batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"'" + EncodeSPStrings(UpdateStr) + "\" by " + LoginCS.GetAttribute(ldappath, Application["uID"].ToString(), "displayName") + "</Field></Method>";

                                    } else
                                    {
                                        if (GetSPID(DocumentType, folPath + UpdateStr, ListNameStr, "New", UpdateStr) == SPIDInt)
                                        {
                                            if (RepModCodeStr.IndexOf("DMSSR") >= 0)
                                                batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"'" + EncodeSPStrings(UpdateStr) + "\" by Vessel Replication</Field></Method>";
                                            else
                                                batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"'" + EncodeSPStrings(UpdateStr) + "\" by " + LoginCS.GetAttribute(ldappath, Application["uID"].ToString(), "displayName") + "</Field></Method>";

                                        } else
                                        {
                                            //SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' existed.  Please choose another name for " + UpdateType + " Folder!";
                                            SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' exists.  Please try again!";
                                            return SPListItemsUpdateStatus;

                                        }

                                    }*/

                                    if (RepModCodeStr.IndexOf("DMSSR") >= 0)
                                        batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"'" + EncodeSPStrings(UpdateStr) + "\" by Vessel Replication</Field></Method>";
                                    /*else
                                        batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Folder renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"'" + EncodeSPStrings(UpdateStr) + "\" by " + LoginCS.GetAttribute(ldappath, Session["uID"].ToString(), "displayName") + "</Field></Method>";*/

                                }
                                else
                                {
                                    //SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' existed.  Please choose another name for " + UpdateType + " Folder!";
                                    SPListItemsUpdateStatus = "Folder ' " + UpdateStr + " ' exists.  Please try again!";
                                    goto doReturn;
                                }

                                break;
                            case "Check":

                                //if (GetSPID(DocumentType, folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), ListNameStr) == -1)
                                if (GetSPID(DocumentType, folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites")), ListNameStr, UpdateType, UpdateStr) == -1)
                                    SPListItemsUpdateStatus = "Folder NOT exists.";
                                else
                                    SPListItemsUpdateStatus = "Folder exists.";
                                goto doReturn;

                            case "Delete":
                            case "RemoteDelete":
                                //batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr.Replace("&", "&amp;") + "</Field></Method>";
                                if (ApprovalStatusStr == "Approved" || RepModCodeStr.IndexOf("DMSAR") < 0)
                                {
                                    batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field></Method>";
                                    //SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), DocumentType, UpdateType);
                                    SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites")), DocumentType, UpdateType);
                                }
                                /*else
                                {
                                    //int[] SelectedIMO = getAllActiveVessels();
                                    SPListItemsUpdateStatus = SendApprovalRequest(folPath, UpdateType, SelectedIMO, DocumentType, UpdateStr);
                                }*/
                                break;
                            default:
                                break;
                        }
                        break;
                    case "File":
                        switch (UpdateType)
                        {
                            case "Update":

                                TempStr = folPath.Split('/').Last();
                                folPath = folPath.Substring(0, folPath.Length - TempStr.Length);

                                //batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>11</Field><Field Name='owshiddenversion'>3</Field><Field Name='FileRef'>"+ RootFolderStr + "</Field><Field Name='BaseName'>" + UpdateStr + "</Field></Method>";
                                if (GetSPID(DocumentType, folPath + UpdateStr + "." + TempStr.Split('.').Last(), ListNameStr, UpdateType, UpdateStr) == -1)
                                {
                                    if (RepModCodeStr.IndexOf("DMSSR") >= 0)
                                        batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Document renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"" + EncodeSPStrings(UpdateStr) + "." + TempStr.Split('.').Last() + "\" by Vessel Replication</Field></Method>";
                                    /*else
                                        batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='" + UpdateType + "'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='BaseName'>" + EncodeSPStrings(UpdateStr) + "</Field><Field Name='Remarks'>Document renamed from \"" + EncodeSPStrings(TempStr) + "\" to \"" + EncodeSPStrings(UpdateStr) + "." + TempStr.Split('.').Last() + "\" by " + LoginCS.GetAttribute(ldappath, Session["uID"].ToString(), "displayName") + "</Field></Method>";*/

                                }
                                else
                                {
                                    //SPListItemsUpdateStatus = "File ' " + UpdateStr + "." + TempStr.Split('.').Last() + " ' existed.  Please choose another name for " + UpdateType + " File!";
                                    SPListItemsUpdateStatus = "File ' " + UpdateStr + "." + TempStr.Split('.').Last() + " ' exists.  Please try again!";
                                    goto doReturn;
                                }

                                break;
                            case "Check":

                                //RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview"));
                                //if (GetSPID(DocumentType, folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), ListNameStr) == -1)

                                RootFolderStr = folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites"));
                                if (GetSPID(DocumentType, folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites")), ListNameStr, UpdateType, UpdateStr) == -1)
                                    SPListItemsUpdateStatus = "File NOT exists.";
                                else
                                    SPListItemsUpdateStatus = "File exists.";

                                goto doReturn;

                            case "Delete":
                            case "RemoteDelete":
                                //RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview"));
                                RootFolderStr = folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites"));
                                batchElement.InnerXml = "<Method ID='" + MethodInt + "' Cmd='Delete'><Field Name='ID'>" + SPIDInt + "</Field><Field Name='FileRef'>" + EncodeSPStrings(RootFolderStr) + "</Field></Method>";
                                //SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), DocumentType, UpdateType);
                                SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites"), folPath.Length - folPath.IndexOf("/sites")), DocumentType, UpdateType);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;

                }

                if (ApprovalStatusStr == "Approved")
                {
                    /*Update list items. This example uses the list GUID, which is recommended, but the list display name will also work.*/
                    try
                    {
                        System.Xml.XmlNode ResultXML = clientContext.UpdateListItems(strListID, batchElement);

                        /*
                        // Save SPListUpdateResult to XML for replication
                        XmlDocument newXMLDoc = new XmlDocument();
                        newXMLDoc.LoadXml(ResultXML.OuterXml);
                        newXMLDoc.Save(ProcessedFilesPath + "SPListUpdateResult.xml");
                        */

                        // Create Auto Replication Request to All Vessels

                        switch (DocumentType)
                        {
                            case "Folder":
                                switch (UpdateType)
                                {
                                    case "New":
                                        folPath = folPath + "/" + UpdateStr;
                                        //SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), DocumentType, UpdateType);
                                        SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")), DocumentType, UpdateType);
                                        itemUniqueId = SPDocsInfo.strspUniqueId;
                                        break;
                                    case "Update":
                                        folPath = folPath.Substring(0, folPath.Length - folPath.Split('/').Last().Length) + UpdateStr;
                                        //SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), DocumentType, UpdateType);
                                        SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")), DocumentType, UpdateType);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "File":
                                switch (UpdateType)
                                {
                                    case "Update":
                                        folPath = folPath.Substring(0, folPath.Length - folPath.Split('/').Last().Length) + UpdateStr + "." + TempStr.Split('.').Last();
                                        //SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), DocumentType, UpdateType);
                                        SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")), DocumentType, UpdateType);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;

                        }

                        if (RepModCodeStr.IndexOf("DMSMR") < 0)
                        {
                            class_ReplicationFileFolderJSONFile JSONFile = new class_ReplicationFileFolderJSONFile();
                            // To be obtained after Replication Database has been updated
                            JSONFile.RepID = -1;
                            JSONFile.SourceURLStr = folPath;
                            JSONFile.UpdateType = UpdateType;
                            JSONFile.UpdateStr = UpdateStr;
                            JSONFile.RepSPDocsInfo = SPDocsInfo;
                            JSONFile.RepDistributionStatus = "CR";

                            if (RepModCodeStr.IndexOf("DMSAR") >= 0)
                                SPListItemsUpdateStatus += CreateFileFolderReplication(JSONFile, RepModCodeStr, -1, false);
                            else
                            //DMSSR
                            {
                                SPListItemsUpdateStatus += CreateFileFolderReplication(JSONFile, RepModCodeStr, SelectedIMOInt, true);
                            }

                        }

                        // Only Return Action Done to User
                        //SPListItemsUpdateStatus = "\r\n" + UpdateType + " action for " + DocumentType + " has been done successfully.";
                        switch (UpdateType)
                        {
                            case "New":
                                SPListItemsUpdateStatus = "\r\n" + DocumentType + " ' " + UpdateStr + " ' has been created successfully.";
                                break;
                            case "Update":
                                SPListItemsUpdateStatus = "\r\n" + DocumentType + " ' " + UpdateStr + " ' has been renamed successfully.";
                                break;
                            case "Delete":
                                SPListItemsUpdateStatus = "\r\n" + DocumentType + " ' " + SPDocsInfo.strspBaseName + " ' has been deleted successfully.";
                                break;
                            default:
                                break;

                        }

                    }
                    catch (System.Exception ex)
                    {
                        SPListItemsUpdateStatus = ex.Message;
                        goto doReturn;
                        //return "Excpetion";
                    }

                }
                

            }

            doReturn:
            if (DocumentType == "Folder" && itemUniqueId.Length > 0)
            {
                itemUniqueId = itemUniqueId.Replace("}", "").Replace("{", "").ToLower();
            }
            List<string> returnValue = new List<string>();
            returnValue.Add(SPListItemsUpdateStatus);
            returnValue.Add(itemUniqueId);
            return returnValue;

            /*} else
            {
                SPListItemsUpdateStatus = "Error in performing " + UpdateType + " action for " + DocumentType + ".";
                return SPListItemsUpdateStatus;
            }*/

        }

        private DateTime GetSPFolderLMDByUniqueId(string UIDStr)
        {
            try
            {   
                //using (ClientContext clientContext = new ClientContext(@"http://oneview.angloeastern.com/sites/oneview/nsdl"))
                using (ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl"))
                {
                    //clientContext.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    //clientContext.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                    clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");

                    Microsoft.SharePoint.Client.Folder fol = clientContext.Web.GetFolderById(Guid.Parse(UIDStr));
                    clientContext.Load(fol);
                    clientContext.ExecuteQuery();

                    return fol.TimeLastModified;
                }
            }
            catch
            {
                return DateTime.Now;
            }

        }

        public string GetSPServerURLByUniqueId(string UIDStr)
        {
            try
            {
                //using (ClientContext clientContext = new ClientContext(@"http://oneview.angloeastern.com/sites/oneview/nsdl"))
                using (ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl"))
                {
                    //clientContext.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    //clientContext.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                    clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");

                    Microsoft.SharePoint.Client.Folder fol = clientContext.Web.GetFolderById(Guid.Parse(UIDStr));
                    clientContext.Load(fol);
                    clientContext.ExecuteQuery();

                    return fol.ServerRelativeUrl;
                }
            }
            catch
            {
                return string.Empty;
            }

        }

        public string SPMultipleUploadFile(List<class_UploadFile> UploadFileInfos, string RepModCodeStr, bool SingleFileUpload, int UploadModeInt = 0)
        {
            /*
             * UploadModeInt
             * 0 - Upload (Default)
             * 1 - Ship Replication 
             * 2 - Copied From Ship Board Filing
             */
            string UploadStatus = string.Empty;
            bool ExistedSourceUrl = false;
            //int BatchIDint = -1;
            int CountSuccessUploadInt = 0;
            //int UploadModeInt = 0;

            /*
            //Enquiry from SQL
            string strConnString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            SqlConnection con = new SqlConnection(strConnString);

            SqlCommand cmd = new SqlCommand();

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "spApproverTicketSet";
            cmd.Parameters.Add("@pUserId", SqlDbType.VarChar).Value = Session["uID"].ToString();
            cmd.Connection = con;

            try
            {
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.IsDBNull(0))
                    {
                        UploadStatus = "Error in equiry from Approval DataTable! Please contact System Administration!";
                        return UploadStatus;

                    }
                    else
                        BatchIDint = Convert.ToInt32(dr[0].ToString());

                }
                Console.WriteLine("Record enquiried successfully");

            }
            catch (System.Exception ex)
            {
                //throw ex;
                UploadStatus = "Error in equiry from Approval DataTable! Please contact System Administration!";
                return UploadStatus;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
            */

            using (var clientContext = new nsdlWebService1.Copy())
            {
                string ldappath = "LDAP://hkgsrvdc1.angloeasterngroup.com:389";

                //client.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //client.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");
                //client.Url = "http://oneview.angloeastern.com/sites/oneview/nsdl/_vti_bin/Copy.asmx";
                clientContext.Url = "http://dms.angloeastern.com/sites/nsdl/_vti_bin/Copy.asmx";

                for (int i = 0; i < UploadFileInfos.Count; i++)
                {
                    int TimeDelaySecondInt = 5; // Sleep for N seconds to cater for laterncy by System.IO

                    //string ListNameStr = UploadFileInfos[i].DestinationUrl.Replace("http://oneview.angloeastern.com/sites/oneview/nsdl/", "").Split('/').First();
                    //string RelativeURLStr = UploadFileInfos[i].DestinationUrl.Replace("http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr, "");
                    //ListNameStr = getListName(UploadFileInfos[i].DestinationUrl.Replace("http://oneview.angloeastern.com", ""));

                    string ListNameStr = UploadFileInfos[i].DestinationUrl.Replace("http://dms.angloeastern.com/sites/nsdl/", "").Split('/').First();
                    string RelativeURLStr = UploadFileInfos[i].DestinationUrl.Replace("http://dms.angloeastern.com/sites/nsdl/" + ListNameStr, "");
                    ListNameStr = getListName(UploadFileInfos[i].DestinationUrl.Replace("http://dms.angloeastern.com", ""));

                    ListNameStr = ListNameStr.Replace("&", "&amp;");
                    ListNameStr = ListNameStr.Replace("'", "&apos;");
                    ListNameStr = ListNameStr.Replace("(", "");
                    ListNameStr = ListNameStr.Replace(")", "");

                    //UploadFileInfos[i].DestinationUrl = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + ListNameStr + RelativeURLStr;

                    if (ListNameStr.IndexOf('-') >= 0)
                        ListNameStr = ListNameStr.Replace("-", "");

                    UploadFileInfos[i].DestinationUrl = "http://dms.angloeastern.com/sites/nsdl/" + ListNameStr + RelativeURLStr;

                    string[] DestinationUrls = { UploadFileInfos[i].DestinationUrl };
                    string DestinationNameStr = UploadFileInfos[i].DestinationUrl.Split('/').Last();

                    var FileFieldInformation = new nsdlWebService1.FieldInformation();

                    switch (UploadModeInt)
                    {

                        case 1:
                            FileFieldInformation = new nsdlWebService1.FieldInformation()
                            {
                                DisplayName = "Remarks",
                                InternalName = "Remarks",
                                Type = nsdlWebService1.FieldType.Text,
                                Value = "Document replicated from Vessel"
                            };
                            break;
                        case 2:
                            FileFieldInformation = new nsdlWebService1.FieldInformation()
                            {
                                DisplayName = "Remarks",
                                InternalName = "Remarks",
                                Type = nsdlWebService1.FieldType.Text,
                                Value = "Document copied from Ship Board Filing"// by " + GetAttribute(ldappath, Session["uID"].ToString(), "displayName")
                            };
                            break;
                        default:
                            FileFieldInformation = new nsdlWebService1.FieldInformation()
                            {
                                DisplayName = "Remarks",
                                InternalName = "Remarks",
                                Type = nsdlWebService1.FieldType.Text,
                                Value = "Document uploaded" // by " + GetAttribute(ldappath, Session["uID"].ToString(), "displayName")
                            };
                            break;
                    }
                    nsdlWebService1.FieldInformation[] FileFieldInfo = { FileFieldInformation };

                    UploadFileInfos[i].SourceUrl = ProcessedFilesPath + UploadFileInfos[i].SourceUrl;

                    var copyResult = new nsdlWebService1.CopyResult();
                    nsdlWebService1.CopyResult[] copyResults = { copyResult };


                    //Get the document from SharePoint
                    //uint myGetUint = client.GetItem(folPath, out FileFieldInfo, out UploadedByteArray);

                    //byte[] SourceUrlContents = Encoding.ASCII.GetBytes(SourceUrlContent);
                    //byte[] SourceUrlContents = System.Convert.FromBase64String(SourceUrlContent);

                    /*  Parameters

                        SourceUrl - A String that contains the absolute source URL of the document to be copied.

                        DestinationUrls - An array of Strings that contain one or more absolute URLs specifying the destination location or locations of the copied document.

                        Fields - An array of FieldInformation objects that define and optionally assign values to one or more fields associated with the copied document.

                        Stream - An array of Bytes that contain the document to copy using base-64 encoding.

                        Results - An array of CopyResult objects, passed as an out parameter.

                    */

                    try
                    {
                        do
                        {
                            if (System.IO.File.Exists(@UploadFileInfos[i].SourceUrl))
                            {
                                //Copy the document from Local to SharePoint
                                byte[] SourceUrlContents = System.IO.File.ReadAllBytes(@UploadFileInfos[i].SourceUrl);
                                uint success = clientContext.CopyIntoItems(UploadFileInfos[i].SourceUrl, DestinationUrls, FileFieldInfo, SourceUrlContents, out copyResults);
                                if (success == 0)
                                {
                                    /*string CheckInStatus = SPCheckInFile(UploadFileInfos[i].DestinationUrl, "File uploaded by Nautic System");

                                    using (Microsoft.SharePoint.SPSite osite = new Microsoft.SharePoint.SPSite("http://oneview.angloeastern.com"))
                                    {
                                        using (Microsoft.SharePoint.SPWeb oweb = osite.OpenWeb())
                                        {
                                            /*
                                            SPList olist = oweb.Lists["Controlled Document"];
                                            SPListItem oitem = olist.Items[0];

                                            StartSPWorkflow(oitem, osite, "UploadForCheckIn");
                                            /*
                                            SPWorkflowManager manager = site.WorkflowManager;
                                            SPWorkflowAssociation association = list.WorkflowAssociations[0];
                                            string data = association.AssociationData;
                                            SPWorkflow wf = manager.StartWorkflow(item, association, data, true);
                                            
                                        }
                                    }
                                    */

                                    /*if (RepModCodeStr == "DMSAR")
                                    {
                                        int[] SelectedIMO = getAllActiveVessels();
                                        //string TempStr = CreateReplication(UploadFileInfos[i].DestinationUrl, RepModCodeStr, SelectedIMO);

                                        string TempStr = SendApprovalRequest(UploadFileInfos[i].DestinationUrl, "Upload", SelectedIMO, "File", null, BatchIDint);

                                    }*/

                                    UploadStatus += i.ToString() + " ";
                                    ExistedSourceUrl = true;
                                    CountSuccessUploadInt++;

                                }
                                else
                                {
                                    UploadStatus += i.ToString() + " but the file '" + @UploadFileInfos[i].SourceUrl + "' cannot be checked-in ";
                                }



                            }
                            else
                            {
                                Thread.Sleep(TimeDelaySecondInt * 1000);

                                //UploadStatus += i.ToString() + " but the file '" + @UploadFileInfos[i].SourceUrl + "' is not existed ";

                            }

                        } while (!ExistedSourceUrl);

                        ExistedSourceUrl = false;

                    }
                    catch (System.Exception exc)
                    {
                        UploadStatus += "Exception: " + exc.ToString() + " " + i.ToString() + " ";

                        return "Exception in SPMultipleUploadFile";
                    }

                    if ((i == (UploadFileInfos.Count - 1)) && (CountSuccessUploadInt > 0))
                    {
                        if ((CountSuccessUploadInt <= UploadFileInfos.Count) && (RepModCodeStr == "DMSAR"))
                        {
                            if (i > 0)
                                UploadStatus = (i + 1) + " files have been uploaded for approval!";
                            else
                                UploadStatus = "File '" + UploadFileInfos[i].SourceUrl.Split('\\').Last() + "' has been uploaded for approval!";

                        }
                        else if ((CountSuccessUploadInt <= UploadFileInfos.Count) && (RepModCodeStr != "DMSAR"))
                        {
                            if (i > 0)
                                UploadStatus = (i + 1) + " files have been uploaded successfully!";
                            else
                                UploadStatus = "File '" + UploadFileInfos[i].SourceUrl.Split('\\').Last() + "' has been uploaded successfully!";

                        }

                    }

                } // End of FOor-Loop

            } // End of Using

            for (int i = 0; i < UploadFileInfos.Count; i++)
            {
                if (System.IO.File.Exists(@UploadFileInfos[i].SourceUrl))
                    System.IO.File.Delete(@UploadFileInfos[i].SourceUrl);
            }

            /*
            //Enquiry from SQL
            strConnString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            con = new SqlConnection(strConnString);

            cmd = new SqlCommand();

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "spApproverTicketReset";
            cmd.Parameters.Add("@pUserId", SqlDbType.NVarChar).Value = Session["uID"].ToString();
            cmd.Connection = con;

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                //throw ex;
                UploadStatus = "Error in resetting Approval DataTable! Please contact System Administration!";
                return UploadStatus;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
            */

            /*if ((CountSuccessUploadInt > 0) && (RepModCodeStr == "DMSAR"))
            {
                if (!EmailApprovalRequest(BatchIDint, CountSuccessUploadInt))
                    UploadStatus = "Error in emailing to Approver! Please contact System Administration!";

            }*/

            return UploadStatus;

        }

        public string CreateReplication(string folPath, string RepModCodeStr, int[] SelectedIMO, string uniqueId = null, string DocumentType = "File")
        {
            CreateReplicationHandler(folPath, RepModCodeStr, SelectedIMO, uniqueId, DocumentType);

            string ReplicationStatus = string.Empty;

            //Replication is Ready
            switch (RepModCodeStr)
            {
                case "DMSAR":
                    ReplicationStatus += "\r\nAuto Replication request has been created successfully!!";
                    break;
                case "DMSMR":
                    ReplicationStatus += "\r\nManual Replication request has been created successfully!!";
                    break;
                case "DMSSR":
                    ReplicationStatus += "\r\nReplication Acknowledgement to Ship has been created successfully!!";
                    break;
                default:
                    break;
            }

            return ReplicationStatus;
        }

        public string CreateReplicationHandler(string folPath, string RepModCodeStr, int[] SelectedIMO, string uniqueId = null, string DocumentType = "File")
        {
            string DestinationNameStr = folPath.Split('/').Last();
            class_ReplicationFileFolderJSONFull JSONList = new class_ReplicationFileFolderJSONFull();
            JSONList.entries = new List<class_ReplicationFileFolderJSONFile>();
            class_ReplicationJSONHeader JSONHeader = new class_ReplicationJSONHeader();
            class_ReplicationFileFolderJSONFile JSONFile = new class_ReplicationFileFolderJSONFile();
            string JSONString = string.Empty;
            JavaScriptSerializer js = new JavaScriptSerializer();

            //Preparation for Replication
            //class_replicationlist SPDocsInfo = getNSDLSPFields(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")));
            //SPDocsInfo = cleanNSDLSPFields(SPDocsInfo);

            //class_replicationlist SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), DocumentType, "Existing", uniqueId);



            class_replicationlist SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")), DocumentType, "Existing", uniqueId);

            //int[] SelectedIMO = getAllActiveVessels();
            //int[] SelectedIMO = { 9697832, 9999997, 9999998, 9999999 };
            string RepDirectoryStr = string.Empty;
            string RepRefStr = string.Empty;
            string RepFolSrcStr = "OUT";
            string RepSMTPSrcStr = "nsoffice@oceanone.com";
            DateTime thisDay = DateTime.Today;
            string RepFullDateStr = thisDay.ToString("yyyy-MM-dd");
            string RepHeaderStatusStr = "Active";
            string RepYYMMStr = string.Empty;
            string RepYYMMDDStr = string.Empty;
            int RepSrInt = -1;
            string RepDocDistributionStatusStr = string.Empty;

            if (RepModCodeStr == "DMSSR")
            {
                RepRefStr = getnmRepSr(folPath);
                //RepRefStr = "DMSSR1709007170904100000024";
                RepSrInt = Convert.ToInt32(RepRefStr.Substring(9, 3));
                RepYYMMStr = RepRefStr.Substring(5, 4);
                RepYYMMDDStr = RepRefStr.Substring(12, 6);
                JSONFile.RepID = Convert.ToInt32(RepRefStr.Substring(18, 9));
                RepRefStr = RepRefStr.Substring(0, 12);
                RepDocDistributionStatusStr = "AC";
            }
            else
            {
                RepYYMMStr = thisDay.ToString("yyMM");
                RepYYMMDDStr = thisDay.ToString("yyMMdd");
                RepSrInt = getRepSr(RepModCodeStr, thisDay);
                RepDocDistributionStatusStr = "CR";
                if (DocumentType == "Folder")
                    JSONFile.UpdateType = "Existing";
            }

            //int RepSrInt = 500;
            int RepIMOInt = 0;
            string RepDOCUniqueIdStr = string.Empty;
            string JSONFileNameStr = "fatt.nsf";

            for (int i = 0; i < SelectedIMO.Length; i++)
            {
                if ((SelectedIMO[i] == SingleShipIMOInt) || MultipleShipTest)
                {
                    // Replication existed for today
                    if ((RepModCodeStr == "DMSSR") || (RepModCodeStr != "DMSSR" && (getRepSr(RepModCodeStr, thisDay) > getRepSr(RepModCodeStr, thisDay.AddDays(-1)))))
                    {
                        //Debug
                        //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "Before CreateDirectory(RepDirectoryStr)");

                        RepDirectoryStr = EnquiryRegistryFolderPath("OutboundQueue") + SelectedIMO[i].ToString() + @"\";
                        RepFolSrcStr = @"OUT\" + SelectedIMO[i].ToString() + @"\";
                        RepRefStr = RepModCodeStr + RepYYMMStr;
                        RepRefStr = RepModCodeStr + RepYYMMStr + ConvertIntToString(RepSrInt, 3);
                        RepDirectoryStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepFolSrcStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepDOCUniqueIdStr = SPDocsInfo.strspUniqueId;
                        RepIMOInt = SelectedIMO[i];
                        if (!System.IO.Directory.Exists(RepDirectoryStr))
                        {
                            System.IO.Directory.CreateDirectory(RepDirectoryStr);
                        }

                        try
                        {
                            // Check if file already exists. If yes, delete it. 
                            if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                            {
                                //Debug
                                //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "Before Read " + JSONFileNameStr);

                                System.IO.StreamReader myFile = new System.IO.StreamReader(RepDirectoryStr + "\\" + JSONFileNameStr);
                                string myString = string.Empty;

                                while ((myString = myFile.ReadLine()) != null)
                                {
                                    class_ReplicationFileFolderJSONFull ReplicationJSONFileFromFile = new class_ReplicationFileFolderJSONFull();
                                    ReplicationJSONFileFromFile = js.Deserialize<class_ReplicationFileFolderJSONFull>(myString);

                                    foreach (class_ReplicationFileFolderJSONFile JSONFiles in ReplicationJSONFileFromFile.entries)
                                    {
                                        if ((JSONFiles.RepSPDocsInfo.strspUniqueId == SPDocsInfo.strspUniqueId) && (JSONFiles.RepSPDocsInfo.strspContentType == "Nautic Folder"))
                                        {
                                            JSONFiles.RepSPDocsInfo = GetUpdatedSPDocInfos(SPDocsInfo.strspServerUrl, SPDocsInfo.strspUniqueId, "Folder", "Existing");

                                            JSONHeader.RepRef = RepRefStr;
                                            JSONHeader.RepModCode = RepModCodeStr;
                                            JSONHeader.RepFolSrc = RepFolSrcStr;
                                            JSONHeader.RepSMTPSrc = RepSMTPSrcStr;
                                            JSONHeader.RepFullDate = RepFullDateStr;
                                            JSONHeader.RepHeaderStatus = RepHeaderStatusStr;
                                            JSONHeader.RepYYMM = Convert.ToInt32(RepYYMMStr);
                                            JSONHeader.RepSr = RepSrInt;
                                            JSONHeader.RepIMO = RepIMOInt;
                                        }
                                        else
                                        {
                                            JSONList.entries.Add(JSONFiles);
                                            JSONHeader = ReplicationJSONFileFromFile.RepHeader;
                                        }

                                    }
                                }

                                myFile.Close();
                                myFile.Dispose();
                                //Thread.Sleep(5000);

                                try
                                {
                                    if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                        System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                } catch (System.IO.IOException)
                                {
                                    if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                    {
                                        //Load your file using FileStream class and release an file through stream.Dispose()
                                        using (FileStream stream = new FileStream(RepDirectoryStr + "\\" + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                                        {
                                            stream.Close();
                                            stream.Dispose();
                                        }

                                        // delete the file.
                                        System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                    }
                                    
                                }

                            }
                            else
                            {
                                //Debug
                                //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "No existing " + JSONFileNameStr);

                                JSONHeader.RepRef = RepRefStr;
                                JSONHeader.RepModCode = RepModCodeStr;
                                JSONHeader.RepFolSrc = RepFolSrcStr;
                                JSONHeader.RepSMTPSrc = RepSMTPSrcStr;
                                if (RepModCodeStr != "DMSSR")
                                    JSONHeader.RepFullDate = RepFullDateStr;
                                else
                                    JSONHeader.RepFullDate = "20" + RepYYMMDDStr.Substring(0, 2) + "-" + RepYYMMDDStr.Substring(2, 2) + "-" + RepYYMMDDStr.Substring(4, 2);
                                JSONHeader.RepHeaderStatus = RepHeaderStatusStr;
                                JSONHeader.RepYYMM = Convert.ToInt32(RepYYMMStr);
                                JSONHeader.RepSr = RepSrInt;
                                JSONHeader.RepIMO = RepIMOInt;
                            }

                        } // end of try
                        catch (System.Exception Ex)
                        {
                            //Console.WriteLine(Ex.ToString());
                        }

                    }
                    else // New Replication for today
                    {
                        //Debug
                        //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "New Replication for today");

                        RepDirectoryStr = EnquiryRegistryFolderPath("OutboundQueue") + SelectedIMO[i].ToString() + @"\";
                        RepFolSrcStr = @"OUT\" + SelectedIMO[i].ToString() + @"\";
                        RepSrInt++;
                        RepRefStr = RepModCodeStr + RepYYMMStr;
                        RepRefStr = RepModCodeStr + RepYYMMStr + ConvertIntToString(RepSrInt, 3);
                        RepDirectoryStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepFolSrcStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepDOCUniqueIdStr = SPDocsInfo.strspUniqueId;
                        RepIMOInt = SelectedIMO[i];
                        if (!System.IO.Directory.Exists(RepDirectoryStr))
                        {
                            System.IO.Directory.CreateDirectory(RepDirectoryStr);

                        }

                        JSONHeader.RepRef = RepRefStr;
                        JSONHeader.RepModCode = RepModCodeStr;
                        JSONHeader.RepFolSrc = RepFolSrcStr;
                        JSONHeader.RepSMTPSrc = RepSMTPSrcStr;
                        if (RepModCodeStr != "DMSSR")
                            JSONHeader.RepFullDate = RepFullDateStr;
                        else
                            JSONHeader.RepFullDate = "20" + RepYYMMDDStr.Substring(0, 2) + "-" + RepYYMMDDStr.Substring(2, 2) + "-" + RepYYMMDDStr.Substring(4, 2);
                        JSONHeader.RepHeaderStatus = RepHeaderStatusStr;
                        JSONHeader.RepYYMM = Convert.ToInt32(RepYYMMStr);
                        JSONHeader.RepSr = RepSrInt;
                        JSONHeader.RepIMO = RepIMOInt;

                    }

                    //Guid ReplicationJSONFileGUID = new Guid(SPDocsInfo.strspGUID);
                    JSONFile.RepSPDocsInfo = SPDocsInfo;
                    JSONFile.RepDistributionStatus = RepDocDistributionStatusStr;

                    //Debug
                    //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "JSONFile is ready");
                    
                    // If DMSSR Folder, RepID = 0 (i.e. no replication ID of Ship Folder had been created @ DMSSR 
                    if ((RepModCodeStr == "DMSSR") && (JSONFile.RepSPDocsInfo.intspFSObjType == 1))
                        JSONFile.RepID = 0;

                    /* 
                    if (RepModCodeStr != "DMSSR")
                    {
                        string ldappath = "LDAP://hkgsrvdc1.angloeasterngroup.com:389";

                        if (!string.IsNullOrEmpty(JSONFile.RepSPDocsInfo.strspModifiedBy))
                        {

                            if (JSONFile.UpdateType == "Delete")
                            {
                                JSONFile.RepSPDocsInfo.strspModifiedBy = GetAttribute(ldappath, Session["uID"].ToString(), "displayName");
                            }
                            else
                            {
                                if (JSONFile.RepSPDocsInfo.strspModifiedBy.IndexOf("\\") >= 0)
                                    JSONFile.RepSPDocsInfo.strspModifiedBy = GetAttribute(ldappath, JSONFile.RepSPDocsInfo.strspModifiedBy.Split('\\').Last(), "displayName");

                            }

                        }

                        //Debug
                        //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "Before Write to SQL");

                        //Write to SQL
                        if (getRepSr(RepModCodeStr, thisDay) == getRepSr(RepModCodeStr, thisDay.AddDays(-1)))
                            ReplicationHeaderToDB(JSONHeader);
                        string dbWriteResponse = ReplicationDetailsToDB(JSONHeader, JSONFile);
                        if (string.IsNullOrEmpty(dbWriteResponse)) { }
                        else
                        {
                            return dbWriteResponse;
                        }

                        //Debug
                        //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "After Write to SQL");

                    }   // End of if (RepModCodeStr != "DMSSR")
                    */

                    // Create a new JSON file 
                    //JSONFile.RepID = 100000106;
                    JSONList.RepHeader = JSONHeader;
                    JSONList.entries.Add(JSONFile);
                    JSONString += js.Serialize(JSONList);

                    try
                    {
                        if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                        using (StreamWriter sw = System.IO.File.CreateText(RepDirectoryStr + "\\" + JSONFileNameStr))
                        {
                            sw.WriteLine(JSONString);
                            sw.Close();
                            sw.Dispose();
                        }

                    }
                    catch (System.IO.IOException Ex)
                    {
                        if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                        {
                            using (FileStream stream = new FileStream(RepDirectoryStr + "\\" + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                            {
                                stream.Close();
                                stream.Dispose();
                            }

                            // delete the file.
                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                        }

                        using (StreamWriter sw = System.IO.File.CreateText(RepDirectoryStr + "\\" + JSONFileNameStr))
                        {
                            sw.WriteLine(JSONString);
                            sw.Close();
                            sw.Dispose();
                        }

                    }

                    //Copy the Sharepoint File to Replication Folder
                    //if ((DocumentType == "File") || (DocumentType == "Document"))
                    if (DocumentType == "File")
                    {
                        /*
                        if (System.IO.File.Exists(RepDirectoryStr + "\\" + DestinationNameStr))
                        {
                            System.IO.File.Delete(RepDirectoryStr + "\\" + DestinationNameStr);
                        }
                        */

                        string TempStr = string.Empty;

                        /*if (RepModCodeStr != "DMSSR")
                            TempStr = SPDownloadFile(folPath, RepDirectoryStr + "\\" + JSONFile.RepSPDocsInfo.strspUniqueId + "-" + JSONFile.RepSPDocsInfo.strspBaseName + "_" + JSONFile.RepSPDocsInfo.strspUIVersionString + "." + JSONFile.RepSPDocsInfo.strspFileType);*/

                        TempStr = folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")).Replace("/" + DestinationNameStr, "");
                    }
                    
                    //Non-RootFolder

                    if (JSONFile.RepSPDocsInfo.strspContentType == "Nautic Folder")
                    {
                        folPath = folPath.Substring(0, folPath.Length - folPath.Split('/').Last().Length - 1);
                        if (JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.LastIndexOf('/') > 0)
                            //JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath = JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Substring(1, JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Length).Split('/').First();
                            JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath = JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Substring(0, JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Length - JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Split('/').Last().Length - 1);
                        else
                            JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath = string.Empty;
                    }

                    //Debug
                    //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "Before AddSPFolderInfostoJSON");

                    AddSPFolderInfostoJSON(folPath, RepDirectoryStr + "\\" + JSONFile.RepSPDocsInfo.strspUniqueId + "-" + JSONFile.RepSPDocsInfo.strspBaseName + "_" + JSONFile.RepSPDocsInfo.strspUIVersionString + "." + JSONFile.RepSPDocsInfo.strspFileType, JSONHeader, JSONFile);

                    //}

                    //Debug
                    //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug_" + SelectedIMO[i].ToString() + ".txt", "After AddSPFolderInfostoJSON");

                    JSONList = new class_ReplicationFileFolderJSONFull();
                    JSONList.entries = new List<class_ReplicationFileFolderJSONFile>();
                    JSONHeader = new class_ReplicationJSONHeader();
                    JSONString = string.Empty;
                }

                // Disabled
                //CreateTRGFile(DirectoryShiftLevels(EnquiryRegistryFolderPath("OutboundQueue"), 1), RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_", 1, true);

            } // End of for-loop: SelectedIMO.Length

            return RepRefStr;
        }

        /*
        public int UpdatenmRepStatus(int RepIDInt, string UpdateTypeStr = null)
        {
            int nmRepStatusCount = -1;
            string cs = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("spACKFromShipReplication", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@pAcknowledgementID", SqlDbType.Int).Value = RepIDInt;
                    //Bosco - 23Feb2018
                    if (!string.IsNullOrEmpty(UpdateTypeStr))
                        cmd.Parameters.Add("@pUpdateType", SqlDbType.VarChar).Value = UpdateTypeStr;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        if (dr.IsDBNull(0))
                            nmRepStatusCount = -1;
                        else
                            nmRepStatusCount = Convert.ToInt32(dr[0].ToString());

                    }
                }

            }

            return nmRepStatusCount;

        }

        */
        public class_sharepointlist GetSPFolderInfoByUniqueId(string uid)
        {   
            //using (ClientContext clientContext = new ClientContext(@"http://oneview.angloeastern.com/sites/oneview/nsdl"))
            using (ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl"))
            {
                //clientContext.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");

                Folder FN = clientContext.Web.GetFolderById(Guid.Parse(uid));
                ListItem clientListItem = FN.ListItemAllFields;
                clientContext.Load(FN);
                clientContext.Load(clientListItem);
                clientContext.Load(clientListItem.ParentList);
                clientContext.ExecuteQuery();

                class_sharepointlist item = new class_sharepointlist();
                item.strFileName = FN.Name;
                item.strModified = FN.TimeLastModified;
                item.strFileDirRef = FN.ServerRelativeUrl;
                item.strUniqueID = FN.UniqueId.ToString().Replace("}", "").Replace("{", "");
                item.strParentUniqueID = clientListItem.FieldValues["ParentUniqueId"].ToString().Replace("}", "").Replace("{", "");
                item.strBaseFileName = item.strFileName;
                item.iFSObjType = 1;
                item.isControlledDocument = clientListItem.ParentList.EnableVersioning;
                item.fromShip = (clientListItem.ParentList.Title == "NSG01" || clientListItem.ParentList.Title == "NSG02" || clientListItem.ParentList.Title == "NSG03");

                return item;
            }

        }

        public void CreateTRGFile(string FilePathStr, string FileNameStr, int spFSObjType, bool UAT)
        {
            /*
             * spFSObjType = 0 (Nautic Document)
             * spFSObjType = 1 (Nautic Folder)
             * 
             */

            string IMOStr = string.Empty;
            string ContentStr = string.Empty;
                
            /*
                if (UAT)
                    ContentStr = Session["uID"].ToString() + @"\";
                    */
                // Nautic Folder
                if (spFSObjType > 0)
                {
                    //DMSAR18040119261451180421_
                    //ContentStr += FileNameStr.Substring(12, 7) + @"\" + FileNameStr;
                    ContentStr += FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg";

                }
                else
                {
                //ContentStr += FileNameStr.Substring(3, 7) + "-" + FileNameStr + ".trg";

                    if (FileNameStr.Substring(0, 3) == "ACK")
                        IMOStr = FileNameStr.Substring(3, 7);
                    else if (FileNameStr.Substring(0, 5) == "RMSMR")
                        IMOStr = FileNameStr.Substring(12, 7);
                    else
                        IMOStr = FileNameStr.Substring(3, 7);

                    ContentStr += IMOStr + @"\" + FileNameStr;

                }

                string TempStr = DateTime.Now.ToString("yyyyMMddmmss");

                //if (!System.IO.File.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg"))
                //{
                    // Nautic Folder
                    if (spFSObjType > 0)
                    {
                        if (!System.IO.File.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg"))
                        {
                            //if (System.IO.Directory.Exists(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr))
                            if (System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr))
                            {
                                if (System.IO.File.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg"))
                                {
                                    if (System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr))
                                    {
                                        NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr + "backup" + TempStr + "_");

                                        System.IO.File.Move(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg", FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + "backup" + TempStr + "_.trg");

                                    }

                                    if (!System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7)))
                                        System.IO.Directory.CreateDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7));

                                    //NauticMoveDirectory(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                                    NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);

                                }
                                else
                                {
                                    if (!System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7)))
                                        System.IO.Directory.CreateDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7));

                                    //NauticMoveDirectory(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                                    NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);

                                }

                            }

                    } else if (System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr))
                    {
                        NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr + "backup" + TempStr + "_");

                        System.IO.File.Move(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg", FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + "backup" + TempStr + "_.trg");

                        //NauticMoveDirectory(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                        NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);

                    }
                    else
                    {
                        if (!System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7)))
                            System.IO.Directory.CreateDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7));

                        //NauticMoveDirectory(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                        NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);

                    }

                }// Nautic Document
                    else
                    {
                if (!System.IO.File.Exists(FilePathStr + "\\" + IMOStr + "-" + FileNameStr.Substring(0, FileNameStr.LastIndexOf('.')) + ".trg"))
                {
                    //if (System.IO.File.Exists(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr))
                    if (System.IO.File.Exists(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr))
                    {
                        /*
                        if (System.IO.File.Exists(FilePathStr + "\\" + IMOStr + "-" + FileNameStr.Substring(0, FileNameStr.LastIndexOf('.')) + ".trg"))
                        {
                            if (!System.IO.File.Exists(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr))
                            {
                                if (!System.IO.Directory.Exists(FilePathStr + "\\" + IMOStr))
                                    System.IO.Directory.CreateDirectory(FilePathStr + "\\" + IMOStr);

                                //System.IO.File.Move(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                                System.IO.File.Move(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr, FilePathStr + "\\" + IMOStr + "\\" + FileNameStr);

                            }


                        }
                        else
                        {
                            if (!System.IO.Directory.Exists(FilePathStr + "\\" + IMOStr))
                                System.IO.Directory.CreateDirectory(FilePathStr + "\\" + IMOStr);
                                

                            //System.IO.File.Move(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);

                            System.IO.File.Move(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr, FilePathStr + "\\" + IMOStr + "\\" + FileNameStr);
                            */
                       // }

                    }

                } /*else if (System.IO.File.Exists(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr))
                    //System.IO.File.Delete(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                    System.IO.File.Delete(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr);

                System.IO.File.Delete(FilePathStr + "\\" + IMOStr + "-" + FileNameStr.Substring(0, FileNameStr.LastIndexOf('.')) + ".trg");
                */
            }

                /*}
                else
                {
                    // Nautic Folder
                    if (spFSObjType > 0)
                    {
                        if (System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr))
                        {
                            NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr + "backup" + TempStr + "_");

                            System.IO.File.Move(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg", FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + "backup" + TempStr + "_.trg");

                            //NauticMoveDirectory(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                            NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                            
                        }
                        else
                        {
                            if (!System.IO.Directory.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7)))
                                System.IO.Directory.CreateDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7));

                            //NauticMoveDirectory(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                            NauticMoveDirectory(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr, FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                        
                        }

                    }
                    else
                    {
                        //if (System.IO.File.Exists(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr))
                        if (System.IO.File.Exists(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr))
                            //System.IO.File.Delete(FilePathStr + "\\" + Session["uID"].ToString() + "\\" + FileNameStr.Substring(12, 7) + "\\" + FileNameStr);
                            System.IO.File.Delete(FilePathStr + "\\" + IMOStr + "\\" + FileNameStr);

                        System.IO.File.Delete(FilePathStr + "\\" + IMOStr + "-" + FileNameStr.Substring(0, FileNameStr.LastIndexOf('.')) + ".trg");

                    }

                }*/

                try
                {
                    if (spFSObjType > 0)
                    {
                        if (System.IO.File.Exists(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg"))
                            System.IO.File.Delete(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg");

                        using (StreamWriter sw = System.IO.File.CreateText(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg"))
                        {
                            sw.WriteLine(ContentStr);
                            sw.Close();
                            sw.Dispose();
                        }

                    }
                    else
                    {
                        if (System.IO.File.Exists(FilePathStr + "\\" + IMOStr + "-" + FileNameStr.Substring(0, FileNameStr.LastIndexOf('.')) + ".trg"))
                            System.IO.File.Delete(FilePathStr + "\\" + IMOStr + "-" + FileNameStr.Substring(0, FileNameStr.LastIndexOf('.')) + ".trg");

                        using (StreamWriter sw = System.IO.File.CreateText(FilePathStr + "\\" + IMOStr + "-" + FileNameStr.Substring(0, FileNameStr.LastIndexOf('.')) + ".trg"))
                        {
                            sw.WriteLine(ContentStr);
                            sw.Close();
                            sw.Dispose();
                        }

                    }

                }
                catch (Exception Ex)
                {
                    if (System.IO.File.Exists(FilePathStr + "\\" + ContentStr))
                        //System.IO.File.Move(FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + FileNameStr + ".trg", FilePathStr + "\\" + FileNameStr.Substring(12, 7) + "-" + Session["uID"].ToString() + "-" + FileNameStr + "_" + DateTime.Now.ToString("yyyyMMddmmss") + ".trg.bak");
                        System.IO.File.Move(FilePathStr + "\\" + ContentStr, FilePathStr + "\\" + ContentStr + "_" + DateTime.Now.ToString("yyyyMMddmmss") + ".trg.bak");

                    using (StreamWriter sw = System.IO.File.CreateText(FilePathStr + "\\" + ContentStr))
                    {
                        sw.WriteLine(ContentStr);
                        sw.Close();
                        sw.Dispose();
                    }

                }
            
            return;

        }

        public string DirectoryShiftLevels(string PathStr, int LevelInt, string[] FolderNameStrs = null)
        {
            string ReturnStr = string.Empty;

            if (LevelInt > 0)
            {
                if (PathStr.LastIndexOf('\\') == (PathStr.Length - 1))
                    PathStr = PathStr.Substring(0, PathStr.Length - 1);

                string[] ReturnPathArr = PathStr.Split('\\');

                if (ReturnPathArr.Length >= LevelInt)
                    ReturnStr = string.Join(@"\", ReturnPathArr.Take(ReturnPathArr.Length - LevelInt).ToArray());
                else
                    ReturnStr = PathStr;

            }
            else
                ReturnStr = PathStr;

            return ReturnStr;

        }

        public void DeleteDirectory(string target_dir)
        {
            string[] files = System.IO.Directory.GetFiles(target_dir);
            string[] dirs = System.IO.Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                System.IO.File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            //System.IO.Directory.Delete(target_dir, false);

            try
            {
                System.IO.Directory.Delete(target_dir, true);
            }
            catch (IOException)
            {
                System.IO.Directory.Delete(target_dir, true);
            }
            catch (UnauthorizedAccessException)
            {
                System.IO.Directory.Delete(target_dir, true);
            }

        }

        /*
        private bool ProcessAckFromOffice(string path_acofile, string file_aco, int v_imo)
        {
            bool success = false;

            List<class_replicationlist> DocList = new List<class_replicationlist>();
            List<class_ReplicationFileFolderJSONFile> FileList = new List<class_ReplicationFileFolderJSONFile>();

            List<class_replicationlist> DocListFol = new List<class_replicationlist>();
            List<class_ReplicationFileFolderJSONFile> FileListFol = new List<class_ReplicationFileFolderJSONFile>();


            List<class_replicationlist> DocListDoc = new List<class_replicationlist>();
            List<class_ReplicationFileFolderJSONFile> FileListDoc = new List<class_ReplicationFileFolderJSONFile>();

            List<class_replicationlist> DocListForRep = new List<class_replicationlist>();
            List<class_ReplicationFileFolderJSONFile> FileListForRep = new List<class_ReplicationFileFolderJSONFile>();

            dynamic r_head = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(path_acofile));

            class_ReplicationJSONHeader r_header = new class_ReplicationJSONHeader();
            r_header.RepRef = r_head.RepHeader.RepRef;
            r_header.RepModCode = r_head.RepHeader.RepModCode;
            r_header.RepFolSrc = r_head.RepHeader.RepFolSrc;
            r_header.RepSMTPSrc = r_head.RepHeader.RepSMTPSrc;
            r_header.RepFullDate = r_head.RepHeader.RepFullDate;
            r_header.RepHeaderStatus = r_head.RepHeader.RepHeaderStatus;
            r_header.RepYYMM = r_head.RepHeader.RepYYMM;
            r_header.RepSr = r_head.RepHeader.RepSr;
            r_header.RepIMO = r_head.RepHeader.RepIMO;

            foreach (var file in r_head.entries)
            {
                class_ReplicationFileFolderJSONFile r_files = new class_ReplicationFileFolderJSONFile();
                r_files.RepID = file.RepID;
                r_files.SourceURLStr = file.SourceURLStr;
                r_files.UpdateType = file.UpdateType;
                r_files.UpdateStr = file.UpdateStr;
                r_files.RepDistributionStatus = file.RepDistributionStatus;
                FileList.Add(r_files);

                class_replicationlist r_doclist = new class_replicationlist();
                r_doclist.intspID = file.RepSPDocsInfo.intspID;
                r_doclist.strspUIVersionString = file.RepSPDocsInfo.strspUIVersionString;
                r_doclist.intspContentVersion = file.RepSPDocsInfo.intspContentVersion;
                r_doclist.blnspIsCurrentVersion = file.RepSPDocsInfo.blnspIsCurrentVersion;
                r_doclist.strspAuthor = file.RepSPDocsInfo.strspAuthor;
                r_doclist.strspContentType = file.RepSPDocsInfo.strspContentType;
                r_doclist.intspFSObjType = file.RepSPDocsInfo.intspFSObjType;
                r_doclist.intspFolderChildCount = file.RepSPDocsInfo.intspFolderChildCount;
                r_doclist.intspItemChildCount = file.RepSPDocsInfo.intspItemChildCount;
                r_doclist.strspBaseName = file.RepSPDocsInfo.strspBaseName;
                r_doclist.strspLinkFilename = file.RepSPDocsInfo.strspLinkFilename;
                r_doclist.dttspCreated = file.RepSPDocsInfo.dttspCreated;
                r_doclist.strspCreatedBy = file.RepSPDocsInfo.strspCreatedBy;
                r_doclist.strspLastModified = file.RepSPDocsInfo.strspLastModified;
                r_doclist.dttspModified = file.RepSPDocsInfo.dttspModified;
                r_doclist.strspModifiedBy = file.RepSPDocsInfo.strspModifiedBy;
                r_doclist.strspEditor = file.RepSPDocsInfo.strspEditor;
                r_doclist.intspFileSizeDisplay = file.RepSPDocsInfo.intspFileSizeDisplay;
                r_doclist.strspFileType = file.RepSPDocsInfo.strspFileType;
                r_doclist.strspServerUrl = file.RepSPDocsInfo.strspServerUrl;
                r_doclist.strspUniqueId = file.RepSPDocsInfo.strspUniqueId;
                r_doclist.strspParentUniqueId = file.RepSPDocsInfo.strspParentUniqueId;
                r_doclist.strspGUID = file.RepSPDocsInfo.strspGUID;
                r_doclist.strspVirtual_LocalRelativePath = file.RepSPDocsInfo.strspVirtual_LocalRelativePath;
                r_doclist.strnmDocumentLibrary = file.RepSPDocsInfo.strnmDocumentLibrary;
                DocList.Add(r_doclist);

                if (r_doclist.intspFSObjType == 1) { DocListFol.Add(r_doclist); FileListFol.Add(r_files); }
                else if (r_files.RepID != 0) { DocListForRep.Add(r_doclist); FileListForRep.Add(r_files); }
                //else if (r_doclist.intspFSObjType == 0) { DocListDoc.Add(r_doclist); FileListDoc.Add(r_files); }
            }


            for (int f = 0; f < DocListForRep.Count; f++)
            {
                int nmDocId = 0;
                string file_uid = DocListForRep[f].strspUniqueId;
                string file_pid = DocListForRep[f].strspParentUniqueId;
                int file_repID = FileListForRep[f].RepID;

                // string rootSPPID = string.Empty;
                int rootPID = 1;
                int dbPID_ID = 0;

                nmDocId = GetIDFromDocLibShip(file_repID); // Check if ID in nmDocLibShip exists

                if (nmDocId == 0) { continue; } // ID in nmDocLibShip not exists
                else
                {

                    if (!spUIExists(nmDocId))
                    {
                        UpdateFolShipBoardFilingByDocId(DocListForRep, FileListForRep, f, nmDocId);
                    }
                    else
                    {
                        //DateTime db_lastModDate = Convert.ToDateTime(GetLastModDate(dbPID_ID));
                        DateTime db_lastModDate = Convert.ToDateTime(GetLastModDate(nmDocId));
                        rootPID = GetRootPID(nmDocId);
                        if (db_lastModDate < Convert.ToDateTime(DocListForRep[f].strspLastModified) && rootPID != 0)
                        {
                            UpdateFolShipBoardFilingByDocId(DocListForRep, FileListForRep, f, nmDocId);
                        }
                    }





                    for (int fol = 0; fol < DocListFol.Count; fol++) // Check all folder to see if the parent is existing
                    {

                        //rootSPPID = GetSPRootPID(DocListFol[fol].strspUniqueId);
                        //if (rootSPPID == "0") { break; }

                        for (int doc = 0; doc < DocList.Count; doc++)
                        {
                            //break;
                            if (DocList[doc].strspUniqueId == file_pid) // file_pid is found in the 
                            {

                                dbPID_ID = DBPID_ID(nmDocId);
                                if (dbPID_ID != 0)
                                {
                                    if (!spUIExists(dbPID_ID))
                                    {
                                        UpdateFolShipBoardFilingByDocId(DocList, FileList, doc, dbPID_ID);
                                    }
                                    else
                                    {
                                        //DateTime db_lastModDate = Convert.ToDateTime(GetLastModDate(dbPID_ID));
                                        DateTime db_lastModDate = Convert.ToDateTime(GetLastModDate(dbPID_ID));
                                        rootPID = GetRootPID(dbPID_ID);
                                        if (db_lastModDate < Convert.ToDateTime(DocList[doc].strspLastModified) && rootPID != 0)
                                        {
                                            UpdateFolShipBoardFilingByDocId(DocList, FileList, doc, dbPID_ID);
                                        }
                                    }

                                    file_pid = DocList[doc].strspParentUniqueId;
                                    nmDocId = dbPID_ID;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (rootPID == 0 || dbPID_ID == 0) break;

                    }

                }

                AckAndUpdateReplicationShipBoardFiling(DocListForRep, FileListForRep, f);
                success = true;

            }

            return success;
        }

        private void AckAndUpdateReplicationShipBoardFiling(List<class_replicationlist> DocListForRep_Function, List<class_ReplicationFileFolderJSONFile> FileListForRep_Function, int f)
        {

            //string conStr = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString =
                "Data Source=" + UATGlobals.UATSQLServerIP +
                //";Initial Catalog=NAUTICMASTER;" +
                //";Initial Catalog=NAUTICMASTER_Trial;" +
                ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                "User id=sa;" +
                "Password=" + UATGlobals.UATSQLServerPassword + ";";

                using (SqlCommand cmd = new SqlCommand("spUpdateRepDocsByRepId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@RepId", FileListForRep_Function[f].RepID);
                    cmd.Parameters.AddWithValue("@Author", DocListForRep_Function[f].strspAuthor);
                    cmd.Parameters.AddWithValue("@LastModified", DocListForRep_Function[f].strspLastModified);
                    cmd.Parameters.AddWithValue("@ServerUrl", DocListForRep_Function[f].strspServerUrl);
                    cmd.Parameters.AddWithValue("@UniqueId", DocListForRep_Function[f].strspUniqueId);
                    cmd.Parameters.AddWithValue("@ParentUniqueId", DocListForRep_Function[f].strspParentUniqueId);
                    cmd.Parameters.AddWithValue("@GUID", DocListForRep_Function[f].strspGUID);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }

                conn.Close();
            }

        }

        private void UpdateFolShipBoardFilingByDocId(List<class_replicationlist> DocListForRep_Function, List<class_ReplicationFileFolderJSONFile> FileListForRep_Function, int i, int ID)
        {
            //string conStr = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString =
                "Data Source=" + UATGlobals.UATSQLServerIP +
                //";Initial Catalog=NAUTICMASTER;" +
                //";Initial Catalog=NAUTICMASTER_Trial;" +
                ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                "User id=sa;" +
                "Password=" + UATGlobals.UATSQLServerPassword + ";";


                using (SqlCommand cmd = new SqlCommand("spUpdateDocLibShipById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", ID);
                    cmd.Parameters.AddWithValue("@Author", DocListForRep_Function[i].strspAuthor);
                    cmd.Parameters.AddWithValue("@FolderChildCount", DocListForRep_Function[i].intspFolderChildCount);
                    cmd.Parameters.AddWithValue("@ItemChildCount", DocListForRep_Function[i].intspItemChildCount);
                    cmd.Parameters.AddWithValue("@Created_x0020_By", DocListForRep_Function[i].strspCreatedBy);
                    cmd.Parameters.AddWithValue("@Last_x0020_Modified", DocListForRep_Function[i].strspLastModified);
                    cmd.Parameters.AddWithValue("@Modified", DocListForRep_Function[i].dttspModified);
                    cmd.Parameters.AddWithValue("@Modified_x0020_By", DocListForRep_Function[i].strspModifiedBy);
                    cmd.Parameters.AddWithValue("@Editor", DocListForRep_Function[i].strspEditor);
                    cmd.Parameters.AddWithValue("@ServerUrl", DocListForRep_Function[i].strspServerUrl);
                    cmd.Parameters.AddWithValue("@UniqueId", DocListForRep_Function[i].strspUniqueId);
                    cmd.Parameters.AddWithValue("@ParentUniqueId", DocListForRep_Function[i].strspParentUniqueId);
                    cmd.Parameters.AddWithValue("@GUID", DocListForRep_Function[i].strspGUID);
                    cmd.Parameters.AddWithValue("@_UIVersionString", DocListForRep_Function[i].strspUIVersionString);
                    cmd.Parameters.AddWithValue("@CreatedDate", DocListForRep_Function[i].dttspCreated);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }

                conn.Close();
            }

        }

        private int GetRootPID(int ID)
        {
            int rootID = 1;

            //string con_dluid = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc_dluid = new SqlConnection())
            {

                sc_dluid.ConnectionString =
                "Data Source=" + UATGlobals.UATSQLServerIP +
                //";Initial Catalog=NAUTICMASTER;" +
                //";Initial Catalog=NAUTICMASTER_Trial;" +
                ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                "User id=sa;" +
                "Password=" + UATGlobals.UATSQLServerPassword + ";";

                using (SqlCommand cmd = new SqlCommand("spGetDocShipListParentId", sc_dluid))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", ID);
                    sc_dluid.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        rootID = Convert.ToInt32(dr["PID"]);
                    }
                }
                sc_dluid.Close();
            }

            return rootID;
        }

        private string GetLastModDate(int ID)
        {
            string dbLastModDate = string.Empty;

            // string con_dluid = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc_dluid = new SqlConnection())
            {

                sc_dluid.ConnectionString =
                "Data Source=" + UATGlobals.UATSQLServerIP +
                //";Initial Catalog=NAUTICMASTER;" +
                //";Initial Catalog=NAUTICMASTER_Trial;" +
                ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                "User id=sa;" +
                "Password=" + UATGlobals.UATSQLServerPassword + ";";

                using (SqlCommand cmd = new SqlCommand("spGetLastModifiedDate", sc_dluid))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", ID);
                    sc_dluid.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        dbLastModDate = dr["Last_x0020_Modified"].ToString();

                    }
                }
                sc_dluid.Close();
            }

            return dbLastModDate;
        }

        private bool spUIExists(int ID)
        {
            bool exists = false;

            // string con_dluid = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc_dluid = new SqlConnection())
            {

                sc_dluid.ConnectionString =
                "Data Source=" + UATGlobals.UATSQLServerIP +
                //";Initial Catalog=NAUTICMASTER;" +
                //";Initial Catalog=NAUTICMASTER_Trial;" +
                ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                "User id=sa;" +
                "Password=" + UATGlobals.UATSQLServerPassword + ";";

                using (SqlCommand cmd = new SqlCommand("spSharePointUIDExists", sc_dluid))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", ID);
                    sc_dluid.Open();

                    exists = (bool)cmd.ExecuteScalar();

                }
                sc_dluid.Close();
            }

            return exists;

        }
        private int DBPID_ID(int ID)
        {
            int id = 0;

            //string con_dluid = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc_dluid = new SqlConnection())
            {

                sc_dluid.ConnectionString =
                "Data Source=" + UATGlobals.UATSQLServerIP +
                //";Initial Catalog=NAUTICMASTER;" +
                //";Initial Catalog=NAUTICMASTER_Trial;" +
                ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                "User id=sa;" +
                "Password=" + UATGlobals.UATSQLServerPassword + ";";

                using (SqlCommand cmd = new SqlCommand("spGetShipDocIdByParentId", sc_dluid))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OldId", ID);
                    sc_dluid.Open();

                    id = (int)cmd.ExecuteScalar();

                }
                sc_dluid.Close();
            }
            return id;
        }

        private int GetIDFromDocLibShip(int RepID)
        {
            int ID_DocLibShip = 0, PID_DocLibShip = 0;

            //string con_dluid = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc_dluid = new SqlConnection())
            {

                sc_dluid.ConnectionString =
                "Data Source=" + UATGlobals.UATSQLServerIP +
                //";Initial Catalog=NAUTICMASTER;" +
                //";Initial Catalog=NAUTICMASTER_Trial;" +
                ";Initial Catalog=" + UATGlobals.UATSQLServerDatabase + ";" +
                "User id=sa;" +
                "Password=" + UATGlobals.UATSQLServerPassword + ";";


                using (SqlCommand cmd = new SqlCommand("spGetnmDocLibShipIdPid", sc_dluid))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RepID", RepID);
                    sc_dluid.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        ID_DocLibShip = Convert.ToInt32(dr["DocID"]);
                        PID_DocLibShip = Convert.ToInt32(dr["DocPID"]);
                    }
                }
                sc_dluid.Close();
            }

            return ID_DocLibShip;

        }
        */

        public class_replicationlist GetSPDocInfos(string folPath, string DocumentType, string UpdateType, string uniqueId = null)
        {
            string ListNameStr = getListName(folPath.Replace("http://dms.angloeastern.com", ""));
            string RelativeStr = folPath.Split('/').Last();

            switch (RelativeStr)
            {

                case "01B Ship Board Manuals Russian":
                    RelativeStr = "01B Ship Board Manuals (Russian)";
                    break;
                case "02 Ship Board Manuals Chinese":
                    RelativeStr = "02 Ship Board Manuals (Chinese)";
                    break;
                case "01G RORO Manuals":
                    RelativeStr = "01G RO-RO Manuals";
                    break;
                case "11 NonRoutine Events":
                    RelativeStr = "11 Non-Routine Events";
                    break;
                case "01I Ship Board Manuals Bahasa":
                    RelativeStr = "01I Ship Board Manuals (Bahasa)";
                    break;
                case "01Z Manuals for Ship Types Currently not under Mgm":
                    RelativeStr = "01Z Manuals for Ship Types Currently not under Mgmt";
                    break;
                default:
                    break;
            }

            if (ListNameStr == RelativeStr)
                return GetSPDLInfos(folPath);
            else
            {

                string RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl") - RelativeStr.Length - 1);
                //RelativeStr = RootFolderStr.Replace("/sites/oneview/nsdl/"+ ListNameStr,"")+"/"+ RelativeStr;

                // Remove all special characters including (, ), <, > or -
                RootFolderStr = RootFolderStr.Replace("&", "&amp;");
                RootFolderStr = RootFolderStr.Replace("'", "&apos;");
                //RootFolderStr = RootFolderStr.Replace("(", ""); 
                //RootFolderStr = RootFolderStr.Replace(")", "");

                if (RootFolderStr.IndexOf('-') >= 0)
                    RootFolderStr = RootFolderStr.Replace(ListNameStr, ListNameStr.Replace("-", ""));

                class_replicationlist item = new class_replicationlist();

                using (var clientContext = new nsdlWebService.Lists())
                {
                    //client.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    //client.Credentials = new System.Net.NetworkCredential(HttpContext.Current.Session["uID"].ToString(), HttpContext.Current.Session["uPWD"].ToString(), "anglo");
                    //client.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                    clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");
                    //client.Url = "http://oneview.angloeastern.com/sites/oneview/nsdl/_vti_bin/lists.asmx";
                    clientContext.Url = "http://dms.angloeastern.com/sites/nsdl/_vti_bin/lists.asmx";
                    XmlNode ndListItems = null;
                    XmlDocument xdoc = new XmlDocument();
                    XmlNode ndQuery = xdoc.CreateNode(XmlNodeType.Element, "Query", "");
                    XmlNode ndViewFields = xdoc.CreateNode(XmlNodeType.Element, "ViewFields", "");
                    XmlNode ndQueryOptions = xdoc.CreateNode(XmlNodeType.Element, "QueryOptions", "");
                    ndQuery.InnerXml = "";
                    ndViewFields.InnerXml = "";

                    StringBuilder sbQuery = new StringBuilder();
                    if (string.IsNullOrEmpty(uniqueId))
                    {
                        switch (DocumentType)
                        {
                            case "Folder":
                                sbQuery.AppendLine(@"<Where><Eq><FieldRef Name = 'FSObjType'/><Value Type = 'Integer'>1</Value></Eq></Where>");
                                break;
                            case "File":
                                sbQuery.AppendLine(@"<Where><Eq><FieldRef Name = 'FSObjType'/><Value Type = 'Integer'>0</Value></Eq></Where>");
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        sbQuery.AppendLine(@"<Where><Eq><FieldRef Name = 'UniqueId'/><Value Type = 'Lookup'>" + uniqueId + "</Value></Eq></Where>");
                    }

                    ndQuery.InnerXml = sbQuery.ToString();

                    StringBuilder sbQueryOptions = new StringBuilder();
                    sbQueryOptions.AppendLine(@"<IncludeAttachmentUrls>TRUE </IncludeAttachmentUrls>");
                    sbQueryOptions.AppendLine(@"<QueryOptions>");
                    if (string.IsNullOrEmpty(uniqueId))
                    {
                        sbQueryOptions.AppendLine(@"<Folder>" + RootFolderStr + "</Folder>");

                        if (folPath != RootFolderStr.Replace("(", "").Replace(")", ""))
                            sbQueryOptions.AppendLine(@"<Folder>" + ListNameStr + "</Folder>");
                        //sbQueryOptions.AppendLine(@"<ViewAttributes Scope='Recursive' />");
                    }
                    else
                    {
                        sbQueryOptions.AppendLine(@"<ViewAttributes Scope='RecursiveAll' />");
                    }
                    sbQueryOptions.AppendLine(@"</QueryOptions>");
                    ndQueryOptions.InnerXml = sbQueryOptions.ToString();

                    ndListItems = clientContext.GetListItems(ListNameStr, "", ndQuery, ndViewFields, "0", ndQueryOptions, null);

                    /*
                    XmlDocument TempXMLDoc = new XmlDocument();
                    TempXMLDoc.LoadXml(ndListItems.OuterXml);
                    TempXMLDoc.Save(ProcessedFilesPath + "SPDocInfos.xml");
                    */

                    /*
                    string TempStr = ListNameStr;
                    TempStr = TempStr.Replace("&", "&amp;");
                    TempStr = TempStr.Replace("'", "&apos;");
                    TempStr = TempStr.Replace("(", "");
                    TempStr = TempStr.Replace(")", "");

                    folPath = "/sites/oneview/nsdl/" + TempStr + RelativeStr;
                    //folPath = folPath.Replace("&amp;", "&");
                    //folPath = folPath.Replace("&apos;","'");
                    */

                    if (ndListItems != null)
                    {
                        foreach (XmlNode node in ndListItems.ChildNodes)
                        {
                            XmlNodeReader objReader = new XmlNodeReader(node);
                            while (objReader.Read())
                            {
                                if (objReader["ows_EncodedAbsUrl"] != null)
                                {
                                    //@"/sites/oneview/nsdl/01B Ship Board Manuals Russian/201 MSM/03.00 Mission statement &amp; Core values of the company.pdf"

                                    string TempStr = objReader["ows_ServerUrl"].ToString();

                                    if (!string.IsNullOrEmpty(uniqueId) || (objReader["ows_ServerUrl"].ToString() == folPath.Replace(ListNameStr, ListNameStr.Replace("-", ""))) || (UpdateType == "RemoteDelete"))
                                    {
                                        // please do in here
                                        item.intspID = Convert.ToInt32(objReader["ows_ID"]);
                                        item.strspUIVersionString = objReader["ows__UIVersionString"].ToString();
                                        if (DocumentType == "Folder")
                                            item.intspContentVersion = 0;
                                        else
                                            //item.intspContentVersion = Convert.ToInt32(objReader["ows_ContentVersion"].Split('#').Last());
                                            item.intspContentVersion = -1;
                                        item.blnspIsCurrentVersion = Convert.ToBoolean(Convert.ToInt32(objReader["ows__IsCurrentVersion"].ToString()));
                                        item.strspAuthor = objReader["ows_Author"].Split('#').Last();
                                        item.strspContentType = objReader["ows_ContentType"].ToString();
                                        item.intspFSObjType = Convert.ToInt32(objReader["ows_FSObjType"].Split('#').Last());
                                        item.intspFolderChildCount = Convert.ToInt32(objReader["ows_FolderChildCount"].Split('#').Last());
                                        item.intspItemChildCount = Convert.ToInt32(objReader["ows_ItemChildCount"].Split('#').Last());
                                        // Converting "\u0026amp;" from SharePoint
                                        item.strspBaseName = objReader["ows_BaseName"].ToString().Replace("\u0026", "&");
                                        item.strspBaseName = item.strspBaseName.Replace("&amp;", "&");
                                        item.strspBaseName = item.strspBaseName.Replace("&#39;", "'");
                                        // End
                                        item.strspLinkFilename = objReader["ows_LinkFilename"].ToString();
                                        item.dttspCreated = Convert.ToDateTime(objReader["ows_Created"]); // for "2017-08-10 14:00:59"
                                        item.strspLastModified = objReader["ows_Last_x0020_Modified"].Split('#').Last(); //"54;#2017-08-10 14:00:59"->Convert.TODateTime
                                        item.dttspModified = Convert.ToDateTime(objReader["ows_Modified"].Split('#').Last()); //->Convert.TODateTime for "2017-08-10 14:00:59"
                                        item.strspEditor = objReader["ows_Editor"].Split('#').Last(); //->Clean "888;#HKG - Bosco WONG"
                                        item.strspServerUrl = objReader["ows_ServerUrl"].ToString();
                                        item.strspUniqueId = objReader["ows_UniqueId"].Split('#').Last().Replace("{", "").Replace("}", "").ToUpper(); //->Clean "54;#{D29AA14E-F6D4-452C-A127-B2908B16C3B8}"
                                        item.strspParentUniqueId = objReader["ows_ParentUniqueId"].Split('#').Last().Replace("{", "").Replace("}", ""); //->Clean "54;#{C1ED19BB-9A3F-4E64-8C84-6351092838A1}"
                                        item.strspGUID = objReader["ows_GUID"].ToString().Replace("{", "").Replace("}", "");  //Clean "{476D351A-4342-4472-AF42-58AC7351383C}"
                                        item.strnmDocumentLibrary = ListNameStr; //->Convert from "ServerUrl"

                                        if ((item.strspUIVersionString == "1.0") || (item.strspContentType == "Nautic Folder"))
                                            item.strspCheckInComments = string.Empty;
                                        else
                                            item.strspCheckInComments = objReader["ows__CheckinComment"].ToString().Split('#').Last();

                                        //Enquiry from SQL
                                        //string strConnString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

                                        string strConnString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
                                        SqlConnection con = new SqlConnection(strConnString);

                                        SqlCommand cmd = new SqlCommand();

                                        cmd.CommandType = CommandType.StoredProcedure;

                                        cmd.CommandText = "spGetKMSApproval";

                                        cmd.Parameters.Add("@pspUIVersionString", SqlDbType.VarChar).Value = item.strspUIVersionString;
                                        cmd.Parameters.Add("@pspUniqueId", SqlDbType.VarChar).Value = item.strspUniqueId;
                                        cmd.Parameters.Add("@pnmDocumentLibrary", SqlDbType.VarChar).Value = ListNameStr;
                                        cmd.Connection = con;

                                        bool EmptyResultSet = true;

                                        try
                                        {
                                            con.Open();
                                            SqlDataReader dr = cmd.ExecuteReader();

                                            while (dr.Read())
                                            {
                                                if (!dr.IsDBNull(0))
                                                {
                                                    item.strApprovalStatus = dr["Status"].ToString();
                                                    item.strApprovedBy = dr["approver"].ToString();
                                                    item.strApproverComments = dr["comments"].ToString();
                                                    EmptyResultSet = false;

                                                }

                                            }
                                            //Console.WriteLine("Record enquiried successfully");
                                        }
                                        catch (System.Exception ex)
                                        {
                                            //throw ex;
                                            return item;

                                        }
                                        finally
                                        {
                                            con.Close();
                                            con.Dispose();
                                        }

                                        if (EmptyResultSet)
                                        {
                                            item.strApprovalStatus = "Approved";
                                            item.strApprovedBy = string.Empty;
                                            item.strApproverComments = string.Empty;
                                        }

                                        /*
                                        if (!String.IsNullOrEmpty(objReader["ows_Approver"]))
                                        {
                                            item.strApprovedBy = objReader["ows_Approver"].ToString();

                                        }

                                        if (!String.IsNullOrEmpty(objReader["ows_Approval_x0020_Comments"]))
                                            item.strApproverComments = objReader["ows_Approval_x0020_Comments"].ToString();

                                        if (!String.IsNullOrEmpty(objReader["ows__ModerationStatus"]))
                                        {
                                            //ows__ModerationStatus
                                            // 0 = Apprvoed
                                            // 1 = Rejected
                                            // 2 = Pending
                                            // 3 = Draft
                                            // 4 = Scheduled

                                            switch (Convert.ToInt32(objReader["ows__ModerationStatus"].ToString()))
                                            {
                                                case 0:
                                                    item.strApprovalStatus = "Approved";
                                                    break;
                                                case 1:
                                                    item.strApprovalStatus = "Rejected";
                                                    break;
                                                case 2:
                                                    item.strApprovalStatus = "Pending";
                                                    break;
                                                case 3:
                                                    item.strApprovalStatus = "Draft";
                                                    break;
                                                case 4:
                                                    item.strApprovalStatus = "Scheduled";
                                                    break;
                                                default:
                                                    item.strApprovalStatus = string.Empty;
                                                    break;
                                            }

                                        }
                                        */

                                        switch (DocumentType)
                                        {
                                            case "File":
                                                item.strspCreatedBy = objReader["ows_Created_x0020_By"].Split('#').Last();
                                                item.strspModifiedBy = objReader["ows_Modified_x0020_By"].Split('#').Last(); //->Clean "i:0#.w|anglo\wongbo"
                                                item.intspFileSizeDisplay = Convert.ToInt32(objReader["ows_FileSizeDisplay"]); // in B NOT KB
                                                item.strspFileType = objReader["ows_File_x0020_Type"].ToString();
                                                TempStr = ListNameStr.Replace("(", "").Replace(")", "").Replace("-", "");
                                                //item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "").Substring(0, item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "").Length - item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "").Split('/').Last().Length - 1);
                                                item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "").Substring(0, item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "").Length - item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "").Split('/').Last().Length - 1);
                                                break;
                                            case "Folder":
                                                item.strspCreatedBy = objReader["ows_Author"].Split('#').Last();
                                                item.intspFileSizeDisplay = -1;
                                                item.strspFileType = "";
                                                TempStr = ListNameStr.Replace("(", "").Replace(")", "").Replace("-", "");

                                                //item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "");
                                                item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "");
                                                if (string.IsNullOrEmpty(item.strspVirtual_LocalRelativePath))
                                                    item.strspVirtual_LocalRelativePath = "/";
                                                switch (UpdateType)
                                                {
                                                    case "New":
                                                    case "Delete":
                                                    case "RemoteDelete":
                                                    case "Existing":
                                                        item.strspModifiedBy = objReader["ows_Author"].Split('#').Last();
                                                        break;
                                                    default:
                                                        //Only Rename has "ows_Modified_x0020_By"
                                                        item.strspModifiedBy = objReader["ows_Modified_x0020_By"].Split('#').Last(); //->Clean "i:0#.w|anglo\wongbo"
                                                        break;
                                                }
                                                break;
                                            default:
                                                break;
                                        }

                                        break;

                                    }

                                }

                            }
                        }
                    }
                }

                return item;

            }

        }

        public int[] getAllActiveVessels()
        {
            List<int> TempList = new List<int>();

            //Write to SQL
            //string strConnString = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

            string strConnString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
            SqlConnection con = new SqlConnection(strConnString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_AllActiveVessels";
            //cmd.Parameters.Add("@pNM_Active", SqlDbType.Bit).Value = true;
            cmd.Connection = con;

            try
            {
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                int i = 0;

                while (dr.Read())
                {
                    if (dr.IsDBNull(0))
                        TempList.Add(i);
                    else
                    {
                        TempList.Add(Convert.ToInt32(dr["IMO"].ToString()));
                        i++;
                    }

                }

                //Console.WriteLine("Record enquiried successfully");

            }
            catch (System.Exception ex)
            {
                //throw ex;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }

            TempList.OrderByDescending(i => i);

            int[] TempIntArray = TempList.ToArray();
            return TempIntArray;

        }

        public string EncodeSPStrings(string InStr)
        {
            InStr = InStr.Replace("&", "&amp;");
            InStr = InStr.Replace("'", "&apos;");
            return InStr;
        }

        public string DecodeSPStrings(string InStr)
        {
            InStr = InStr.Replace("\u0026", "&");
            InStr = InStr.Replace("&amp;", "&");
            InStr = InStr.Replace("&apos;", "'");
            return InStr;
        }

        public void RemoveTRGFile(string FilePathStr, string FileNameStr, bool UAT)
        {
            /*
             * spFSObjType = 0 (Nautic Document)
             * spFSObjType = 1 (Nautic Folder)
             * 
             */

                string ContentStr = string.Empty;
                string CurrentTimeStr = DateTime.Now.ToString("yyyyMMddmmss");
                string ErrorFilePath = string.Empty;

                try
                {
                    foreach (string FileStr in System.IO.Directory.GetFiles(FilePathStr, "*.trg"))
                    {
                        ErrorFilePath = FileStr;

                        ContentStr = FileStr.Split('\\').Last().Substring(0, FileStr.Split('\\').Last().Length - 4);

                        if (System.IO.File.Exists(FileStr))
                            System.IO.File.Move(FileStr, FilePathStr + "\\" + ContentStr + "_backup" + CurrentTimeStr + ".trg.bak");

                        ContentStr = string.Empty;

                    }

                    /*
                    if (System.IO.File.Exists(FilePathStr + "\\" + Session["uID"].ToString() + "-" + FileNameStr + ".trg"))
                        System.IO.File.Move(FilePathStr + "\\" + Session["uID"].ToString() + "-" + FileNameStr + ".trg", FilePathStr + "\\" + Session["uID"].ToString() + "-" + FileNameStr + "_backup" + CurrentTimeStr + ".trg.bak");
                    */

                }
                catch (Exception Ex)
                {
                    /*
                    if (System.IO.File.Exists(FilePathStr + "\\" + Session["uID"].ToString() + "-" + FileNameStr + "_error" + CurrentTimeStr + ".trg"))
                        System.IO.File.Delete(FilePathStr + "\\" + Session["uID"].ToString() + "-" + FileNameStr + "_error" + CurrentTimeStr + ".trg");

                    using (StreamWriter sw = System.IO.File.CreateText(FilePathStr + "\\" + Session["uID"].ToString() + "-" + FileNameStr + "_error" + CurrentTimeStr + ".trg"))
                    {
                        sw.WriteLine(Ex.Message);
                        sw.Close();
                        sw.Dispose();
                    }*/

                    if (!string.IsNullOrEmpty(ContentStr))
                    {
                        if (System.IO.File.Exists(FilePathStr + "\\" + ContentStr + ".trg"))
                        {
                            using (StreamWriter sw = System.IO.File.CreateText(FilePathStr + "\\" + ContentStr + "_error" + CurrentTimeStr + ".trg.bak"))
                            {
                                sw.WriteLine(Ex.Message);
                                sw.Close();
                                sw.Dispose();
                            }

                            System.IO.File.Delete(FilePathStr + "\\" + ContentStr + ".trg");

                        }

                    }

                }

            return;

        }

        public int getRepSr(string ModCodeStr, DateTime SystemDate)
        {
            string YYMMStr = SystemDate.ToString("yyMM");
            int RepSr = 0;
            bool GotRepSr = false;

            while ((YYMMStr == SystemDate.ToString("yyMM")) && (RepSr == 0))
            {
                //string cs = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

                string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_RepSr", conn))
                    {
                        string RepFullDateStr = SystemDate.ToString("yyyy-MM-dd");

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@pModCode", SqlDbType.VarChar).Value = ModCodeStr;
                        cmd.Parameters.Add("@pDate", SqlDbType.Date).Value = RepFullDateStr;
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            if (dr.IsDBNull(0))
                                RepSr = 0;
                            else
                            {
                                RepSr = Convert.ToInt32(dr[0].ToString());
                                GotRepSr = true;
                            }

                        }

                    }
                }

                if (RepSr == 0)
                    SystemDate = SystemDate.AddDays(-1);

            }

            return RepSr;

        }

        private class_ReplicationFileFolderJSONFile AddSPDocumentLibraryInfoToJSON(string docLib)
        {
            class_ReplicationFileFolderJSONFile SPFolderInfosJSON = new class_ReplicationFileFolderJSONFile();

            //using (ClientContext clientContext = new ClientContext(@"http://oneview.angloeastern.com/sites/oneview/nsdl"))
            using (ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl"))
            {
                //clientContext.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //clientContext.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");

                Microsoft.SharePoint.Client.Folder doclib = clientContext.Web.GetFolderByServerRelativeUrl(docLib.Replace("(", "").Replace(")", "").Replace("-", ""));
                clientContext.Load(doclib);
                //clientContext.Load(doclib.Folders);
                clientContext.ExecuteQuery();

                string TempStr = doclib.ServerRelativeUrl;

                Microsoft.SharePoint.Client.ListItem DLListItems = doclib.ListItemAllFields;
                
                class_replicationlist item = new class_replicationlist();
                //item.intspID = Convert.ToInt32(doclib.ProgID);
                //item.strspUIVersionString = objReader["ows__UIVersionString"].ToString();
                //item.intspContentVersion = Convert.ToInt32(objReader["ows_ContentVersion"].Split('#').Last());
                item.blnspIsCurrentVersion = true;//Convert.ToBoolean(Convert.ToInt32(objReader["ows__IsCurrentVersion"].ToString()));
                //item.strspAuthor = objReader["ows_Author"].Split('#').Last();
                item.strspContentType = "Nautic Folder";// bjReader["ows_ContentType"].ToString();
                item.intspFSObjType = 1;// Convert.ToInt32(objReader["ows_FSObjType"].Split('#').Last());
                //item.intspFolderChildCount = doclib.Folders.Count;// Convert.ToInt32(objReader["ows_FolderChildCount"].Split('#').Last());
                //item.intspItemChildCount = doclib.ItemCount;// Convert.ToInt32(objReader["ows_ItemChildCount"].Split('#').Last());
                // Converting "\u0026amp;" from SharePoint
                //item.strspBaseName = doclib.Name;// objReader["ows_BaseName"].ToString().Replace("\u0026", "&");
                item.strspBaseName = docLib.Replace("/sites/nsdl/", "");
                //item.strspBaseName = item.strspBaseName.Replace("&amp;", "&");
                //item.strspBaseName = item.strspBaseName.Replace("&#39;", "'");
                // End
                item.strspLinkFilename = item.strspBaseName;// objReader["ows_LinkFilename"].ToString();
                item.dttspCreated = doclib.TimeCreated;// Convert.ToDateTime(objReader["ows_Created"]); // for "2017-08-10 14:00:59"
                item.strspLastModified = doclib.TimeLastModified.ToString("yyyy-MM-dd hh:mm:ss");// objReader["ows_Last_x0020_Modified"].Split('#').Last(); //"54;#2017-08-10 14:00:59"->Convert.TODateTime
                item.dttspModified = doclib.TimeLastModified;// Convert.ToDateTime(objReader["ows_Modified"].Split('#').Last()); //->Convert.TODateTime for "2017-08-10 14:00:59"
                //item.strspEditor = objReader["ows_Editor"].Split('#').Last(); //->Clean "888;#HKG - Bosco WONG"
                item.strspServerUrl = doclib.ServerRelativeUrl;// objReader["ows_ServerUrl"].ToString();
                item.strspUniqueId = doclib.UniqueId.ToString().ToUpper();// objReader["ows_UniqueId"].Split('#').Last().Replace("{", "").Replace("}", ""); //->Clean "54;#{D29AA14E-F6D4-452C-A127-B2908B16C3B8}"
                item.strspParentUniqueId = "0";// objReader["ows_ParentUniqueId"].Split('#').Last().Replace("{", "").Replace("}", ""); //->Clean "54;#{C1ED19BB-9A3F-4E64-8C84-6351092838A1}"
                //item.strspGUID = objReader["ows_GUID"].ToString().Replace("{", "").Replace("}", "");  //Clean "{476D351A-4342-4472-AF42-58AC7351383C}"
                item.strnmDocumentLibrary = item.strspBaseName;// ListNameStr; //->Convert from "ServerUrl"
                item.intspFileSizeDisplay = -1;
                item.strspFileType = "";

                //SPFolderInfosJSON.UpdateType = "Existing";
                SPFolderInfosJSON.RepSPDocsInfo = item;
                SPFolderInfosJSON.RepDistributionStatus = "CR";
            }

            return SPFolderInfosJSON;
        }

        public string CreateFileFolderReplication(class_ReplicationFileFolderJSONFile NewJSONFile, string RepModCodeStr, int SelectedIMOInt, bool IMOProvided = false)//, int[] SelectedIMOList = null)
        {
            string ORGstrspVirtual_LocalRelativePath = NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath;

            class_ReplicationFileFolderJSONFull ReplicationFileFolderJSONFileList = new class_ReplicationFileFolderJSONFull();
            ReplicationFileFolderJSONFileList.entries = new List<class_ReplicationFileFolderJSONFile>();
            class_ReplicationJSONHeader JSONHeader = new class_ReplicationJSONHeader();
            string JSONString = string.Empty;
            JavaScriptSerializer js = new JavaScriptSerializer();

            string ReplicationStatus = string.Empty;
            int[] SelectedIMO = null;

            if (IMOProvided)
                SelectedIMO = new int[] { SelectedIMOInt };
            else
                SelectedIMO = getAllActiveVessels();
            //int[] SelectedIMO = { 9697832, 9999997, 9999998, 9999999 };

            string RepDirectoryStr = string.Empty;
            string RepRefStr = string.Empty;
            string RepFolSrcStr = "OUT";
            string RepSMTPSrcStr = "nsoffice@oceanone.com";
            DateTime thisDay = DateTime.Today;
            string RepFullDateStr = thisDay.ToString("yyyy-MM-dd");
            string RepHeaderStatusStr = "Active";
            //string RepYYMMStr = thisDay.ToString("yyMM");
            string RepYYMMStr = string.Empty;
            //string RepYYMMDDStr = thisDay.ToString("yyMMdd");
            string RepYYMMDDStr = string.Empty;
            //int RepSrInt = getRepSr(RepModCodeStr, thisDay);
            int RepSrInt = -1;
            int RepIMOInt = 0;
            string RepDOCUniqueIdStr = string.Empty;
            string JSONFileNameStr = "fatt.nsf";
            //string RepDocDistributionStatusStr = string.Empty;

            if (RepModCodeStr.IndexOf("DMSSR") >= 0)
            {
                //RepRefStr = getnmRepSr(NewJSONFile.SourceURLStr);
                //RepRefStr = "DMSSR17120349732589171211_RepID"
                RepRefStr = RepModCodeStr;
                RepSrInt = Convert.ToInt32(RepRefStr.Substring(9, 3));
                RepYYMMStr = RepRefStr.Substring(5, 4);
                RepIMOInt = Convert.ToInt32(RepRefStr.Substring(12, 7));
                RepYYMMDDStr = RepRefStr.Substring(19, 6);
                NewJSONFile.RepID = Convert.ToInt32(RepRefStr.Split('_').Last());
                RepRefStr = RepRefStr.Substring(0, 12);
                //RepDocDistributionStatusStr = "AC";
                RepModCodeStr = "DMSSR";
            }
            else
            {
                RepYYMMStr = thisDay.ToString("yyMM");
                RepYYMMDDStr = thisDay.ToString("yyMMdd");
                RepSrInt = getRepSr(RepModCodeStr, thisDay);
                //RepDocDistributionStatusStr = "CR";
            }

            for (int i = 0; i < SelectedIMO.Length; i++)
            {
                if ((SelectedIMO[i] == SingleShipIMOInt) || MultipleShipTest)
                {
                    // Replication existed for today
                    //if (getRepSr(RepModCodeStr, thisDay) > getRepSr(RepModCodeStr, thisDay.AddDays(-1)))
                    if ((RepModCodeStr == "DMSSR") || (RepModCodeStr != "DMSSR" && (getRepSr(RepModCodeStr, thisDay) > getRepSr(RepModCodeStr, thisDay.AddDays(-1)))))
                    {
                        RepDirectoryStr = EnquiryRegistryFolderPath("OutboundQueue") + SelectedIMO[i].ToString() + @"\";
                        RepFolSrcStr = @"OUT\" + SelectedIMO[i].ToString() + @"\";
                        RepRefStr = RepModCodeStr + RepYYMMStr;
                        RepRefStr = RepModCodeStr + RepYYMMStr + ConvertIntToString(RepSrInt, 3);
                        RepDirectoryStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepFolSrcStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepIMOInt = SelectedIMO[i];
                        if (!System.IO.Directory.Exists(RepDirectoryStr))
                        {
                            System.IO.Directory.CreateDirectory(RepDirectoryStr);
                        }

                        try
                        {
                            // Check if file already exists. If yes, delete it. 
                            if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                            {
                                System.IO.StreamReader myFile = new System.IO.StreamReader(RepDirectoryStr + "\\" + JSONFileNameStr);
                                string myString = string.Empty;

                                while ((myString = myFile.ReadLine()) != null)
                                {
                                    class_ReplicationFileFolderJSONFull ReplicationFileFolderJSONFileFromFile = new class_ReplicationFileFolderJSONFull();
                                    ReplicationFileFolderJSONFileFromFile = js.Deserialize<class_ReplicationFileFolderJSONFull>(myString);

                                    foreach (class_ReplicationFileFolderJSONFile JSONFiles in ReplicationFileFolderJSONFileFromFile.entries)
                                    {
                                        //ReplicationFileFolderJSONFileList.entries.Add(JSONFiles);

                                        if ((JSONFiles.RepSPDocsInfo.strspUniqueId == NewJSONFile.RepSPDocsInfo.strspUniqueId) && (JSONFiles.RepSPDocsInfo.strspContentType == "Nautic Folder"))
                                        {
                                            JSONFiles.RepSPDocsInfo = GetUpdatedSPDocInfos(NewJSONFile.RepSPDocsInfo.strspServerUrl, NewJSONFile.RepSPDocsInfo.strspUniqueId, "Folder", "Existing");

                                            JSONHeader.RepRef = RepRefStr;
                                            JSONHeader.RepModCode = RepModCodeStr;
                                            JSONHeader.RepFolSrc = RepFolSrcStr;
                                            JSONHeader.RepSMTPSrc = RepSMTPSrcStr;
                                            JSONHeader.RepFullDate = RepFullDateStr;
                                            JSONHeader.RepHeaderStatus = RepHeaderStatusStr;
                                            JSONHeader.RepYYMM = Convert.ToInt32(RepYYMMStr);
                                            JSONHeader.RepSr = RepSrInt;
                                            JSONHeader.RepIMO = RepIMOInt;

                                        }
                                        else if ((JSONFiles.RepSPDocsInfo.strspUniqueId == NewJSONFile.RepSPDocsInfo.strspUniqueId) && ((NewJSONFile.UpdateType == "Delete") || (NewJSONFile.UpdateType == "RemoteDelete")))
                                        {
                                            string DestinationNameStr = JSONFiles.RepSPDocsInfo.strspUniqueId + "-" + JSONFiles.RepSPDocsInfo.strspBaseName + "_" + JSONFiles.RepSPDocsInfo.strspUIVersionString + "." + JSONFiles.RepSPDocsInfo.strspFileType;
                                            if (System.IO.File.Exists(RepDirectoryStr + "\\" + DestinationNameStr))
                                            {
                                                System.IO.File.Delete(RepDirectoryStr + "\\" + DestinationNameStr);
                                            }
                                        }
                                        else
                                        {
                                            ReplicationFileFolderJSONFileList.entries.Add(JSONFiles);
                                            JSONHeader = ReplicationFileFolderJSONFileFromFile.RepHeader;
                                        }

                                    }

                                }

                                myFile.Close();
                                myFile.Dispose();

                                try
                                {
                                    if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                        System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                } catch (System.IO.IOException)
                                {
                                    if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                                    {
                                        //Load your file using FileStream class and release an file through stream.Dispose()
                                        using (FileStream stream = new FileStream(RepDirectoryStr + "\\" + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                                        {
                                            stream.Close();
                                            stream.Dispose();
                                        }

                                        // delete the file.
                                        System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                                    }

                                }
                            
                            }
                            else
                            {
                                JSONHeader.RepRef = RepRefStr;
                                JSONHeader.RepModCode = RepModCodeStr;
                                JSONHeader.RepFolSrc = RepFolSrcStr;
                                JSONHeader.RepSMTPSrc = RepSMTPSrcStr;
                                if (RepModCodeStr != "DMSSR")
                                    JSONHeader.RepFullDate = RepFullDateStr;
                                else
                                    JSONHeader.RepFullDate = "20" + RepYYMMDDStr.Substring(0, 2) + "-" + RepYYMMDDStr.Substring(2, 2) + "-" + RepYYMMDDStr.Substring(4, 2);
                                JSONHeader.RepHeaderStatus = RepHeaderStatusStr;
                                JSONHeader.RepYYMM = Convert.ToInt32(RepYYMMStr);
                                JSONHeader.RepSr = RepSrInt;
                                JSONHeader.RepIMO = RepIMOInt;
                            }

                        } // end of try
                        catch (System.Exception Ex)
                        {
                            //Console.WriteLine(Ex.ToString());
                        }

                    }
                    else // New Replication for today
                    {
                        RepDirectoryStr = EnquiryRegistryFolderPath("OutboundQueue") + SelectedIMO[i].ToString() + @"\";
                        RepFolSrcStr = @"OUT\" + SelectedIMO[i].ToString() + @"\";
                        RepSrInt++;
                        RepRefStr = RepModCodeStr + RepYYMMStr;
                        RepRefStr = RepModCodeStr + RepYYMMStr + ConvertIntToString(RepSrInt, 3);
                        RepDirectoryStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepFolSrcStr += RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_";
                        RepIMOInt = SelectedIMO[i];
                        if (!System.IO.Directory.Exists(RepDirectoryStr))
                        {
                            System.IO.Directory.CreateDirectory(RepDirectoryStr);

                        }

                        JSONHeader.RepRef = RepRefStr;
                        JSONHeader.RepModCode = RepModCodeStr;
                        JSONHeader.RepFolSrc = RepFolSrcStr;
                        JSONHeader.RepSMTPSrc = RepSMTPSrcStr;
                        if (RepModCodeStr != "DMSSR")
                            JSONHeader.RepFullDate = RepFullDateStr;
                        else
                            JSONHeader.RepFullDate = "20" + RepYYMMDDStr.Substring(0, 2) + "-" + RepYYMMDDStr.Substring(2, 2) + "-" + RepYYMMDDStr.Substring(4, 2);
                        JSONHeader.RepHeaderStatus = RepHeaderStatusStr;
                        JSONHeader.RepYYMM = Convert.ToInt32(RepYYMMStr);
                        JSONHeader.RepSr = RepSrInt;
                        JSONHeader.RepIMO = RepIMOInt;

                    }
                    
                    // If DMSSR Folder, RepID = 0 (i.e. no replication ID of Ship Folder had been created @ DMSSR 
                    if ((RepModCodeStr == "DMSSR") && (NewJSONFile.RepSPDocsInfo.intspFSObjType == 1))
                        NewJSONFile.RepID = 0;

                    /*
                    if (RepModCodeStr != "DMSSR")
                    {
                        string ldappath = "LDAP://hkgsrvdc1.angloeasterngroup.com:389";

                        if (!string.IsNullOrEmpty(NewJSONFile.RepSPDocsInfo.strspModifiedBy))
                        {
                            if (NewJSONFile.UpdateType == "Delete")
                            {
                                NewJSONFile.RepSPDocsInfo.strspModifiedBy = GetAttribute(ldappath, Session["uID"].ToString(), "displayName");

                            }
                            else
                            {
                                if (NewJSONFile.RepSPDocsInfo.strspModifiedBy.IndexOf("\\") >= 0)
                                    NewJSONFile.RepSPDocsInfo.strspModifiedBy = GetAttribute(ldappath, NewJSONFile.RepSPDocsInfo.strspModifiedBy.Split('\\').Last(), "displayName");

                            }

                        }

                        ReplicationHeaderToDB(JSONHeader);
                        string dbWriteResponse = ReplicationDetailsToDB(JSONHeader, NewJSONFile);

                        if (string.IsNullOrEmpty(dbWriteResponse)) { }
                        else
                        {
                            return dbWriteResponse;
                        }

                    }
                    */

                    // Create a new JSON file 
                    //NewJSONFile.RepID = 100000106;
                    ReplicationFileFolderJSONFileList.RepHeader = JSONHeader;
                    ReplicationFileFolderJSONFileList.entries.Add(NewJSONFile);
                    JSONString += js.Serialize(ReplicationFileFolderJSONFileList);

                    try
                    {
                        if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                        using (StreamWriter sw = System.IO.File.CreateText(RepDirectoryStr + "\\" + JSONFileNameStr))
                        {
                            sw.WriteLine(JSONString);
                            sw.Close();
                            sw.Dispose();

                        }

                    }
                    catch (System.IO.IOException)
                    {
                        if (System.IO.File.Exists(RepDirectoryStr + "\\" + JSONFileNameStr))
                        {
                            //Load your file using FileStream class and release an file through stream.Dispose()
                            using (FileStream stream = new FileStream(RepDirectoryStr + "\\" + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                            {
                                stream.Close();
                                stream.Dispose();
                            }

                            // delete the file.
                            System.IO.File.Delete(RepDirectoryStr + "\\" + JSONFileNameStr);

                            using (StreamWriter sw = System.IO.File.CreateText(RepDirectoryStr + "\\" + JSONFileNameStr))
                            {
                                sw.WriteLine(JSONString);
                                sw.Close();
                                sw.Dispose();

                            }

                        }

                    }

                    //string folPath = "http://oneview.angloeastern.com" + NewJSONFile.RepSPDocsInfo.strspServerUrl;
                    string folPath = "http://dms.angloeastern.com" + NewJSONFile.RepSPDocsInfo.strspServerUrl;

                    //Non-RootFolder

                    string ListNameStr = getListName(folPath.Replace("http://dms.angloeastern.com", ""));

                    if (folPath.Split('/').Last() != ListNameStr)
                    {

                        if ((NewJSONFile.UpdateType == "Delete") || (NewJSONFile.UpdateType == "RemoteDelete"))
                        {
                            if (NewJSONFile.RepSPDocsInfo.strspContentType == "Nautic Folder")
                            {
                                //Shift Up one level for existing folder
                                folPath = folPath.Substring(0, folPath.Length - folPath.Split('/').Last().Length - 1);
                                //NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath = NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Substring(0, NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Length - NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Split('/').Last().Length - 1);
                                NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath = NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Substring(0, NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Length - NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Split('/').Last().Length - 1);

                            }

                        }

                        AddSPFolderInfostoJSON(folPath, RepDirectoryStr + "\\" + NewJSONFile.RepSPDocsInfo.strspUniqueId + "-" + NewJSONFile.RepSPDocsInfo.strspBaseName + "_" + NewJSONFile.RepSPDocsInfo.strspUIVersionString + "." + NewJSONFile.RepSPDocsInfo.strspFileType, JSONHeader, NewJSONFile);

                    }

                    //}

                    ReplicationFileFolderJSONFileList = new class_ReplicationFileFolderJSONFull();
                    ReplicationFileFolderJSONFileList.entries = new List<class_ReplicationFileFolderJSONFile>();
                    JSONHeader = new class_ReplicationJSONHeader();
                    JSONString = string.Empty;
                    NewJSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath = ORGstrspVirtual_LocalRelativePath;

                }

                // Disabled
                //CreateTRGFile(DirectoryShiftLevels(EnquiryRegistryFolderPath("OutboundQueue"), 1), RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_", 1, true);
                if (RepModCodeStr != "DMSSR")
                    CreateTRGFile(EnquiryRegistryFolderPath("OutboundQueue"), RepRefStr + SelectedIMO[i].ToString() + RepYYMMDDStr + "_", 1, true);

            } // End of for-loop: SelectedIMO.Length

            //Replication is Ready
            switch (RepModCodeStr)
            {
                case "DMSAR":
                    ReplicationStatus += "\r\nAuto Replication request has been created successfully!!";
                    break;
                case "DMSSR":
                    ReplicationStatus += "\r\nShip Replication acknowledgement has been created successfully!!";
                    break;
                default:
                    break;
            }

            return ReplicationStatus;

        }

        public string getListName(string strSearch)
        {
            //string strDocumentList = strSearch.Replace("/sites/oneview/nsdl/", "");
            string strDocumentList = strSearch.Replace("/sites/nsdl/", "");
            int intPos = strDocumentList.IndexOf("/");
            if (intPos >= 0)
                strDocumentList = strDocumentList.Substring(0, intPos);

            switch (strDocumentList)
            {

                /*case "01 Ship Board Manuals and Plans":
                    strDocumentList = "01 Shipboard Manuals and Plans";
                    break;*/
                case "01B Ship Board Manuals Russian":
                    strDocumentList = "01B Ship Board Manuals (Russian)";
                    break;
                case "02 Ship Board Manuals Chinese":
                    strDocumentList = "02 Ship Board Manuals (Chinese)";
                    break;
                case "01G RORO Manuals":
                    strDocumentList = "01G RO-RO Manuals";
                    break;
                case "11 NonRoutine Events":
                    strDocumentList = "11 Non-Routine Events";
                    break;
                case "01I Ship Board Manuals Bahasa":
                    strDocumentList = "01I Ship Board Manuals (Bahasa)";
                    break;
                case "01Z Manuals for Ship Types Currently not under Mgm":
                    strDocumentList = "01Z Manuals for Ship Types Currently not under Mgmt";
                    break;
                default:
                    break;
            }

            return strDocumentList;

        }

        public int GetSPID(string DocumentType, string folPath, string listName, string UpdateTypeStr, string UpdateStr = null)
        {

            /*
            listName = listName.Replace("&", "&amp;"); // Do not change the original document list NAME even it has ( or - character 

            folPath = folPath.Replace("&", "&amp;");
            folPath = folPath.Replace("(", ""); // Remove all special characters including (, ), <, > or -
            folPath = folPath.Replace(")", "");
            //string RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview"));
            string RootFolderStr = folPath.Split('/').Last();
            RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview") - RootFolderStr.Split('/').Last().Length - 1);
            */


            string RelativeStr = folPath.Split('/').Last();

            //string RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview") - RelativeStr.Length - 1);

            string RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl") - RelativeStr.Length - 1);

            // Remove all special characters including (, ), <, > or -
            RootFolderStr = RootFolderStr.Replace("&", "&amp;");
            RootFolderStr = RootFolderStr.Replace("'", "&apos;");

            using (var clientContext = new nsdlWebService.Lists())
            {
                //client.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //client.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");
                //client.Url = "http://oneview.angloeastern.com/sites/oneview/nsdl/_vti_bin/lists.asmx";
                clientContext.Url = "http://dms.angloeastern.com/sites/nsdl/_vti_bin/lists.asmx";
                XmlNode ndListItems = null;
                XmlDocument xdoc = new XmlDocument();
                XmlNode ndQuery = xdoc.CreateNode(XmlNodeType.Element, "Query", "");
                XmlNode ndViewFields = xdoc.CreateNode(XmlNodeType.Element, "ViewFields", "");
                XmlNode ndQueryOptions = xdoc.CreateNode(XmlNodeType.Element, "QueryOptions", "");
                ndQuery.InnerXml = "";
                ndViewFields.InnerXml = "";

                StringBuilder sbQuery = new StringBuilder();
                switch (DocumentType)
                {
                    case "Folder":
                        sbQuery.AppendLine(@"<Where><Eq><FieldRef Name = 'FSObjType'/><Value Type = 'Integer'>1</Value></Eq></Where>");
                        break;
                    case "File":
                        sbQuery.AppendLine(@"<Where><Eq><FieldRef Name = 'FSObjType'/><Value Type = 'Integer'>0</Value></Eq></Where>");
                        break;
                    default:
                        break;
                }
                ndQuery.InnerXml = sbQuery.ToString();


                StringBuilder sbQueryOptions = new StringBuilder();
                sbQueryOptions.AppendLine(@"<IncludeAttachmentUrls>TRUE </IncludeAttachmentUrls>");
                sbQueryOptions.AppendLine(@"<QueryOptions>");
                sbQueryOptions.AppendLine(@"<Folder>" + RootFolderStr + "</Folder>");
                sbQueryOptions.AppendLine(@"<Folder>" + listName + "</Folder>");
                sbQueryOptions.AppendLine(@"</QueryOptions>");
                ndQueryOptions.InnerXml = sbQueryOptions.ToString();


                ndListItems = clientContext.GetListItems(listName, "", ndQuery, ndViewFields, "0", ndQueryOptions, null);

                /*
                XmlDocument TempXMLDoc = new XmlDocument();
                TempXMLDoc.LoadXml(ndListItems.OuterXml);
                TempXMLDoc.Save(ProcessedFilesPath + "SPDocInfos.xml");
                */

                if (ndListItems != null)
                {
                    foreach (XmlNode node in ndListItems.ChildNodes)
                    {
                        XmlNodeReader objReader = new XmlNodeReader(node);
                        while (objReader.Read())
                        {
                            if (objReader["ows_EncodedAbsUrl"] != null)
                            {
                                //string TempStr = string.Empty;
                                //TempStr = objReader["ows_ServerUrl"].ToString();
                                //TempStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview"));

                                //folPath = folPath.Replace("&amp;", "&");

                                //@"/sites/oneview/nsdl/01B Ship Board Manuals Russian/202 H&amp;S"
                                //if (objReader["ows_ServerUrl"].ToString() == folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")))

                                if (UpdateTypeStr == "New")
                                {
                                    if (objReader["ows_ServerUrl"].ToString().ToUpper() == folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")).ToUpper())
                                        return Convert.ToInt32(objReader["ows_ID"].ToString());

                                }
                                else
                                {
                                    if (objReader["ows_ServerUrl"].ToString() == folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")))
                                        return Convert.ToInt32(objReader["ows_ID"].ToString());

                                }

                            }

                        }
                    }
                }
            }

            return -1;
        }

        public string getnmRepSr(string folpath)
        {
            string nmRepRefStr = string.Empty;
            //string ServerUrlStr = folpath.Replace("http://oneview.angloeastern.com/sites/oneview/nsdl/", "");
            string ServerUrlStr = folpath.Replace("http://dms.angloeastern.com/sites/nsdl/", "");
            string ListNameStr = ServerUrlStr.Split('/').First();
            ServerUrlStr = ServerUrlStr.Replace(ListNameStr, "");
            string nmDocumentLibrarylStr = ListNameStr + "." + ServerUrlStr.Substring(1, ServerUrlStr.Length - 1).Split('/').First().Replace("-", ".");

            //string cs = @"Data Source=10.0.1.214\SQL2016;Initial Catalog=NAUTIC" + OperationModeStr + ";" + "User id=sa;Password=@Oneview1;";

            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("spShipToShorerepRefGet", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@pnmServerUrl", SqlDbType.VarChar).Value = ServerUrlStr;
                    cmd.Parameters.Add("@pnmDocumentLibrary", SqlDbType.VarChar).Value = nmDocumentLibrarylStr;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        if (dr.IsDBNull(0))
                            nmRepRefStr = string.Empty;
                        else
                        {
                            nmRepRefStr = dr[1].ToString();
                            nmRepRefStr += dr[2].ToString();
                            nmRepRefStr += dr[0].ToString();

                        }

                    }
                }

            }

            return nmRepRefStr;

        }

        public class_replicationlist GetUpdatedSPDocInfos(string folPath, string SPUniquedIDStr, string DocumentType, string UpdateType)
        {
            string ListNameStr = getListName(folPath);
            string RelativeStr = folPath.Split('/').Last();

            //string RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview") - RelativeStr.Length - 1);
            string RootFolderStr = folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl") - RelativeStr.Length - 1);
            //RelativeStr = RootFolderStr.Replace("/sites/oneview/nsdl/"+ ListNameStr,"")+"/"+ RelativeStr;

            // Remove all special characters including (, ), <, > or -
            RootFolderStr = RootFolderStr.Replace("&", "&amp;");
            RootFolderStr = RootFolderStr.Replace("'", "&apos;");
            //RootFolderStr = RootFolderStr.Replace("(", ""); 
            //RootFolderStr = RootFolderStr.Replace(")", "");

            class_replicationlist item = new class_replicationlist();

            using (var clientContext = new nsdlWebService.Lists())
            {
                //client.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //client.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");
                //client.Url = "http://oneview.angloeastern.com/sites/oneview/nsdl/_vti_bin/lists.asmx";
                clientContext.Url = "http://dms.angloeastern.com/sites/nsdl/_vti_bin/lists.asmx";
                XmlNode ndListItems = null;
                XmlDocument xdoc = new XmlDocument();
                XmlNode ndQuery = xdoc.CreateNode(XmlNodeType.Element, "Query", "");
                XmlNode ndViewFields = xdoc.CreateNode(XmlNodeType.Element, "ViewFields", "");
                XmlNode ndQueryOptions = xdoc.CreateNode(XmlNodeType.Element, "QueryOptions", "");
                ndQuery.InnerXml = "";
                ndViewFields.InnerXml = "";

                StringBuilder sbQuery = new StringBuilder();
                switch (DocumentType)
                {
                    case "Folder":
                        sbQuery.AppendLine(@"<Where><Eq><FieldRef Name = 'FSObjType'/><Value Type = 'Integer'>1</Value></Eq></Where>");
                        break;
                    case "File":
                        sbQuery.AppendLine(@"<Where><Eq><FieldRef Name = 'FSObjType'/><Value Type = 'Integer'>0</Value></Eq></Where>");
                        break;
                    default:
                        break;
                }
                ndQuery.InnerXml = sbQuery.ToString();

                StringBuilder sbQueryOptions = new StringBuilder();
                sbQueryOptions.AppendLine(@"<IncludeAttachmentUrls>TRUE </IncludeAttachmentUrls>");
                sbQueryOptions.AppendLine(@"<QueryOptions>");
                sbQueryOptions.AppendLine(@"<Folder>" + RootFolderStr + "</Folder>");
                sbQueryOptions.AppendLine(@"<Folder>" + ListNameStr + "</Folder>");
                sbQueryOptions.AppendLine(@"</QueryOptions>");
                ndQueryOptions.InnerXml = sbQueryOptions.ToString();

                ndListItems = clientContext.GetListItems(ListNameStr, "", ndQuery, ndViewFields, "0", ndQueryOptions, null);

                /*
                XmlDocument TempXMLDoc = new XmlDocument();
                TempXMLDoc.LoadXml(ndListItems.OuterXml);
                TempXMLDoc.Save(ProcessedFilesPath + "SPDocInfos.xml");
                */

                /*
                string TempStr = ListNameStr;
                TempStr = TempStr.Replace("&", "&amp;");
                TempStr = TempStr.Replace("'", "&apos;");
                TempStr = TempStr.Replace("(", "");
                TempStr = TempStr.Replace(")", "");

                folPath = "/sites/oneview/nsdl/" + TempStr + RelativeStr;
                //folPath = folPath.Replace("&amp;", "&");
                //folPath = folPath.Replace("&apos;","'");
                */

                if (ndListItems != null)
                {
                    foreach (XmlNode node in ndListItems.ChildNodes)
                    {
                        XmlNodeReader objReader = new XmlNodeReader(node);
                        while (objReader.Read())
                        {
                            if (objReader["ows_EncodedAbsUrl"] != null)
                            {
                                //@"/sites/oneview/nsdl/01B Ship Board Manuals Russian/201 MSM/03.00 Mission statement &amp; Core values of the company.pdf"

                                string TempStr = objReader["ows_ServerUrl"].ToString();

                                if (objReader["ows_UniqueId"].Split('#').Last().Replace("{", "").Replace("}", "") == SPUniquedIDStr)
                                {
                                    // please do in here
                                    item.intspID = Convert.ToInt32(objReader["ows_ID"]);
                                    item.strspUIVersionString = objReader["ows__UIVersionString"].ToString();
                                    if (DocumentType == "Folder")
                                        item.intspContentVersion = 0;
                                    else
                                        //item.intspContentVersion = Convert.ToInt32(objReader["ows_ContentVersion"].Split('#').Last());
                                        item.intspContentVersion = -1;
                                    item.blnspIsCurrentVersion = Convert.ToBoolean(Convert.ToInt32(objReader["ows__IsCurrentVersion"].ToString()));
                                    item.strspAuthor = objReader["ows_Author"].Split('#').Last();
                                    item.strspContentType = objReader["ows_ContentType"].ToString();
                                    item.intspFSObjType = Convert.ToInt32(objReader["ows_FSObjType"].Split('#').Last());
                                    item.intspFolderChildCount = Convert.ToInt32(objReader["ows_FolderChildCount"].Split('#').Last());
                                    item.intspItemChildCount = Convert.ToInt32(objReader["ows_ItemChildCount"].Split('#').Last());
                                    // Converting "\u0026amp;" from SharePoint
                                    item.strspBaseName = objReader["ows_BaseName"].ToString().Replace("\u0026", "&");
                                    item.strspBaseName = item.strspBaseName.Replace("&amp;", "&");
                                    item.strspBaseName = item.strspBaseName.Replace("&#39;", "'");
                                    // End
                                    item.strspLinkFilename = objReader["ows_LinkFilename"].ToString();
                                    item.dttspCreated = Convert.ToDateTime(objReader["ows_Created"]); // for "2017-08-10 14:00:59"
                                    item.strspLastModified = objReader["ows_Last_x0020_Modified"].Split('#').Last(); //"54;#2017-08-10 14:00:59"->Convert.TODateTime
                                    item.dttspModified = Convert.ToDateTime(objReader["ows_Modified"].Split('#').Last()); //->Convert.TODateTime for "2017-08-10 14:00:59"
                                    item.strspEditor = objReader["ows_Editor"].Split('#').Last(); //->Clean "888;#HKG - Bosco WONG"
                                    item.strspServerUrl = objReader["ows_ServerUrl"].ToString();
                                    item.strspUniqueId = objReader["ows_UniqueId"].Split('#').Last().Replace("{", "").Replace("}", "").ToUpper(); //->Clean "54;#{D29AA14E-F6D4-452C-A127-B2908B16C3B8}"
                                    item.strspParentUniqueId = objReader["ows_ParentUniqueId"].Split('#').Last().Replace("{", "").Replace("}", ""); //->Clean "54;#{C1ED19BB-9A3F-4E64-8C84-6351092838A1}"
                                    item.strspGUID = objReader["ows_GUID"].ToString().Replace("{", "").Replace("}", "");  //Clean "{476D351A-4342-4472-AF42-58AC7351383C}"
                                    item.strnmDocumentLibrary = ListNameStr; //->Convert from "ServerUrl"

                                    if ((item.strspUIVersionString == "1.0") || (item.strspContentType == "Nautic Folder"))
                                        item.strspCheckInComments = string.Empty;
                                    else
                                        item.strspCheckInComments = objReader["ows__CheckinComment"].ToString().Split('#').Last();

                                    switch (DocumentType)
                                    {
                                        case "File":
                                            item.strspCreatedBy = objReader["ows_Created_x0020_By"].Split('#').Last();
                                            item.strspModifiedBy = objReader["ows_Modified_x0020_By"].Split('#').Last(); //->Clean "i:0#.w|anglo\wongbo"
                                            item.intspFileSizeDisplay = Convert.ToInt32(objReader["ows_FileSizeDisplay"]); // in B NOT KB
                                            item.strspFileType = objReader["ows_File_x0020_Type"].ToString();
                                            TempStr = ListNameStr.Replace("(", "");
                                            TempStr = TempStr.Replace(")", "");
                                            //item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "").Substring(0, item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "").Length - item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "").Split('/').Last().Length - 1);
                                            item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "").Substring(0, item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "").Length - item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "").Split('/').Last().Length - 1);
                                            break;
                                        case "Folder":
                                            item.strspCreatedBy = objReader["ows_Author"].Split('#').Last();
                                            item.intspFileSizeDisplay = -1;
                                            item.strspFileType = "";
                                            TempStr = ListNameStr.Replace("(", "");
                                            TempStr = TempStr.Replace(")", "");
                                            //item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/oneview/nsdl/" + TempStr, "");
                                            item.strspVirtual_LocalRelativePath = item.strspServerUrl.Replace("/sites/nsdl/" + TempStr, "");
                                            switch (UpdateType)
                                            {
                                                case "New":
                                                case "Delete":
                                                case "Existing":
                                                    item.strspModifiedBy = objReader["ows_Author"].Split('#').Last();
                                                    break;
                                                default:
                                                    //Only Rename has "ows_Modified_x0020_By"
                                                    item.strspModifiedBy = objReader["ows_Modified_x0020_By"].Split('#').Last(); //->Clean "i:0#.w|anglo\wongbo"
                                                    break;
                                            }
                                            break;
                                        default:
                                            break;
                                    }

                                    break;

                                }

                            }

                        }
                    }
                }
            }

            return item;

        }

        /*
        private void ReplicationHeaderToDB(class_ReplicationJSONHeader JSONHeader)
        {
            string strConnString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            SqlConnection con = new SqlConnection(strConnString);

            SqlCommand cmd1 = new SqlCommand();
            cmd1.CommandType = CommandType.StoredProcedure;
            cmd1.CommandText = "sprepRepHeaderAdd";

            cmd1.Parameters.Add("@pRepRef", SqlDbType.VarChar).Value = JSONHeader.RepRef;
            cmd1.Parameters.Add("@pModCode", SqlDbType.VarChar).Value = JSONHeader.RepModCode;
            cmd1.Parameters.Add("@pRepFolSrc", SqlDbType.VarChar).Value = JSONHeader.RepFolSrc;
            cmd1.Parameters.Add("@pSMTPSrc", SqlDbType.VarChar).Value = JSONHeader.RepSMTPSrc;
            cmd1.Parameters.Add("@pDate", SqlDbType.Date).Value = JSONHeader.RepFullDate;
            cmd1.Parameters.Add("@pStatus", SqlDbType.VarChar).Value = JSONHeader.RepHeaderStatus;
            cmd1.Parameters.Add("@pRepYyMm", SqlDbType.Int).Value = JSONHeader.RepYYMM;
            cmd1.Parameters.Add("@pRepSr", SqlDbType.Int).Value = JSONHeader.RepSr;
            cmd1.Connection = con;

            con.Open();
            cmd1.ExecuteNonQuery();
        }

        private string ReplicationDetailsToDB(class_ReplicationJSONHeader JSONHeader, class_ReplicationFileFolderJSONFile JSONFile)
        {
            //Debug
            //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug.txt", "Before sprepRepDocsAdd");

            string strConnString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            SqlConnection con = new SqlConnection(strConnString);

            SqlCommand cmd2 = new SqlCommand();
            SqlCommand cmd4 = new SqlCommand();

            cmd2.CommandType = CommandType.StoredProcedure;
            cmd4.CommandType = CommandType.StoredProcedure;

            cmd2.CommandText = "sprepRepDocsAdd";
            cmd4.CommandText = "sprepDocDistributionAdd";

            cmd2.Parameters.Add("@pRepRef", SqlDbType.VarChar).Value = JSONHeader.RepRef;
            cmd2.Parameters.Add("@pspID", SqlDbType.Int).Value = JSONFile.RepSPDocsInfo.intspID;
            cmd2.Parameters.Add("@pspUIVersionString", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspUIVersionString;
            cmd2.Parameters.Add("@pspContentVersion", SqlDbType.Int).Value = JSONFile.RepSPDocsInfo.intspContentVersion;
            cmd2.Parameters.Add("@pspIsCurrentVersion", SqlDbType.Bit).Value = JSONFile.RepSPDocsInfo.blnspIsCurrentVersion;
            cmd2.Parameters.Add("@pspCheckInComments", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspCheckInComments;
            cmd2.Parameters.Add("@pspAuthor", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspAuthor;
            cmd2.Parameters.Add("@pspContentType", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspContentType;
            cmd2.Parameters.Add("@pspFSObjType", SqlDbType.Int).Value = JSONFile.RepSPDocsInfo.intspFSObjType;
            cmd2.Parameters.Add("@pspFolderChildCount", SqlDbType.Int).Value = JSONFile.RepSPDocsInfo.intspFolderChildCount;
            cmd2.Parameters.Add("@pspItemChildCount", SqlDbType.Int).Value = JSONFile.RepSPDocsInfo.intspItemChildCount;
            cmd2.Parameters.Add("@pspBaseName", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspBaseName.Replace("'", "&apos;");
            cmd2.Parameters.Add("@pspLinkFilename", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspLinkFilename.Replace("'", "&apos;");
            cmd2.Parameters.Add("@pspCreated", SqlDbType.DateTime).Value = JSONFile.RepSPDocsInfo.dttspCreated;
            cmd2.Parameters.Add("@pspCreatedBy", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspCreatedBy;
            cmd2.Parameters.Add("@pspLastModified", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspLastModified;
            cmd2.Parameters.Add("@pspModified", SqlDbType.DateTime).Value = JSONFile.RepSPDocsInfo.dttspModified;
            cmd2.Parameters.Add("@pspModifiedBy", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspModifiedBy;
            cmd2.Parameters.Add("@pspEditor", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspEditor;
            cmd2.Parameters.Add("@pspFileSizeDisplay", SqlDbType.BigInt).Value = JSONFile.RepSPDocsInfo.intspFileSizeDisplay;
            cmd2.Parameters.Add("@pspFileType", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspFileType;
            cmd2.Parameters.Add("@pspServerUrl", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspServerUrl.Replace("'", "&apos;");
            cmd2.Parameters.Add("@pspUniqueId", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspUniqueId;
            cmd2.Parameters.Add("@pspParentUniqueId", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspParentUniqueId;
            cmd2.Parameters.Add("@pspGUID", SqlDbType.UniqueIdentifier).Value = string.IsNullOrEmpty(JSONFile.RepSPDocsInfo.strspGUID) ? new Guid() : new Guid(JSONFile.RepSPDocsInfo.strspGUID);
            cmd2.Parameters.Add("@pspVirtual_LocalRelativePath", SqlDbType.VarChar).Value = string.IsNullOrEmpty(JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath) ? null : JSONFile.RepSPDocsInfo.strspVirtual_LocalRelativePath.Replace("'", "&apos;");
            cmd2.Parameters.Add("@pnmDocumentLibrary", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strnmDocumentLibrary;
            cmd2.Connection = con;

            int RepDocs_IDInt = -1;

            try
            {
                con.Open();
                SqlDataReader dr = cmd2.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.IsDBNull(0))
                        RepDocs_IDInt = 0;
                    else
                        RepDocs_IDInt = Convert.ToInt32(dr[0].ToString());

                }
                Console.WriteLine("Record enquiried successfully");
            }
            catch (System.Exception ex)
            {
                //throw ex;
                return ex.Message;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }

            //Debug
            //System.IO.File.WriteAllText(EnquiryRegistryFolderPath("OutboundQueue") + "Debug.txt", "Before sprepDocDistributionAdd");

            con = new SqlConnection(strConnString);
            cmd4.Parameters.Add("@pRepRef", SqlDbType.VarChar).Value = JSONHeader.RepRef;
            cmd4.Parameters.Add("@pIMO", SqlDbType.Int).Value = JSONHeader.RepIMO;
            cmd4.Parameters.Add("@pDocUniqueId", SqlDbType.VarChar).Value = JSONFile.RepSPDocsInfo.strspUniqueId;
            cmd4.Parameters.Add("@pRepDocs_ID", SqlDbType.Int).Value = RepDocs_IDInt;
            if (JSONFile.UpdateType != null)
                cmd4.Parameters.Add("@pUpdateType", SqlDbType.VarChar).Value = JSONFile.UpdateType;
            cmd4.Parameters.Add("@pStatus", SqlDbType.VarChar).Value = JSONFile.RepDistributionStatus;
            cmd4.Connection = con;

            try
            {
                con.Open();
                Console.WriteLine("Record inserted successfully");
                SqlDataReader dr = cmd4.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.IsDBNull(0))
                        JSONFile.RepID = 0;
                    else
                    {
                        JSONFile.RepID = Convert.ToInt32(dr[0].ToString());
                        if (dr.IsDBNull(1)) { }
                        else
                        {
                            JSONFile.UpdateType = dr[1].ToString();
                        }
                    }

                }
                Console.WriteLine("Record enquiried successfully");

            }
            catch (System.Exception ex)
            {
                //ex.Data.Clear();
                //return "Cleared Exception Info";
                return ex.Message;
                //return "Excpetion";
            }
            finally
            {
                con.Close();
                con.Dispose();
            }

            return string.Empty;
        }
        */

        public void AddSPFolderInfostoJSON(string folPath, string DestinationUrl, class_ReplicationJSONHeader JHeader, class_ReplicationFileFolderJSONFile JFile)
        {
            class_ReplicationFileFolderJSONFull SPFolderInfosJSONList = new class_ReplicationFileFolderJSONFull();
            SPFolderInfosJSONList.entries = new List<class_ReplicationFileFolderJSONFile>();
            class_ReplicationFileFolderJSONFile SPFolderInfosJSON = new class_ReplicationFileFolderJSONFile();
            string JSONFileNameStr = "fatt.nsf";
            JavaScriptSerializer js = new JavaScriptSerializer();
            class_replicationlist SPDocsInfo = new class_replicationlist();
            string FileNameStr = folPath.Split('/').Last();
            string CurrentFolderStr = string.IsNullOrEmpty(JFile.RepSPDocsInfo.strspVirtual_LocalRelativePath) ? "" : JFile.RepSPDocsInfo.strspVirtual_LocalRelativePath;
            string IDFileNameStr = DestinationUrl.Split('\\').Last();

            List<string> StrList = new List<string>();

            //if (JFile.RepSPDocsInfo.strspFileType == "Document")
            if (JFile.RepSPDocsInfo.strspContentType == "Nautic Document")
                // Remove File Name from folPath
                folPath = folPath.Substring(0, folPath.Length - FileNameStr.Length - 1);
            //CurrentFolderStr = folPath.Replace("http://oneview.angloeastern.com/sites/oneview/nsdl/" + JFile.RepSPDocsInfo.strnmDocumentLibrary.Replace("(","").Replace(")",""), "");

            if (!System.IO.Directory.Exists(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length - 1)))
            {
                System.IO.Directory.CreateDirectory(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length - 1));
            }

            try
            {
                // Check if file already exists. If yes, delete it. 
                if (System.IO.File.Exists(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr))
                {
                    System.IO.StreamReader myFile = new System.IO.StreamReader(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr);
                    string myString = string.Empty;

                    while ((myString = myFile.ReadLine()) != null)
                    {
                        class_ReplicationFileFolderJSONFull ReplicationJSONFileFromFile = new class_ReplicationFileFolderJSONFull();
                        ReplicationJSONFileFromFile = js.Deserialize<class_ReplicationFileFolderJSONFull>(myString);
                        bool AllLocalRelativePathChecked = false;
                        bool UpdatedSPFolderInfos = false;

                        do
                        {
                            if (string.IsNullOrEmpty(CurrentFolderStr))
                            {
                                SPDocsInfo = AddSPDocumentLibraryInfoToJSON("/sites/nsdl/" + JFile.RepSPDocsInfo.strnmDocumentLibrary).RepSPDocsInfo;
                            }
                            else
                            {
                                //folPath = "http://oneview.angloeastern.com/sites/oneview/nsdl/" + JFile.RepSPDocsInfo.strnmDocumentLibrary.Replace("(", "").Replace(")", "") + CurrentFolderStr;
                                folPath = "http://dms.angloeastern.com/sites/nsdl/" + JFile.RepSPDocsInfo.strnmDocumentLibrary.Replace("(", "").Replace(")", "") + CurrentFolderStr;

                                //SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/oneview"), folPath.Length - folPath.IndexOf("/sites/oneview")), "Folder", "Existing");
                                SPDocsInfo = GetSPDocInfos(folPath.Substring(folPath.IndexOf("/sites/nsdl"), folPath.Length - folPath.IndexOf("/sites/nsdl")), "Folder", "Existing");
                            }

                            foreach (class_ReplicationFileFolderJSONFile JSONFiles in ReplicationJSONFileFromFile.entries)
                            {
                                if ((JSONFiles.RepSPDocsInfo.strspUniqueId == SPDocsInfo.strspUniqueId) && (JSONFiles.RepSPDocsInfo.strspContentType == "Nautic Folder"))
                                {
                                    if (JSONFiles.RepSPDocsInfo.strspBaseName == JSONFiles.RepSPDocsInfo.strnmDocumentLibrary.Replace("-", ""))
                                    {
                                        JSONFiles.RepSPDocsInfo = AddSPDocumentLibraryInfoToJSON(JFile.RepSPDocsInfo.strnmDocumentLibrary).RepSPDocsInfo;
                                    }
                                    else
                                    {
                                        JSONFiles.RepSPDocsInfo = GetUpdatedSPDocInfos(SPDocsInfo.strspServerUrl, SPDocsInfo.strspUniqueId, "Folder", "Existing");
                                    }
                                    //JSONFiles.UpdateType = "Existing";
                                    SPFolderInfosJSONList.entries.Add(JSONFiles);
                                    UpdatedSPFolderInfos = true;
                                    break;
                                }

                            }

                            if (!UpdatedSPFolderInfos)
                            {
                                SPFolderInfosJSON.UpdateType = "Existing";
                                SPFolderInfosJSON.RepSPDocsInfo = SPDocsInfo;
                                SPFolderInfosJSON.RepDistributionStatus = "CR";
                                SPFolderInfosJSONList.entries.Add(SPFolderInfosJSON);
                                StrList.Add(SPDocsInfo.strspUniqueId);

                            }
                            else
                                UpdatedSPFolderInfos = false;


                            if (CurrentFolderStr.IndexOf('/') >= 0)
                            {
                                CurrentFolderStr = CurrentFolderStr.Substring(0, CurrentFolderStr.Length - CurrentFolderStr.Split('/').Last().Length - 1);
                                SPFolderInfosJSON = new class_ReplicationFileFolderJSONFile();
                                if ((CurrentFolderStr.IndexOf('/') >= 0) && (CurrentFolderStr.Length > 1))
                                {
                                    //ReplicationJSONFileFromFile = js.Deserialize<class_ReplicationFileFolderJSONFull>(myString);
                                    //SPFolderInfosJSON = new class_ReplicationFileFolderJSONFile();
                                }
                                else
                                {
                                    //AllLocalRelativePathChecked = true;
                                    CurrentFolderStr = string.Empty;
                                }
                            }
                            else
                            {
                                AllLocalRelativePathChecked = true;
                            }

                        } while (AllLocalRelativePathChecked == false);

                        ReplicationJSONFileFromFile = js.Deserialize<class_ReplicationFileFolderJSONFull>(myString);

                        foreach (class_ReplicationFileFolderJSONFile JSONFiles in ReplicationJSONFileFromFile.entries)
                        {
                            if (JSONFiles.RepSPDocsInfo.strspContentType == "Nautic Document")
                            {
                                SPFolderInfosJSONList.entries.Add(JSONFiles);
                            }
                            else
                            {
                                bool ExistedJSONFolder = false;
                                foreach (class_ReplicationFileFolderJSONFile SPFolderJSONFiles in SPFolderInfosJSONList.entries)
                                {
                                    if (SPFolderJSONFiles.RepSPDocsInfo.strspUniqueId == JSONFiles.RepSPDocsInfo.strspUniqueId)
                                    {
                                        ExistedJSONFolder = true;
                                        break;

                                    }

                                }

                                if (!ExistedJSONFolder)
                                {
                                    if (JSONFiles.RepSPDocsInfo.intspFSObjType == 1)
                                        JSONFiles.RepID = 0;

                                    SPFolderInfosJSONList.entries.Add(JSONFiles);

                                }

                            }

                        }

                    }

                    myFile.Close();
                    myFile.Dispose();

                    try
                    {
                        if (System.IO.File.Exists(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr))
                            System.IO.File.Delete(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr);

                    }
                    catch (System.IO.IOException)
                    {
                        if (System.IO.File.Exists(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr))
                        {
                            //Load your file using FileStream class and release an file through stream.Dispose()
                            using (FileStream stream = new FileStream(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                            {
                                stream.Close();
                                stream.Dispose();
                            }

                            // delete the file.
                            System.IO.File.Delete(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr);

                        }

                    }
                }

            } // end of try
            catch (System.Exception Ex)
            {
                //Console.WriteLine(Ex.ToString());
            }

            SPFolderInfosJSONList.RepHeader = JHeader;
            string JSONString = js.Serialize(SPFolderInfosJSONList);

            try
            {
                if (System.IO.File.Exists(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr))
                    System.IO.File.Delete(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr);

                using (StreamWriter sw = System.IO.File.CreateText(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr))
                {
                    sw.WriteLine(JSONString);
                    sw.Close();
                    sw.Dispose();
                }

            } catch (System.IO.IOException Ex)
            {
                if (System.IO.File.Exists(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr))
                {
                    //Load your file using FileStream class and release an file through stream.Dispose()
                    using (FileStream stream = new FileStream(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr, FileMode.Open, FileAccess.Read))
                    {
                        stream.Close();
                        stream.Dispose();
                    }

                    // delete the file.
                    System.IO.File.Delete(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr);

                }

                using (StreamWriter sw = System.IO.File.CreateText(DestinationUrl.Substring(0, DestinationUrl.Length - IDFileNameStr.Length) + JSONFileNameStr))
                {
                    sw.WriteLine(JSONString);
                    sw.Close();
                    sw.Dispose();
                }

            }

        }

        public void NauticMoveDirectory(string source_dir, string target_dir, bool OverWritten = false)
        {
            if (System.IO.Directory.Exists(source_dir))
            {
                if (System.IO.Directory.Exists(target_dir))
                {
                    string TempStr = target_dir + "_" + DateTime.Now.ToString("yyyyMMdd-HHMMss");

                    if (OverWritten)
                        NauticOverWriteDirectory(source_dir, target_dir);
                    else
                    {
                        System.IO.Directory.Move(target_dir, TempStr);
                        System.IO.Directory.Move(source_dir, target_dir);
                        System.IO.Directory.Delete(TempStr, true);

                    }

                }
                else
                {
                    System.IO.Directory.CreateDirectory(target_dir);
                    //Directory.Move(source_dir, target_dir);

                    foreach (string Files in System.IO.Directory.GetFiles(source_dir))
                    {
                        System.IO.File.Copy(Files, target_dir + @"\" + Files.Split('\\').Last());
                        System.IO.File.Delete(Files);
                    }

                    System.IO.Directory.Delete(source_dir);

                }
            }

        }

        public void NauticOverWriteDirectory(string source_dir, string target_dir)
        {
            /*
            foreach (string SubFolders in System.IO.Directory.GetDirectories(source_dir))
            {
                if (System.IO.Directory.Exists(target_dir + "\\" + SubFolders.Split('\\').Last()))
                    NauticOverWriteDirectory(SubFolders, target_dir + "\\" + SubFolders.Split('\\').Last());
                else
                    System.IO.Directory.Move(SubFolders, target_dir + "\\" + SubFolders.Split('\\').Last());

            }

            foreach (string Files in System.IO.Directory.GetFiles(source_dir))
            {
                if (System.IO.File.Exists(target_dir + "\\" + Files.Split('\\').Last()))
                    System.IO.File.Delete(target_dir + "\\" + Files.Split('\\').Last());

                System.IO.File.Move(Files, target_dir + "\\" + Files.Split('\\').Last());

            }
            */

            DeleteDirectory(target_dir);
            NauticMoveDirectory(source_dir, target_dir);

        }

        public class_replicationlist GetSPDLInfos(string folPath)
        {
            class_replicationlist item = new class_replicationlist();

            using (ClientContext clientContext = new ClientContext(@"http://dms.angloeastern.com/sites/nsdl"))
            {
                //clientContext.Credentials = new System.Net.NetworkCredential(Session["uID"].ToString(), Session["uPWD"].ToString(), Session["uDomain"].ToString());
                clientContext.Credentials = new System.Net.NetworkCredential("sharepoint.admin", "7N@iledit", "anglo");

                Microsoft.SharePoint.Client.Folder doclib = clientContext.Web.GetFolderByServerRelativeUrl(folPath.Replace("(", "").Replace(")", "").Replace("-", ""));
                clientContext.Load(doclib);
                clientContext.ExecuteQuery();

                string TempStr = doclib.ServerRelativeUrl;

                Microsoft.SharePoint.Client.ListItem DLListItems = doclib.ListItemAllFields;

                item.blnspIsCurrentVersion = true;
                item.strspContentType = "Nautic Folder";
                item.intspFSObjType = 1;
                item.strspBaseName = doclib.Name;
                item.strspLinkFilename = item.strspBaseName;
                item.dttspCreated = doclib.TimeCreated;
                item.strspLastModified = doclib.TimeLastModified.ToString("yyyy-MM-dd hh:mm:ss");
                item.dttspModified = doclib.TimeLastModified;
                item.strspServerUrl = doclib.ServerRelativeUrl;
                item.strspUniqueId = doclib.UniqueId.ToString().ToUpper();
                item.strspParentUniqueId = "0";
                item.strnmDocumentLibrary = doclib.Name;
                item.intspFileSizeDisplay = -1;
                item.strspFileType = "";

            }

            return item;

        }

        public string GetAttribute(string ldappath, string sAMAccountName, string attribute)
        {
            string OUT = string.Empty;

            try
            {
                DirectoryEntry de = new DirectoryEntry(ldappath);
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.Filter = "(&(objectClass=user)(objectCategory=person)(sAMAccountName=" + sAMAccountName.Replace(@"\\", @"\").Split('\\').Last() + "))";

                SearchResultCollection results = ds.FindAll();

                foreach (SearchResult result in results)
                {
                    OUT = GetProperty(result, attribute);

                }
            }
            catch (Exception t)
            {
                System.Diagnostics.Debug.WriteLine(t.Message);
                
            }

            return (OUT != null) ? OUT : string.Empty;
        }

        public static string GetProperty(SearchResult searchResult, string PropertyName)
        {
            if (searchResult.Properties.Contains(PropertyName))
            {
                return searchResult.Properties[PropertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        // ACK file from Ship to Office

        private void ReceiveACK(int IMOInt)
        {
            // Start
            string FromShipReplicationIMOPath = EnquiryRegistryFolderPath("InboundQueue") + IMOInt.ToString();
            string ACKFileNameStr = string.Empty;
            
            if (!System.IO.Directory.Exists(FromShipReplicationIMOPath))
                return;

            //////////
            string[] NSINACKFiles = Directory.GetFiles(FromShipReplicationIMOPath, "*.ack", SearchOption.TopDirectoryOnly);

            if (!useTPL)
            {
                Console.WriteLine("Processing " + NSINACKFiles.Length + " files for IMO: " + IMOInt + "...");

                //foreach (string NSINACKFile in Directory.GetFiles(FromShipReplicationIMOPath, "*.ack", SearchOption.TopDirectoryOnly))
                foreach (string NSINACKFile in NSINACKFiles)
                {
                    ACKFileNameStr = NSINACKFile.Split('\\').Last();

                    try
                    {
                        dynamic r_head = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(NSINACKFile));
                        foreach (var ackId in r_head)
                        {
                            class_ACKId ackRepID = new class_ACKId();
                            ackRepID.Id = int.Parse(ackId.repID.ToString());

                            AcknowledgeReplication(ackRepID.Id, IMOInt.ToString());

                        }

                        if (!System.IO.Directory.Exists(FromShipReplicationIMOPath + @"\" + "_ProcessedFiles"))
                            System.IO.Directory.CreateDirectory(FromShipReplicationIMOPath + @"\" + "_ProcessedFiles");

                        System.IO.File.Move(NSINACKFile, FromShipReplicationIMOPath + @"\" + @"_ProcessedFiles\" + ACKFileNameStr.Substring(0, ACKFileNameStr.LastIndexOf('.')) + "_backup" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".ack");

                    }
                    catch (System.Exception ex)
                    {
                        //throw ex
                    }

                }

                Console.WriteLine("Finished processing " + NSINACKFiles.Length + " files for IMO: " + IMOInt + "...");
            }

            else
            {
                // Implement TPL on ReceiveACK function
                Task[] ReceiveACKFilesTasks = new Task[NSINACKFiles.Length];

                Console.WriteLine("Processing " + NSINACKFiles.Length + " files for IMO: " + IMOInt + "...");

                int exCtr = 0;

                for(int ctr = 0; ctr < NSINACKFiles.Length; ctr++)
                {
                    ReceiveACKFilesTasks[ctr] = Task.Factory.StartNew(
                        (Object obj) =>
                        {
                            FileHandlerObj fhObj = obj as FileHandlerObj;

                            try
                            {
                                dynamic r_head = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(NSINACKFiles[fhObj.index]));
                                foreach (var ackId in r_head)
                                {
                                    class_ACKId ackRepID = new class_ACKId();
                                    ackRepID.Id = int.Parse(ackId.repID.ToString());

                                    AcknowledgeReplication(ackRepID.Id, IMOInt.ToString());

                                }

                                if (!System.IO.Directory.Exists(FromShipReplicationIMOPath + @"\" + "_ProcessedFiles"))
                                    System.IO.Directory.CreateDirectory(FromShipReplicationIMOPath + @"\" + "_ProcessedFiles");

                                System.IO.File.Move(NSINACKFiles[fhObj.index], FromShipReplicationIMOPath + @"\" + @"_ProcessedFiles\" + Path.GetFileName(NSINACKFiles[fhObj.index]).Substring(0, Path.GetFileName(NSINACKFiles[fhObj.index]).LastIndexOf('.')) + "_backup" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".ack");
                                
                            }
                            catch (System.Exception ex)
                            {
                                exCtr++;
                                Console.WriteLine(ex.Message);
                                //throw ex
                            }
                        },
                        new FileHandlerObj() { index = ctr });      

                }

                Task.WaitAll(ReceiveACKFilesTasks);

            }



            if (!string.IsNullOrEmpty(ACKFileNameStr)) {

                //===== log
                inLog += DateTime.Now.ToString() + " [ACK File processed: " + ACKFileNameStr + "] \r\n";
                //===== log
                // End

            }

        }
        
        private void AcknowledgeReplication(int repIDInt, string IMOInt)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                //conn.ConnectionString = "Data Source=HKGSRVSPTSQL\\SQL2016;User id=sa;Password=@Oneview1;";

                conn.ConnectionString = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlCommand cmd = new SqlCommand("spAcknowledgeDocDistribution", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pAcknowledgementId", repIDInt);
                    cmd.Parameters.AddWithValue("@IMO", IMOInt);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
           
            //===== log
            inLog += DateTime.Now.ToString() + " [Replication Acknowledged in Ship] \r\n";
            //===== log

        }

        // Rest Hours (RHM) Replication from Ship to Office.cs

        /*
    private void btnRecRHS_Click(object sender, EventArgs e)
    {
        ReceivedRHS();

    }
    */

        private void ReceiveRHS(int IMOInt)
        {
            /*
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Nautic Systems_stg\nsReplication", true);
            Object o_iq = key.GetValue("InboundQueue");
            Object o_oq = key.GetValue("OutboundQueue");
            string shipIMO = "9732589";
            */
            Object o_iq = EnquiryRegistryFolderPath("InboundQueue");
            Object o_oq = EnquiryRegistryFolderPath("OutboundQueue");

            string shipIMO = IMOInt.ToString();

            string[] RHSReplicationFromShipFiles = Directory.GetFiles(o_iq + shipIMO + "\\", "*.rhs");

            List<int> repid_processed = new List<int>();

            if (!useTPL)
            {
                foreach (string rhsfilepath in RHSReplicationFromShipFiles)
                {
                    int repId = ProcessRHSShipToOffice(rhsfilepath, shipIMO);

                    repid_processed.Add(repId);

                    string rhsfileName = string.Empty;
                    rhsfileName = new DirectoryInfo(rhsfilepath).Name;
                    string process_rhsfileName = rhsfileName.Split('.').First() + "_" + DateTime.Now.ToString("hhmmss");

                if (!System.IO.File.Exists(o_iq + shipIMO + @"\_ProcessedFiles\" + process_rhsfileName + ".rhs"))
                {
                    if (!System.IO.Directory.Exists(o_iq + shipIMO + @"\_ProcessedFiles"))
                        System.IO.Directory.CreateDirectory(o_iq + shipIMO + @"\_ProcessedFiles");

                    System.IO.File.Move(rhsfilepath, o_iq + shipIMO + @"\_ProcessedFiles\" + process_rhsfileName + ".rhs");
                }
                else { System.IO.File.Delete(rhsfilepath); }

                }
            }

            else
            {
                Task[] ReceiveRHSFilesTasks = new Task[RHSReplicationFromShipFiles.Length];

                for (int ctr = 0; ctr < RHSReplicationFromShipFiles.Length; ctr++)
                {
                    ReceiveRHSFilesTasks[ctr] = Task.Factory.StartNew(
                        (Object obj) =>
                        {
                            FileHandlerObj fhObj = obj as FileHandlerObj;

                            string rhsfilepath = RHSReplicationFromShipFiles[fhObj.index];
                            int repId = ProcessRHSShipToOffice(rhsfilepath, shipIMO);

                            repid_processed.Add(repId);

                            string rhsfileName = string.Empty;
                            rhsfileName = new DirectoryInfo(rhsfilepath).Name;
                            string process_rhsfileName = rhsfileName.Split('.').First() + "_" + DateTime.Now.ToString("hhmmss");
                            
                            if (!System.IO.File.Exists(o_iq + shipIMO + @"\_ProcessedFiles\" + process_rhsfileName + ".rhs"))
                            {
                                if (!System.IO.Directory.Exists(o_iq + shipIMO + @"\_ProcessedFiles"))
                                    System.IO.Directory.CreateDirectory(o_iq + shipIMO + @"\_ProcessedFiles");

                                System.IO.File.Move(rhsfilepath, o_iq + shipIMO + @"\_ProcessedFiles\" + process_rhsfileName + ".rhs");
                            }
                            else { System.IO.File.Delete(rhsfilepath); }
                        },
                        new FileHandlerObj() { index = ctr });
                }

                Task.WaitAll(ReceiveRHSFilesTasks);
            }
            

            if (repid_processed.Count > 0)
            {
                // Acknowledge Rest Hours from Ships

                string json = JsonConvert.SerializeObject(repid_processed);
                string rhsacknowledgement = "RHO" + shipIMO + DateTime.Now.ToString("yyMMddhhmmss");
                System.IO.File.WriteAllText(o_oq + shipIMO + "\\" + rhsacknowledgement + ".rho", json);

                System.IO.File.WriteAllText(o_oq + rhsacknowledgement + ".trg", rhsacknowledgement + ".rho");

                //===== log
                inLog += DateTime.Now.ToString() + " [Rest Hours Acknowledgement File Created: " + rhsacknowledgement + ".rho] \r\n";
                //===== log
                //MessageBox.Show("Rest Hourse Acknowledgement File Created", "JSON Rest Hours Acknowledgement", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }
        
        private int ProcessRHSShipToOffice(string rhs_path, string imo)
        {

            dynamic r_head = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(rhs_path));

            List<ReplicationRestHeader> RestHeaders = new List<ReplicationRestHeader>();

            ReplicationRestHeader rh_header = new ReplicationRestHeader();

            rh_header.Id = r_head.RepRestHeader.Id;
            rh_header.RepRef = r_head.RepRestHeader.RepRef;
            rh_header.Created = r_head.RepRestHeader.Created;
            rh_header.Status = r_head.RepRestHeader.Status;
            rh_header.Updated = r_head.RepRestHeader.Updated;
            rh_header.Imo = r_head.RepRestHeader.Imo;

            RestHeaders.Add(rh_header);

            List<ReplicationRestCrewMember> CrewMembers = new List<ReplicationRestCrewMember>();

            foreach (var file in r_head.entries)
            {
                ReplicationRestCrewMember rh_crew = new ReplicationRestCrewMember();
                rh_crew.ForeignID = file.ForeignID;
                rh_crew.FirstName = file.FirstName;
                rh_crew.MiddleNames = file.MiddleNames;
                rh_crew.LastName = file.LastName;
                rh_crew.Rank = file.Rank;
                rh_crew.SignOn = file.SignOn;
                rh_crew.PlannedSignOff = file.PlannedSignOff;
                rh_crew.ActualSignOff = file.ActualSignOff;
                rh_crew.Citizenship = file.Citizenship;
                rh_crew.Active = file.Active;
                rh_crew.Passport = file.Passport;
                rh_crew.PassportExpiry = file.PassportExpiry;
                //rh_crew.Watches = file.Watches;
                //CrewMembers.Add(rh_crew);

                List<ReplicationRestCrewWatch> CrewMembersWatches = new List<ReplicationRestCrewWatch>();

                foreach (var crewwatch in file.Watches)
                {
                    ReplicationRestCrewWatch rh_crew_watch = new ReplicationRestCrewWatch();
                    rh_crew_watch.ForeignID = crewwatch.ForeignID;
                    rh_crew_watch.Created = crewwatch.Created;
                    rh_crew_watch.ReplacedBy = crewwatch.ReplacedBy;
                    rh_crew_watch.Deleted = crewwatch.Deleted;
                    rh_crew_watch.StartDate = crewwatch.StartDate;
                    rh_crew_watch.EndDate = crewwatch.EndDate;
                    rh_crew_watch.Type = crewwatch.Type;

                    CrewMembersWatches.Add(rh_crew_watch);
                }


                List<ReplicationRestMasterComment> MasterComments = new List<ReplicationRestMasterComment>();

                foreach (var mtrcomm in file.MasterComments)
                {
                    ReplicationRestMasterComment rh_master_comments = new ReplicationRestMasterComment();
                    //rh_master_comments.AtPort = mtrcomm.AtPort;
                    rh_master_comments.Day = mtrcomm.Day;
                    rh_master_comments.Text = mtrcomm.Text;
                    rh_master_comments.Created = mtrcomm.Created;
                    rh_master_comments.Updated = mtrcomm.Updated;
                    rh_master_comments.Deleted = mtrcomm.Deleted;

                    MasterComments.Add(rh_master_comments);
                }


                List<ReplicationRestNonConformance> NonConformances = new List<ReplicationRestNonConformance>();

                foreach (var nonconf in file.NonConformances)
                {
                    ReplicationRestNonConformance rh_non_comformance = new ReplicationRestNonConformance();
                    rh_non_comformance.StartDate = nonconf.StartDate;
                    rh_non_comformance.EndDate = nonconf.EndDate;
                    rh_non_comformance.Type = nonconf.Type;
                    rh_non_comformance.Created = nonconf.Created;
                    rh_non_comformance.Updated = nonconf.Updated;
                    rh_non_comformance.Deleted = nonconf.Deleted;

                    NonConformances.Add(rh_non_comformance);
                }

                List<ReplicationRestCrewDuty> DutyLogs = new List<ReplicationRestCrewDuty>();

                foreach (var dutylog in file.DutyLog)
                {
                    ReplicationRestCrewDuty rh_dutylog = new ReplicationRestCrewDuty();
                    rh_dutylog.SignedOn = dutylog.SignedOn;
                    rh_dutylog.SignedOff = dutylog.SignedOff;
                    rh_dutylog.Created = dutylog.Created;
                    rh_dutylog.Updated = dutylog.Updated;

                    DutyLogs.Add(rh_dutylog);
                }

                List<ReplicationRestCrewComment> CrewComments = new List<ReplicationRestCrewComment>();

                foreach (var crewcomments in file.CrewComments)
                {
                    ReplicationRestCrewComment rh_crew_comments = new ReplicationRestCrewComment();

                    rh_crew_comments.Day = crewcomments.Day;
                    rh_crew_comments.Text = crewcomments.Text;
                    rh_crew_comments.Created = crewcomments.Created;
                    rh_crew_comments.Updated = crewcomments.Updated;
                    rh_crew_comments.Deleted = crewcomments.Deleted;

                    CrewComments.Add(rh_crew_comments);
                }



                rh_crew.Watches = CrewMembersWatches;
                rh_crew.MasterComments = MasterComments;
                rh_crew.NonConformances = NonConformances;
                rh_crew.RestCrewDutyLog = DutyLogs;
                rh_crew.CrewComments = CrewComments;

                CrewMembers.Add(rh_crew);

            }

            List<ReplicationShipTimes> ShipTimes = new List<ReplicationShipTimes>();
            foreach (var stime in r_head.ShipTimes)
            {
                ReplicationShipTimes shiptime = new ReplicationShipTimes();
                shiptime.TookEffect = stime.TookEffect;
                shiptime.Offset = stime.Offset;
                ShipTimes.Add(shiptime);
            }


            // Process REST HOURS Header
            string rhsfilename = new DirectoryInfo(rhs_path).Name;

            for (int h = 0; h < RestHeaders.Count(); h++)
            {

                string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand("sprepRHMFilesRec", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RepId", RestHeaders[h].Id);
                        cmd.Parameters.AddWithValue("@RhmFileName", rhsfilename);
                        cmd.Parameters.AddWithValue("@IMO", imo);
                        cmd.Parameters.AddWithValue("@RepRef", RestHeaders[h].RepRef);
                        cmd.Parameters.AddWithValue("@Status", RestHeaders[h].Status);
                        cmd.Parameters.AddWithValue("@Created", DateTime.Parse(RestHeaders[h].Created));
                        conn.Open();

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }

            int rh_vesselid = GetVesselID(rh_header.Imo);
            // Process Ships Time

            for (int st = 0; st < ShipTimes.Count(); st++)
            {

                string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand("sprepRHMShipsTimeAdd", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ShipID", rh_vesselid);
                        cmd.Parameters.AddWithValue("@UTCOffset", ShipTimes[st].Offset);
                        cmd.Parameters.AddWithValue("@TookEffect", DateTime.Parse(ShipTimes[st].TookEffect));
                        conn.Open();

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }



            for (int c = 0; c < CrewMembers.Count(); c++)
            {
                int crewForeignId = CrewMembers[c].ForeignID;
                int rh_rankid = GetRankID(CrewMembers[c].Rank);


                UpdateCrewDetails(CrewMembers, c, rh_rankid, rh_vesselid);

                int crewId = GetForeignCrewId(CrewMembers[c].ForeignID);

                for (int w = 0; w < CrewMembers[c].Watches.Count(); w++)
                {
                    UpdateCrewWatchDetails(CrewMembers[c].Watches, w, rh_vesselid, crewId, crewForeignId);
                }

                for (int m = 0; m < CrewMembers[c].MasterComments.Count(); m++)
                {
                    UpdateMasterCommentsDetails(CrewMembers[c].MasterComments, m, rh_vesselid, crewId, crewForeignId);
                }

                for (int nc = 0; nc < CrewMembers[c].NonConformances.Count(); nc++)
                {
                    UpdateNonConformancesDetails(CrewMembers[c].NonConformances, nc, rh_vesselid, crewId, crewForeignId);
                }

                for (int dl = 0; dl < CrewMembers[c].RestCrewDutyLog.Count(); dl++)
                {
                    UpdateDutyLogDetails(CrewMembers[c].RestCrewDutyLog, dl, rh_vesselid, crewId, crewForeignId);
                }

                for (int cc = 0; cc < CrewMembers[c].CrewComments.Count(); cc++)
                {

                    UpdateCrewCommentsDetails(CrewMembers[c].CrewComments, cc, rh_vesselid, crewId, crewForeignId);
                }



                //success = true;

            }

            //Acknowledge Document Received
            int repid = rh_header.Id;
            UpdateAcknowledgement(repid, "RC", rh_header.Imo);

            return repid;
        }




        private void UpdateCrewDetails(List<ReplicationRestCrewMember> CrewMember_Function, int f, int rankId, int vslId)
        {
            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepRHMCrewAdd", conn))
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Vessel", vslId);
                    cmd.Parameters.AddWithValue("@ForeignID", CrewMember_Function[f].ForeignID);
                    cmd.Parameters.AddWithValue("@FirstName", CrewMember_Function[f].FirstName);
                    cmd.Parameters.AddWithValue("@MiddleNames", CrewMember_Function[f].MiddleNames);
                    cmd.Parameters.AddWithValue("@LastName", CrewMember_Function[f].LastName);
                    cmd.Parameters.AddWithValue("@Active", CrewMember_Function[f].Active);
                    cmd.Parameters.AddWithValue("@Citizenship", CrewMember_Function[f].Citizenship);

                    // SignOn
                    if (CrewMember_Function[f].SignOn != null)
                    {
                        cmd.Parameters.AddWithValue("@SignOn", DateTime.ParseExact(CrewMember_Function[f].SignOn, "yyyy-MM-dd", provider));
                    }
                    else { cmd.Parameters.AddWithValue("@SignOn", DBNull.Value); }


                    // PlannedSignOff
                    if (CrewMember_Function[f].PlannedSignOff != null)
                    {
                        cmd.Parameters.AddWithValue("@PlannedSignOff", DateTime.ParseExact(CrewMember_Function[f].PlannedSignOff, "yyyy-MM-dd", provider));
                    }
                    else { cmd.Parameters.AddWithValue("@PlannedSignOff", DBNull.Value); }


                    // Actual SignOff
                    if (CrewMember_Function[f].ActualSignOff != null)
                    {
                        cmd.Parameters.AddWithValue("@ActualSignOff", DateTime.ParseExact(CrewMember_Function[f].ActualSignOff, "yyyy-MM-dd", provider));
                    }
                    else { cmd.Parameters.AddWithValue("@ActualSignOff", DBNull.Value); }




                    cmd.Parameters.AddWithValue("@RankID", rankId);


                    cmd.Parameters.AddWithValue("@Passport", CrewMember_Function[f].Passport);


                    // Passport Expiry
                    if (CrewMember_Function[f].PassportExpiry == null)
                    {
                        cmd.Parameters.AddWithValue("@PassportExpiry", DBNull.Value);
                    }
                    else { cmd.Parameters.AddWithValue("@PassportExpiry", DateTime.ParseExact(CrewMember_Function[f].PassportExpiry, "yyyy-MM-dd", provider)); }

                    conn.Open();
                    //conn.Close();
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }





        private void UpdateCrewWatchDetails(List<ReplicationRestCrewWatch> CrewWatch_Function, int f, int vslId, int crewid, int crewforeignId)
        {
            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepRHMCrewWatchAdd", conn))
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Vessel", vslId);
                    cmd.Parameters.AddWithValue("@ForeignID", CrewWatch_Function[f].ForeignID);
                    cmd.Parameters.AddWithValue("@CrewID", crewid);
                    cmd.Parameters.AddWithValue("@ForeignCrewID", crewforeignId);

                    // Start Date cmd.Parameters.AddWithValue("@StartDate", CrewWatch_Function[f].StartDate);
                    if (CrewWatch_Function[f].StartDate != null)
                    {
                        cmd.Parameters.AddWithValue("@StartDate", DateTime.Parse(CrewWatch_Function[f].StartDate));
                    }
                    else { cmd.Parameters.AddWithValue("@StartDate", DBNull.Value); }


                    // End Date cmd.Parameters.AddWithValue("@EndDate", CrewWatch_Function[f].EndDate);
                    if (CrewWatch_Function[f].EndDate != null)
                    {
                        cmd.Parameters.AddWithValue("@EndDate", DateTime.Parse(CrewWatch_Function[f].EndDate));
                    }
                    else { cmd.Parameters.AddWithValue("@EndDate", DBNull.Value); }


                    // Replaced By -  cmd.Parameters.AddWithValue("@ReplacedBy", CrewWatch_Function[f].ReplacedBy);
                    if (CrewWatch_Function[f].ReplacedBy != null)
                    {
                        cmd.Parameters.AddWithValue("@ReplacedBy", CrewWatch_Function[f].ReplacedBy);
                    }
                    else { cmd.Parameters.AddWithValue("@ReplacedBy", DBNull.Value); }


                    // Deleted Date cmd.Parameters.AddWithValue("@Deleted", CrewWatch_Function[f].Deleted);
                    if (CrewWatch_Function[f].Deleted != null)
                    {
                        cmd.Parameters.AddWithValue("@Deleted", DateTime.Parse(CrewWatch_Function[f].Deleted));
                    }
                    else { cmd.Parameters.AddWithValue("@Deleted", DBNull.Value); }


                    // Created Date cmd.Parameters.AddWithValue("@Created", CrewWatch_Function[f].Created);
                    if (CrewWatch_Function[f].Created != null)
                    {
                        cmd.Parameters.AddWithValue("@Created", DateTime.Parse(CrewWatch_Function[f].Created));
                    }
                    else { cmd.Parameters.AddWithValue("@Created", DBNull.Value); }

                    // Type
                    if (CrewWatch_Function[f].Created != null)
                    {
                        cmd.Parameters.AddWithValue("@Type", CrewWatch_Function[f].Type);
                    }
                    else { cmd.Parameters.AddWithValue("@Type", DBNull.Value); }

                    conn.Open();
                    //conn.Close();
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }




        private void UpdateMasterCommentsDetails(List<ReplicationRestMasterComment> CrewMasterComments_Function, int f, int vslId, int crewid, int crewforeignId)
        {
            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepRHMMasterCommAdd", conn))
                {

                    CultureInfo provider = CultureInfo.InvariantCulture;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ForeignID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CrewID", crewid);
                    cmd.Parameters.AddWithValue("@ForeignCrewID", crewforeignId);
                    cmd.Parameters.AddWithValue("@Vessel", vslId);


                    // Day Date - cmd.Parameters.AddWithValue("@Day", CrewMasterComments_Function[f].Day);
                    if (CrewMasterComments_Function[f].Day != null)
                    {
                        cmd.Parameters.AddWithValue("@Day", DateTime.ParseExact(CrewMasterComments_Function[f].Day, "yyyy-MM-dd", provider));
                    }
                    else { cmd.Parameters.AddWithValue("@Day", DBNull.Value); }



                    // Created Date - cmd.Parameters.AddWithValue("@Created", CrewMasterComments_Function[f].Created);
                    if (CrewMasterComments_Function[f].Created != null)
                    {
                        cmd.Parameters.AddWithValue("@Created", DateTime.Parse(CrewMasterComments_Function[f].Created));
                    }
                    else { cmd.Parameters.AddWithValue("@Created", DBNull.Value); }


                    // Updated Date - cmd.Parameters.AddWithValue("@Updated", CrewMasterComments_Function[f].Updated);
                    if (CrewMasterComments_Function[f].Updated != null)
                    {
                        cmd.Parameters.AddWithValue("@Updated", DateTime.Parse(CrewMasterComments_Function[f].Updated));
                    }
                    else { cmd.Parameters.AddWithValue("@Updated", DBNull.Value); }


                    // Deleted Date - cmd.Parameters.AddWithValue("@Deleted", CrewMasterComments_Function[f].Deleted);
                    if (CrewMasterComments_Function[f].Deleted != null)
                    {
                        cmd.Parameters.AddWithValue("@Deleted", DateTime.Parse(CrewMasterComments_Function[f].Deleted));
                    }
                    else { cmd.Parameters.AddWithValue("@Deleted", DBNull.Value); }


                    // Comment - cmd.Parameters.AddWithValue("@Comment", CrewMasterComments_Function[f].Text);
                    if (CrewMasterComments_Function[f].Text != null)
                    {
                        cmd.Parameters.AddWithValue("@Comment", CrewMasterComments_Function[f].Text);
                    }
                    else { cmd.Parameters.AddWithValue("@Comment", DBNull.Value); }

                    // At Port - cmd.Parameters.AddWithValue("@AtPort", CrewMasterComments_Function[f].AtPort);
                    //if (CrewMasterComments_Function[f].AtPort != null)
                    //{
                    //    cmd.Parameters.AddWithValue("@AtPort", CrewMasterComments_Function[f].AtPort);
                    //}
                    //else { cmd.Parameters.AddWithValue("@AtPort", DBNull.Value); }

                    conn.Open();
                    //conn.Close();
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }




        private void UpdateNonConformancesDetails(List<ReplicationRestNonConformance> CrewNonConformances_Function, int f, int vslId, int crewid, int foreigncrewId)
        {
            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepRHMNonConformancesAdd", conn))
                {

                    CultureInfo provider = CultureInfo.InvariantCulture;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Vessel", vslId);
                    cmd.Parameters.AddWithValue("@ForeignID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CrewID", crewid);
                    cmd.Parameters.AddWithValue("@ForeignCrewID", foreigncrewId);

                    // Start Date
                    if (CrewNonConformances_Function[f].StartDate != null)
                    {
                        cmd.Parameters.AddWithValue("@StartDate", DateTime.Parse(CrewNonConformances_Function[f].StartDate));
                    }
                    else { cmd.Parameters.AddWithValue("@StartDate", DBNull.Value); }

                    // End Date
                    if (CrewNonConformances_Function[f].EndDate != null)
                    {
                        cmd.Parameters.AddWithValue("@EndDate", DateTime.Parse(CrewNonConformances_Function[f].EndDate));
                    }
                    else { cmd.Parameters.AddWithValue("@EndDate", DBNull.Value); }


                    // Text - cmd.Parameters.AddWithValue("@Text", CrewNonConformances_Function[f].Text);
                    if (CrewNonConformances_Function[f].Type != null)
                    {
                        cmd.Parameters.AddWithValue("@Type", CrewNonConformances_Function[f].Type);
                    }
                    else { cmd.Parameters.AddWithValue("@Type", DBNull.Value); }


                    // Created Date - cmd.Parameters.AddWithValue("@Created", CrewNonConformances_Function[f].Created);
                    if (CrewNonConformances_Function[f].Created != null)
                    {
                        cmd.Parameters.AddWithValue("@Created", DateTime.Parse(CrewNonConformances_Function[f].Created));
                    }
                    else { cmd.Parameters.AddWithValue("@Created", DBNull.Value); }



                    // Updated Date - cmd.Parameters.AddWithValue("@Updated", CrewNonConformances_Function[f].Updated);
                    if (CrewNonConformances_Function[f].Updated != null)
                    {
                        cmd.Parameters.AddWithValue("@Updated", DateTime.Parse(CrewNonConformances_Function[f].Updated));
                    }
                    else { cmd.Parameters.AddWithValue("@Updated", DBNull.Value); }


                    // Deleted Date - cmd.Parameters.AddWithValue("@Deleted", CrewNonConformances_Function[f].Deleted);
                    if (CrewNonConformances_Function[f].Deleted != null)
                    {
                        cmd.Parameters.AddWithValue("@Deleted", DateTime.Parse(CrewNonConformances_Function[f].Deleted));
                    }
                    else { cmd.Parameters.AddWithValue("@Deleted", DBNull.Value); }


                    conn.Open();
                    //conn.Close();
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        private void UpdateDutyLogDetails(List<ReplicationRestCrewDuty> CrewDutyLog_Function, int f, int vslId, int crewid, int foreigncrewId)
        {
            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepRHMDutyLogAdd", conn))
                {

                    CultureInfo provider = CultureInfo.InvariantCulture;


                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CrewID", crewid);
                    cmd.Parameters.AddWithValue("@ForeignCrewID", foreigncrewId);
                    cmd.Parameters.AddWithValue("@Vessel", vslId);

                    // Signed On
                    if (CrewDutyLog_Function[f].SignedOn != null)
                    {
                        cmd.Parameters.AddWithValue("@SignedOn", DateTime.Parse(CrewDutyLog_Function[f].SignedOn));
                    }
                    else { cmd.Parameters.AddWithValue("@SignedOn", DBNull.Value); }

                    // Signed Off
                    if (CrewDutyLog_Function[f].SignedOff != null)
                    {
                        cmd.Parameters.AddWithValue("@SignedOff", DateTime.Parse(CrewDutyLog_Function[f].SignedOff));
                    }
                    else { cmd.Parameters.AddWithValue("@SignedOff", DBNull.Value); }

                    // Created Date - cmd.Parameters.AddWithValue("@Created", CrewNonConformances_Function[f].Created);
                    if (CrewDutyLog_Function[f].Created != null)
                    {
                        cmd.Parameters.AddWithValue("@Created", DateTime.Parse(CrewDutyLog_Function[f].Created));
                    }
                    else { cmd.Parameters.AddWithValue("@Created", DBNull.Value); }


                    // Updated Date - cmd.Parameters.AddWithValue("@Updated", CrewNonConformances_Function[f].Updated);
                    if (CrewDutyLog_Function[f].Updated != null)
                    {
                        cmd.Parameters.AddWithValue("@Updated", DateTime.Parse(CrewDutyLog_Function[f].Updated));
                    }
                    else { cmd.Parameters.AddWithValue("@Updated", DBNull.Value); }


                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }



        private void UpdateCrewCommentsDetails(List<ReplicationRestCrewComment> CrewComments_Function, int f, int vslId, int crewid, int crewforeignId)
        {
            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepRHMMasterCommAdd", conn))
                {

                    CultureInfo provider = CultureInfo.InvariantCulture;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ForeignID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CrewID", crewid);
                    cmd.Parameters.AddWithValue("@ForeignCrewID", crewforeignId);
                    cmd.Parameters.AddWithValue("@Vessel", vslId);


                    // Day Date - cmd.Parameters.AddWithValue("@Day", CrewMasterComments_Function[f].Day);
                    if (CrewComments_Function[f].Day != null)
                    {
                        cmd.Parameters.AddWithValue("@Day", DateTime.ParseExact(CrewComments_Function[f].Day, "yyyy-MM-dd", provider));
                    }
                    else { cmd.Parameters.AddWithValue("@Day", DBNull.Value); }



                    // Created Date - cmd.Parameters.AddWithValue("@Created", CrewMasterComments_Function[f].Created);
                    if (CrewComments_Function[f].Created != null)
                    {
                        cmd.Parameters.AddWithValue("@Created", DateTime.Parse(CrewComments_Function[f].Created));
                    }
                    else { cmd.Parameters.AddWithValue("@Created", DBNull.Value); }


                    // Updated Date - cmd.Parameters.AddWithValue("@Updated", CrewMasterComments_Function[f].Updated);
                    if (CrewComments_Function[f].Updated != null)
                    {
                        cmd.Parameters.AddWithValue("@Updated", DateTime.Parse(CrewComments_Function[f].Updated));
                    }
                    else { cmd.Parameters.AddWithValue("@Updated", DBNull.Value); }


                    // Deleted Date - cmd.Parameters.AddWithValue("@Deleted", CrewMasterComments_Function[f].Deleted);
                    if (CrewComments_Function[f].Deleted != null)
                    {
                        cmd.Parameters.AddWithValue("@Deleted", DateTime.Parse(CrewComments_Function[f].Deleted));
                    }
                    else { cmd.Parameters.AddWithValue("@Deleted", DBNull.Value); }


                    // Comment - cmd.Parameters.AddWithValue("@Comment", CrewMasterComments_Function[f].Text);
                    if (CrewComments_Function[f].Text != null)
                    {
                        cmd.Parameters.AddWithValue("@Comment", CrewComments_Function[f].Text);
                    }
                    else { cmd.Parameters.AddWithValue("@Comment", DBNull.Value); }

                    conn.Open();
                    //conn.Close();
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }



        private void UpdateAcknowledgement(int uniquerec, string newrem, int imo)
        {
            string csv = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc = new SqlConnection(csv))
            {
                using (SqlCommand cmd = new SqlCommand("sprepUpdateAck", sc))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RepId", uniquerec);
                    cmd.Parameters.AddWithValue("@NewRemarks", newrem);
                    cmd.Parameters.AddWithValue("@IMO", imo);
                    sc.Open();
                    cmd.ExecuteReader();

                }
                sc.Close();
            }
        }


        private int GetVesselID(int rh_imo)
        {
            int vslID = 0;

            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepGetVesselID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IMO", rh_imo);
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        vslID = Convert.ToInt32(dr["ID"]);
                    }

                }

                conn.Close();

            }

            return vslID;

        }




        private int GetRankID(string rankcode)
        {
            int rankID = 0;

            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepGetRankID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RankCode", rankcode);
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rankID = Convert.ToInt32(dr["ID"]);
                    }

                }

                conn.Close();

            }

            return rankID;

        }
        
        private int GetForeignCrewId(int foreignId)
        {
            int forId = 0;

            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepGetCrewForeignID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ForeignID", foreignId);
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        forId = Convert.ToInt32(dr["ID"]);
                    }

                }

                conn.Close();

            }

            return forId;

        }

        // Risk Management (RMS) Replication from Ship to Office.cs

        //private void btnRecRMS_Click(object sender, EventArgs e)
        private void ReceiveRMS(int IMOInt)
        {
            //RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Nautic Systems\nsReplication", true); // Do not use the Registry, instead use the SP: [spGetRepNSRepConfig]
            //Object o_iq = key.GetValue("InboundQueue"); // Table: [rep_nsRepConfig] \ SP: [spGetRepNSRepConfig]
            //Object o_oq = key.GetValue("OutboundQueue");
            
            //string shipIMO = "9732589"; // Replace this with the variable while looping from the active ships (NM_Active = 1) - Table: [tbl_Ships] \ SP: [sp_AllActiveVessels] 

            // Do not use the Registry, instead use the SP: [spGetRepNSRepConfig]
            Object o_iq = EnquiryRegistryFolderPath("InboundQueue"); // Table: [rep_nsRepConfig] \ SP: [spGetRepNSRepConfig]
            Object o_oq = EnquiryRegistryFolderPath("OutboundQueue");

            string shipIMO = IMOInt.ToString(); // Replace this with the variable while looping from the active ships (NM_Active = 1) - Table: [tbl_Ships] \ SP: [sp_AllActiveVessels] 

            string[] RMSReplicationFromShipFiles = Directory.GetFiles(o_iq + shipIMO + "\\", "*.rmm");

            List<RMM_Acknowledgement> RMMAcknowledgementsProcessed = new List<RMM_Acknowledgement>();

            //if (!useTPL)
            //{
                foreach (string rmsfilepath in RMSReplicationFromShipFiles)
                {

                    List<RMM_Acknowledgement> RMMAcknowledgementFunction = new List<RMM_Acknowledgement>();

                    RMMAcknowledgementFunction = ProcessRMSShipToOffice(rmsfilepath);
                    if (RMMAcknowledgementFunction == null)
                    {
                        continue;
                    }


                    RMM_Acknowledgement repref_processed = new RMM_Acknowledgement();
                    for (int rm = 0; rm < RMMAcknowledgementFunction.Count(); rm++)
                    {
                        repref_processed.repref = RMMAcknowledgementFunction[rm].repref;
                        repref_processed.Task_Ids = RMMAcknowledgementFunction[rm].Task_Ids;
                        repref_processed.Hazard_Ids = RMMAcknowledgementFunction[rm].Hazard_Ids;
                        repref_processed.RMM_RA_Ids = RMMAcknowledgementFunction[rm].RMM_RA_Ids;
                        repref_processed.RMM_WP_Ids = RMMAcknowledgementFunction[rm].RMM_WP_Ids;
                    }

                    if (repref_processed.repref != null)
                    {
                        RMMAcknowledgementsProcessed.Add(repref_processed);
                    }

                }
                
            //}

            //else
            //{
                //Task[] ReceiveRMSFilesTasks = new Task[RMSReplicationFromShipFiles.Length];

                //for (int ctr = 0; ctr < RMSReplicationFromShipFiles.Length; ctr++)
                //{
                //    ReceiveRMSFilesTasks[ctr] = Task.Factory.StartNew(
                //        (Object obj) =>
                //        {
                //            FileHandlerObj fhObj = obj as FileHandlerObj;

                //            string rmsfilepath = RMSReplicationFromShipFiles[fhObj.index];

                //            List<RMM_Acknowledgement> RMMAcknowledgementFunction = new List<RMM_Acknowledgement>();
                //            RMMAcknowledgementFunction = ProcessRMSShipToOffice(rmsfilepath);

                //            RMM_Acknowledgement repref_processed = new RMM_Acknowledgement();
                //            for (int rm = 0; rm < RMMAcknowledgementFunction.Count(); rm++)
                //            {
                //                repref_processed.repref = RMMAcknowledgementFunction[rm].repref;
                //                repref_processed.strRMMAck_Type = RMMAcknowledgementFunction[rm].strRMMAck_Type;
                //                repref_processed.RMM_Ids = RMMAcknowledgementFunction[rm].RMM_Ids;
                //            }

                //            if (repref_processed.repref != null)
                //            {
                //                RMMAcknowledgementsProcessed.Add(repref_processed);
                //            }
                //        },
                //        new FileHandlerObj() { index = ctr });
                //}

                //Task.WaitAll(ReceiveRMSFilesTasks);

            //}

            if (RMMAcknowledgementsProcessed.Count > 0)
            {
                // Acknowlege Risk from Ships

                string json = JsonConvert.SerializeObject(RMMAcknowledgementsProcessed);
                string rmsacknowledgement = "RMO" + shipIMO + DateTime.Now.ToString("yyMMddhhmmss");
                System.IO.File.WriteAllText(o_oq + shipIMO + "\\" + rmsacknowledgement + ".rmo", json);

                System.IO.File.WriteAllText(o_oq + rmsacknowledgement + ".trg", rmsacknowledgement + ".rmo");
            }

            //Process Risk Management Acknowledgements from Ship (RMA)

            string[] RMAAcknowledgementFromShipFiles = Directory.GetFiles(o_iq + shipIMO + "\\", "*.rma");

            foreach (string rmafilepath in RMAAcknowledgementFromShipFiles)
            {
                ProcessRMA(rmafilepath);
            }

        }

        private List<RMM_Acknowledgement> ProcessRMSShipToOffice(string rmmfilepath)
        {

            string filename = rmmfilepath.Split('\\').Last();

            dynamic r_head = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(rmmfilepath));

            class_RepHeader rm_header = new class_RepHeader();

            List<class_rmsRepItem> RMSRepItems = new List<class_rmsRepItem>();
            List<class_operationlist> OperationsGroups = new List<class_operationlist>();
            List<class_tasklist> TaskLists = new List<class_tasklist>();
            List<class_hazardlist> HazardLists = new List<class_hazardlist>();
            List<class_checklistlist> CheckListProcs = new List<class_checklistlist>();
            List<class_workpermitlist> WorkPermits = new List<class_workpermitlist>();
            List<class_ralist> RALists = new List<class_ralist>();
            List<class_workplanlist> WorkPlans = new List<class_workplanlist>();


            rm_header.RepRef = r_head.RepHeader.RepRef;
            rm_header.RepModCode = r_head.RepHeader.RepModCode;
            rm_header.RepFolSrc = r_head.RepHeader.RepFolSrc;
            rm_header.RepSMTPSrc = r_head.RepHeader.RepSMTPSrc;
            rm_header.RepFullDate = r_head.RepHeader.RepFullDate;
            rm_header.RepHeaderStatus = r_head.RepHeader.RepHeaderStatus;
            rm_header.RepYYMM = r_head.RepHeader.RepYYMM;
            rm_header.RepSr = r_head.RepHeader.RepSr;
            rm_header.RepIMO = r_head.RepHeader.RepIMO;

            //type=0 (Operations Group)


            foreach (var entry in r_head.entries)
            {

                class_rmsRepItem RepItem = new class_rmsRepItem();
                RepItem.type = entry.type;
                RepItem.strType = entry.strType;
                RMSRepItems.Add(RepItem);

                switch (RepItem.strType)
                {

                    case "Task":
                        class_tasklist TaskList = new class_tasklist();
                        TaskList.iID = entry.RepItemDetails.iID;
                        TaskList.strName = entry.RepItemDetails.strName;
                        TaskList.strCode = entry.RepItemDetails.strCode;
                        TaskList.strOpsGroupCode = entry.RepItemDetails.strOpsGroupCode;
                        TaskList.strOpsGroup = entry.RepItemDetails.strOpsGroup;
                        TaskList.bIsCritical = entry.RepItemDetails.bIsCritical;
                        TaskList.iTaskIndex = entry.RepItemDetails.iTaskIndex;
                        TaskList.bHasChildren = entry.RepItemDetails.bHasChildren;
                        TaskList.bFromShore = entry.RepItemDetails.bFromShore;
                        TaskList.iNSID = entry.RepItemDetails.iNSID;

                        TaskLists.Add(TaskList);

                        break;

                    case "Hazard":
                        class_hazardlist HazardsList = new class_hazardlist();
                        HazardsList.iID = entry.RepItemDetails.iID;
                        HazardsList.strName = entry.RepItemDetails.strName;
                        HazardsList.iTypeId = entry.RepItemDetails.iTypeId;
                        HazardsList.strType = entry.RepItemDetails.strType;
                        HazardsList.bHasChildren = entry.RepItemDetails.bHasChildren;

                        HazardLists.Add(HazardsList);

                        break;

                    case "ChecklistProcs":
                        class_checklistlist CheckListProc = new class_checklistlist();

                        CheckListProc.iID = entry.RepItemDetails.iID;
                        CheckListProc.strName = entry.RepItemDetails.strName;
                        CheckListProc.strCode = entry.RepItemDetails.strCode;
                        CheckListProc.iShipType = entry.RepItemDetails.iShipType;
                        CheckListProc.strShipType = entry.RepItemDetails.strShipType;
                        CheckListProc.strDocUrl = entry.RepItemDetails.strDocUrl;
                        CheckListProc.strDocName = entry.RepItemDetails.strDocName;
                        CheckListProc.strDocVersion = entry.RepItemDetails.strDocVersion;
                        CheckListProc.strVersion = entry.RepItemDetails.strVersion;
                        CheckListProc.iMajorVersion = entry.RepItemDetails.iMajorVersion;
                        CheckListProc.iMinorVersion = entry.RepItemDetails.iMinorVersion;
                        CheckListProc.bHasChildren = entry.RepItemDetails.bHasChildren;
                        CheckListProc.bIsApproved = entry.RepItemDetails.bIsApproved;
                        CheckListProc.bIsCurrentVersion = entry.RepItemDetails.bIsCurrentVersion;

                        List<int> cl_Tasks = new List<int>();
                        foreach (int cltask in entry.RepItemDetails.Tasks)
                        {
                            cl_Tasks.Add(cltask);
                        }

                        CheckListProc.Tasks = cl_Tasks;
                        CheckListProc.UpdateDate = entry.RepItemDetails.UpdateDate;
                        CheckListProcs.Add(CheckListProc);
                        break;

                    case "WorkPermit":
                        class_workpermitlist WorkPermit = new class_workpermitlist();

                        WorkPermit.iID = entry.RepItemDetails.iID;
                        WorkPermit.strName = entry.RepItemDetails.strName;
                        WorkPermit.strCode = entry.RepItemDetails.strCode;
                        WorkPermit.strDocUrl = entry.RepItemDetails.strDocUrl;
                        WorkPermit.strDocName = entry.RepItemDetails.strDocName;
                        WorkPermit.strDocVersion = entry.RepItemDetails.strDocVersion;
                        WorkPermit.KMSRepRef = entry.RepItemDetails.KMSRepRef;


                        WorkPermits.Add(WorkPermit);

                        break;

                    case "RASheet":
                        class_ralist RAList = new class_ralist();

                        RAList.iID = entry.RepItemDetails.iID;
                        RAList.strOpsGroupCode = entry.RepItemDetails.strOpsGroupCode;
                        RAList.strOpsGroup = entry.RepItemDetails.strOpsGroup;
                        RAList.iTaskID = entry.RepItemDetails.iTaskID;
                        RAList.strTaskCode = entry.RepItemDetails.strTaskCode;
                        RAList.strTask = entry.RepItemDetails.strTask;
                        RAList.strCode = entry.RepItemDetails.strCode;
                        RAList.strStatus = entry.RepItemDetails.strStatus;
                        RAList.iIssueNo = entry.RepItemDetails.iIssueNo;
                        RAList.iRevNo = entry.RepItemDetails.iRevNo;
                        RAList.CreateDate = entry.RepItemDetails.CreateDate;
                        RAList.UpdateDate = entry.RepItemDetails.UpdateDate;
                        RAList.ApprovedDate = entry.RepItemDetails.ApprovedDate;
                        RAList.bIsApproved = entry.RepItemDetails.bIsApproved;
                        RAList.strPreparedBy = entry.RepItemDetails.strPreparedBy;
                        RAList.strModifiedBy = entry.RepItemDetails.strModifiedBy;
                        RAList.strApprovedBy = entry.RepItemDetails.strApprovedBy;
                        RAList.bFromShore = entry.RepItemDetails.bFromShore;
                        RAList.iNSID = entry.RepItemDetails.iNSID;

                        List<class_rahazardlist> RAHazardLists = new List<class_rahazardlist>();
                        foreach (var hl in entry.RepItemDetails.Hazards)
                        {
                            class_rahazardlist RAHazardsList = new class_rahazardlist();

                            RAHazardsList.iID = hl.iID;
                            RAHazardsList.iRAID = hl.iRAID;
                            RAHazardsList.iHazardID = hl.iHazardID;
                            RAHazardsList.strHazard = hl.strHazard;
                            RAHazardsList.strConsequence = hl.strConsequence;
                            RAHazardsList.strProcRef = hl.strProcRef;
                            RAHazardsList.strSafetyPrec = hl.strSafetyPrec;
                            RAHazardsList.strAddMeasures = hl.strAddMeasures;
                            RAHazardsList.strByWhom = hl.strByWhom;
                            RAHazardsList.ByWhen = hl.ByWhen;
                            RAHazardsList.bFromShore = hl.bFromShore;

                            List<class_rahazardrisklist> RAHazardRiskLists = new List<class_rahazardrisklist>();
                            foreach (var rahrl in hl.RiskList)
                            {
                                class_rahazardrisklist RARiskList = new class_rahazardrisklist();
                                RARiskList.iID = rahrl.iID;
                                RARiskList.iCategoryID = rahrl.iCategoryID;
                                RARiskList.strCategory = rahrl.strCategory;
                                RARiskList.iRiskIndex = rahrl.iRiskIndex;
                                RARiskList.iSeverity = rahrl.iSeverity;
                                RARiskList.iFrequency = rahrl.iFrequency;
                                RARiskList.strValue = rahrl.strValue;
                                RAHazardRiskLists.Add(RARiskList);
                            }

                            RAHazardsList.RiskList = RAHazardRiskLists;
                            RAHazardLists.Add(RAHazardsList);
                        }





                        RAList.Hazards = RAHazardLists;
                        List<int> RAListCheckLists = new List<int>();
                        foreach (int racl in entry.RepItemDetails.Checklists)
                        {
                            RAListCheckLists.Add(racl);
                        }
                        RAList.Checklists = RAListCheckLists;

                        List<int> RAListWorkPermits = new List<int>();
                        foreach (int rawp in entry.RepItemDetails.WorkPermits)
                        {
                            RAListWorkPermits.Add(rawp);
                        }
                        RAList.WorkPermits = RAListWorkPermits;

                        RALists.Add(RAList);

                        break;

                    case "WorkPlan":

                        class_workplanlist WorkPlan = new class_workplanlist();
                        WorkPlan.iID = entry.RepItemDetails.iID;
                        WorkPlan.strJob = entry.RepItemDetails.strJob;
                        WorkPlan.JobDate = entry.RepItemDetails.JobDate;
                        WorkPlan.strPreparedBy = entry.RepItemDetails.strPreparedBy;
                        WorkPlan.CreateDate = entry.RepItemDetails.CreateDate;
                        WorkPlan.bIsApproved = entry.RepItemDetails.bIsApproved;
                        WorkPlan.strApprovedBy = entry.RepItemDetails.strApprovedBy;

                        List<string> WorkPlan_RAs = new List<string>();
                        foreach (var ras in entry.RepItemDetails.RAs)
                        {
                            WorkPlan_RAs.Add(ras.sType.Value);
                        }

                        WorkPlan.RACodes = WorkPlan_RAs;

                        List<CrewMember> CrewMembers = new List<CrewMember>();
                        foreach (var crew in entry.RepItemDetails.Crew)
                        {
                            CrewMember CrewMember = new CrewMember();
                            CrewMember.ID = crew.ID;
                            CrewMember.Guid = crew.Guid;
                            CrewMember.FirstName = crew.FirstName;
                            CrewMember.LastName = crew.LastName;
                            CrewMember.Rank = crew.Rank;
                            CrewMember.Active = crew.Active;

                            CrewMembers.Add(CrewMember);

                        }
                        WorkPlan.Crew = CrewMembers;

                        List<class_rahazardlist> WP_HazardLists = new List<class_rahazardlist>();
                        foreach (var wp_hazard in entry.RepItemDetails.Hazards)
                        {
                            class_rahazardlist WP_HazardList = new class_rahazardlist();
                            WP_HazardList.iID = wp_hazard.iID;

                            WP_HazardList.iRAID = wp_hazard.iRAID;
                            WP_HazardList.iHazardID = wp_hazard.iHazardID;
                            WP_HazardList.strHazard = wp_hazard.strHazard;
                            WP_HazardList.strConsequence = wp_hazard.strConsequence;
                            WP_HazardList.strProcRef = wp_hazard.strProcRef;
                            WP_HazardList.strSafetyPrec = wp_hazard.strSafetyPrec;
                            WP_HazardList.strAddMeasures = wp_hazard.strAddMeasures;
                            WP_HazardList.strByWhom = wp_hazard.strByWhom;
                            WP_HazardList.ByWhen = wp_hazard.ByWhen;
                            WP_HazardList.bFromShore = wp_hazard.bFromShore;



                            List<class_rahazardrisklist> WP_HazardRiskLists = new List<class_rahazardrisklist>();
                            foreach (var wp_hazardrisklist in wp_hazard.RiskList)
                            {
                                class_rahazardrisklist WP_HazardRiskList = new class_rahazardrisklist();
                                WP_HazardRiskList.iID = wp_hazardrisklist.iID;
                                WP_HazardRiskList.iCategoryID = wp_hazardrisklist.iCategoryID;
                                WP_HazardRiskList.strCategory = wp_hazardrisklist.strCategory;
                                WP_HazardRiskList.iRiskIndex = wp_hazardrisklist.iRiskIndex;
                                WP_HazardRiskList.iSeverity = wp_hazardrisklist.iSeverity;
                                WP_HazardRiskList.iFrequency = wp_hazardrisklist.iFrequency;
                                WP_HazardRiskList.strValue = wp_hazardrisklist.strValue;

                                WP_HazardRiskLists.Add(WP_HazardRiskList);

                            }

                            WP_HazardList.RiskList = WP_HazardRiskLists;

                            WP_HazardLists.Add(WP_HazardList);

                        }

                        WorkPlan.Hazards = WP_HazardLists;

                        List<int> WPCheckLists = new List<int>();
                        foreach (int wpcl in entry.RepItemDetails.Checklists)
                        {
                            WPCheckLists.Add(wpcl);
                        }
                        WorkPlan.Checklists = WPCheckLists;


                        List<int> WPWorkPermits = new List<int>();
                        foreach (int wpwp in entry.RepItemDetails.WorkPermits)
                        {
                            WPWorkPermits.Add(wpwp);
                        }
                        WorkPlan.WorkPermits = WPWorkPermits;

                        WorkPlans.Add(WorkPlan);

                        break;

                }

            }




            int returnCode = 0;

            string conStrRepRisk = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(conStrRepRisk))
            {
                using (SqlCommand cmd = new SqlCommand("sprmsSaveRepRisk", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@RepRef", rm_header.RepRef);
                    cmd.Parameters.AddWithValue("@RepModCode", rm_header.RepModCode);
                    cmd.Parameters.AddWithValue("@RepFolSrc", rm_header.RepFolSrc);
                    cmd.Parameters.AddWithValue("@RepSMTPSrc", rm_header.RepSMTPSrc);
                    cmd.Parameters.AddWithValue("@RepFullDate", rm_header.RepFullDate);
                    cmd.Parameters.AddWithValue("@RepHeaderStatus", "CR");
                    cmd.Parameters.AddWithValue("@RepYYMM", rm_header.RepYYMM);
                    cmd.Parameters.AddWithValue("@RepSr", rm_header.RepSr);
                    cmd.Parameters.AddWithValue("@RepIMO", rm_header.RepIMO);
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    int db_code = 0;
                    string db_status = string.Empty;

                    while (dr.Read())
                    {
                        db_code = Convert.ToInt32(dr["code"]);
                        db_status = dr["Status"].ToString();
                    }

                    if (db_code == 0 && db_status == "RC")
                    {

                        //MessageBox.Show("Replication Already Exists!, Process will NOT save to the Database");

                        if (!System.IO.File.Exists(rmmfilepath.Substring(0, rmmfilepath.Length - filename.Length) + @"\_ProcessedFiles\" + filename))
                        {
                            string baseName = filename.Substring(0, filename.Length - 4);
                            System.IO.File.Move(rmmfilepath, rmmfilepath.Substring(0, rmmfilepath.Length - filename.Length) + @"\_ProcessedFiles\" + baseName + "_" + DateTime.Now.ToString("hhmmss") + ".rmm");
                        }
                        else { System.IO.File.Delete(rmmfilepath); }

                        return null;
                    }

                    conn.Close();
                }

            }


            // Save Task List in the database
            List<RMM_Acknowledgement> RMM_Acknowledgements = new List<RMM_Acknowledgement>();

            RMM_Acknowledgement rmm_acknowledgement = new RMM_Acknowledgement();

            List<RMM_TasksId> Tasks = new List<RMM_TasksId>();

            int TaskIdOffice;

            for (int tl = 0; tl < TaskLists.Count(); tl++)
            {
                string conStr = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditTasks", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", TaskLists[tl].strName);
                        cmd.Parameters.AddWithValue("@OpsGroupCode", TaskLists[tl].strOpsGroupCode);
                        cmd.Parameters.AddWithValue("@IsCritical", TaskLists[tl].bIsCritical);
                        cmd.Parameters.AddWithValue("@ShipIMO", rm_header.RepIMO);
                        cmd.Parameters.AddWithValue("@Code", TaskLists[tl].strCode);

                        conn.Open();

                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Read();
                        TaskIdOffice = Convert.ToInt32(dr["ID"]);

                    }

                    conn.Close();
                }

                RMM_TasksId Task = new RMM_TasksId();
                Task.tas_NMID = TaskLists[tl].iID;
                Task.tas_NSID = TaskIdOffice;
                Tasks.Add(Task);

                // for Acknowledgement Process
                rmm_acknowledgement.Task_Ids = Tasks;

            }



            // Save Hazard List in the database

            List<RMM_HazardsId> Hazards = new List<RMM_HazardsId>();

            int HazIdOffice;

            for (int hl = 0; hl < HazardLists.Count(); hl++)
            {
                string conStr = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditHazards", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", HazardLists[hl].strName);
                        cmd.Parameters.AddWithValue("@Type", HazardLists[hl].iTypeId);
                        cmd.Parameters.AddWithValue("@ShipIMO", rm_header.RepIMO);
                        conn.Open();

                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Read();
                        HazIdOffice = Convert.ToInt32(dr["ID"]);
                    }

                    conn.Close();

                }

                RMM_HazardsId hazard = new RMM_HazardsId();
                hazard.haz_NMID = HazardLists[hl].iID;
                hazard.haz_NSID = HazIdOffice;
                Hazards.Add(hazard);

                // for Acknowledgement Process
                rmm_acknowledgement.Hazard_Ids = Hazards;
            }



            // Process Checklist


            List<RMM_CheckListsProcsId> ChecklistsIds = new List<RMM_CheckListsProcsId>();

            int ChecklistIdOffice;

            for (int cl = 0; cl < CheckListProcs.Count(); cl++)
            {
                string conStr = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditChecklists", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Code", CheckListProcs[cl].strCode);
                        cmd.Parameters.AddWithValue("@Name", CheckListProcs[cl].strName);
                        cmd.Parameters.AddWithValue("@ShipTypeId", CheckListProcs[cl].iShipType);
                        cmd.Parameters.AddWithValue("@DocLoc", CheckListProcs[cl].iID.ToString());
                        cmd.Parameters.AddWithValue("@DocName", CheckListProcs[cl].strDocName);
                        cmd.Parameters.AddWithValue("@DocVer", CheckListProcs[cl].strDocVersion);
                        cmd.Parameters.AddWithValue("@MajorVer", CheckListProcs[cl].iMajorVersion);
                        cmd.Parameters.AddWithValue("@MinorVer", CheckListProcs[cl].iMinorVersion);
                        cmd.Parameters.AddWithValue("@ShipIMO", rm_header.RepIMO);
                        cmd.Parameters.AddWithValue("@KMSRepRef", rm_header.RepRef);

                        conn.Open();

                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Read();
                        ChecklistIdOffice = Convert.ToInt32(dr["ID"]);

                    }

                    conn.Close();

                }

                RMM_CheckListsProcsId checklist = new RMM_CheckListsProcsId();
                checklist.chk_NMID = CheckListProcs[cl].iID;
                checklist.chk_NSID = ChecklistIdOffice;
                ChecklistsIds.Add(checklist);

                // for Acknowledgement Process
                rmm_acknowledgement.Checklist_Ids = ChecklistsIds;




                // Save Checklist Tasks

                string ChecklistTasks = string.Empty;

                for (int clt = 0; clt < CheckListProcs[cl].Tasks.Count; clt++)
                {
                    ChecklistTasks += CheckListProcs[cl].Tasks[clt] + ",";

                }

                ChecklistTasks = ChecklistTasks.Substring(0, ChecklistTasks.Length - 1);

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditChecklistTasks", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ChecklistID", ChecklistIdOffice);
                        cmd.Parameters.AddWithValue("@TaskIDs", ChecklistTasks);
                        conn.Open();

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();

                }

            }




            // Save RA List in the database

            int RaId;
            int RAHazardId;
            bool result = false;

            List<int> OfficeRAIds = new List<int>();

            for (int ral = 0; ral < RALists.Count(); ral++)
            {

                List<RMM_RA_AckIds> RAIds = new List<RMM_RA_AckIds>();


                int OperationsGroupID = GetOperationsGroupIdByCode(RALists[ral].strOpsGroupCode);

                string conStr = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditRASheets", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Code", RALists[ral].strCode);
                        cmd.Parameters.AddWithValue("@OpsGroupCode", RALists[ral].strOpsGroupCode);

                        for (int tl = 0; tl < Tasks.Count(); tl++)
                        {
                            if (Tasks[tl].tas_NMID == RALists[ral].iTaskID)
                            {
                                cmd.Parameters.AddWithValue("@TaskID", Tasks[tl].tas_NSID);
                                break;
                            }

                        }
                        cmd.Parameters.AddWithValue("@Editor", RALists[ral].strModifiedBy);
                        cmd.Parameters.AddWithValue("@ShipIMO", rm_header.RepIMO);
                        cmd.Parameters.AddWithValue("@OpsGroupId", OperationsGroupID);
                        cmd.Parameters.AddWithValue("@IssueNo", RALists[ral].iIssueNo);
                        cmd.Parameters.AddWithValue("@RevNo", RALists[ral].iRevNo);
                        cmd.Parameters.AddWithValue("@CreatedDate", RALists[ral].CreateDate);
                        cmd.Parameters.AddWithValue("@ModifiedDate", RALists[ral].UpdateDate);
                        cmd.Parameters.AddWithValue("@PreparedBy", RALists[ral].strPreparedBy);
                        cmd.Parameters.AddWithValue("@IsApproved", RALists[ral].bIsApproved);

                        conn.Open();

                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Read();
                        RaId = Convert.ToInt32(dr["ID"]);
                        result = Convert.ToBoolean(dr["result"]);

                        RMM_RA_AckIds Ra = new RMM_RA_AckIds();
                        Ra.RA_nmId = RALists[ral].iID;
                        Ra.RA_nsId = RaId;
                        RAIds.Add(Ra);
                        OfficeRAIds.Add(Ra.RA_nsId);

                        rmm_acknowledgement.repref = rm_header.RepRef;

                        // for Acknowledgement Process
                        rmm_acknowledgement.RMM_RA_Ids = RAIds;

                        // if RA exists move to next RA
                        if (!result)
                        {
                            continue;
                        }
                    }

                    conn.Close();
                }


                for (int rah = 0; rah < RALists[ral].Hazards.Count(); rah++)
                {


                    HazardId hazard = new HazardId();

                    string conStr_rahazards = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                    using (SqlConnection conn_rahazards = new SqlConnection(conStr_rahazards))
                    {
                        using (SqlCommand cmd = new SqlCommand("sprmsAddEditRAHazards", conn_rahazards))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@RaID", RaId);


                            for (int h = 0; h < Hazards.Count(); h++)
                            {
                                if (Hazards[h].haz_NMID == RALists[ral].Hazards[rah].iHazardID)
                                {
                                    hazard.Id = Hazards[h].haz_NSID;
                                    cmd.Parameters.AddWithValue("@HazardID", hazard.Id);
                                    break;
                                }

                            }

                            cmd.Parameters.AddWithValue("@Consequence", RALists[ral].Hazards[rah].strConsequence);
                            cmd.Parameters.AddWithValue("@RefToProcs", RALists[ral].Hazards[rah].strProcRef);
                            cmd.Parameters.AddWithValue("@SafetyPrecToBeTaken", RALists[ral].Hazards[rah].strSafetyPrec);
                            cmd.Parameters.AddWithValue("@AdditionalMeasures", RALists[ral].Hazards[rah].strAddMeasures);

                            conn_rahazards.Open();

                            SqlDataReader dr = cmd.ExecuteReader();
                            dr.Read();
                            RAHazardId = Convert.ToInt32(dr["ID"]);



                        }

                        conn_rahazards.Close();
                    }


                    for (int rahrl = 0; rahrl < RALists[ral].Hazards[rah].RiskList.Count(); rahrl++)
                    {
                        string conStr_rarisklist = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                        using (SqlConnection conn_rarisklist = new SqlConnection(conStr_rarisklist))
                        {
                            using (SqlCommand cmd = new SqlCommand("sprmsAddEditRAHazardMatrix", conn_rarisklist))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@RaID", RaId);
                                cmd.Parameters.AddWithValue("@RAHazardsID", RAHazardId);
                                cmd.Parameters.AddWithValue("@Severity", RALists[ral].Hazards[rah].RiskList[rahrl].iSeverity);
                                cmd.Parameters.AddWithValue("@Frequency", RALists[ral].Hazards[rah].RiskList[rahrl].iFrequency);
                                cmd.Parameters.AddWithValue("@RiskIndex", RALists[ral].Hazards[rah].RiskList[rahrl].iRiskIndex);
                                cmd.Parameters.AddWithValue("@Value", RALists[ral].Hazards[rah].RiskList[rahrl].strValue);
                                cmd.Parameters.AddWithValue("@CategoryID", RALists[ral].Hazards[rah].RiskList[rahrl].iCategoryID);

                                conn_rarisklist.Open();

                                cmd.ExecuteNonQuery();

                            }

                            conn_rarisklist.Close();

                        }
                    }


                }


                // Save RA Checklist 

                string RAChecklists = string.Empty;

                for (int racl = 0; racl < RALists[ral].Checklists.Count; racl++)
                {
                    RAChecklists += RALists[ral].Checklists[racl] + ",";

                }

                RAChecklists = RAChecklists.Substring(0, RAChecklists.Length - 1);

                string conStr_rachecklist = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn_rachecklist = new SqlConnection(conStr_rachecklist))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditRAChecklists", conn_rachecklist))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RaID", RaId);
                        cmd.Parameters.AddWithValue("@ChecklistIDs", RAChecklists);
                        conn_rachecklist.Open();
                        cmd.ExecuteNonQuery();
                    }

                    conn_rachecklist.Close();

                }


                // Save RA Work Permits 
                string RAWorkPermits = string.Empty;


                for (int rawp = 0; rawp < RALists[ral].WorkPermits.Count; rawp++)
                {
                    RAWorkPermits += RALists[ral].WorkPermits[rawp] + ",";
                }

                RAWorkPermits = RAWorkPermits.Substring(0, RAWorkPermits.Length - 1);

                string conStr_raworkpermits = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn_raworkpermits = new SqlConnection(conStr_raworkpermits))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditRAWorkPermits", conn_raworkpermits))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RaID", RaId);
                        cmd.Parameters.AddWithValue("@WorkPermitIDs", RAWorkPermits);
                        conn_raworkpermits.Open();
                        cmd.ExecuteNonQuery();
                    }

                    conn_raworkpermits.Close();

                }

            }

            int WorkPlanId = 0;

            for (int wp = 0; wp < WorkPlans.Count(); wp++)
            {

                List<RMM_WP_AckIds> WPIds = new List<RMM_WP_AckIds>();

                string conStr = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddEditWorkPlan", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@NMID", WorkPlans[wp].iID);
                        cmd.Parameters.AddWithValue("@ShipIMO", rm_header.RepIMO);
                        cmd.Parameters.AddWithValue("@Job", WorkPlans[wp].strJob);
                        cmd.Parameters.AddWithValue("@JobDate", WorkPlans[wp].JobDate);
                        cmd.Parameters.AddWithValue("@PlannedBy", WorkPlans[wp].strPreparedBy);
                        cmd.Parameters.AddWithValue("@CreatedDate", WorkPlans[wp].CreateDate);
                        cmd.Parameters.AddWithValue("@IsApproved", WorkPlans[wp].bIsApproved);
                        cmd.Parameters.AddWithValue("@ApprovedBy", WorkPlans[wp].strApprovedBy);

                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        dr.Read();
                        WorkPlanId = Convert.ToInt32(dr["ID"]);
                        result = Convert.ToBoolean(dr["result"]);

                        RMM_WP_AckIds Wp = new RMM_WP_AckIds();
                        Wp.WP_nmId = WorkPlans[wp].iID;
                        Wp.WP_nsId = WorkPlanId;
                        WPIds.Add(Wp);

                        rmm_acknowledgement.repref = rm_header.RepRef;

                        // For Acknowledgment Purposes
                        rmm_acknowledgement.RMM_WP_Ids = WPIds;

                        if (!result)
                        {
                            continue;
                        }
                    }

                }

                // Work Plan RAs

                List<int> WorkPlan_RAids = new List<int>();

                string strWPRAIds = string.Empty;
                for (int wpra = 0; wpra < WorkPlans[wp].RACodes.Count(); wpra++)
                {
                    int wpRAIds = GetRAIdByCode(WorkPlans[wp].RACodes[wpra]);
                    strWPRAIds += wpRAIds + ",";
                }

                strWPRAIds = strWPRAIds.Substring(0, strWPRAIds.Length - 1);

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand("sprmsAddWorkPlanRAs", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@WorkPlanId", WorkPlanId);
                        cmd.Parameters.AddWithValue("@RaIDs", strWPRAIds);

                        conn.Open();

                        cmd.ExecuteNonQuery();
                    }

                }

                for (int wpc = 0; wpc < WorkPlans[wp].Crew.Count(); wpc++)
                {

                    int rankId = GetRankIDByForeignId(WorkPlans[wp].Crew[wpc].ID);
                    int crewId = GetCrewIDByForeignId(WorkPlans[wp].Crew[wpc].ID);

                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        using (SqlCommand cmd = new SqlCommand("sprmsAddWorkPlanCrew", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@WorkPlanId", WorkPlanId);
                            cmd.Parameters.AddWithValue("@ShipIMO", rm_header.RepIMO);
                            cmd.Parameters.AddWithValue("@CrewID", crewId);
                            cmd.Parameters.AddWithValue("@FirstName", WorkPlans[wp].Crew[wpc].FirstName);
                            cmd.Parameters.AddWithValue("@LastName", WorkPlans[wp].Crew[wpc].LastName);
                            cmd.Parameters.AddWithValue("@CrewGuid", WorkPlans[wp].Crew[wpc].Guid);
                            cmd.Parameters.AddWithValue("@RankID", rankId);

                            conn.Open();

                            cmd.ExecuteNonQuery();

                        }
                    }

                }


                for (int wph = 0; wph < WorkPlans[wp].Hazards.Count(); wph++)
                {

                    int WPHazardId = 0;
                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        using (SqlCommand cmd = new SqlCommand("sprmsAddWorkPlanHazards", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@WorkPlanID", WorkPlanId);
                            cmd.Parameters.AddWithValue("@HazardID", WorkPlans[wp].Hazards[wph].iHazardID);
                            cmd.Parameters.AddWithValue("@Consequence", WorkPlans[wp].Hazards[wph].strConsequence);
                            cmd.Parameters.AddWithValue("@RefToProcs", WorkPlans[wp].Hazards[wph].strProcRef);
                            cmd.Parameters.AddWithValue("@SafetyPrecToBeTaken", WorkPlans[wp].Hazards[wph].strSafetyPrec);
                            cmd.Parameters.AddWithValue("@AdditionalMeasures", WorkPlans[wp].Hazards[wph].strAddMeasures);
                            cmd.Parameters.AddWithValue("@ByWhom", WorkPlans[wp].Hazards[wph].strByWhom);
                            //cmd.Parameters.AddWithValue("@ByWhen", WorkPlans[wp].Hazards[wph].ByWhen);

                            conn.Open();

                            SqlDataReader dr = cmd.ExecuteReader();
                            dr.Read();
                            WPHazardId = Convert.ToInt32(dr["ID"]);
                        }
                    }


                    for (int wphrl = 0; wphrl < WorkPlans[wp].Hazards[wph].RiskList.Count(); wphrl++)
                    {
                        string conStr_wprisklist = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                        using (SqlConnection conn_wprisklist = new SqlConnection(conStr_wprisklist))
                        {
                            using (SqlCommand cmd = new SqlCommand("sprmsAddWorkPlanHazardMatrix", conn_wprisklist))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@WorkPlanID", WorkPlanId);
                                cmd.Parameters.AddWithValue("@WorkPlanHazardsID", WPHazardId);
                                cmd.Parameters.AddWithValue("@Severity", WorkPlans[wp].Hazards[wph].RiskList[wphrl].iSeverity);
                                cmd.Parameters.AddWithValue("@Frequency", WorkPlans[wp].Hazards[wph].RiskList[wphrl].iFrequency);
                                cmd.Parameters.AddWithValue("@RiskIndex", WorkPlans[wp].Hazards[wph].RiskList[wphrl].iRiskIndex);
                                cmd.Parameters.AddWithValue("@Value", WorkPlans[wp].Hazards[wph].RiskList[wphrl].strValue);
                                cmd.Parameters.AddWithValue("@CategoryID", WorkPlans[wp].Hazards[wph].RiskList[wphrl].iCategoryID);

                                conn_wprisklist.Open();

                                cmd.ExecuteNonQuery();

                            }

                            conn_wprisklist.Close();

                        }
                    }


                }

                string WPChecklists = string.Empty;

                for (int wpcl = 0; wpcl < WorkPlans[wp].Checklists.Count; wpcl++)
                {
                    WPChecklists += WorkPlans[wp].Checklists[wpcl] + ",";

                }

                if (WPChecklists != "")
                {
                    WPChecklists = WPChecklists.Substring(0, WPChecklists.Length - 1);

                    string conStr_wpchecklist = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                    using (SqlConnection conn_wpchecklist = new SqlConnection(conStr_wpchecklist))
                    {
                        using (SqlCommand cmd = new SqlCommand("sprmsAddWorkPlanChecklists", conn_wpchecklist))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@WorkPlanId", WorkPlanId);
                            cmd.Parameters.AddWithValue("@ChecklistIDs", WPChecklists);
                            conn_wpchecklist.Open();
                            cmd.ExecuteNonQuery();
                        }

                        conn_wpchecklist.Close();

                    }

                }


                string WPWorkPermits = string.Empty;

                for (int wpwp = 0; wpwp < WorkPlans[wp].WorkPermits.Count; wpwp++)
                {
                    WPWorkPermits += WorkPlans[wp].WorkPermits[wpwp] + ",";
                }
                if (WPWorkPermits != "")
                {
                    WPWorkPermits = WPWorkPermits.Substring(0, WPWorkPermits.Length - 1);

                    string conStr_wpworkpermits = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

                    using (SqlConnection conn_wpworkpermits = new SqlConnection(conStr_wpworkpermits))
                    {
                        using (SqlCommand cmd = new SqlCommand("sprmsAddWorkPlanWorkPermits", conn_wpworkpermits))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@WorkPlanId", WorkPlanId);
                            cmd.Parameters.AddWithValue("@WorkPermitIDs", WPWorkPermits);
                            conn_wpworkpermits.Open();
                            cmd.ExecuteNonQuery();
                        }
                        conn_wpworkpermits.Close();
                    }

                }
            }

            RMM_Acknowledgements.Add(rmm_acknowledgement);

            //Acknowledge Document Received
            string repref = rm_header.RepRef;
            UpdateAcknowledgementRMM(repref, "RC", rm_header.RepIMO);

            //Move to the processed folder

            //===== log
            inLog += DateTime.Now.ToString() + ": [Successfully processed acknowledgment file " + filename + " RepRef: " + rm_header.RepRef + "] \r\n";
            //===== end log
            
            if (!System.IO.File.Exists(rmmfilepath.Substring(0, rmmfilepath.Length - filename.Length) + @"\_ProcessedFiles\" + filename))
            {
                string baseName = filename.Substring(0, filename.Length - 4);
                System.IO.File.Move(rmmfilepath, rmmfilepath.Substring(0, rmmfilepath.Length - filename.Length) + @"\_ProcessedFiles\" + baseName + "_" + DateTime.Now.ToString("hhmmss") + ".rmm");
            }
            else { System.IO.File.Delete(rmmfilepath); }
            
            return RMM_Acknowledgements;
        }

        private void UpdateAcknowledgementRMM(string uniquerec, string newrem, int imo)
        {
            string csv = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc = new SqlConnection(csv))
            {
                using (SqlCommand cmd = new SqlCommand("sprmsUpdateAckRepRisk", sc))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RepRef", uniquerec);
                    cmd.Parameters.AddWithValue("@NewRemarks", newrem);
                    cmd.Parameters.AddWithValue("@RepIMO", imo);
                    sc.Open();
                    cmd.ExecuteReader();

                }
                sc.Close();
            }
        }


        private bool ProcessRMA(string rmafilepath)
        {
            bool success;
            List<RmaAcknowledgement> repRefList = new List<RmaAcknowledgement>();
            try
            {
                dynamic r_head = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(rmafilepath));
                foreach (var refrefitem in r_head)
                {
                    RmaAcknowledgement rmaRepRef = new RmaAcknowledgement();
                    rmaRepRef.repRef = refrefitem.repref;
                    repRefList.Add(rmaRepRef);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("ERROR: " + ex.Message);
                //===== log
                inLog += DateTime.Now.ToString() + ": ERROR_[" + ex.Message.ToString() + "] \r\n";
                //===== end log
                success = false;
                return success;
            }


            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprmsAcknowledgeRMSHeader", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RepRef", SqlDbType.Text);
                    conn.Open();

                    for (int i = 0; i < repRefList.Count; i++)
                    {
                        cmd.Parameters["@RepRef"].Value = repRefList[i].repRef;

                        bool updated = (bool)cmd.ExecuteScalar();
                    }
                }
                conn.Close();
                success = true;
            }


            string filename = rmafilepath.Split('\\').Last();
            //===== log
            inLog += DateTime.Now.ToString() + ": [Successfully processed acknowledgment file " + filename + "] \r\n";
            //===== end log


            if (!System.IO.File.Exists(rmafilepath.Substring(0, rmafilepath.Length - filename.Length) + @"\_ProcessedFiles\" + filename))
            {
                string baseName = filename.Substring(0, filename.Length - 4);
                System.IO.File.Move(rmafilepath, rmafilepath.Substring(0, rmafilepath.Length - filename.Length) + @"\_ProcessedFiles\" + baseName + "_" + DateTime.Now.ToString("hhmmss") + ".rma");
            }
            else { System.IO.File.Delete(rmafilepath); }

            return success;
        }

        private int GetRankIDByForeignId(int foreignid)
        {
            int rankID = 0;

            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepGetRankIDByForeignId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ForeignId", foreignid);
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rankID = Convert.ToInt32(dr["RankID"]);
                    }

                }

                conn.Close();

            }

            return rankID;

        }


        private int GetCrewIDByForeignId(int foreignid)
        {
            int crewID = 0;

            string cs = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sprepGetCrewIDByForeignId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ForeignId", foreignid);
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        crewID = Convert.ToInt32(dr["ID"]);
                    }

                }

                conn.Close();

            }

            return crewID;

        }

        private int GetOperationsGroupIdByCode(string operationsgroupcode)
        {
            int og_id;

            string con = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc = new SqlConnection(con))
            {
                using (SqlCommand cmd = new SqlCommand("sprmsRepGetOperationsGroupIdByCode", sc))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Code_OperationsGroup", operationsgroupcode);
                    sc.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dr.Read();
                    og_id = Convert.ToInt32(dr["ID"]);
                }
                sc.Close();
            }

            return og_id;

        }

        private int GetRAIdByCode(string RAcode)
        {
            int ra_id;

            string con = ConfigurationManager.ConnectionStrings["dbConn"].ToString();

            using (SqlConnection sc = new SqlConnection(con))
            {
                using (SqlCommand cmd = new SqlCommand("sprmsRepGetRaIdByRaCode", sc))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RaCode", RAcode);
                    sc.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dr.Read();
                    ra_id = Convert.ToInt32(dr["ID"]);
                }
                sc.Close();
            }

            return ra_id;

        }

        public class class_ACKId
        {
            public int Id { get; set; }

        }

        public class FileHandlerObj
        {
            public int index;
        }

    }

}
