using System;

namespace CdnWarmupApp.helpers
{
    public class LoggerHelper
    {
        static volatile log4net.ILog _instance;
        static readonly object SyncRoot = new Object();

        public static log4net.ILog GetLogger
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }

                return _instance;
            }
        }
    }
}