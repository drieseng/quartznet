using BenchmarkDotNet.Attributes;
using Quartz.Core;
using Quartz.Listener;
using System;
using System.Collections.Generic;

namespace Quartz.Benchmarks
{
    [MemoryDiagnoser]
    public class QuartzSchedulerBenchmarks
    {
        private QuartzScheduler _schedulerWithoutListeners;
        private QuartzScheduler _schedulerWithOneInternalListener;
        private QuartzScheduler _schedulerWithOneManagerListener;
        private QuartzScheduler _schedulerWithMultipleInternalListeners;
        private QuartzScheduler _schedulerWithMultipleManagerListeners;
        private QuartzScheduler _schedulerWithMultipleMixedListeners;
        private QuartzScheduler _schedulerWithOneMixedListener;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _schedulerWithoutListeners = new QuartzScheduler(new QuartzSchedulerResources(), TimeSpan.Zero);

            _schedulerWithOneInternalListener = new QuartzScheduler(new QuartzSchedulerResources(), TimeSpan.Zero);
            _schedulerWithOneInternalListener.AddInternalJobListener(new BroadcastJobListener("A"));

            _schedulerWithOneManagerListener = new QuartzScheduler(new QuartzSchedulerResources(), TimeSpan.Zero);
            _schedulerWithOneManagerListener.ListenerManager.AddJobListener(new BroadcastJobListener("A"));

            _schedulerWithOneMixedListener = new QuartzScheduler(new QuartzSchedulerResources(), TimeSpan.Zero);
            _schedulerWithOneMixedListener.AddInternalJobListener(new BroadcastJobListener("A"));
            _schedulerWithOneMixedListener.ListenerManager.AddJobListener(new BroadcastJobListener("B"));

            _schedulerWithMultipleInternalListeners = new QuartzScheduler(new QuartzSchedulerResources(), TimeSpan.Zero);
            _schedulerWithMultipleInternalListeners.AddInternalJobListener(new BroadcastJobListener("A"));
            _schedulerWithMultipleInternalListeners.AddInternalJobListener(new BroadcastJobListener("B"));

            _schedulerWithMultipleManagerListeners = new QuartzScheduler(new QuartzSchedulerResources(), TimeSpan.Zero);
            _schedulerWithMultipleManagerListeners.ListenerManager.AddJobListener(new BroadcastJobListener("A"));
            _schedulerWithMultipleManagerListeners.ListenerManager.AddJobListener(new BroadcastJobListener("B"));

            _schedulerWithMultipleMixedListeners = new QuartzScheduler(new QuartzSchedulerResources(), TimeSpan.Zero);
            _schedulerWithMultipleMixedListeners.AddInternalJobListener(new BroadcastJobListener("A"));
            _schedulerWithMultipleMixedListeners.AddInternalJobListener(new BroadcastJobListener("B"));
            _schedulerWithMultipleMixedListeners.ListenerManager.AddJobListener(new BroadcastJobListener("C"));
            _schedulerWithMultipleMixedListeners.ListenerManager.AddJobListener(new BroadcastJobListener("D"));
        }

        [Benchmark]
        public IReadOnlyList<IJobListener> Empty()
        {
            return _schedulerWithoutListeners.BuildJobListenerList();
        }

        [Benchmark]
        public IReadOnlyList<IJobListener> OneInternal()
        {
            return _schedulerWithOneInternalListener.BuildJobListenerList();
        }

        /*
        [Benchmark]
        public IJobListener[] OneManager()
        {
            return _schedulerWithOneManagerListener.BuildJobListenerList();
        }

        [Benchmark]
        public IJobListener[] OneMixed()
        {
            return _schedulerWithOneMixedListener.BuildJobListenerList();
        }

        [Benchmark]
        public IJobListener[] MultipleInternal()
        {
            return _schedulerWithMultipleInternalListeners.BuildJobListenerList();
        }

        [Benchmark]
        public IJobListener[] MultipleManager()
        {
            return _schedulerWithMultipleManagerListeners.BuildJobListenerList();
        }

        [Benchmark]
        public IJobListener[] MultipleMixed()
        {
            return _schedulerWithMultipleMixedListeners.BuildJobListenerList();
        }
        */
    }
}
