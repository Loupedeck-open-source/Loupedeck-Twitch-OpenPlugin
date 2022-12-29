namespace TwitchLib.Client.Models
{
    using System;

    /// <summary>Class representing cheer badge.</summary>
    public class CheerBadge
    {
        /// <summary>Property representing raw cheer amount represented by badge.</summary>
        public Int32 CheerAmount { get; }
        /// <summary>Property representing the color of badge via an enum.</summary>
        public Enums.BadgeColor Color { get; }

        /// <summary>Constructor for CheerBadge</summary>
        public CheerBadge(Int32 cheerAmount)
        {
            this.CheerAmount = cheerAmount;
            this.Color = this.getColor(cheerAmount);
        }

        private Enums.BadgeColor getColor(Int32 cheerAmount)
        {
            if (cheerAmount >= 10000)
                return Enums.BadgeColor.Red;
            if (cheerAmount >= 5000)
                return Enums.BadgeColor.Blue;
            if (cheerAmount >= 1000)
                return Enums.BadgeColor.Green;
            return cheerAmount >= 100 ? Enums.BadgeColor.Purple : Enums.BadgeColor.Gray;
        }
    }
}
