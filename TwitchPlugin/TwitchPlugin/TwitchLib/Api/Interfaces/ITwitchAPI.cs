namespace TwitchLib.Api.Interfaces
{
    using Core.Interfaces;
    using Core.Undocumented;

    public interface ITwitchAPI
    {
        IApiSettings Settings { get; }
        Helix.Helix Helix { get; }
        ThirdParty.ThirdParty ThirdParty { get; }
        Undocumented Undocumented { get; }
    }
}