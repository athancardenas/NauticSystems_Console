using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    public class ReplicationFile
    {
        public Ship ParentShip { get; set; }
        public ReplicationFolder ParentFolder { get; set; }

        public string FullPath { get; set; }
        public string Filename { get; set; }
        public string RelocationFullPath { get; set; }
        public bool IsRelocated { get; set; }

        public ReplicationFile(ReplicationFolder parentFolder, string shipReplicationFilePath)
        {
            this.ParentShip = parentFolder.ParentShip;
            this.ParentFolder = parentFolder;

            FullPath = shipReplicationFilePath;
            Filename = Path.GetFileName(FullPath);

            RelocationFullPath = parentFolder.RelocationFullPath + Filename;

            IsRelocated = false;
        }

        public static List<ReplicationFile> LoadShipReplicationFiles(ReplicationFolder parentFolder)
        {
            List<ReplicationFile> shipReplicationFiles = new List<ReplicationFile>();

            List<Task> loadShipReplicationFilesTask = new List<Task>();
            foreach (string shipReplicationFilePath in Directory.GetFiles(parentFolder.FullPath))
            {
                loadShipReplicationFilesTask.Add
                (
                    Task.Factory.StartNew
                    (
                        () =>
                        shipReplicationFiles.Add( new ReplicationFile(parentFolder, shipReplicationFilePath) )
                    )    
                );
            }
            Task.WaitAll(loadShipReplicationFilesTask.ToArray());

            return shipReplicationFiles;
        }

        public void RelocateFile()
        {
            if (File.Exists(RelocationFullPath))
            {
                File.Delete(RelocationFullPath);
            }
        }

    }
}
