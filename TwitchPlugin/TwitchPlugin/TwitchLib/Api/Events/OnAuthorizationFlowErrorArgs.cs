namespace TwitchLib.Api.Events
{
    using System;

    public class OnAuthorizationFlowErrorArgs
    {
        public Int32 Error { get; set; }
        public String Message { get; set; }
    }
}
