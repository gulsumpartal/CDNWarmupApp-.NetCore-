using System;
using CdnWarmupApp.helpers;

namespace CdnWarmupApp
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("CDN Warmup Console Schedular App Started....");
            Console.WriteLine("--------------------------------------------");

            Schedular.Run().GetAwaiter().GetResult();
            
            Console.ReadKey();
        }
    }
}