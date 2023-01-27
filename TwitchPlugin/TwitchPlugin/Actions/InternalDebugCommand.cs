namespace  Loupedeck.TwitchPlugin.Actions
{
    using System;

#if DEBUG

    internal class InternalDebugCommand : PluginDynamicCommand
    {
        public InternalDebugCommand()
        {
            this.DisplayName = "Debug command";
            this.GroupName = "Debug Commands";
            this.Description = "Does what you program it to do";

            foreach (var cmd in TwitchPlugin.Proxy.GetDebugCommands())
            {
                this.AddParameter(cmd.Key, cmd.Value, this.GroupName).Description = cmd.Key;
            }
        }

	
        protected override Boolean OnLoad()
        {
            return true;
        }

        protected override Boolean OnUnload()
        {
            
            return true;
        }

        protected override void RunCommand(String actionParameter) 
	    {
            TwitchPlugin.Proxy.RunDebugCommand(actionParameter);
	    }
    }
#endif
}
