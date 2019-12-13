using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Quartz.Impl.Matchers;
using Quartz.Util;

namespace Quartz.Core
{
    /// <summary>
    /// Default concrete implementation of <see cref="IListenerManager" />.
    /// </summary>
    public class ListenerManagerImpl : IListenerManager
    {
        private static readonly IReadOnlyCollection<IJobListener> EmptyCollectionOfJobListeners = new List<IJobListener>().AsReadOnly();

        private readonly OrderedDictionary globalJobListeners = new OrderedDictionary(10);

        private readonly OrderedDictionary globalTriggerListeners = new OrderedDictionary(10);

        private readonly Dictionary<string, List<IMatcher<JobKey>>> globalJobListenersMatchers = new Dictionary<string, List<IMatcher<JobKey>>>(10);

        private readonly Dictionary<string, List<IMatcher<TriggerKey>>> globalTriggerListenersMatchers = new Dictionary<string, List<IMatcher<TriggerKey>>>(10);

        private readonly List<ISchedulerListener> schedulerListeners = new List<ISchedulerListener>(10);

        public void AddJobListener(IJobListener jobListener, params IMatcher<JobKey>[] matchers)
        {
            AddJobListener(jobListener, new List<IMatcher<JobKey>>(matchers));
        }

        public void AddJobListener(IJobListener jobListener, IReadOnlyCollection<IMatcher<JobKey>> matchers)
        {
            if (string.IsNullOrEmpty(jobListener.Name))
            {
                throw new ArgumentException(
                    "JobListener name cannot be empty.");
            }

            lock (globalJobListeners)
            {
                globalJobListeners[jobListener.Name] = jobListener;

#if NOPERF
                List<IMatcher<JobKey>> matchersL = new List<IMatcher<JobKey>>();
                if (matchers != null && matchers.Count > 0)
                {
                    matchersL.AddRange(matchers);
                }
                else
                {
                    matchersL.Add(EverythingMatcher<JobKey>.AllJobs());
                }
#else
                List<IMatcher<JobKey>> matchersL;
                if (matchers != null && matchers.Count > 0)
                {
                    matchersL = new List<IMatcher<JobKey>>(matchers);
                }
                else
                {
                    matchersL = new List<IMatcher<JobKey>>(1);
                    matchersL.Add(EverythingMatcher<JobKey>.AllJobs());
                }
#endif

                globalJobListenersMatchers[jobListener.Name] = matchersL;
            }
        }

        public bool AddJobListenerMatcher(string listenerName, IMatcher<JobKey> matcher)
        {
            if (matcher == null)
            {
                throw new ArgumentException("Non-null value not acceptable.");
            }

            lock (globalJobListeners)
            {
                List<IMatcher<JobKey>> matchers = globalJobListenersMatchers.TryGetAndReturn(listenerName);
                if (matchers == null)
                {
                    return false;
                }
                matchers.Add(matcher);
                return true;
            }
        }

        public bool RemoveJobListenerMatcher(string listenerName, IMatcher<JobKey> matcher)
        {
            if (matcher == null)
            {
                throw new ArgumentException("Non-null value not acceptable.");
            }

            lock (globalJobListeners)
            {
                List<IMatcher<JobKey>> matchers = globalJobListenersMatchers.TryGetAndReturn(listenerName);
                if (matchers == null)
                {
                    return false;
                }
                return matchers.Remove(matcher);
            }
        }

        public IReadOnlyCollection<IMatcher<JobKey>> GetJobListenerMatchers(string listenerName)
        {
            lock (globalJobListeners)
            {
                List<IMatcher<JobKey>> matchers = globalJobListenersMatchers.TryGetAndReturn(listenerName);
                return matchers?.AsReadOnly();
            }
        }

        public bool SetJobListenerMatchers(string listenerName, IReadOnlyCollection<IMatcher<JobKey>> matchers)
        {
            if (matchers == null)
            {
                throw new ArgumentException("Non-null value not acceptable.");
            }

            lock (globalJobListeners)
            {
                List<IMatcher<JobKey>> oldMatchers = globalJobListenersMatchers.TryGetAndReturn(listenerName);
                if (oldMatchers == null)
                {
                    return false;
                }
                globalJobListenersMatchers[listenerName] = new List<IMatcher<JobKey>>(matchers);
                return true;
            }
        }

        public bool RemoveJobListener(string name)
        {
            lock (globalJobListeners)
            {
                if (globalJobListeners.Contains(name))
                {
                    globalJobListeners.Remove(name);
                    return true;
                }
                return false;
            }
        }

        public IReadOnlyCollection<IJobListener> GetJobListenersOld()
        {
            lock (globalJobListeners)
            {
                return new List<IJobListener>(globalJobListeners.Values.Cast<IJobListener>()).AsReadOnly();
            }
        }

        public IReadOnlyCollection<IJobListener> GetJobListenersNew()
        {
            lock (globalJobListeners)
            {
                var globalJobListenersValues = globalJobListeners.Values;
                var jobListeners = new List<IJobListener>(globalJobListenersValues.Count);

                foreach (var jobListener in globalJobListenersValues)
                {
                    jobListeners.Add((IJobListener)jobListener);
                }

                return jobListeners.AsReadOnly();
            }
        }

        public IReadOnlyCollection<IJobListener> GetJobListeners()
        {
#if NOPERF
            lock (globalJobListeners)
            {
                return new List<IJobListener>(globalJobListeners.Values.Cast<IJobListener>()).AsReadOnly();
            }
#else
            lock (globalJobListeners)
            {
                var globalJobListenersValues = globalJobListeners.Values;
                var jobListeners = new List<IJobListener>(globalJobListenersValues.Count);

                foreach (var jobListener in globalJobListenersValues)
                {
                    jobListeners.Add((IJobListener) jobListener);
                }

                return jobListeners.AsReadOnly();
            }
#endif
        }

