using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace move2sftp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var localPath = ConfigurationManager.AppSettings["localPath"];
            var destPath = ConfigurationManager.AppSettings["destPath"];
            var thenMoveTo = ConfigurationManager.AppSettings["thenMoveTo"];
            var thenDelete = ConfigurationManager.AppSettings["thenDelete"];
            var sftpServer = ConfigurationManager.AppSettings["sftpServer"];
            var sftpUsername = ConfigurationManager.AppSettings["sftpUsername"];
            var sftpPrivateKeyPath = ConfigurationManager.AppSettings["sftpPrivateKeyPath"];
            var sftpPrivateKeyPassphrase = ConfigurationManager.AppSettings["sftpPrivateKeyPassphrase"];

            var authenticationMethod =
              new PrivateKeyAuthenticationMethod(sftpUsername,
                new PrivateKeyFile(sftpPrivateKeyPath, sftpPrivateKeyPassphrase));

            var connectionInfo = new ConnectionInfo(sftpServer, sftpUsername, authenticationMethod);

            var dir = new DirectoryInfo(localPath);

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                foreach (var file in dir.GetFiles())
                {
                    //if (IsFileLocked(file))
                    //{
                    //    Console.WriteLine($"Locked file: {file.FullName}");
                    //    continue;
                    //}

                    try
                    {
                        Console.WriteLine($"Uploading: {file.FullName}");
                        using (var reader = File.OpenRead(file.FullName))
                        {
                            client.UploadFile(reader, destPath + "/" + file.Name);
                            reader.Close();
                        }
                        if (thenDelete == "1")
                        {
                            file.Delete();
                        }
                        else if (!string.IsNullOrEmpty(thenMoveTo))
                        {
                            file.MoveTo(Path.Combine(thenMoveTo, file.Name));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                client.Disconnect();
                // client.Dispose();
            }
        }

        static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }
    }
}
