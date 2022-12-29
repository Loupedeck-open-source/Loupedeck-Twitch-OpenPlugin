namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;

    public class TwitchPluginConfig
    {
        public String ClientId { get; set; }
        public String ClientSecret { get; set; }
        public Int32 Timeout { get; set; }
        public List<Int32> Ports { get; set; }
    }
}