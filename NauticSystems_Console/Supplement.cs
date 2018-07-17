using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    /*
    public static class UATGlobals
    {
        // parameterless constructor required for static class
        static UATGlobals()
        {
            UATIMO = -1;
            UATSQLServerIP = string.Empty;
            UATSQLServerPassword = string.Empty;
            UATSQLServerDatabase = string.Empty;
        } // default value

        // public get, and private set for strict access control
        public static int UATIMO { get; private set; }
        public static string UATSQLServerIP { get; private set; }
        public static string UATSQLServerPassword { get; private set; }
        public static string UATSQLServerDatabase { get; private set; }

        // GlobalInt can be changed only via this method
        public static void SetUATGlobals(int IMO, string ServerIP, string SQLPassword, string SQLDatabase)
        {
            UATIMO = IMO;
            UATSQLServerIP = ServerIP;
            UATSQLServerPassword = SQLPassword;
            UATSQLServerDatabase = SQLDatabase;

        }

    }

    public class class_UATConfig
    {
        public int IMO { get; set; }
        public string IPAddress { get; set; }
        public string SQLServerPassword { get; set; }
        public string SQLServerDatabase { get; set; }
    }
    */

    public class class_nsdlFunction
    {
        public static string getBaseName(string strFileName)
        {
            string BaseName;
            int fileLen = strFileName.LastIndexOf('.');
            if (fileLen > 0)
            {
                int extNameLen = strFileName.Length - fileLen;
                string extName = strFileName.Substring(fileLen);
                BaseName = strFileName.Replace(extName, "");
            }
            else
            {
                BaseName = strFileName;
            }
            return BaseName;
        }
    }
    
    public class class_nmReplicationJSONFull
    {
        public class_ReplicationJSONHeader RepHeader { get; set; }
        public List<class_nmReplicationJSONFile> entries { get; set; }

    }

    public class class_nmReplicationJSONFile
    {
        public int RepID { get; set; }
        public class_nmreplicationlist RepSPDocsInfo { get; set; }
        //public string RepDocDistributionStatus { get; set; }
        public string RepDistributionStatus { get; set; }
        public string UpdateType { get; set; }
    }

    public class class_ReplicationJSONHeader
    {
        public string RepRef { get; set; }
        public string RepModCode { get; set; }
        public string RepFolSrc { get; set; }
        public string RepSMTPSrc { get; set; }
        public string RepFullDate { get; set; }
        public string RepHeaderStatus { get; set; }
        public int RepYYMM { get; set; }
        public int RepSr { get; set; }
        public int RepIMO { get; set; }

    }

    public class class_nmreplicationlist
    {
        //SQL table : rep_RepDocs 
        public string strAuthor { get; set; }
        public string strContentType { get; set; }
        public int intFSObjType { get; set; }
        public int intFolderChildCount { get; set; }
        public int intItemChildCount { get; set; }
        public string strBaseName { get; set; }
        public string strLinkFilename { get; set; }
        public DateTime dttCreated { get; set; }
        public string strCreatedBy { get; set; }
        public string strLastModified { get; set; }
        public DateTime dttModified { get; set; }
        public string strModifiedBy { get; set; }
        public string strEditor { get; set; }
        public int intFileSizeDisplay { get; set; }
        public string strFileType { get; set; }
        public string strServerUrl { get; set; }
        public string strUniqueId { get; set; }
        public string strParentUniqueId { get; set; }
        public string strGUID { get; set; }
        public string strVirtual_LocalRelativePath { get; set; }
        public string strDocumentLibrary { get; set; }
        public int intId { get; set; }
        public int intPid { get; set; }
        public string strIDPath { get; set; }

    }
    public class class_UploadFile
    {
        public string SourceUrl { get; set; }
        public string DestinationUrl { get; set; }

    }

    public class class_sharepointlist
    {
        public string strFileName { get; set; }
        public string strVersion { get; set; }
        public DateTime strModified { get; set; }
        public string strModifiedBy { get; set; }
        public string strDocType { get; set; }
        public int ictr { get; set; }

        //wg:start
        public string strFileDirRef { get; set; }
        public string strUniqueID { get; set; }
        public string strParentUniqueID { get; set; }
        public string strBaseFileName { get; set; }
        public string strCheckoutUser { get; set; }
        public string strStatus { get; set; }
        public string strApprovedBy { get; set; }
        public string strContentType { get; set; }
        public int iFSObjType { get; set; }
        //wg:end

        //bw:start
        public int iFileSizeDisplay { get; set; }
        public bool IsCurrentVersion { get; set; }
        public string strApproverComments { get; set; }
        public string strApprovalStatus { get; set; }
        //bw:end

        public bool isControlledDocument { get; set; }
        public bool isFavouritedDocument { get; set; }
        public bool fromShip { get; set; }
    }

    public class class_replicationlist
    {
        //SQL table : rep_RepDocs 
        public int intspID { get; set; }
        public string strspUIVersionString { get; set; }
        public int intspContentVersion { get; set; }
        public bool blnspIsCurrentVersion { get; set; }
        public string strspAuthor { get; set; }
        public string strspContentType { get; set; }
        public int intspFSObjType { get; set; }
        public int intspFolderChildCount { get; set; }
        public int intspItemChildCount { get; set; }
        public string strspBaseName { get; set; }
        public string strspLinkFilename { get; set; }
        public DateTime dttspCreated { get; set; }
        public string strspCreatedBy { get; set; }
        public string strspLastModified { get; set; }
        public DateTime dttspModified { get; set; }
        public string strspModifiedBy { get; set; }
        public string strspEditor { get; set; }
        public int intspFileSizeDisplay { get; set; }
        public string strspFileType { get; set; }
        public string strspServerUrl { get; set; }
        public string strspUniqueId { get; set; }
        public string strspParentUniqueId { get; set; }
        public string strspGUID { get; set; }
        public string strspVirtual_LocalRelativePath { get; set; }
        public string strnmDocumentLibrary { get; set; }
        //SharePoint Approval Information
        public string strApprovedBy { get; set; }
        public string strApproverComments { get; set; }
        public string strApprovalStatus { get; set; }
        //SharePoint Approval Information
        public string strspCheckInComments { get; set; }
    }

    public class class_ReplicationFileFolderJSONFile
    {
        public int RepID { get; set; }
        public string SourceURLStr { get; set; }
        public string UpdateType { get; set; }
        public string UpdateStr { get; set; }
        public class_replicationlist RepSPDocsInfo { get; set; }
        public string RepDistributionStatus { get; set; }

    }

    public class class_ReplicationFileFolderJSONFull
    {
        public class_ReplicationJSONHeader RepHeader { get; set; }
        public List<class_ReplicationFileFolderJSONFile> entries { get; set; }

    }

}
