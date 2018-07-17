using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    public class Ship
    {
        public string IMO { get; set; }
        public string InboundFolderPath { get; set; }
        public string RelocationFolder { get; set; }
        public string ShipInboundFolderPath { get; set; }
        public string RelocationFolderPath { get; set; }
        public bool IsShipInboundFolderExists { get; set; }
        public bool HasReplicationFolderForAcknowledgement { get; set; }
        
        public List<ReplicationFolder> ReplicationFolders { get; set; }

        public Ship(string imo, string inboundFolderPath, string relocationFolder)
        {
            IMO = imo;
            InboundFolderPath = inboundFolderPath;
            RelocationFolder = relocationFolder;

            ShipInboundFolderPath = InboundFolderPath + IMO + "\\";

            if (Directory.Exists(ShipInboundFolderPath))
            {
                IsShipInboundFolderExists = true;                
                RelocationFolderPath = ShipInboundFolderPath + RelocationFolder + "\\";
                CreateDirectories(new string[] { RelocationFolderPath });
            }

            HasReplicationFolderForAcknowledgement = false;
        }

        public static List<Ship> InitializeShips(int[] imos, string inboundFolderPath, string relocationFolder)
        {
            List<Ship> InitializedShips = new List<Ship>();

            List<Task> initializeShipsTask = new List<Task>();
            foreach (int imo in imos)
            {
                initializeShipsTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        {
                            if (Directory.Exists(inboundFolderPath + imo))
                            {
                                InitializedShips.Add(new Ship(imo.ToString(), inboundFolderPath, relocationFolder));
                            }
                        }
                    )    
                ); 
            }
            Task.WaitAll(initializeShipsTask.ToArray());

            return InitializedShips;
        }
       

        public static void RelocateReplicationFiles(List<Ship> ships, string jsonFileName)
        {
            List<ReplicationFolder> shipReplicationFolders = _GetAllShipReplicationFolders(ships);
            List<ReplicationFile> shipReplicationFiles = _GetAllShipReplicationFiles(shipReplicationFolders);

            _CopyReplicationFilesToRelocation(shipReplicationFiles, jsonFileName);

            _DeleteReplicationFoldersDefaultLocation(shipReplicationFolders);
        }

        private static void _DeleteReplicationFoldersDefaultLocation(List<ReplicationFolder> shipReplicationFolders)
        {
            List<Task> deleteReplicationFoldersDefaultLocationTask = new List<Task>();
            foreach (ReplicationFolder replicationfolder in shipReplicationFolders)
            {
                deleteReplicationFoldersDefaultLocationTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        {
                            if (replicationfolder.IsDefaultLocationForDeletion)
                            {
                                DeleteDirectory(replicationfolder.FullPath);
                                replicationfolder.IsRelocated = true;
                            }
                        }
                    )
                );
            }
            Task.WaitAll(deleteReplicationFoldersDefaultLocationTask.ToArray());
        }

        private static void _CopyReplicationFilesToRelocation(List<ReplicationFile> shipReplicationFiles, string jsonFileName)
        {
            List<Task> copyReplicationFilesToRelocationTask = new List<Task>();
            foreach (ReplicationFile replicationFile in shipReplicationFiles)
            {
                copyReplicationFilesToRelocationTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        {
                            File.Copy(replicationFile.FullPath, replicationFile.RelocationFullPath);

                            replicationFile.IsRelocated = true;
                            if (jsonFileName.Equals(replicationFile.Filename))
                            {
                                replicationFile.ParentFolder.JSONFileFullPath = replicationFile.FullPath;
                                replicationFile.ParentFolder.IsForShipReplication = true;
                                replicationFile.ParentShip.HasReplicationFolderForAcknowledgement = true;
                            }

                            replicationFile.ParentFolder.IsDefaultLocationForDeletion = true;
                        }    
                    )
                );
                
            }
            Task.WaitAll(copyReplicationFilesToRelocationTask.ToArray());
        }

        private static List<ReplicationFile> _GetAllShipReplicationFiles(List<ReplicationFolder> shipReplicationFolders)
        {
            List<ReplicationFile> shipReplicationFiles = new List<ReplicationFile>(10000000);

            List<Task> getAllShipRepicationFilesTask = new List<Task>();
            foreach (ReplicationFolder replicationfolder in shipReplicationFolders)
            {
                getAllShipRepicationFilesTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        {
                            if (!Directory.Exists(replicationfolder.RelocationFullPath))
                            {
                                Directory.CreateDirectory(replicationfolder.RelocationFullPath);
                            }

                            shipReplicationFiles.AddRange(replicationfolder.ReplicationFiles);
                        }
                    )
                );
            }
            Task.WaitAll(getAllShipRepicationFilesTask.ToArray());

            return shipReplicationFiles;
        }

        private static void CreateDirectories(string[] directories)
        {
            foreach(string directoryPath in directories)
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
        }

        public static void DeleteDirectory(string targetDirectory)
        {
            string[] files = Directory.GetFiles(targetDirectory);
            string[] directories = Directory.GetDirectories(targetDirectory);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string directory in directories)
            {
                DeleteDirectory(directory);
            }

            //System.IO.Directory.Delete(target_dir, false);

            try
            {
                Directory.Delete(targetDirectory, true);
            }
            catch (IOException)
            {
                Directory.Delete(targetDirectory, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(targetDirectory, true);
            }

        }

        private static List<ReplicationFolder> _GetAllShipReplicationFolders(List<Ship> ships)
        {
            List<ReplicationFolder> shipReplicationFolders = new List<ReplicationFolder>();

            List<Task> listReplicationFoldersTask = new List<Task>();
            foreach (Ship ship in ships)
            {
                listReplicationFoldersTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        shipReplicationFolders.AddRange(ship.ReplicationFolders)
                    )
                );
            }
            Task.WaitAll(listReplicationFoldersTask.ToArray());

            return shipReplicationFolders;
        }
    }
}
