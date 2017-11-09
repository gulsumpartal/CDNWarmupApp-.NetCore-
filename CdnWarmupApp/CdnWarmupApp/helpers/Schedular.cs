using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace CdnWarmupApp.helpers
{
    public class Schedular
    {
        private const int TaskIntervalSecond = 10;
        private const string JobName = "CdnWarmupJob";
        private const string GroupName = "CdnWarmupGroup";
        private const string TriggerName = "CdnWarmupTrigger";

        public static async Task Run()
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler sched = factory.GetScheduler().Result;
                sched.Start().Wait();

                IJobDetail job = JobBuilder.Create<CdnWarmupJob>().WithIdentity(JobName, GroupName).Build();

                ITrigger trigger = TriggerBuilder.Create().WithIdentity(TriggerName, GroupName).StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(TaskIntervalSecond).RepeatForever()).Build();

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
