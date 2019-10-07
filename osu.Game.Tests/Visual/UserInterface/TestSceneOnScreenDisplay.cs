﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOnScreenDisplay : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var config = new TestConfigManager();

            var osd = new TestOnScreenDisplay();
            osd.BeginTracking(this, config);
            Add(osd);

            AddStep("Display empty osd toast", () => osd.Display(new EmptyToast()));
            AddAssert("Toast width is 240", () => osd.Child.Width == 240);

            AddStep("Display toast with lengthy text", () => osd.Display(new LengthyToast()));
            AddAssert("Toast width is greater than 240", () => osd.Child.Width > 240);

            AddRepeatStep("Change toggle (no bind)", () => config.ToggleSetting(TestConfigSetting.ToggleSettingNoKeybind), 2);
            AddRepeatStep("Change toggle (with bind)", () => config.ToggleSetting(TestConfigSetting.ToggleSettingWithKeybind), 2);
            AddRepeatStep("Change enum (no bind)", () => config.IncrementEnumSetting(TestConfigSetting.EnumSettingNoKeybind), 3);
            AddRepeatStep("Change enum (with bind)", () => config.IncrementEnumSetting(TestConfigSetting.EnumSettingWithKeybind), 3);
        }

        private class TestConfigManager : ConfigManager<TestConfigSetting>
        {
            public TestConfigManager()
            {
                InitialiseDefaults();
            }

            protected override void InitialiseDefaults()
            {
                Set(TestConfigSetting.ToggleSettingNoKeybind, false);
                Set(TestConfigSetting.EnumSettingNoKeybind, EnumSetting.Setting1);
                Set(TestConfigSetting.ToggleSettingWithKeybind, false);
                Set(TestConfigSetting.EnumSettingWithKeybind, EnumSetting.Setting1);

                base.InitialiseDefaults();
            }

            public void ToggleSetting(TestConfigSetting setting) => Set(setting, !Get<bool>(setting));

            public void IncrementEnumSetting(TestConfigSetting setting)
            {
                var nextValue = Get<EnumSetting>(setting) + 1;
                if (nextValue > EnumSetting.Setting4)
                    nextValue = EnumSetting.Setting1;
                Set(setting, nextValue);
            }

            public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
            {
                new TrackedSetting<bool>(TestConfigSetting.ToggleSettingNoKeybind, b => new SettingDescription(b, "toggle setting with no keybind", b ? "enabled" : "disabled")),
                new TrackedSetting<EnumSetting>(TestConfigSetting.EnumSettingNoKeybind, v => new SettingDescription(v, "enum setting with no keybind", v.ToString())),
                new TrackedSetting<bool>(TestConfigSetting.ToggleSettingWithKeybind, b => new SettingDescription(b, "toggle setting with keybind", b ? "enabled" : "disabled", "fake keybind")),
                new TrackedSetting<EnumSetting>(TestConfigSetting.EnumSettingWithKeybind, v => new SettingDescription(v, "enum setting with keybind", v.ToString(), "fake keybind")),
            };

            protected override void PerformLoad()
            {
            }

            protected override bool PerformSave() => false;
        }

        private enum TestConfigSetting
        {
            ToggleSettingNoKeybind,
            EnumSettingNoKeybind,
            ToggleSettingWithKeybind,
            EnumSettingWithKeybind
        }

        private enum EnumSetting
        {
            Setting1,
            Setting2,
            Setting3,
            Setting4
        }

        private class EmptyToast : Toast
        {
            public EmptyToast()
                : base("", "", "")
            {
            }
        }

        private class LengthyToast : Toast
        {
            public LengthyToast()
                : base("Toast with a very very very long text", "A very very very very very very long text also", "A very very very very very long shortcut")
            {
            }
        }

        private class TestOnScreenDisplay : OnScreenDisplay
        {
            protected override void DisplayTemporarily(Drawable toDisplay) => toDisplay.FadeIn().ResizeHeightTo(110);
        }
    }
}
