﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Game.Audio;
using osu.Game.Screens.Play;

namespace osu.Game.Skinning
{
    public class SkinnableSound : SkinReloadableDrawable
    {
        private readonly ISampleInfo[] hitSamples;

        [Resolved]
        private ISampleStore samples { get; set; }

        private bool requestedPlaying;

        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;

        private readonly AudioContainer<DrawableSample> samplesContainer;

        public SkinnableSound(ISampleInfo hitSamples)
            : this(new[] { hitSamples })
        {
        }

        public SkinnableSound(IEnumerable<ISampleInfo> hitSamples)
        {
            this.hitSamples = hitSamples.ToArray();
            InternalChild = samplesContainer = new AudioContainer<DrawableSample>();
        }

        private Bindable<bool> gameplayClockPaused;

        [BackgroundDependencyLoader(true)]
        private void load(GameplayClock gameplayClock)
        {
            // if in a gameplay context, pause sample playback when gameplay is paused.
            gameplayClockPaused = gameplayClock?.IsPaused.GetBoundCopy();
            gameplayClockPaused?.BindValueChanged(paused =>
            {
                if (requestedPlaying)
                {
                    if (paused.NewValue)
                        stop();
                    // it's not easy to know if a sample has finished playing (to end).
                    // to keep things simple only resume playing looping samples.
                    else if (Looping)
                        play();
                }
            });
        }

        private bool looping;

        public bool Looping
        {
            get => looping;
            set
            {
                if (value == looping) return;

                looping = value;

                samplesContainer.ForEach(c => c.Looping = looping);
            }
        }

        public void Play()
        {
            requestedPlaying = true;
            play();
        }

        private void play()
        {
            samplesContainer.ForEach(c =>
            {
                if (c.AggregateVolume.Value > 0)
                    c.Play();
            });
        }

        public void Stop()
        {
            requestedPlaying = false;
            stop();
        }

        private void stop()
        {
            samplesContainer.ForEach(c => c.Stop());
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            var channels = hitSamples.Select(s =>
            {
                var ch = skin.GetSample(s);

                if (ch == null && allowFallback)
                {
                    foreach (var lookup in s.LookupNames)
                    {
                        if ((ch = samples.Get($"Gameplay/{lookup}")) != null)
                            break;
                    }
                }

                if (ch != null)
                {
                    ch.Looping = looping;
                    ch.Volume.Value = s.Volume / 100.0;
                }

                return ch;
            }).Where(c => c != null);

            samplesContainer.ChildrenEnumerable = channels.Select(c => new DrawableSample(c));

            if (requestedPlaying)
                Play();
        }

        #region Re-expose AudioContainer

        public BindableNumber<double> Volume => samplesContainer.Volume;

        public BindableNumber<double> Balance => samplesContainer.Balance;

        public BindableNumber<double> Frequency => samplesContainer.Frequency;

        public BindableNumber<double> Tempo => samplesContainer.Tempo;

        public bool IsPlaying => samplesContainer.Any(s => s.Playing);

        /// <summary>
        /// Smoothly adjusts <see cref="Volume"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public TransformSequence<AudioContainer<DrawableSample>> VolumeTo(double newVolume, double duration = 0, Easing easing = Easing.None) =>
            samplesContainer.VolumeTo(newVolume, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Balance"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public TransformSequence<AudioContainer<DrawableSample>> BalanceTo(double newBalance, double duration = 0, Easing easing = Easing.None) =>
            samplesContainer.BalanceTo(newBalance, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Frequency"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public TransformSequence<AudioContainer<DrawableSample>> FrequencyTo(double newFrequency, double duration = 0, Easing easing = Easing.None) =>
            samplesContainer.FrequencyTo(newFrequency, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Tempo"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public TransformSequence<AudioContainer<DrawableSample>> TempoTo(double newTempo, double duration = 0, Easing easing = Easing.None) =>
            samplesContainer.TempoTo(newTempo, duration, easing);

        #endregion
    }
}
