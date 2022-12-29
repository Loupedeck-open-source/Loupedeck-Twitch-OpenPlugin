namespace TwitchLib.Api.Core.Interfaces
{
    using System;

    public interface IUser
    {
        String Id { get; }
        String Bio { get; }
        DateTime CreatedAt { get; }
        String DisplayName { get; }
        String Logo { get; }
        String Name { get; }
        String Type { get; }
        DateTime UpdatedAt { get; }

    }
}
