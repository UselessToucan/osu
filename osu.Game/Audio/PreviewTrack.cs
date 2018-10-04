// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Threading;

namespace osu.Game.Audio
{
    public abstract class PreviewTrack : Component
    {
        /// <summary>
        /// Invoked when this <see cref="PreviewTrack"/> has stopped playing.
        /// </summary>
        public event Action Stopped;

        /// <summary>
        /// Invoked when this <see cref="PreviewTrack"/> has started playing.
        /// </summary>
        public event Action Started;

        public readonly Track Track;
        private bool hasStarted;

        protected PreviewTrack(Track track)
        {
            this.Track = track;
        }

        /// <summary>
        /// Length of the track.
        /// </summary>
        public double Length => Track?.Length ?? 0;

        /// <summary>
        /// The current track time.
        /// </summary>
        public double CurrentTime => Track?.CurrentTime ?? 0;

        /// <summary>
        /// Whether the track is loaded.
        /// </summary>
        public bool TrackLoaded => Track?.IsLoaded ?? false;

        /// <summary>
        /// Whether the track is playing.
        /// </summary>
        public bool IsRunning => Track?.IsRunning ?? false;

        protected override void Update()
        {
            base.Update();

            if (!hasStarted && IsRunning)
                hasStarted = true;

            // Todo: Track currently doesn't signal its completion, so we have to handle it manually
            if (hasStarted && Track.HasCompleted)
                Stop();
        }

        private ScheduledDelegate startDelegate;

        /// <summary>
        /// Starts playing this <see cref="PreviewTrack"/>.
        /// </summary>
        public void Start() => startDelegate = Schedule(() =>
        {
            if (Track == null)
                return;

            if (!hasStarted && IsRunning)
                hasStarted = true;

            if (hasStarted)
                return;
            hasStarted = true;

            Track.Restart();
            Started?.Invoke();
        });

        /// <summary>
        /// Stops playing this <see cref="PreviewTrack"/>.
        /// </summary>
        public void Stop()
        {
            startDelegate?.Cancel();

            if (Track == null)
                return;

            if (!hasStarted)
                return;
            hasStarted = false;

            Track.Stop();
            Stopped?.Invoke();
        }
    }
}
