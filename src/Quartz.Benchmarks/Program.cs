using BenchmarkDotNet.Running;
using System;

namespace Quartz.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
            var job = new QuartzScheduler_NotifyJobListenersToBeExecuted();
            job.GlobalSetup();
            job.XX();
#else
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
        }
    }
}
