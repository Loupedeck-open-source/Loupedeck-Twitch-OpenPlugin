namespace TwitchLib.Client.Models.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Enums.Internal;

    public class IrcMessage
    {
        /// <summary>
        /// The channel the message was sent in
        /// </summary>
        public String Channel => this.Params.StartsWith("#") ? this.Params.Remove(0, 1) : this.Params;

        public String Params => this._parameters != null && this._parameters.Length > 0 ? this._parameters[0] : "";

        /// <summary>
        /// Message itself
        /// </summary>
        public String Message => this.Trailing;

        public String Trailing => this._parameters != null && this._parameters.Length > 1 ? this._parameters[this._parameters.Length - 1] : "";

        /// <summary>
        /// Command parameters
        /// </summary>
        private readonly String[] _parameters;

        /// <summary>
        /// The user whose message it is
        /// </summary>
        public readonly String User;

        /// <summary>
        /// Hostmask of the user
        /// </summary>
        public readonly String Hostmask;

        /// <summary>
        /// Raw Command
        /// </summary>
        public readonly IrcCommand Command;

        /// <summary>
        /// IRCv3 tags
        /// </summary>
        public readonly Dictionary<String, String> Tags;

        /// <summary>
        /// Create an INCOMPLETE IrcMessage only carrying username
        /// </summary>
        /// <param name="user"></param>
        public IrcMessage(String user)
        {
            this._parameters = null;
            this.User = user;
            this.Hostmask = null;
            this.Command = IrcCommand.Unknown;
            this.Tags = null;
        }

        /// <summary>
        /// Create an IrcMessage
        /// </summary>
        /// <param name="command">IRC Command</param>
        /// <param name="parameters">Command params</param>
        /// <param name="hostmask">User</param>
        /// <param name="tags">IRCv3 tags</param>
        public IrcMessage(IrcCommand command, String[] parameters, String hostmask, Dictionary<String, String> tags = null)
        {
            var idx = hostmask.IndexOf('!');
            this.User = idx != -1 ? hostmask.Substring(0, idx) : hostmask;
            this.Hostmask = hostmask;
            this._parameters = parameters;
            this.Command = command;
            this.Tags = tags;
        }

        public new String ToString()
        {
            var raw = new StringBuilder(32);
            if (this.Tags != null)
            {
                var tags = new String[this.Tags.Count];
                var i = 0;
                foreach (var tag in this.Tags)
                {
                    tags[i] = tag.Key + "=" + tag.Value;
                    ++i;
                }
                if (tags.Length > 0)
                {
                    raw.Append("@").Append(String.Join(";", tags)).Append(" ");
                }
            }
            if (!String.IsNullOrEmpty(this.Hostmask))
            {
                raw.Append(":").Append(this.Hostmask).Append(" ");
            }
            raw.Append(this.Command.ToString().ToUpper().Replace("RPL_", ""));
            if (this._parameters.Length <= 0) return raw.ToString();

            if (this._parameters[0] != null && this._parameters[0].Length > 0)
            {
                raw.Append(" ").Append(this._parameters[0]);
            }
            if (this._parameters.Length > 1 && this._parameters[1] != null && this._parameters[1].Length > 0)
            {
                raw.Append(" :").Append(this._parameters[1]);
            }
            return raw.ToString();
        }
    }
}