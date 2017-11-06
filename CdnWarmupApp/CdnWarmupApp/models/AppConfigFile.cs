using System;
using System.Collections.Generic;

namespace CdnWarmupApp.models
{
    [Serializable]
    public class AppConfigFile
    {
        public string Url
        {
            get;
            set;
        }

        public string UrlResizeSection
        {
            get;
            set;
        }

        public bool ShowConsoleLog
        {
            get;
            set;
        }

        public List<Dimension> Dimensions
        {
            get;
            set;
        }

        public List<TimeThreadConfig> Times
        {
            get;
            set;
        }
    }
}