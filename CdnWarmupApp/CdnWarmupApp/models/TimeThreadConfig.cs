using System;
using System.Globalization;
using CdnWarmupApp.helpers;

namespace CdnWarmupApp.models
{
    [Serializable]
    public class TimeThreadConfig
    {
        public string StartTime
        {
            get;
            set;
        }

        public string EndTime
        {
            get;
            set;
        }

        public int ThreadCount
        {
            get;
            set;
        }

        public Boolean ValidateDate ()
        {
            var response = false;

            try
            {
                var starDateTime = DateTime.ParseExact(StartTime, "HH:mm", CultureInfo.InvariantCulture);
                var endDateTime = DateTime.ParseExact(EndTime, "HH:mm", CultureInfo.InvariantCulture);

                var compareDate = new DateTime(starDateTime.Year, starDateTime.Month, starDateTime.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

                if (compareDate >= starDateTime && compareDate <= endDateTime)
                {
                    response = true;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.GetLogger.Error(ex);
            }

            return response;
        }
    }
}
