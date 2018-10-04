// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.States;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class PlayButton : Container
    {
        public readonly BindableBool Playing = new BindableBool();
        public PreviewTrack Preview { get; private set; }

        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                if (Preview != null)
                {
                    Preview.Started -= previewStarted;
                    Preview.Stopped -= previewStopped;
                    Preview.Expire();
                }
                
                Preview = null;

                Playing.Value = false;

                loadPreviewIfExists();
            }
        }

        private Color4 hoverColour;
        private readonly SpriteIcon icon;
        private readonly LoadingAnimation loadingAnimation;

        private const float transition_duration = 500;

        private bool loading
        {
            set
            {
                if (value)
                {
                    icon.FadeTo(0.5f, transition_duration, Easing.OutQuint);
                    loadingAnimation.Show();
                }
                else
                {
                    icon.FadeTo(1, transition_duration, Easing.OutQuint);
                    loadingAnimation.Hide();
                }
            }
        }

        public PlayButton(BeatmapSetInfo setInfo = null)
        {
            BeatmapSet = setInfo;
            AddRange(new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_play,
                },
                loadingAnimation = new LoadingAnimation
                {
                    Size = new Vector2(15),
                },
            });

            Playing.ValueChanged += playingStateChanged;
        }

        private PreviewTrackManager previewTrackManager;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, PreviewTrackManager previewTrackManager)
        {
            this.previewTrackManager = previewTrackManager;
            previewTrackManager.TrackStarted += previewTrackManagerTrackStarted;
            previewTrackManager.TrackStopped += previewTrackManagerTrackStopped;

            loadPreviewIfExists();

            hoverColour = colour.Yellow;
        }

        private void previewTrackManagerTrackStarted(BeatmapSetInfo obj)
        {
            if (Preview==null&& BeatmapSet==obj)
            {
                loadPreviewIfExists();
                previewStarted();
            }
        }

        private void previewTrackManagerTrackStopped(BeatmapSetInfo obj)
        {
            if (BeatmapSet == obj)
                previewStopped();
        }

        private void loadPreviewIfExists()
        {
            if (previewTrackManager != null && previewTrackManager.Exists(beatmapSet))
            {
                loadPreview();
                Playing.Value = Playing.Value || Preview.IsRunning;
            }
        }

        protected override bool OnClick(InputState state)
        {
            Playing.Toggle();
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            icon.FadeColour(hoverColour, 120, Easing.InOutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (!Playing.Value)
                icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
            base.OnHoverLost(state);
        }

        private void playingStateChanged(bool playing)
        {
            icon.Icon = playing ? FontAwesome.fa_stop : FontAwesome.fa_play;
            icon.FadeColour(playing || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);

            if (playing)
            {
                if (BeatmapSet == null)
                {
                    Playing.Value = false;
                    return;
                }

                if (Preview != null)
                {
                    Preview.Start();
                    return;
                }

                loading = true;

                loadPreview();
            }
            else
            {
                Preview?.Stop();
                loading = false;
            }
        }

        private void loadPreview()
        {
            LoadComponentAsync(Preview = previewTrackManager.Get(beatmapSet), preview =>
            {
                // beatmapset may have changed.
                if (Preview != preview)
                    return;

                AddInternal(preview);

                loading = false;

                preview.Started += previewStarted;
                preview.Stopped += previewStopped;

                // user may have changed their mind.
                if (Playing || preview.IsRunning)
                    preview.Start();
            });
        }

        private void previewStarted() => Playing.Value = true;

        private void previewStopped() => Playing.Value = false;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Playing.Value = false;
            previewTrackManager.TrackStarted -= previewTrackManagerTrackStarted;
        }
    }
}
