using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            NauticSystemsReplicate nsr = new NauticSystemsReplicate();
            nsr.inLog = string.Empty;
            nsr.AckFromShipReplication();

            /*
            int[] ActiveIMO = nsr.getAllActiveVessels();
            //string NSFolder = @"\\10.0.1.234\c$\Nautic\Resources\Services\nsRep\";
            //string NM1Folder = @"\\10.0.1.231\c$\NauticMaster\nmReplication\";
            //string NM2Folder = @"\\10.0.1.135\c$\NauticMaster\nmReplication\";
            //string NM3Folder = @"\\10.0.1.136\c$\NauticMaster\nmReplication\";

            string NSFolder = @"H:\";
            string NM1Folder = @"I:\";
            string NM2Folder = @"J:\";
            string NM3Folder = @"K:\";

            for (int i = 0; i < ActiveIMO.Length; i++)
            {
                switch (ActiveIMO[i])
                {
                    case 9261451:

                        foreach (string CurrentNSOutFolder in System.IO.Directory.GetDirectories(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*_#*"))
                        {
                            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(CurrentNSOutFolder);

                            if (CurrentNSOutFolder.Split('\\').Last().Substring(0, 5) != "DMSSR")
                            {
                                if (!System.IO.Directory.Exists(NM3Folder + "InboundQueue"))
                                    System.IO.Directory.CreateDirectory(NM3Folder + "InboundQueue");

                                nsr.NauticMoveDirectory(CurrentNSOutFolder, NM3Folder + @"InboundQueue\" + CurrentNSOutFolder.Split('\\').Last(), true);

                            }

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.aco", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM3Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM3Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM3Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rho", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM3Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM3Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM3Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rmo", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM3Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM3Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM3Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rms", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM3Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM3Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM3Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out", "*.trg", System.IO.SearchOption.TopDirectoryOnly))
                            System.IO.File.Delete(CurrentNSOutFile);

                        foreach (string CurrentNMOutFolder in System.IO.Directory.GetDirectories(NM3Folder + @"OutboundQueue", "*_"))
                        {
                            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(CurrentNMOutFolder);

                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            nsr.NauticMoveDirectory(CurrentNMOutFolder, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFolder.Split('\\').Last(), true);

                        }

                        //foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM3Folder + @"OutboundQueue", "*.ack|*.rma"))
                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM3Folder + @"OutboundQueue", "*.ack", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM3Folder + @"OutboundQueue", "*.rhs", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM3Folder + @"OutboundQueue", "*.rmm", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM3Folder + @"OutboundQueue", "*.rma", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM3Folder + @"OutboundQueue", "*.trg", System.IO.SearchOption.TopDirectoryOnly))
                            System.IO.File.Delete(CurrentNMOutFile);

                        break;
                    case 9697832:

                        foreach (string CurrentNSOutFolder in System.IO.Directory.GetDirectories(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*_#*"))
                        {
                            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(CurrentNSOutFolder);

                            if (CurrentNSOutFolder.Split('\\').Last().Substring(0, 5) != "DMSSR")
                            {
                                if (!System.IO.Directory.Exists(NM2Folder + "InboundQueue"))
                                    System.IO.Directory.CreateDirectory(NM2Folder + "InboundQueue");

                                nsr.NauticMoveDirectory(CurrentNSOutFolder, NM2Folder + @"InboundQueue\" + CurrentNSOutFolder.Split('\\').Last(), true);

                            }

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.aco", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM2Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM2Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM2Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rho", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM2Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM2Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM2Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rmo", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM2Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM2Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM2Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rms", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM2Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM2Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM2Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out", "*.trg", System.IO.SearchOption.TopDirectoryOnly))
                            System.IO.File.Delete(CurrentNSOutFile);

                        foreach (string CurrentNMOutFolder in System.IO.Directory.GetDirectories(NM2Folder + @"OutboundQueue", "*_"))
                        {
                            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(CurrentNMOutFolder);

                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            nsr.NauticMoveDirectory(CurrentNMOutFolder, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFolder.Split('\\').Last(), true);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM2Folder + @"OutboundQueue", "*.ack", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM2Folder + @"OutboundQueue", "*.rhs", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM2Folder + @"OutboundQueue", "*.rmm", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM2Folder + @"OutboundQueue", "*.rma", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM2Folder + @"OutboundQueue", "*.trg", System.IO.SearchOption.TopDirectoryOnly))
                            System.IO.File.Delete(CurrentNMOutFile);

                        break;
                    case 9732589:

                        foreach (string CurrentNSOutFolder in System.IO.Directory.GetDirectories(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*_#*"))
                        {
                            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(CurrentNSOutFolder);

                            if (CurrentNSOutFolder.Split('\\').Last().Substring(0, 5) != "DMSSR")
                            {
                                if (!System.IO.Directory.Exists(NM1Folder + "InboundQueue"))
                                    System.IO.Directory.CreateDirectory(NM1Folder + "InboundQueue");

                                nsr.NauticMoveDirectory(CurrentNSOutFolder, NM1Folder + @"InboundQueue\" + CurrentNSOutFolder.Split('\\').Last(), true);

                            }

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.aco", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM1Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM1Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM1Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rho", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM1Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM1Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM1Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rmo", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM1Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM1Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM1Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out\" + ActiveIMO[i].ToString(), "*.rms", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NM1Folder + "InboundQueue"))
                                System.IO.Directory.CreateDirectory(NM1Folder + "InboundQueue");

                            System.IO.File.Copy(CurrentNSOutFile, NM1Folder + "InboundQueue" + @"\" + CurrentNSOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNSOutFile);

                        }

                        foreach (string CurrentNSOutFile in System.IO.Directory.GetFiles(NSFolder + @"Out", "*.trg"))
                            System.IO.File.Delete(CurrentNSOutFile);

                        foreach (string CurrentNMOutFolder in System.IO.Directory.GetDirectories(NM1Folder + @"OutboundQueue", "*_"))
                        {
                            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(CurrentNMOutFolder);

                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            nsr.NauticMoveDirectory(CurrentNMOutFolder, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFolder.Split('\\').Last(), true);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM1Folder + @"OutboundQueue", "*.ack", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM1Folder + @"OutboundQueue", "*.rhs", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM1Folder + @"OutboundQueue", "*.rmm", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM1Folder + @"OutboundQueue", "*.rma", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            if (!System.IO.Directory.Exists(NSFolder + @"In\" + ActiveIMO[i].ToString()))
                                System.IO.Directory.CreateDirectory(NSFolder + @"In\" + ActiveIMO[i].ToString());

                            System.IO.File.Copy(CurrentNMOutFile, NSFolder + @"In\" + ActiveIMO[i].ToString() + @"\" + CurrentNMOutFile.Split('\\').Last());

                            System.IO.File.Delete(CurrentNMOutFile);

                        }

                        foreach (string CurrentNMOutFile in System.IO.Directory.GetFiles(NM1Folder + @"OutboundQueue", "*.trg", System.IO.SearchOption.TopDirectoryOnly))
                            System.IO.File.Delete(CurrentNMOutFile);

                        break;
                    default:
                        break;
                }


            }
            */
        }

    }

}
