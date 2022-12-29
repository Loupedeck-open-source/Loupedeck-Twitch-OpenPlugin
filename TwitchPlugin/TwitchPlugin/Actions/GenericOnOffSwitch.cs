﻿namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    public abstract class GenericOnOffSwitch : PluginTwoStateDynamicCommand
    {
        public GenericOnOffSwitch(
            String name ,
            String displayName, String description, String groupName,
            String offStateName, String onStateName,
            String offStateImage, String onStateImage):base(displayName,description,groupName)
        {
            this.Name = name;

            var p = this.AddToggleCommand(
                displayName,
                EmbeddedResources.ReadImage(TwitchPlugin.ImageResPrefix + onStateImage),
                EmbeddedResources.ReadImage(TwitchPlugin.ImageResPrefix + offStateImage));

            p.Description = description;
            

            this.SetOffStateDisplayName(offStateName);
            this.SetOnStateDisplayName(onStateName);
        }

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.Connected += this.OnAppConnected;
            TwitchPlugin.Proxy.Disconnected += this.OnAppDisconnected;

            this.IsEnabled = false;
            this.ConnectAppEvents(this.AppEvtTurnedOn, this.AppEvtTurnedOff);

            return true;
        }

        protected override Boolean OnUnload()
        {
            TwitchPlugin.Proxy.Connected -= this.OnAppConnected;
            TwitchPlugin.Proxy.Disconnected -= this.OnAppDisconnected;

            this.DisconnectAppEvents(this.AppEvtTurnedOn, this.AppEvtTurnedOff);

            return true;
        }

        /// <summary>
        /// Connects command's On and Off events to the source (application)
        /// </summary>
        /// <param name="eventSwitchedOff">Event fired AFTER switch is turned off</param>
        /// <param name="eventSwitchedOn">Event fired AFTER switch is turned on</param>
        protected abstract void ConnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn);

        /// <summary>
        /// Disconnects command's On and Off events to the source (application)
        /// </summary>
        /// <param name="eventSwitchedOff">Event fired AFTER switch is turned off</param>
        /// <param name="eventSwitchedOn">Event fired AFTER switch is turned on</param>
        protected abstract void DisconnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn);

        private void OnAppConnected(Object sender, String s)
        {
            this.IsEnabled = true;
            this.AppEvtTurnedOff(sender, null); // Setting off by default
        }

        private void OnAppDisconnected(Object sender, EventArgs e) => this.IsEnabled = false;

        private Boolean _legacyActionDetected = false; 

        private void AppEvtTurnedOff(Object sender, EventArgs e)
        {
            TwitchPlugin.Trace($"Action {this.Name}: Setting state to OFF");
            this.TurnOff();
            if(this._legacyActionDetected)
            {  
                //If there were legacy calls we'll update all images
                this.ActionImageChanged();
            }
        }

        private void AppEvtTurnedOn(Object sender, EventArgs e)
        {
            TwitchPlugin.Trace($"Action {this.Name}: Setting state to ON");
            this.TurnOn();
            if (this._legacyActionDetected)
            {
                //If there were legacy calls we'll update all images
                this.ActionImageChanged();
            }
        }

        /// <summary>
        /// executes toggle at the application side.
        /// </summary>
        /// <param name="currentState">Index of current state of the control: 0 - off, 1 - on </param>
        protected abstract void RunToggle();

        protected override void RunCommand(TwoStateCommand command)
        {
            switch (command)
            {
                case TwoStateCommand.TurnOff:
                // Unimplemented

                case TwoStateCommand.TurnOn:
                    TwitchPlugin.Trace($"Action {this.Name}: On and Off direct switches not implemented");
                    break;

                case TwoStateCommand.Toggle:
                    this.RunToggle();
                    break;
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
            {
                this._legacyActionDetected = true;
                // Handling legacy (old OBS plugin) actions
                TwitchPlugin.Trace($"Legacy run for {this.Name}");
                this.RunToggle();
            }
            else
            {
                base.RunCommand(actionParameter);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (String.IsNullOrEmpty(actionParameter))
            {
                // Handling legacy (old OBS plugin) actions. For some strange reason TryGetStateIndex is not working...
                var stateIndex = this.IsTurnedOn ? 1 : 0;
                this._legacyActionDetected = true;
                TwitchPlugin.Trace($"Legacy GCM for {this.Name}, state index {stateIndex}");
                return this.GetCommandImage($"{(Int32)TwoStateCommand.Toggle}", stateIndex, imageSize);
            }
            else
            {
                return base.GetCommandImage(actionParameter, imageSize);
            }
        }
    }
}
