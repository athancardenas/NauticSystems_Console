using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    public class ReplicationFolder
    {
        public Ship ParentShip { get; set; }

        public string FullPath { get; set; }
        public string DirectoryName { get; set; }
        public string ReplicationMode { get; set; }
        public string ReplicationYYMM { get; set; }
        public int ReplicationId { get; set; }
        public string ReplicationReference { get; set; }
        public int ReplicationAckIMO { get; set; }
        public string ReplicationYYMMDD { get; set; }
        public string RelocationFolder { get; set; }
        public string RelocationFullPath { get; set; }
        public bool IsDefaultLocationForDeletion { get; set; }
        public bool IsRelocated { get; set; }
        public bool IsForShipReplication { get; set; }
        public string JSONFileFullPath { get; set; }
        public string SharePointRootFolder { get; set; }
        public string SharePointListName { get; set; }
        public bool IsForAcknowledgement { get; set; }
        public string ErrorMessage { get; set; }


        public List<ReplicationFile> ReplicationFiles { get; set; }

        public ReplicationFolder(Ship parentShip, string directoryFullPath, string relocationFolderName)
        {
            this.ParentShip = parentShip;

            FullPath = directoryFullPath;
            DirectoryName = Path.GetFileName(FullPath);

            RelocationFolder = relocationFolderName;
            RelocationFullPath = Path.GetDirectoryName(directoryFullPath) + "\\" + relocationFolderName + "\\" + DirectoryName + "\\";

            if (!Directory.Exists(RelocationFullPath))
            {
                Directory.CreateDirectory(RelocationFullPath);
            }

            if (directoryFullPath.Length == 26)
            {               
                ReplicationMode = DirectoryName.Substring(0, 5);
                ReplicationYYMM = DirectoryName.Substring(5, 4);
                ReplicationId = Convert.ToInt32(DirectoryName.Substring(9, 3));
                ReplicationReference = ReplicationMode + ReplicationYYMM + ReplicationId.ToString("D3");
                ReplicationAckIMO = Convert.ToInt32(DirectoryName.Substring(12, 7));
                ReplicationYYMMDD = DirectoryName.Substring(19, 6);
            }

            IsDefaultLocationForDeletion = false;
            IsRelocated = false;
            IsForShipReplication = false;
            SharePointListName = string.Empty;
            IsForAcknowledgement = false;
            ErrorMessage = string.Empty;
        }

        public static List<ReplicationFolder> LoadShipReplicationFolders(Ship parentShip, string folderSearchPattern, string relocationFolderName)
        {
            List<ReplicationFolder> replicationFolders = new List<ReplicationFolder>();

            List<Task> loadShipReplicationFoldersTask = new List<Task>();
            foreach (string shipInboundSubfolders in Directory.GetDirectories(parentShip.ShipInboundFolderPath, folderSearchPattern, SearchOption.TopDirectoryOnly))
            {
                loadShipReplicationFoldersTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        replicationFolders.Add(new ReplicationFolder(parentShip, shipInboundSubfolders, relocationFolderName))
                    )  
                );
            }
            Task.WaitAll(loadShipReplicationFoldersTask.ToArray());

            return replicationFolders;
        }

        public static void LoadReplicationFolderSharePointInfo(List<ReplicationFolder> replicationFolders, List<class_sharepointlist> sharepointInfos)
        {
            List<Task> loadReplicationFolderSharePointInfoTask = new List<Task>();
            foreach (ReplicationFolder replicationFolder in replicationFolders)
            {
                loadReplicationFolderSharePointInfoTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        {
                            var sharepointInfo = sharepointInfos
                                .Where(info => info.strBaseFileName.Substring(0, 2).Equals("NS") && info.strBaseFileName.Split('-').Last().Equals(replicationFolder.ParentShip.IMO))
                                .Select(info => info)
                                .First();

                            if (sharepointInfo != null)
                            {
                                replicationFolder.SharePointRootFolder = "/" + sharepointInfo.strBaseFileName;
                                replicationFolder.SharePointListName = sharepointInfo.strFileDirRef.Split('/').Last();
                            }
                        }
                    )
                );
            }
            Task.WaitAll(loadReplicationFolderSharePointInfoTask.ToArray());

        }
    }
}
