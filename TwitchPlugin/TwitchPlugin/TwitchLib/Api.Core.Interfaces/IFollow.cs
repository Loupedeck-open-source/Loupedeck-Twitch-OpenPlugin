namespace TwitchLib.Api.Core.Interfaces
{
    using System;

    public interface IFollow
    {
        DateTime CreatedAt { get; }
        Boolean Notifications { get; }

        IUser User { get;  }
    }
}
