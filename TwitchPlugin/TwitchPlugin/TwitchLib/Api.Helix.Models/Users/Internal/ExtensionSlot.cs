namespace TwitchLib.Api.Helix.Models.Users.Internal
{
    using System;
    using Core.Enums;

    public class ExtensionSlot
    {
        public ExtensionType Type;
        public String Slot;
        public UserExtensionState UserExtensionState;

        public ExtensionSlot(ExtensionType type, String slot, UserExtensionState userExtensionState)
        {
            this.Type = type;
            this.Slot = slot;
            this.UserExtensionState = userExtensionState;
        }
    }
}
