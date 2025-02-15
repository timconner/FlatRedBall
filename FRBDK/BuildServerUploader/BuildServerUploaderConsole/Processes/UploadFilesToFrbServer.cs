﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using FlatRedBall.IO;
using BuildServerUploaderConsole.Sftp;

namespace BuildServerUploaderConsole.Processes
{
    #region enums

    public enum UploadType
    {
        Entire,
        EngineAndTemplatesOnly,
        FrbdkOnly,
        GumOnly,
    }

    #endregion

    public class UploadFilesToFrbServer : ProcessStep
    {
        #region Fields/Properties

        static string absolutePath = @"C:\FlatRedBallProjects\UploadInfo.txt";


        UploadType uploadType;

        static string cachedPassword = null;
        public static string Password
        {
            get
            {
                if (cachedPassword == null)
                {
                    var doesFileExist = System.IO.File.Exists(absolutePath);

                    if (doesFileExist)
                    {
                        ReadUsernameAndPassword();
                    }
                    else
                    {
                        throw new Exception($"Could not find the file {absolutePath}. " + 
                            "This file is needed and should contain the password to the FRB server.");
                    }
                }

                return cachedPassword;
            }
        }

        static string cachedUsername = null;
        public static string Username
        {
            get
            {
                if(cachedUsername == null)
                {
                    var doesFileExist = System.IO.File.Exists(absolutePath);

                    if (doesFileExist)
                    {
                        ReadUsernameAndPassword();
                    }
                    else
                    {
                        throw new Exception($"Could not find the file {absolutePath}. " +
                            "This file is needed and should contain the password to the FRB server.");
                    }
                }

                return cachedUsername;
            }
        }

        private static void ReadUsernameAndPassword()
        {
            string fileContents = System.IO.File.ReadAllText(absolutePath);
            var split = fileContents.Split(' ');

            cachedUsername = split[0];
            cachedPassword = split[1];
        }

        private readonly DateTime _deleteBeforeDate;
        private string _ftpFolder =
                "files.flatredball.com/content/FrbXnaTemplates/";
        private readonly string _backupFolder =
                "files.flatredball.com/content/FrbXnaTemplates/";
        //private readonly string _ftpCopyToFolder = null;

        private readonly string gumFolder =
                "files.flatredball.com/content/Tools/Gum/";


        //private const string host = "sftp://flatredball.com/";
        private const string host = "files.flatredball.com";
        private const string _backupFile =
            "files.flatredball.com/content/FrbXnaTemplates/BackupFolders.txt";

        #endregion

        public UploadFilesToFrbServer(IResults results, UploadType uploadType, string username, string password)
            : base(
            @"Upload files to daily build location", results)
        {
            cachedUsername = username; 
            cachedPassword = password;
            //string fileName = "build_" + DateTime.Now.ToString("yyyy") + "_" + DateTime.Now.ToString("MM") + "_" +
            //    DateTime.Now.ToString("dd") + "_";

            this.uploadType = uploadType;

            _deleteBeforeDate = DateTime.Now.AddDays(-7);
            _ftpFolder += "DailyBuild/";
            _backupFolder += "DailyBuild/";
            //_ftpCopyToFolder = "files.flatredball.com/content/FrbXnaTemplates/DailyBuild/";



        }


        public override void ExecuteStep()
        {
            Results.WriteMessage($"Uploading {uploadType}");
            if(uploadType == UploadType.EngineAndTemplatesOnly)
            {
                UploadEngineFiles();
                UploadTemplateFiles(_ftpFolder, Results);
            }
            else if(uploadType == UploadType.Entire)
            {
                UploadGumFiles();
                UploadFrbdkFiles();
                UploadEngineFiles();
                UploadTemplateFiles(_ftpFolder, Results);
            }
            else if(uploadType == UploadType.FrbdkOnly)
            {
                
                UploadFrbdkFiles();
            }
            else if(uploadType == UploadType.GumOnly)
            {
                UploadGumFiles();
            }
        }

        private void UploadGumFiles()
        {
            string localFile = FileManager.GetDirectory(DirectoryHelper.GumBuildDirectory) + "Gum.zip";
            if(!System.IO.File.Exists(localFile))
            {
                throw new Exception($"{localFile} file doesn't exist for upload");
            }
            string targetFile = gumFolder + FileManager.RemovePath(localFile);
            Results.WriteMessage("Uploading " + localFile + " to " + targetFile);
            SftpManager.UploadFile(
                localFile, host, targetFile, Username, Password, PrintOutput);

            Results.WriteMessage(localFile + " uploaded to " + targetFile);
        }

        DateTime lastWrite = DateTime.Now;
        void PrintOutput(ulong amount)
        {
            if(DateTime.Now - lastWrite > TimeSpan.FromSeconds(1))
            {
                Results.WriteMessage("Uploaded " + (amount / 1024).ToString("N0") + " kbytes");
                lastWrite = DateTime.Now;
            }
        }

        private void UploadFrbdkFiles()
        {
            string localFile = ZipFrbdk.DestinationFile;

            string targetFile = _ftpFolder + FileManager.RemovePath(localFile);

            SftpManager.UploadFile(
                localFile, host, targetFile, Username, Password, PrintOutput);

            Results.WriteMessage(localFile + " uploaded to " + targetFile);


        }

        private void UploadEngineFiles()
        {
            List<CopyInformation> engineFiles = CopyBuiltEnginesToReleaseFolder.CopyInformationList;

            using (var client = SftpManager.GetClient(host, Username, Password))
            {
                client.Connect();
                for (int i = 0; i < engineFiles.Count; i++)
                {
                    string localFile = engineFiles[i].DestinationFile;

                    string engineName = FileManager.RemovePath(FileManager.GetDirectory(localFile));
                    string debugOrRelease = null;
                    if (engineName.ToLower() == "debug/" || engineName.ToLower() == "release/")
                    {
                        debugOrRelease = engineName;
                        engineName = FileManager.RemovePath(FileManager.GetDirectory(FileManager.GetDirectory(localFile)));
                    }


                    string fileName = engineName + debugOrRelease +
                        FileManager.RemovePath(localFile);
                    string destination = _ftpFolder + "SingleDlls/" + fileName;

                    SftpManager.UploadFileWithOpenConnection(
                        localFile, destination, client, PrintOutput);


                    Results.WriteMessage(engineFiles[i].DestinationFile + " uploaded to " + destination);
                }
                client.Disconnect();
            }
        }

        private void UploadTemplateFiles(string _ftpFolder, IResults Results)
        {
            string templateDirectory = DirectoryHelper.ReleaseDirectory + @"ZippedTemplates/";


            var zippedTemplateFiles = Directory.GetFiles(templateDirectory, "*.zip").ToArray();

            Results.WriteMessage($"Found {zippedTemplateFiles.Length} files to upload");

            using (var client = SftpManager.GetClient(host, Username, Password))
            {
                client.Connect();


                foreach (var file in zippedTemplateFiles)
                {
                    var fileName = FileManager.RemovePath(file);

                    string localFile = templateDirectory + fileName;
                    string destination = _ftpFolder + "ZippedTemplates/" + fileName;
                    SftpManager.UploadFileWithOpenConnection(
                        localFile, destination, client, PrintOutput);


                    Results.WriteMessage(file + " uploaded to " + destination);
                }

                client.Disconnect();
            }
        }
    }
}
