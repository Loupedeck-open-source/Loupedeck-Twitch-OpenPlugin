namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>Object representing emote set from a chat message.</summary>
    public class EmoteSet
    {
        /// <summary>List containing all emotes in the message.</summary>
        public List<Emote> Emotes { get; }
        /// <summary>The raw emote set string obtained from Twitch, for legacy purposes.</summary>
        public String RawEmoteSetString { get; }

        /// <summary>Constructor for ChatEmoteSet object.</summary>
        /// <param name="emoteSetData"></param>
        /// <param name="message"></param>
        public EmoteSet(String emoteSetData, String message)
        {
            this.Emotes = new List<Emote>();
            this.RawEmoteSetString = emoteSetData;
            if (String.IsNullOrEmpty(emoteSetData))
                return;
            if (emoteSetData.Contains("/"))
            {
                // Message contains multiple different emotes, first parse by unique emotes: 28087:15-21/25:5-9,28-32
                foreach (var emoteData in emoteSetData.Split('/'))
                {
                    var emoteId = Int32.Parse(emoteData.Split(':')[0]);
                    if (emoteData.Contains(","))
                    {
                        // Multiple copies of a single emote: 25:5-9,28-32
                        foreach (var emote in emoteData.Replace($"{emoteId}:", "").Split(','))
                            this.AddEmote(emote, emoteId, message);

                    }
                    else
                    {
                        // Single copy of single emote: 25:5-9/28087:16-22
                        this.AddEmote(emoteData, emoteId, message, true);
                    }
                }
            }
            else
            {
                var emoteId = Int32.Parse(emoteSetData.Split(':')[0]);
                // Message contains a single, or multiple of the same emote
                if (emoteSetData.Contains(","))
                {
                    // Multiple copies of a single emote: 25:5-9,28-32
                    foreach (var emote in emoteSetData.Replace($"{emoteId}:", "").Split(','))
                        this.AddEmote(emote, emoteId, message);
                } else
                {
                    // Single copy of single emote: 25:5-9
                    this.AddEmote(emoteSetData, emoteId, message, true);
                }
            }
        }

        private void AddEmote(String emoteData, Int32 emoteId, String message, Boolean single = false)
        {
            Int32 startIndex = -1, endIndex = -1;
            if (single)
            {
                startIndex = Int32.Parse(emoteData.Split(':')[1].Split('-')[0]);
                endIndex = Int32.Parse(emoteData.Split(':')[1].Split('-')[1]);
            } else
            {
                startIndex = Int32.Parse(emoteData.Split('-')[0]);
                endIndex = Int32.Parse(emoteData.Split('-')[1]);
            }
            this.Emotes.Add(new Emote(emoteId, message.Substring(startIndex, (endIndex - startIndex) + 1), startIndex, endIndex));
        }

        /// <summary>
        /// Object representing an emote in an EmoteSet in a chat message.
        /// </summary>
        public class Emote
        {
            /// <summary>Twitch-assigned emote Id.</summary>
            public Int32 Id { get; }
            /// <summary>The name of the emote. For example, if the message was "This is Kappa test.", the name would be 'Kappa'.</summary>
            public String Name { get; }
            /// <summary>Character starting index. For example, if the message was "This is Kappa test.", the start index would be 8 for 'Kappa'.</summary>
            public Int32 StartIndex { get; }
            /// <summary>Character ending index. For example, if the message was "This is Kappa test.", the start index would be 12 for 'Kappa'.</summary>
            public Int32 EndIndex { get; }
            /// <summary>URL to Twitch hosted emote image.</summary>
            public String ImageUrl { get; }

            /// <summary>
            /// Emote constructor.
            /// </summary>
            /// <param name="emoteId"></param>
            /// <param name="name"></param>
            /// <param name="emoteStartIndex"></param>
            /// <param name="emoteEndIndex"></param>
            public Emote(Int32 emoteId, String name, Int32 emoteStartIndex, Int32 emoteEndIndex)
            {
                this.Id = emoteId;
                this.Name = name;
                this.StartIndex = emoteStartIndex;
                this.EndIndex = emoteEndIndex;
                this.ImageUrl = $"https://static-cdn.jtvnw.net/emoticons/v1/{emoteId}/1.0";
            }
        }
    }
}
