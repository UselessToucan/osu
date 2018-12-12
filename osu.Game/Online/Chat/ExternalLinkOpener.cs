// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;

namespace osu.Game.Online.Chat
{
    public class ExternalLinkOpener : Component
    {
        private GameHost host;
        private OsuConfigManager config;
        private HostnameStore hostnameStore;
        private DialogOverlay dialogOverlay;
        private Bindable<bool> externalLinkWarning;

        [BackgroundDependencyLoader(true)]
        private void load(GameHost host, DialogOverlay dialogOverlay, OsuConfigManager config, HostnameStore hostnameStore)
        {
            this.host = host;
            this.hostnameStore = hostnameStore;
            this.dialogOverlay = dialogOverlay;
            externalLinkWarning = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning);
        }

        public void OpenUrlExternally(string url)
        {
            var hostname = new Uri(url).Host;
            var databasedHostname = hostnameStore.Query(hostname);

            if (databasedHostname?.State == HostnameInfo.HostnameState.Denied)
                return;

            if (externalLinkWarning && databasedHostname == null)
                dialogOverlay.Push(new ExternalLinkDialog(url, () => host.OpenUrlExternally(url)));
            else
                host.OpenUrlExternally(url);
        }
    }
}
