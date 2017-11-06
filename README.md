# CDNWarmupApp .NetCore
CDNWarmupApp .Net Core App (base on medianova cdn company) Developed on MacOSX with VS for Mac CE

# Self-Contained Deploy Scripts 
dotnet publish -f netcoreapp2.0 -r linux-x64 -c Release  
dotnet publish -f netcoreapp2.0 -r centos.7-x64 -c Release

# Technologies
.Net Core 2.0  
Log4Net  
Quartz.Net  
Newtonsoft.Json

# Config File (configs/appconfig.json)
Url: Request domain address like "https://medianova.image.net"  
UrlResizeSection: If want to request resized url for medianova "mnresize"  
ShowConsoleLog: Show request url and infos on console, like file readed, request url, file size, reqıest and download total time; this info also logged in log file  
Dimensions: width and height variation list   
Times : Setting array for active thread count  

# Sample Config File
{  
  "Url": "https://medianova.image.net",  
  "UrlResizeSection": "mnresize",  
  "ShowConsoleLog":  true,  
  "Dimensions": [  
    {  
      "Height": 300,  
      "Width": 400  
    },  
    { . 
      "Height": 350,  
      "Width": 500 . 
    }   
  ],  
  "Times": [  
    {   
      "StartTime": "00:00",  
      "EndTime": "16:59",  
      "ThreadCount": 100  
    },  
    {   
      "StartTime": "17:00",  
      "EndTime": "23:59",  
      "ThreadCount": 50  
    }  
  ]  
}  

# Process Flow
