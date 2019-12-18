using BenchmarkDotNet.Attributes;
using Quartz.Core;
using Quartz.Listener;
using System.Collections.Generic;
using System.Linq;

namespace Quartz.Benchmarks
{
    [MemoryDiagnoser]
    public class ListenerManagerImpl_GetJobListeners
    {
        private ListenerManagerImpl _emptyistenerManager;
        private ListenerManagerImpl _smallListenerManager;
        private ListenerManagerImpl _largeListenerManager;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _emptyistenerManager = new ListenerManagerImpl();

            _smallListenerManager = new ListenerManagerImpl();
            _smallListenerManager.AddJobListener(new BroadcastJobListener("A"));
            /*
            _smallListenerManager.AddJobListener(new BroadcastJobListener("B"));
            _smallListenerManager.AddJobListener(new BroadcastJobListener("C"));
            _smallListenerManager.AddJobListener(new BroadcastJobListener("D"));
            _smallListenerManager.AddJobListener(new BroadcastJobListener("E"));
            */

            _largeListenerManager = new ListenerManagerImpl();
            foreach (var i in Enumerable.Range(1, 200))
            {
                _largeListenerManager.AddJobListener(new BroadcastJobListener("" + i));
            }
        }

        [Benchmark]
        public IReadOnlyCollection<IJobListener> Empty()
        {
            return _emptyistenerManager.GetJobListeners();
        }

        [Benchmark]
        public IReadOnlyCollection<IJobListener> Small()
        {
            return _smallListenerManager.GetJobListeners();
        }

        /*
        [Benchmark]
        public IReadOnlyCollection<IJobListener> Large_Old()
        {
            return _largeListenerManager.GetJobListenersOld();
        }

        [Benchmark]
        public IReadOnlyCollection<IJobListener> Large_New()
        {
            return _largeListenerManager.GetJobListenersNew();
        }
        */
    }
}
