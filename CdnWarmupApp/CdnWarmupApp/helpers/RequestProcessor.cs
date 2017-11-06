using System;
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
        static string appConfigFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "configs/appconfig.json");
        static string log4netConfigFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "configs/log4net.config");

        static string unprocessedFileFolderFullPath = Path.Combine(Directory.GetCurrentDirectory(), "files/unprocessedFiles");
        static string processingFileFolderFullPath = Path.Combine(Directory.GetCurrentDirectory(), "files/processingFiles");
        static string processedFileFolderFullPath = Path.Combine(Directory.GetCurrentDirectory(), "files/processedFiles");

        static string logLineTemplate = "FileName : {0} - RequestUrl : {1} - FileSize : {2} kb - RequestDownloadTotalTime : {3} second - LogTime : {4}";

        static string urlTemplate = "{0}/{1}";
        static string urlTemplateWithResize = "{0}/{1}/{2}/{3}/{4}";

        static int defaultThreadCount = 4;

        static AppConfigFile configFile
        {
            get;
            set;
        }

        public void Run(){
            InitilizeConfiguration();

            ConsoleLog(string.Format("Task Started({0})...", DateTime.Now), true);

            try
            {
                var unprocessedFiles = Directory.GetFiles(unprocessedFileFolderFullPath);
                foreach (var unprocessedFile in unprocessedFiles)
                {
                    FileInfo currentUnprocessedFileInfo = new FileInfo(unprocessedFile);

                    ConsoleLog("Current File: " + currentUnprocessedFileInfo.Name, true);

                    var currentUnprocessedFileFullPath = currentUnprocessedFileInfo.FullName;
                    var tempFileName = currentUnprocessedFileInfo.Name.Replace(currentUnprocessedFileInfo.Extension, string.Empty) + "-" + Guid.NewGuid().ToString("N") + currentUnprocessedFileInfo.Extension;
                    var processingFileFullPath = processingFileFolderFullPath + "/" + tempFileName;

                    File.Copy(currentUnprocessedFileFullPath, processingFileFullPath);

                    var processingFileContent = File.ReadAllText(processingFileFullPath);
                    var processingFileContentAsList = JsonConvert.DeserializeObject<List<String>>(processingFileContent);

                    var paraleloptions = new ParallelOptions { MaxDegreeOfParallelism = GetThreadCount() };
                    Parallel.ForEach(processingFileContentAsList, paraleloptions, fileLine => 
                    { 
                        var requestUrls = PrepareUrl(fileLine);
                        Parallel.ForEach(requestUrls, paraleloptions, currentRequestUrl => { MakeRequest(currentRequestUrl, currentUnprocessedFileInfo.Name); });
                    });

                    var processedFileFullPath = processedFileFolderFullPath + "/" + tempFileName;
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
                var fileContent = File.ReadAllText(appConfigFileFullPath);
                configFile = JsonConvert.DeserializeObject<AppConfigFile>(fileContent);

                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo(log4netConfigFileFullPath));
            }
            catch (Exception ex)
            {
                LoggerHelper.GetLogger.Error(ex);

                Console.WriteLine("Initilize Config File Error:");
                Console.WriteLine("-----------------------------");
                Console.WriteLine(string.Empty);
                throw ex;
            }
        }

        void MakeRequest(string currentRequestUrl, string currentUnprocessedFileInfoName)
        {
            try
            {
                var fileSize = 0;
                Stopwatch requestReswponseTimer = new Stopwatch();
                requestReswponseTimer.Start();
                using (WebClient client = new WebClient())
                {
                    var data = client.DownloadData(currentRequestUrl);
                    fileSize = data.Length / 1024;
                }
                requestReswponseTimer.Stop();
                var operationTime = requestReswponseTimer.Elapsed.TotalSeconds;

                var logMessage = string.Format(logLineTemplate, currentUnprocessedFileInfoName, currentRequestUrl, fileSize.ToString(), operationTime.ToString(), DateTime.Now);
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
            response.Add(string.Format(urlTemplate, configFile.Url, fileName));
            if (!string.IsNullOrEmpty(configFile.UrlResizeSection) && configFile.Dimensions != null && configFile.Dimensions.Any())
            {
                foreach (var currentDimension in configFile.Dimensions)
                {
                    response.Add(string.Format(urlTemplateWithResize, configFile.Url, configFile.UrlResizeSection, currentDimension.Width, currentDimension.Height, fileName));
                }
            }

            return response;
        }

        int GetThreadCount()
        {
            var response = defaultThreadCount;
            if (configFile.Times != null && configFile.Times.Any())
            {
                var currentThreadConfig = configFile.Times.FirstOrDefault(p => p.ValidateDate());
                if (currentThreadConfig != null && currentThreadConfig.ThreadCount != 0)
                {
                    response = currentThreadConfig.ThreadCount;
                }
            }

            return response;
        }

        void ConsoleLog(string message, Boolean overrideConfigSetting = false, Boolean emptyLine = false){
            if (overrideConfigSetting || configFile.ShowConsoleLog)
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