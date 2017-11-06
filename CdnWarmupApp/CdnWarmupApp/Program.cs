using System;
using CdnWarmupApp.helpers;

namespace CdnWarmupApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CDN Warmup Console Schedular App Started....");
            Console.WriteLine("--------------------------------------------");

            Schedular.Run().GetAwaiter().GetResult();
            
            Console.ReadKey();
        }
    }
}