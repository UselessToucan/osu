// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Audio
{
    /// <summary>
    /// A central store for the retrieval of <see cref="PreviewTrack"/>s.
    /// </summary>
    public class PreviewTrackManager : Component
    {
        public event Action<BeatmapSetInfo> TrackStarted;
        public event Action<BeatmapSetInfo> TrackStopped;

        private readonly BindableDouble muteBindable = new BindableDouble();

        private AudioManager audio;
        private TrackManager trackManager;

        private TrackManagerPreviewTrack current;

        private readonly Dictionary<BeatmapSetInfo, WeakReference> existingTracks;

        public PreviewTrackManager()
        {
            existingTracks = new Dictionary<BeatmapSetInfo, WeakReference>();
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, FrameworkConfigManager config)
        {
            trackManager = new TrackManager(new OnlineStore());

            this.audio = audio;
            audio.AddItem(trackManager);

            config.BindWith(FrameworkSetting.VolumeMusic, trackManager.Volume);
        }

        /// <summary>
        /// Retrieves a <see cref="PreviewTrack"/> for a <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to retrieve the preview track for.</param>
        /// <returns>The playable <see cref="PreviewTrack"/>.</returns>
        public PreviewTrack Get(BeatmapSetInfo beatmapSetInfo)
        {
            var track = CreatePreviewTrack(beatmapSetInfo, trackManager);

            track.Started += () =>
            {
                if (current != null && current == track)
                    return;

                current?.Stop();
                current = track;
                audio.Track.AddAdjustment(AdjustableProperty.Volume, muteBindable);
                TrackStarted?.Invoke(track.BeatmapSet);
            };

            track.Stopped += () =>
            {
                if (current != null && current != track)
                    return;

                current = null;
                audio.Track.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
                TrackStopped?.Invoke(track.BeatmapSet);
            };

            return track;
        }

        /// <summary>
        /// Checks whether required <see cref="Track"/> is already created/>
        /// </summary>
        /// <param name="beatmapSetInfo"><see cref="BeatmapSetInfo"/></param>
        /// <returns>whether required <see cref="Track"/> is already created</returns>
        public bool Exists(BeatmapSetInfo beatmapSetInfo) => beatmapSetInfo != null && existingTracks.ContainsKey(beatmapSetInfo) && existingTracks[beatmapSetInfo].IsAlive;

        /// <summary>
        /// Stops any currently playing <see cref="PreviewTrack"/>.
        /// </summary>
        /// <remarks>
        /// Only the immediate owner (an object that implements <see cref="IPreviewTrackOwner"/>) of the playing <see cref="PreviewTrack"/>
        /// can globally stop the currently playing <see cref="PreviewTrack"/>. The object holding a reference to the <see cref="PreviewTrack"/>
        /// can always stop the <see cref="PreviewTrack"/> themselves through <see cref="PreviewTrack.Stop()"/>.
        /// </remarks>
        /// <param name="source">The <see cref="IPreviewTrackOwner"/> which may be the owner of the <see cref="PreviewTrack"/>.</param>
        public void StopAnyPlaying(IPreviewTrackOwner source)
        {
            //if (current == null || current.Owner != source)
            //    return;

            //current.Stop();
            //current = null;
        }

        /// <summary>
        /// Creates the <see cref="TrackManagerPreviewTrack"/>.
        /// </summary>
        protected virtual TrackManagerPreviewTrack CreatePreviewTrack(BeatmapSetInfo beatmapSetInfo, TrackManager trackManager)
        {
            if (Exists(beatmapSetInfo))
                return new TrackManagerPreviewTrack(beatmapSetInfo, (Track)existingTracks[beatmapSetInfo].Target);

            var track = trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo?.OnlineBeatmapSetID}.mp3");

            if (!existingTracks.ContainsKey(beatmapSetInfo))
                existingTracks.Add(beatmapSetInfo, new WeakReference(track));
            existingTracks[beatmapSetInfo] = new WeakReference(track);

            var trackManagerPreviewTrack = new TrackManagerPreviewTrack(beatmapSetInfo, track);
            return trackManagerPreviewTrack;
        }

        protected class TrackManagerPreviewTrack : PreviewTrack
        {
            public IPreviewTrackOwner Owner { get; private set; }
            public readonly BeatmapSetInfo BeatmapSet;

            public TrackManagerPreviewTrack(BeatmapSetInfo beatmapSet, Track track)
                : base(track)
            {
                BeatmapSet = beatmapSet;
            }

            [BackgroundDependencyLoader]
            private void load(IPreviewTrackOwner owner)
            {
                Owner = owner;
            }
        }
    }
}
