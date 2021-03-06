﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CdnWarmupApp.models;
using log4net;
using log4net.Config;
using Newtonsoft.Json;

namespace CdnWarmupApp.helpers
{
    public class RequestProcessor
    {
        private static readonly string AppConfigFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "configs/appconfig.json");
        private static readonly string Log4NetConfigFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "configs/log4net.config");

        private static readonly string UnprocessedFileFolderFullPath = Path.Combine(Directory.GetCurrentDirectory(), "files/unprocessedFiles");
        private static readonly string ProcessingFileFolderFullPath = Path.Combine(Directory.GetCurrentDirectory(), "files/processingFiles");
        private static readonly string ProcessedFileFolderFullPath = Path.Combine(Directory.GetCurrentDirectory(), "files/processedFiles");

        private const string LogLineTemplate = "FileName : {0} - RequestUrl : {1} - FileSize : {2} kb - RequestDownloadTotalTime : {3} second - LogTime : {4}";

        private const string UrlTemplate = "{0}/{1}";
        private const string UrlTemplateWithResize = "{0}/{1}/{2}/{3}/{4}";

        private const int DefaultThreadCount = 4;

        static AppConfigFile ConfigFile
        {
            get;
            set;
        }

        public void Run(){
            InitilizeConfiguration();

            ConsoleLog(string.Format("Task Started({0})...", DateTime.Now), true);

            try
            {
                var unprocessedFiles = Directory.GetFiles(UnprocessedFileFolderFullPath);
                foreach (var unprocessedFile in unprocessedFiles)
                {
                    FileInfo currentUnprocessedFileInfo = new FileInfo(unprocessedFile);

                    ConsoleLog("Current File: " + currentUnprocessedFileInfo.Name, true);

                    var currentUnprocessedFileFullPath = currentUnprocessedFileInfo.FullName;
                    var tempFileName = currentUnprocessedFileInfo.Name.Replace(currentUnprocessedFileInfo.Extension, string.Empty) + "-" + Guid.NewGuid().ToString("N") + currentUnprocessedFileInfo.Extension;
                    var processingFileFullPath = ProcessingFileFolderFullPath + "/" + tempFileName;

                    File.Copy(currentUnprocessedFileFullPath, processingFileFullPath);

                    var processingFileContent = File.ReadAllText(processingFileFullPath);
                    var processingFileContentAsList = JsonConvert.DeserializeObject<List<String>>(processingFileContent);

                    var paraleloptions = new ParallelOptions { MaxDegreeOfParallelism = GetThreadCount() };
                    Parallel.ForEach(processingFileContentAsList, paraleloptions, fileLine => 
                    { 
                        var requestUrls = PrepareUrl(fileLine);
                        Parallel.ForEach(requestUrls, paraleloptions, currentRequestUrl => { MakeRequest(currentRequestUrl, currentUnprocessedFileInfo.Name); });
                    });

                    var processedFileFullPath = ProcessedFileFolderFullPath + "/" + tempFileName;
                    File.Copy(processingFileFullPath, processedFileFullPath);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.GetLogger.Error(ex);
            }

            ConsoleLog(string.Format("Task Finished({0})...", DateTime.Now), true,true);
        }

        void InitilizeConfiguration()
        {
            try
            {
                var fileContent = File.ReadAllText(AppConfigFileFullPath);
                ConfigFile = JsonConvert.DeserializeObject<AppConfigFile>(fileContent);

                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo(Log4NetConfigFileFullPath));
            }
            catch (Exception ex)
            {
                LoggerHelper.GetLogger.Error(ex);

                Console.WriteLine("Initilize Config File Error:");
                Console.WriteLine("-----------------------------");
                Console.WriteLine(string.Empty);
                throw;
            }
        }

        void MakeRequest(string currentRequestUrl, string currentUnprocessedFileInfoName)
        {
            try
            {
                int fileSize;
                Stopwatch requestReswponseTimer = new Stopwatch();
                requestReswponseTimer.Start();
                using (WebClient client = new WebClient())
                {
                    var data = client.DownloadData(currentRequestUrl);
                    fileSize = data.Length / 1024;
                }
                requestReswponseTimer.Stop();
                var operationTime = requestReswponseTimer.Elapsed.TotalSeconds;

                var logMessage = string.Format(LogLineTemplate, currentUnprocessedFileInfoName, currentRequestUrl, fileSize.ToString(), operationTime.ToString(), DateTime.Now);
                LoggerHelper.GetLogger.Info(logMessage);

                ConsoleLog(logMessage);
            }
            catch (Exception ex)
            {
                LoggerHelper.GetLogger.Error(ex);
            }
        }

        List<string> PrepareUrl(string fileName)
        {
            var response = new List<string>();
            response.Add(string.Format(UrlTemplate, ConfigFile.Url, fileName));
            if (!string.IsNullOrEmpty(ConfigFile.UrlResizeSection) && ConfigFile.Dimensions != null && ConfigFile.Dimensions.Any())
            {
                foreach (var currentDimension in ConfigFile.Dimensions)
                {
                    response.Add(string.Format(UrlTemplateWithResize, ConfigFile.Url, ConfigFile.UrlResizeSection, currentDimension.Width, currentDimension.Height, fileName));
                }
            }

            return response;
        }

        int GetThreadCount()
        {
            var response = DefaultThreadCount;
            if (ConfigFile.Times != null && ConfigFile.Times.Any())
            {
                var currentThreadConfig = ConfigFile.Times.FirstOrDefault(p => p.ValidateDate());
                if (currentThreadConfig != null && currentThreadConfig.ThreadCount != 0)
                {
                    response = currentThreadConfig.ThreadCount;
                }
            }

            return response;
        }

        void ConsoleLog(string message, Boolean overrideConfigSetting = false, Boolean emptyLine = false){
            if (overrideConfigSetting || ConfigFile.ShowConsoleLog)
            {
                Console.WriteLine(message);
            }

            if (emptyLine)
            {
                Console.WriteLine(string.Empty);
            }
        }
    }
}