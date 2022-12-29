namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Loupedeck.Devices.Loupedeck2Devices;
    public class ChattersWheelTool : WheelTool
    {
        private enum State
        {
            BrowseChatters,
            ChatterSelected
        }

        private static readonly List<KeyValuePair<String, String>> ChatterInteractionCommands = new List<KeyValuePair<String, String>>
        {
            new KeyValuePair<String, String>("[...] ", String.Empty),
            new KeyValuePair<String, String>("Timeout ", ".timeout"),
            new KeyValuePair<String, String>("Ban ", ".ban"),
            new KeyValuePair<String, String>("Lift Ban/Timeout", ".unban")
        };

        private String _selectedChatter;
        private List<String> _chatters;
        private Int32 _chattersIndex;
        private Int32 _commandsIndex;
        private TwitchPlugin _plugin;
        private State _state;

        public ChattersWheelTool() : base("Twitch Chatters", "ChattersWheelTool")
        {
            this._chatters = new List<String>();
        }

        protected override void OnInit()
        {
            TwitchPlugin.PluginLog.Info("ChattersWheelTool OnInit");
            this._plugin = (TwitchPlugin)this.Plugin;
            this._state = State.BrowseChatters;
        }

        protected override void OnStart()
        {
            TwitchPlugin.PluginLog.Info("ChattersWheelTool OnStart");
            this._chatters = new List<String>(TwitchPlugin.Proxy.Chatters);
            TwitchPlugin.Proxy.ChattersChanged += this.ChattersChanged;
            this.Draw(this.CreateImage());
        }

        protected override void OnStop()
        {
            TwitchPlugin.PluginLog.Info("ChattersWheelTool OnStop");
            TwitchPlugin.Proxy.ChattersChanged -= this.ChattersChanged;
        }

        protected override void OnEncoderEvent(DeviceEncoderEvent deviceEncoderEvent)
        {
            this.Scroll(deviceEncoderEvent.Clicks > 0 ? 1 : -1);
        }

        protected override void OnTouchEvent(DeviceTouchEvent deviceTouchEvent)
        {
            switch (deviceTouchEvent.EventType)
            {
                case DeviceTouchEventType.None:
                    break;
                case DeviceTouchEventType.Tap:
                case DeviceTouchEventType.DoubleTap:
                case DeviceTouchEventType.TwoFingerTap:
                    this.ProcessTapEvent(deviceTouchEvent.IsFnPressed());
                    break;
                case DeviceTouchEventType.HorizontalSwipe:
                    break;
                case DeviceTouchEventType.VerticalSwipe:
                    this.Scroll(deviceTouchEvent.DeltaY > 0 ? -1 : 1);
                    break;
                case DeviceTouchEventType.Move:
                case DeviceTouchEventType.TouchDown:
                case DeviceTouchEventType.TouchUp:
                case DeviceTouchEventType.LongPress:
                case DeviceTouchEventType.LongRelease:
                    break;
                default:
                    return;
            }
        }

        protected override BitmapImage CreateDemoImage() => this.CreateImage();

        protected override BitmapImage CreateImage()
        {
            switch (this._state)
            {
                case State.BrowseChatters:
                    return this.DrawChatters();
                case State.ChatterSelected:
                    return this.DrawChatterInteractionCommands();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessTapEvent(Boolean isFnPressed)
        {
            if (this._chatters.Count == 0)
            {
                return;
            }

            switch (this._state)
            {
                case State.BrowseChatters:
                    this._commandsIndex = 0;
                    this._selectedChatter = this._chatters[this._chattersIndex];
                    this._state = State.ChatterSelected;
                    this.Draw(this.CreateImage());
                    break;
                case State.ChatterSelected:
                    this.ProcessChatterCommand(isFnPressed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessChatterCommand(Boolean isFnPressed)
        {
            try
            {
                if (isFnPressed || this._commandsIndex == 0)
                {
                    this.BrowseChatters();
                    return;
                }

                var command = $"{ChatterInteractionCommands[this._commandsIndex].Value} {this._selectedChatter}";
                TwitchPlugin.Proxy.SendMessage(command);
                this.BrowseChatters();
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e,"ChattersWheelTool.ProcessTapEvent error: " + e.Message);
            }
        }

        private void BrowseChatters()
        {
            this._state = State.BrowseChatters;
            this.Draw(this.CreateImage());
        }

        private BitmapImage DrawChatters() => this.DrawCollection(this._chatters, this._chattersIndex, "Chatters");

        private BitmapImage DrawChatterInteractionCommands() => this.DrawCollection(
            ChatterInteractionCommands.Select(c => this.Plugin.Localization.GetString(c.Key)).ToList(),
            this._commandsIndex);

        private BitmapImage DrawCollection(List<String> collection, Int32 index, String defaultCaption = null)
        {
            using (var bitmapBuilder = this.CreateBitmapBuilder())
            {
                if (collection.Count == 0)
                {
                    bitmapBuilder.DrawText(defaultCaption, 0, 90, 240, 60, BitmapColor.White, 28);
                }
                else
                {
                    var dimGray = new BitmapColor(105, 105, 105);

                    DrawText(index - 2, 10, 40, 10, 32, dimGray);
                    DrawText(index - 1, 50, 40, 14, 64, dimGray);
                    DrawText(index + 0, 90, 60, 20, 255, BitmapColor.White);
                    DrawText(index + 1, 150, 40, 14, 64, dimGray);
                    DrawText(index + 2, 190, 40, 10, 32, dimGray);
                }

                return bitmapBuilder.ToImage();

                void DrawText(Int32 i, Int32 y, Int32 dy, Int32 fontSize, Int32 opacity, BitmapColor color)
                {
                    if (i >= 0 && i < collection.Count)
                    {
                        bitmapBuilder.DrawText(collection[i], 0, y, 240, dy, new BitmapColor(color, opacity), fontSize);
                    }
                }
            }
        }

        private void ChattersChanged(Object sender, HashSet<String> chatters)
        {
            if (this._chatters.Count > 0)
            {
                var selectedChatter = this._chatters[this._chattersIndex];
                this._chatters = new List<String>(chatters);
                var selectedChatterIndex = this._chatters.IndexOf(selectedChatter);
                this._chattersIndex = selectedChatterIndex < 0
                    ? Math.Max(Math.Min(this._chattersIndex, this._chatters.Count - 1), 0)
                    : selectedChatterIndex;
            }
            else
            {
                this._chattersIndex = 0;
                this._chatters = new List<String>(chatters);
            }

            if (this._state == State.BrowseChatters)
            {
                this.Draw(this.CreateImage());
            }
        }

        private void Scroll(Int32 diff)
        {
            switch (this._state)
            {
                case State.BrowseChatters:
                    this.Scroll(this._chatters, ref this._chattersIndex, diff);
                    break;
                case State.ChatterSelected:
                    this.Scroll(ChatterInteractionCommands, ref this._commandsIndex, diff);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Scroll<T>(List<T> collection, ref Int32 index, Int32 diff)
        {
            if (collection.Count == 0)
            {
                return;
            }

            var newIndex = Math.Max(Math.Min(index + diff, collection.Count - 1), 0);
            if (index == newIndex)
            {
                return;
            }

            index = newIndex;
            this.Draw(this.CreateImage());
        }
    }
}