using BenchmarkDotNet.Attributes;
using Quartz.Impl;
using System.Threading.Tasks;

namespace Quartz.Benchmarks
{
    [MemoryDiagnoser]
    public class JobDetailImplBenchmarks
    {
        private JobDetailImpl _jobDetail;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _jobDetail = new JobDetailImpl("name", typeof(TestJob));
            _jobDetail.JobDataMap.Add("Test", 7);
        }

        [Benchmark]
        public IJobDetail CloneNew()
        {
            //return _jobDetail.CloneNoReflection();
            return null;
        }

        [Benchmark]
        public IJobDetail CloneOld()
        {
            return _jobDetail.Clone();
        }

        private class TestJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
