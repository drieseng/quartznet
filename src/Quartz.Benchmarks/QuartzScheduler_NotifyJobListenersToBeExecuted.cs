using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Quartz.Core;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Simpl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Benchmarks
{
    [MemoryDiagnoser]
    //[NativeMemoryProfiler]
    public class QuartzScheduler_NotifyJobListenersToBeExecuted
    {
        private IScheduler _quartzScheduler;
        private ManualResetEvent _waitHandle;
        private JobDetailImpl _jobDetail;
        private JobDetailImpl _singularJobDetail;

        private const int _iterationCount = 1000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            DirectSchedulerFactory.Instance.CreateScheduler(new DefaultThreadPool(), new RAMJobStore());
            _quartzScheduler = DirectSchedulerFactory.Instance.GetScheduler(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            _quartzScheduler.JobFactory = new SimpleJobFactory();

            _waitHandle = new ManualResetEvent(false);

            _jobDetail = new JobDetailImpl("xx", typeof(Job));
            _jobDetail.JobDataMap.Add("waitHandle", _waitHandle);

            _singularJobDetail = new JobDetailImpl("singular", typeof(SingularJob));
            _singularJobDetail.JobDataMap.Add("waitHandle", _waitHandle);

            _quartzScheduler.Start();
        }

        //[Benchmark(OperationsPerInvoke = _iterationCount)]
        public void XX()
        {
            Job.Reset();
            _waitHandle.Reset();
            _quartzScheduler.ScheduleJob(_jobDetail, new SimpleTriggerImpl("xx", _iterationCount, TimeSpan.FromTicks(1)));
            _waitHandle.WaitOne();
            _quartzScheduler.DeleteJob(_jobDetail.Key);
        }

        [Benchmark]
        public void Singular()
        {
            SingularJob.Reset();
            _waitHandle.Reset();
            _quartzScheduler.ScheduleJob(_singularJobDetail, new SimpleTriggerImpl(_singularJobDetail.Key.Name, 0, TimeSpan.FromTicks(1)));
            _waitHandle.WaitOne();
            _quartzScheduler.DeleteJob(_singularJobDetail.Key);
        }

        [DisallowConcurrentExecution]
        private class Job : IJob
        {
            private static long _counter = 0;

            public static void Reset()
            {
                _counter = 0;
            }

            public Task Execute(IJobExecutionContext context)
            {
                var value = Interlocked.Increment(ref _counter);
                if (value == _iterationCount)
                {
                    var waitHandle = (ManualResetEvent) context.JobDetail.JobDataMap.Get("waitHandle");
                    waitHandle.Set();
                }

                return Task.CompletedTask;
            }
        }

        [DisallowConcurrentExecution]
        private class SingularJob : IJob
        {
            private static long _counter = 0;

            public static void Reset()
            {
                _counter = 0;
            }

            public Task Execute(IJobExecutionContext context)
            {
                var value = Interlocked.Increment(ref _counter);
                //Console.WriteLine("EXECUTE: " + value);
                if (value == 1)
                {
                    var waitHandle = (ManualResetEvent)context.JobDetail.JobDataMap.Get("waitHandle");
                    waitHandle.Set();
                }

                return Task.CompletedTask;
            }
        }
    }
}
