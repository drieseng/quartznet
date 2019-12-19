using BenchmarkDotNet.Attributes;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Simpl;
using System;
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
        private JobDetailImpl _parallelJobDetail;
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

            _parallelJobDetail = new JobDetailImpl("parallel", typeof(ParallelJob));
            _parallelJobDetail.JobDataMap.Add("waitHandle", _waitHandle);

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

        [Benchmark]
        public void Parallel()
        {
            DirectSchedulerFactory.Instance.CreateScheduler("Parallel", "Parallel", new DefaultThreadPool(), new RAMJobStore());
            var quartzScheduler = DirectSchedulerFactory.Instance.GetScheduler("Parallel", CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            quartzScheduler.JobFactory = new SimpleJobFactory();

            ParallelJob.Reset();
            _waitHandle.Reset();

            var parallelCount = 10;
            var jobsPerThread = (int) (ParallelJob.IterationCount / parallelCount);
            
            for (var i = 0; i < parallelCount; i++)
            {
                quartzScheduler.ScheduleJob(CreateParallelJob(_waitHandle), new SimpleTriggerImpl("P" + i, jobsPerThread, TimeSpan.FromTicks(1)));
            }

            quartzScheduler.Start();

            _waitHandle.WaitOne();

            quartzScheduler.Shutdown(true).GetAwaiter().GetResult();
        }

        private static IJobDetail CreateParallelJob(ManualResetEvent waitHandle)
        {
            var job = new JobDetailImpl(Guid.NewGuid().ToString(), typeof(ParallelJob));
            job.JobDataMap.Add("waitHandle", waitHandle);
            return job;
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
        private class ParallelJob : IJob
        {
            public const long IterationCount = 100_000;
            private static long _counter = 0;

            public static void Reset()
            {
                _counter = 0;
            }

            public Task Execute(IJobExecutionContext context)
            {
                var value = Interlocked.Increment(ref _counter);
                if (value == IterationCount)
                {
                    var waitHandle = (ManualResetEvent)context.JobDetail.JobDataMap.Get("waitHandle");
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
