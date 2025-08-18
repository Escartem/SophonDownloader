using System;
using System.Threading;
// ReSharper disable UnusedType.Global
// ReSharper disable InvalidXmlDocComment
// ReSharper disable IdentifierTypo

namespace Hi3Helper.Sophon
{
    public class SophonDownloadSpeedLimiter
    {
        internal event EventHandler<int>  CurrentChunkProcessingChangedEvent;
        internal event EventHandler<long> DownloadSpeedChangedEvent;
        // ReSharper disable once MemberCanBePrivate.Global
        internal long?              InitialRequestedSpeed { get; set; }
        private  EventHandler<long> InnerListener         { get; set; }
        internal int                CurrentChunkProcessing;

        private SophonDownloadSpeedLimiter(long initialRequestedSpeed)
        {
            InitialRequestedSpeed = initialRequestedSpeed;
        }

        /// <summary>
        /// Create an instance by its initial speed to request.
        /// </summary>
        /// <param name="initialSpeed">The initial speed to be requested</param>
        /// <returns>An instance of the speed limiter</returns>
        public static SophonDownloadSpeedLimiter CreateInstance(long initialSpeed)
            => new(initialSpeed);

        /// <summary>
        /// Get the listener for the parent event
        /// </summary>
        /// <returns>The EventHandler of the listener.</returns>
        /// <seealso cref="EventHandler"/>
        public EventHandler<long> GetListener() => InnerListener ??= DownloadSpeedChangeListener;

        private void DownloadSpeedChangeListener(object sender, long newRequestedSpeed)
        {
            DownloadSpeedChangedEvent?.Invoke(this, newRequestedSpeed);
            InitialRequestedSpeed = newRequestedSpeed;
        }

        internal void IncrementChunkProcessedCount()
        {
            Interlocked.Increment(ref CurrentChunkProcessing);
            CurrentChunkProcessingChangedEvent?.Invoke(this, CurrentChunkProcessing);
        }

        internal void DecrementChunkProcessedCount()
        {
            Interlocked.Decrement(ref CurrentChunkProcessing);
            CurrentChunkProcessingChangedEvent?.Invoke(this, CurrentChunkProcessing);
        }
    }
}
