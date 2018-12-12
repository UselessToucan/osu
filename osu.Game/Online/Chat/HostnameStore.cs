// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Online.Chat
{
    public class HostnameStore : DatabaseBackedStore
    {
        public HostnameStore(IDatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        public void Add(HostnameInfo hostnameInfo)
        {
            using (var write = ContextFactory.GetForWrite())
                write.Context.DatabasedHostnameInfo.Add(hostnameInfo);
        }

        public void Remove(HostnameInfo hostnameInfo)
        {
            using (var write = ContextFactory.GetForWrite())
                write.Context.DatabasedHostnameInfo.Remove(hostnameInfo);
        }

        public HostnameInfo Query(string host)
        {
            return ContextFactory.Get().DatabasedHostnameInfo.SingleOrDefault(h => h.Hostname == host);
        }

        public IEnumerable<HostnameInfo> Query(HostnameInfo.HostnameState state)
        {
            return ContextFactory.Get().DatabasedHostnameInfo.Where(h => h.State == state);
        }

        public HostnameInfo Query(string host, HostnameInfo.HostnameState state)
        {
            return ContextFactory.Get().DatabasedHostnameInfo.SingleOrDefault(h => h.Hostname == host && h.State == state);
        }
    }
}
