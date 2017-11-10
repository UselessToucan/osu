﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuCheckbox : Checkbox, IHandleHover
    {
        private Bindable<bool> bindable;

        public Bindable<bool> Bindable
        {
            set
            {
                bindable = value;
                Current.BindTo(bindable);
            }
        }

        public Color4 CheckedColor { get; set; } = Color4.Cyan;
        public Color4 UncheckedColor { get; set; } = Color4.White;
        public int FadeDuration { get; set; }

        public string LabelText
        {
            get { return labelSpriteText?.Text; }
            set
            {
                if (labelSpriteText != null)
                    labelSpriteText.Text = value;
            }
        }

        public MarginPadding LabelPadding
        {
            get { return labelSpriteText?.Padding ?? new MarginPadding(); }
            set
            {
                if (labelSpriteText != null)
                    labelSpriteText.Padding = value;
            }
        }

        protected readonly Nub Nub;

        private readonly SpriteText labelSpriteText;
        private SampleChannel sampleChecked;
        private SampleChannel sampleUnchecked;

        public OsuCheckbox()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                labelSpriteText = new OsuSpriteText(),
                Nub = new Nub
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 5 },
                }
            };

            Nub.Current.BindTo(Current);

            Current.ValueChanged += newValue =>
            {
                if (newValue)
                    sampleChecked?.Play();
                else
                    sampleUnchecked?.Play();
            };

            Current.DisabledChanged += disabled =>
            {
                Alpha = disabled ? 0.3f : 1;
            };
        }

        public virtual bool OnHover(InputState state)
        {
            Nub.Glowing = true;
            Nub.Expanded = true;
            return false;
        }

        public virtual void OnHoverLost(InputState state)
        {
            Nub.Glowing = false;
            Nub.Expanded = false;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Sample.Get(@"UI/check-on");
            sampleUnchecked = audio.Sample.Get(@"UI/check-off");
        }
    }
}