        public IJobListener GetJobListener(string name)
        {
            lock (globalJobListeners)
            {
                return (IJobListener) globalJobListeners[name];
            }
        }

        public void AddTriggerListener(ITriggerListener triggerListener, params IMatcher<TriggerKey>[] matchers)
        {
            AddTriggerListener(triggerListener, new List<IMatcher<TriggerKey>>(matchers));
        }

        public void AddTriggerListener(ITriggerListener triggerListener, IReadOnlyCollection<IMatcher<TriggerKey>> matchers)
        {
            if (string.IsNullOrEmpty(triggerListener.Name))
            {
                throw new ArgumentException("TriggerListener name cannot be empty.");
            }

            lock (globalTriggerListeners)
            {
                globalTriggerListeners[triggerListener.Name] = triggerListener;

                List<IMatcher<TriggerKey>> matchersL = new List<IMatcher<TriggerKey>>();
                if (matchers != null && matchers.Count > 0)
                {
                    matchersL.AddRange(matchers);
                }
                else
                {
                    matchersL.Add(EverythingMatcher<TriggerKey>.AllTriggers());
                }

                globalTriggerListenersMatchers[triggerListener.Name] = matchersL;
            }
        }

        public void AddTriggerListener(ITriggerListener triggerListener, IMatcher<TriggerKey> matcher)
        {
            if (matcher == null)
            {
                throw new ArgumentException("Non-null value not acceptable for matcher.");
            }

            if (string.IsNullOrEmpty(triggerListener.Name))
            {
                throw new ArgumentException("TriggerListener name cannot be empty.");
            }

            lock (globalTriggerListeners)
            {
                globalTriggerListeners[triggerListener.Name] = triggerListener;
                var matchers = new List<IMatcher<TriggerKey>>(1) {matcher};
                globalTriggerListenersMatchers[triggerListener.Name] = matchers;
            }
        }

        public bool AddTriggerListenerMatcher(string listenerName, IMatcher<TriggerKey> matcher)
        {
            if (matcher == null)
            {
                throw new ArgumentException("Non-null value not acceptable.");
            }

            lock (globalTriggerListeners)
            {
                List<IMatcher<TriggerKey>> matchers = globalTriggerListenersMatchers.TryGetAndReturn(listenerName);
                if (matchers == null)
                {
                    return false;
                }
                matchers.Add(matcher);
                return true;
            }
        }

        public bool RemoveTriggerListenerMatcher(string listenerName, IMatcher<TriggerKey> matcher)
        {
            if (matcher == null)
            {
                throw new ArgumentException("Non-null value not acceptable.");
            }

            lock (globalTriggerListeners)
            {
                List<IMatcher<TriggerKey>> matchers = globalTriggerListenersMatchers.TryGetAndReturn(listenerName);
                if (matchers == null)
                {
                    return false;
                }
                return matchers.Remove(matcher);
            }
        }

        public IReadOnlyCollection<IMatcher<TriggerKey>> GetTriggerListenerMatchers(string listenerName)
        {
            lock (globalTriggerListeners)
            {
                List<IMatcher<TriggerKey>> matchers = globalTriggerListenersMatchers.TryGetAndReturn(listenerName);
                return matchers;
            }
        }

        public bool SetTriggerListenerMatchers(string listenerName, IReadOnlyCollection<IMatcher<TriggerKey>> matchers)
        {
            if (matchers == null)
            {
                throw new ArgumentException("Non-null value not acceptable.");
            }

            lock (globalTriggerListeners)
            {
                List<IMatcher<TriggerKey>> oldMatchers = globalTriggerListenersMatchers.TryGetAndReturn(listenerName);
                if (oldMatchers == null)
                {
                    return false;
                }
                globalTriggerListenersMatchers[listenerName] = new List<IMatcher<TriggerKey>>(matchers);
                return true;
            }
        }

        public bool RemoveTriggerListener(string name)
        {
            lock (globalTriggerListeners)
            {
                if (globalTriggerListeners.Contains(name))
                {
                    globalTriggerListeners.Remove(name);
                    return true;
                }
                return false;
            }
        }

        public IReadOnlyCollection<ITriggerListener> GetTriggerListeners()
        {
#if NOPERF
            lock (globalTriggerListeners)
            {
                return new List<ITriggerListener>(globalTriggerListeners.Values.Cast<ITriggerListener>()).AsReadOnly();
            }
#else
            lock (globalJobListeners)
            {
                var globalTriggerListenersValues = globalTriggerListeners.Values;
                var triggerListeners = new List<ITriggerListener>(globalTriggerListenersValues.Count);

                foreach (var triggerListener in globalTriggerListenersValues)
                {
                    triggerListeners.Add((ITriggerListener) triggerListener);
                }

                return triggerListeners.AsReadOnly();
            }
#endif
        }

        public ITriggerListener GetTriggerListener(string name)
        {
            lock (globalTriggerListeners)
            {
                return (ITriggerListener) globalTriggerListeners[name];
            }
        }

        public void AddSchedulerListener(ISchedulerListener schedulerListener)
        {
            lock (schedulerListeners)
            {
                schedulerListeners.Add(schedulerListener);
            }
        }

        public bool RemoveSchedulerListener(ISchedulerListener schedulerListener)
        {
            lock (schedulerListeners)
            {
                return schedulerListeners.Remove(schedulerListener);
            }
        }

        public IReadOnlyCollection<ISchedulerListener> GetSchedulerListeners()
        {
            lock (schedulerListeners)
            {
                return schedulerListeners.AsReadOnly();
            }
        }
    }
}