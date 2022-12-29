namespace TwitchLib.Api.Helix.Models.Helpers
{
    using System;

    public class ExtensionAnalytics
    {
        public String Date { get; protected set; }
        public String ExtensionName { get; protected set; }
        public String ExtensionClientId { get; protected set; }
        public Int32 Installs { get; protected set; }
        public Int32 Uninstalls { get; protected set; }
        public Int32 Activations { get; protected set; }
        public Int32 UniqueActiveChannels { get; protected set; }
        public Int32 Renders { get; protected set; }
        public Int32 UniqueRenders { get; protected set; }
        public Int32 Views { get; protected set; }
        public Int32 UniqueViewers { get; protected set; }
        public Int32 UniqueInteractors { get; protected set; }
        public Int32 Clicks { get; protected set; }
        public Double ClicksPerInteractor { get; protected set; }
        public Double InteractionRate { get; protected set; }

        public ExtensionAnalytics(String row)
        {
            var p = row.Split(',');
            this.Date = p[0];
            this.ExtensionName = p[1];
            this.ExtensionClientId = p[2];
            this.Installs = Int32.Parse(p[3]);
            this.Uninstalls = Int32.Parse(p[4]);
            this.Activations = Int32.Parse(p[5]);
            this.UniqueActiveChannels = Int32.Parse(p[6]);
            this.Renders = Int32.Parse(p[7]);
            this.UniqueRenders = Int32.Parse(p[8]);
            this.Views = Int32.Parse(p[9]);
            this.UniqueViewers = Int32.Parse(p[10]);
            this.UniqueInteractors = Int32.Parse(p[11]);
            this.Clicks = Int32.Parse(p[12]);
            this.ClicksPerInteractor = Double.Parse(p[13]);
            this.InteractionRate = Double.Parse(p[14]);
        }
    }
}
