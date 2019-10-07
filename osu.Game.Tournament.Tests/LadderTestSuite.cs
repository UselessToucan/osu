// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests
{
    public abstract class LadderTestSuite : TournamentTestSuite
    {
        [Resolved]
        protected LadderInfo Ladder { get; private set; }
    }
}
