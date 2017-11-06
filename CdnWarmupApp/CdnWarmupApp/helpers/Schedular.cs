using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace CdnWarmupApp.helpers
{
    public class Schedular
    {
        static int taskIntervalSecond = 10;
        static string jobName = "CdnWarmupJob";
        static string groupName = "CdnWarmupGroup";
        static string triggerName = "CdnWarmupTrigger";

        public static async Task Run()
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler sched = factory.GetScheduler().Result;
                sched.Start().Wait();

                IJobDetail job = JobBuilder.Create<CdnWarmupJob>().WithIdentity(jobName, groupName).Build();

                ITrigger trigger = TriggerBuilder.Create().WithIdentity(triggerName, groupName).StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(taskIntervalSecond).RepeatForever()).Build();

                await sched.ScheduleJob(job, trigger);
            }
            catch (SchedulerException se)
            {
                LoggerHelper.GetLogger.Error(se);
            }
        }

        [DisallowConcurrentExecution]
        public class CdnWarmupJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                RequestProcessor processor = new RequestProcessor();
                processor.Run();
                return null;
            }
        }
    }
}
