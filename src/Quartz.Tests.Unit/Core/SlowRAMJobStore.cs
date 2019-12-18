using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Quartz.Simpl;
using Quartz.Spi;

namespace Quartz.Tests.Unit.Core
{
    /// <summary>
    /// Custom RAMJobStore for producing context switches.
    /// </summary>
    public class SlowRAMJobStore : RAMJobStore
    {
#if NOPERF
        public override async Task<IReadOnlyCollection<IOperableTrigger>> AcquireNextTriggers(
#else
        public override async Task<IReadOnlyList<IOperableTrigger>> AcquireNextTriggers(
#endif
            DateTimeOffset noLaterThan, 
            int maxCount, 
            TimeSpan timeWindow,
            CancellationToken cancellationToken = default)
        {
            var nextTriggers = await base.AcquireNextTriggers(noLaterThan, maxCount, timeWindow, cancellationToken);

            // Wait just a bit for hopefully having a context switch leading to the race condition
            await Task.Delay(10, cancellationToken);

            return nextTriggers;
        }
    }
}