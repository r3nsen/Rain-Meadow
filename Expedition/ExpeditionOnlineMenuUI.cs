using Menu;
using Menu.Remix.MixedUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    public partial class ExpeditionOnlineMenu : ExpeditionMenu
    {
        private SlugcatStats.Name[] selectableSlugcats;
        public SlugcatStats.Name?[] playerSelectedSlugcats;

        SimplerSymbolButton toggleChat;
        //
        private ButtonScroller? playerScrollBox;
        private Vector2 playerScrollBoxPos;
        public static int MaxVisibleOnList => 8;
        public static float ButtonSpacingOffset => 8;
        public static float ButtonSizeWithSpacing => ButtonSize + ButtonSpacingOffset;
        public static float ButtonSize => 30;
        int _currentPage = 1;
        //

        private Vector2 lobbylabelPos;
        private ChatTextBox chatTextBox;
        private Vector2 chatTextBoxPos;

        bool _pagesMoving;

        void SetupOnlineMenuItens()
        {
            lobbylabelPos = new Vector2(manager.rainWorld.screenSize.x - 190, 553);
            lobbyLabel = new MenuLabel(this, pages[_currentPage], Translate("LOBBY"), lobbylabelPos, new(110, 30), true);
            pages[_currentPage].subObjects.Add(lobbyLabel);

            this.chatTextBoxPos = new Vector2(this.manager.rainWorld.options.ScreenSize.x * 0.001f + (1366f - this.manager.rainWorld.options.ScreenSize.x) / 2f, 0);
            toggleChat = new SimplerSymbolButton(this, pages[_currentPage], "Kill_Slugcat", "", this.chatTextBoxPos);
            toggleChat.OnClick += (_) =>
            {
                ToggleChat(!this.isChatToggled);
                if (input.controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer)
                {
                    selectedObject = null;
                }
            };
            pages[_currentPage].subObjects.Add(toggleChat);
        }

        // update()
        //  - float num5 = ((this.manager.rainWorld.options.ScreenSize.x != 1024f) ? 695f : 728f);
        //  - this.manualButton.pos = new global::UnityEngine.Vector2(this.rightAnchor - (this.leftAnchor + 150f), num5) - this.manualButton.page.pos;
        //  - this.manualButton.lastPos = new global::UnityEngine.Vector2(this.rightAnchor - (this.leftAnchor + 150f), num5) - this.manualButton.page.lastPos;

        // updatePage()
        //  - this.manualButton.RemoveSprites();
        //  - this.manualButton.RemoveSubObject(this.manualButton);
        //  - this.manualButton = new global::Menu.SimpleButton(this, this.pages[this.currentPage], base.Translate("MANUAL"), "MANUAL", new global::UnityEngine.Vector2(this.rightAnchor - 150f, 695f), new global::UnityEngine.Vector2(100f, 30f));

        private void UpdatePlayerList()
        {
            playerScrollBox?.RemoveAllButtons(false);
            if (playerScrollBox == null)
            {
                playerScrollBoxPos = new(manager.rainWorld.screenSize.x - 190, 553 - 30 - ButtonScroller.CalculateHeightBasedOnAmtOfButtons(MaxVisibleOnList, ButtonSize, ButtonSpacingOffset));
                playerScrollBox = new(this, pages[_currentPage], playerScrollBoxPos, MaxVisibleOnList, 200, new(ButtonSize, ButtonSpacingOffset));
                pages[_currentPage].subObjects.Add(playerScrollBox);
            }
            foreach (OnlinePlayer player in OnlineManager.players)
            {
                StoryMenuPlayerButton playerButton = new(this, playerScrollBox, player, OnlineManager.lobby.isOwner && player != OnlineManager.lobby.owner);
                playerScrollBox.AddScrollObjects(playerButton);
            }
            playerScrollBox.ConstrainScroll();

        }

        private void OnlineManager_OnPlayerListReceived(PlayerInfo[] players)
        {
            if (RainMeadow.isExpeditionMode(out var _))
            {
                UpdatePlayerList();
            }
        }

        private void UpdateUI()
        {
            if (this.pagesMoving)
            {
                // Everything gets out of alignment here :rmdead:
                _pagesMoving = pagesMoving;

                lobbyLabel.pos = lobbylabelPos - lobbyLabel.page.pos;
                lobbyLabel.lastPos = lobbylabelPos - lobbyLabel.page.lastPos;

                chatTextBox?.pos = chatTextBoxPos + new Vector2(24, 0) - chatTextBox.page.pos;
                chatTextBox?.lastPos = chatTextBoxPos + new Vector2(24, 0) - chatTextBox.page.lastPos;

                chatTextBox?.roundedRect.pos = new Vector2(24 + 4.4f, 0) - chatTextBox.page.pos;
                chatTextBox?.roundedRect.lastPos = new Vector2(24 + 4.4f, 0) - chatTextBox.page.lastPos;

                toggleChat.pos = chatTextBoxPos + new Vector2(-2.2f, 0) - toggleChat.page.pos;
                toggleChat.lastPos = chatTextBoxPos + new Vector2(-2.2f, 0) - toggleChat.page.lastPos;

                playerScrollBox.pos = playerScrollBoxPos - playerScrollBox.page.pos;
                playerScrollBox.lastPos = playerScrollBoxPos - playerScrollBox.page.lastPos;

            }
        }
        void UpdateOnlinePage(int pageIndex)
        {            
                _currentPage = pageIndex;
            
                pages[currentPage].ClearMenuObject(ref lobbyLabel);

                pages[currentPage].ClearMenuObject(ref toggleChat);

                playerScrollBox?.RemoveAllButtons(false);
                pages[currentPage].ClearMenuObject(ref playerScrollBox);
                
                this.chatTextBox?.DelayedUnload(0.1f);
                pages[currentPage].ClearMenuObject(ref chatTextBox);

                playerScrollBox = null;
                
                SetupOnlineMenuItens();
                UpdatePlayerList();
                ResetChatInput();

        }
    }
}
