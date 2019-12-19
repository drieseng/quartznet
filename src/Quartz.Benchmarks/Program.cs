using BenchmarkDotNet.Running;
using System;
using System.Diagnostics;

namespace Quartz.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.LogManager.GetLogger("xx");
            //.Warn("FUICK");

#if true
            var job = new QuartzScheduler_NotifyJobListenersToBeExecuted();
            job.GlobalSetup();

            Stopwatch sw = Stopwatch.StartNew();

            /*
            for (var i = 0; i < 500; i++)
            {
                job.XX();
            }
            */
            job.Parallel();

            Console.WriteLine(sw.ElapsedMilliseconds);
#else
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
        }
    }
}
