namespace TwitchLib.Api.Core.Interfaces
{
    using System;

    public interface IFollows
    {
        Int32 Total { get; }
        String Cursor { get; }
        IFollow[] Follows { get; }
    }
}
