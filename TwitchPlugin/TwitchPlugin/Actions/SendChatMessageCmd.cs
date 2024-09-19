namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class SendChatMessageCmd : ActionEditorCommand
    {
        private const String IMGAction = "SendChatMessage.svg";
        private const String CmdName = "SendChatMessage";

        public SendChatMessageCmd()
        {
            this.GroupName = "";
            this.DisplayName = "Send Chat Message";
            this.Name = CmdName;

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(name: "MessageText", "Enter message text:"));
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var actionNameAndParameter = actionParameters.Parameters.FirstOrDefault();
            if (actionNameAndParameter.Key != null)
            {
                this.Plugin.RunCommand(CmdName, actionNameAndParameter.Value);
                return true;
            }

            return base.RunCommand(actionParameters);
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight) => 
            EmbeddedResources.ReadBinaryFile(TwitchPlugin.ImageResPrefix + IMGAction).ToImage();
    }
}
