namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    public class OnRaidNotificationArgs : EventArgs
    {
        public RaidNotification RaidNotificaiton;
        public String Channel;
    }
}
