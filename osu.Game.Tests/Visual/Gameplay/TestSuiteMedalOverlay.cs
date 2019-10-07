// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays;
using osu.Game.Overlays.MedalSplash;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSuiteMedalOverlay : OsuTestSuite
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MedalOverlay),
            typeof(DrawableMedal),
        };

        public TestSuiteMedalOverlay()
        {
            AddStep(@"display", () =>
            {
                LoadComponentAsync(new MedalOverlay(new Medal
                {
                    Name = @"Animations",
                    InternalName = @"all-intro-doubletime",
                    Description = @"More complex than you think.",
                }), Add);
            });
        }
    }
}
