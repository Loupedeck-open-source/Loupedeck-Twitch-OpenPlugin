//namespace Loupedeck.TwitchPlugin
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;

//    public class ChattersBrowserWorkspace
//    {
//        private const String ChattersBrowserTouchAction = "ChattersBrowserTouchAction";
//        private const String ChattersBrowserWorkspaceName = "ChattersBrowserWorkspace";
//        private const String ChattersBrowserTouchPageName = "ChattersBrowserTouchPage";
//        private const String SelectedChatterTouchPageName = "SelectedChatterTouchAction";
//        private const String GroupName = "Chatters";

//        private enum State
//        {
//            BrowseChatters,
//            ChatterSelected
//        }

//        private readonly List<KeyValuePair<String, String>> _chatterInteractionCommands;
//        private readonly PluginActionParameter[] _chatterSelectedActions;
//        private readonly TwitchPlugin _plugin;

//        private String _selectedChatter;
//        private State _state;
//        private PluginActionParameter[] _browseChattersActions;

//        public ChattersBrowserWorkspace(TwitchPlugin plugin)
//        {
//            this._browseChattersActions = Array.Empty<PluginActionParameter>();
//            this._state = State.BrowseChatters;
//            this._plugin = plugin;
//            this._chatterInteractionCommands = ChattersBrowserHelper.ChatterInteractionCommands;
//            this._chatterSelectedActions = this._chatterInteractionCommands.Select((vic, i) => new PluginActionParameter(i.ToString(), vic.Key, GroupName)).ToArray();
//        }

//        public PluginActionParameter[] GetPluginActionParameters()
//        {
//            switch (this._state)
//            {
//                case State.BrowseChatters:
//                    return this._browseChattersActions;
//                case State.ChatterSelected:
//                    return this._chatterSelectedActions;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        public void RunCommand(String commandName, String parameter)
//        {
//            if (!commandName.Equals(ChattersBrowserTouchAction))
//            {
//                return;
//            }

//            var layout = this._plugin.Configuration2.GetLayout("Main");
//            switch (this._state)
//            {
//                case State.BrowseChatters:
//                    this.ProcessBrowserCommand(parameter, layout);
//                    break;
//                case State.ChatterSelected:
//                    this.ProcessChatterCommand(parameter, layout);
//                    break;
//            }
//        }

//        public void UpdateChatters(HashSet<String> chatters)
//        {
//            var layout = this._plugin.Configuration2.GetLayout("Main");
//            layout.TouchPages.RemoveAll(p => p.Name.StartsWith(ChattersBrowserTouchPageName));
//            var touchActions = chatters.Select(v => new PluginActionParameter(v, v, GroupName)).ToArray();

//            var touchPages = GetTouchPageLayout(12, ChattersBrowserTouchPageName, ChattersBrowserTouchAction, touchActions);
//            layout.TouchPages.AddRange(touchPages);
//            this._browseChattersActions = touchActions;
//            this.UpdateWorkspace(layout);
//        }

//        private void UpdateWorkspace(PluginLayout2 layout)
//        {
//            var showChattersWorkspace = layout.Workspaces.FirstOrDefault(w => w.Name == ChattersBrowserWorkspaceName);
//            if (showChattersWorkspace == null)
//            {
//                showChattersWorkspace = new PluginLayout2Workspace(ChattersBrowserWorkspaceName, "Show Chatters Workspace");
//                layout.Workspaces.AddItem(showChattersWorkspace);
//            }


//            showChattersWorkspace.TouchPages.Clear();
//            List<PluginLayout2TouchPage> touchPages;
//            switch (this._state)
//            {
//                case State.BrowseChatters:
//                    touchPages = layout.TouchPages.Where(p => p.Name.StartsWith(ChattersBrowserTouchPageName)).ToList();
//                    break;
//                case State.ChatterSelected:
//                    touchPages = layout.TouchPages.Where(p => p.Name.StartsWith(SelectedChatterTouchPageName)).ToList();
//                    if (touchPages.Count == 0)
//                    {
//                        touchPages = GetTouchPageLayout(12, SelectedChatterTouchPageName, ChattersBrowserTouchAction, this._chatterSelectedActions);
//                        layout.TouchPages.AddRange(touchPages);
//                    }
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }

//            showChattersWorkspace.TouchPages.AddRange(touchPages.Select(tp => tp.Name));
//            showChattersWorkspace.AddMissingItems();
//            layout.AddMissingItems();
//            layout.PluginConfiguration.SendModifiedEvent();
//        }

//        private static List<PluginLayout2TouchPage> GetTouchPageLayout(Int32 maxPages, String pageName, String actionName, PluginActionParameter[] actions)
//        {
//            if (actions == null)
//            {
//                return new List<PluginLayout2TouchPage> { new PluginLayout2TouchPage(pageName + Guid.NewGuid(), pageName + "Clear") };
//            }

//            var layoutItems = new List<PluginLayout2TouchPage>();
//            var pageNumber = 1;

//            for (var i = 0; i < actions.Length; i += maxPages)
//            {
//                var segmentCount = maxPages;
//                if (i + maxPages > actions.Length)
//                {
//                    segmentCount = actions.Length - i;
//                }

//                var encodersSegment = new ArraySegment<PluginActionParameter>(actions, i, segmentCount);
//                var newPage = new PluginLayout2TouchPage(pageName + Guid.NewGuid(), pageName + pageNumber);
//                newPage.Commands.AddRange(encodersSegment.Select(s => new PluginLayout2PageItem(actionName, s.Value)));
//                layoutItems.Add(newPage);
//                pageNumber++;
//            }

//            return layoutItems;
//        }

//        private void ProcessBrowserCommand(String parameter, PluginLayout2 layout)
//        {
//            this._selectedChatter = parameter;
//            this._state = State.ChatterSelected;
//            this.UpdateWorkspace(layout);
//        }

//        private void ProcessChatterCommand(String parameter, PluginLayout2 layout)
//        {
//            if (!Int32.TryParse(parameter, out var chatterInteractionCommandIndex))
//            {
//                return;
//            }

//            if (chatterInteractionCommandIndex == 0)
//            {
//                this._state = State.BrowseChatters;
//                this.UpdateWorkspace(layout);
//                return;
//            }

//            var command = $"{this._chatterInteractionCommands[chatterInteractionCommandIndex].Value} {this._selectedChatter}";

//            try
//            {
//                this._plugin.TwitchWrapper.SendMessage(command);
//                this._state = State.BrowseChatters;
//                this.UpdateWorkspace(layout);
//            }
//            catch (Exception e)
//            {
//                TwitchPlugin.PluginLog.Warning("ChatterBrowserWorkspace.ProcessTapEvent error: " + e.Message, e);
//            }
//        }
//    }
//}