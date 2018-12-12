// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Database;

namespace osu.Game.Online.Chat
{
    public class HostnameInfo:IHasPrimaryKey
    {
        public int ID { get; set; }

        public string Hostname { get; set; }
        public HostnameState State { get; set; }

        public enum HostnameState
        {
            Allowed,
            Denied
        }
    }
}
