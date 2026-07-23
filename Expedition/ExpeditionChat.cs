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
        private const float chatMessgesOffset = 20f;
        //Chat variables
        private List<MenuObject> chatSubObjects = [];
        private List<(string, string)> chatLog = [];
        private int currentLogIndex = 0;
        private bool isChatToggled = false;
        private ChatTextBox chatTextBox;
        private Vector2 chatTextBoxPos;
        public NullLobbyError nullLobbyError;
        private ButtonScroller.TextAnchor textAnchor;

        public void AddMessage(string user, string message)
        {
            if (OnlineManager.lobby == null) return;
            if (ChatLogManager.ShouldMuteMessageFromUser(user)) return;

            MatchmakingManager.currentInstance.FilterMessage(ref message);
            if (ChatLogManager.ShouldPingFromMessage(user, message))
            {
                manager.menuMic.PlaySound(RainMeadow.Ext_SoundID.RM_Slugcat_Call, 0f, 1f, 1.2f);
            }
            if (this.isChatToggled && ChatLogManager.ShouldMakeSoundFromMessage(user, message, out bool quiet))
            {
                manager.menuMic.PlaySound(
                    quiet ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick,
                    0,
                    quiet ? 0.7f : 1.5f,
                    quiet ? 0.7f : 0.6f
                );
            }
            this.UpdateLogDisplay();
        }


        internal void ResetChatInput()
        {
            this.chatTextBox?.DelayedUnload(0.1f);
            pages[0].ClearMenuObject(ref this.chatTextBox);
            if (this.isChatToggled && this.chatTextBox is null)
            {
                this.chatTextBox = new ChatTextBox(this, pages[0], "", new Vector2(this.chatTextBoxPos.x + 24, 0), new(575, 30));
                pages[0].subObjects.Add(this.chatTextBox);
            }
        }

        public void ToggleChat(bool toggled)
        {
            this.isChatToggled = toggled;
            this.ResetChatInput();
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
            if (ChatLogManager.chatLog.Count > 0)
            {
                int startIndex = Mathf.Clamp(ChatLogManager.chatLog.Count - maxVisibleMessages - currentLogIndex, 0, ChatLogManager.chatLog.Count - maxVisibleMessages);
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

                var visibleLog = ChatLogManager.chatLog.Skip(startIndex).Take(maxVisibleMessages);
                float yOffSet = textAnchor == ButtonScroller.TextAnchor.Top ? 0 : (maxVisibleMessages - 1 - visibleLog.Count()) * chatMessgesOffset;

                foreach (var (username, message) in visibleLog)
                {
                    ChatLogManager.SystemMessageType? systemMessageType = ChatLogManager.SysMesSignatureToType(username);
                    if (systemMessageType is not null)
                    {
                        // system message
                        var messageLabel = new MenuLabel(this, pages[0], message,
                            new Vector2(1366f - manager.rainWorld.screenSize.x - 660f, 330f - yOffSet),
                            new Vector2(manager.rainWorld.screenSize.x, 30f), false);
                        messageLabel.label.alignment = FLabelAlignment.Left;
                        messageLabel.label.color = ChatLogManager.GetColorOfSystemMessage(systemMessageType);
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
                    yOffSet += chatMessgesOffset;
                }
            }
        }

    }
}
