using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace RainMeadow
{
    public partial class ExpeditionOnlineMenu : ExpeditionMenu, IChatSubscriber
    {
        //Chat constants
        private const int maxVisibleMessages = 13;
        //Chat variables
        private List<MenuObject> chatSubObjects = [];
        private List<(string, string)> chatLog = [];
        private int currentLogIndex = 0;
        private bool isChatToggled = false;
        private ChatTextBox chatTextBox;
        private Vector2 chatTextBoxPos;
        public NullLobbyError nullLobbyError;
        public void AddMessage(string user, string message)
        {
            if (OnlineManager.lobby == null) return;
            if (RainMeadow.rainMeadowOptions.GlobalMute.Value && user != "") return;
            if (RainMeadow.rainMeadowOptions.GlobalMute.Value) return;
            if (OnlineManager.lobby.gameMode.mutedPlayers.Contains(user)) return;
            MatchmakingManager.currentInstance.FilterMessage(ref message);
            if (RainMeadow.rainMeadowOptions.ChatPing.Value && !string.IsNullOrEmpty(user) && user != OnlineManager.mePlayer.id.GetPersonaName() && message.IndexOf(OnlineManager.mePlayer.id.DisplayName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                manager.menuMic.PlaySound(RainMeadow.Ext_SoundID.RM_Slugcat_Call, 0f, 1f, 0f);
            }
            this.chatLog.Add((user, message));
            this.UpdateLogDisplay();
        }
        internal void UpdateLogDisplay()
        {
            if (!this.isChatToggled)
            {
                var list = new List<MenuObject>();
                foreach (var e in chatSubObjects)
                {
                    e.RemoveSprites();
                    list.Add(e);
                }
                foreach (var e in list) pages[0].RemoveSubObject(e);
                chatSubObjects.Clear(); //do not keep gc stuff!
                return;
            }
            if (chatLog.Count > 0)
            {
                int startIndex = Mathf.Clamp(chatLog.Count - maxVisibleMessages - currentLogIndex, 0, chatLog.Count - maxVisibleMessages);
                var logsToRemove = new List<MenuObject>();

                // First, collect all the logs to remove
                foreach (var log in chatSubObjects)
                {
                    log.RemoveSprites();
                    logsToRemove.Add(log);
                }

                // Now remove the logs from the original collection
                foreach (var log in logsToRemove)
                {
                    chatSubObjects.Remove(log);
                    pages[0].RemoveSubObject(log);
                }

                ChatLogManager.UpdatePlayerColors();

                float yOffSet = 0;
                var visibleLog = chatLog.Skip(startIndex).Take(maxVisibleMessages);
                foreach (var (username, message) in visibleLog)
                {
                    if (username is null or "")
                    {
                        // system message
                        var messageLabel = new MenuLabel(this, pages[0], message,
                            new Vector2(1366f - manager.rainWorld.screenSize.x - 660f, 330f - yOffSet),
                            new Vector2(manager.rainWorld.screenSize.x, 30f), false);
                        messageLabel.label.alignment = FLabelAlignment.Left;
                        messageLabel.label.color = ChatLogManager.defaultSystemColor;
                        chatSubObjects.Add(messageLabel);
                        pages[0].subObjects.Add(messageLabel);
                    }
                    else
                    {
                        var color = ChatLogManager.GetDisplayPlayerColor(username);

                        var usernameLabel = new MenuLabel(this, pages[0], username,
                            new Vector2(1366f - manager.rainWorld.screenSize.x - 660f, 330f - yOffSet),
                            new Vector2(manager.rainWorld.screenSize.x, 30f), false);
                        usernameLabel.label.alignment = FLabelAlignment.Left;
                        usernameLabel.label.color = color;
                        chatSubObjects.Add(usernameLabel);
                        pages[0].subObjects.Add(usernameLabel);

                        var usernameWidth = LabelTest.GetWidth(usernameLabel.label.text);
                        var messageLabel = new MenuLabel(this, pages[0], $": {message}",
                            new Vector2(1366f - manager.rainWorld.screenSize.x - 660f + usernameWidth + 2f, 330f - yOffSet),
                            new Vector2(manager.rainWorld.screenSize.x, 30f), false);
                        messageLabel.label.alignment = FLabelAlignment.Left;
                        chatSubObjects.Add(messageLabel);
                        pages[0].subObjects.Add(messageLabel);
                    }
                    yOffSet += 20f;
                }
            }
        }

    }
}
