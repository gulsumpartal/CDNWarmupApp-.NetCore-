using System;

namespace CdnWarmupApp.helpers
{
    public class LoggerHelper
    {
        static volatile log4net.ILog instance;
        static object syncRoot = new Object();

        public static log4net.ILog GetLogger
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }

                return instance;
            }
        }
    }
}