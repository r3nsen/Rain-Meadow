using Menu;
using UnityEngine;
using Expedition;
using static Menu.SlugcatSelectMenu;
using System.Collections.Generic;
using RainMeadow.Generics;
using Steamworks;
using System.Linq;

namespace RainMeadow
{
    public partial class ExpeditionOnlineMenu : ExpeditionMenu
    {
        public static ExpeditionGameMode expeditionGameMode;
        public static List<string> activeUnlocks;
        public static List<string> challengeListStrings;
        public static ChallengeState[] currentChallengeList;

        private CheckBox friendlyFire;
        private CheckBox reqCampaignSlug;
        private MenuLabel? lobbyLabel, slugcatLabel;

        public ExpeditionOnlineMenu(ProcessManager manager) : base(manager)
        {
            expeditionGameMode = (ExpeditionGameMode)OnlineManager.lobby.gameMode;
            expeditionGameMode.Sanitize();

            //SetCampaign(ExpeditionData.slugcatPlayer);
            if (OnlineManager.lobby.isOwner)
            {
                expeditionGameMode.currentCampaign = ExpeditionData.slugcatPlayer;
                expeditionGameMode.saveToDisk = true;
                expeditionGameMode.slugcatSelected = currentSelection;

                if (MatchmakingManager.currentInstance is SteamMatchmakingManager steamMatchmakingManager)
                    SteamMatchmaking.SetLobbyData(steamMatchmakingManager.lobbyID, MatchmakingManager.CAMPAIGN_KEY, "");
            }
            else
            {
                currentSelection = expeditionGameMode.slugcatSelected;
                expeditionGameMode.needSlugUpdate = true;
            }

            RMOverlayHUD.GetOverlay()?.DestroyChatHUD();
            textAnchor = RainMeadow.rainMeadowOptions.ChatTextDownscroll.Value
                ? ButtonScroller.TextAnchor.Bottom
                : ButtonScroller.TextAnchor.Top;

            SetupOnlineMenuItens();

            // player list
            
            UpdatePlayerList();
            MatchmakingManager.OnPlayerListReceived += OnlineManager_OnPlayerListReceived;
            
            // ---

            ChatTextBox.OnShutDownRequest += ResetChatInput;
            ChatLogManager.MessageLogged += OnMessageLogged;

        }

        public override void Update()
        {
            if (nullLobbyError != null)
            {
                base.Update();
                return;
            }

            if (OnlineManager.lobby == null && nullLobbyError == null)
            {
                float x = manager.rainWorld.options.ScreenSize.x;
                float y = manager.rainWorld.options.ScreenSize.y;
                float w = 480;
                float h = 320;
                nullLobbyError = new NullLobbyError(this, pages[_currentPage], new Vector2((x - w) / 2, (y - h) / 2), new Vector2(w, h), Utils.Translate("Story lobby is null! Exiting..."), false);
                pages[_currentPage].subObjects.Add(nullLobbyError);
                return;
            }

            if (ChatTextBox.blockInput)
            {
                ChatTextBox.blockInput = false;
                if ((RWInput.CheckPauseButton(0) || Input.GetKeyDown(KeyCode.Escape)) && !lastPauseButton)
                {
                    PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                    ToggleChat(false);
                    lastPauseButton = true;
                }
                ChatTextBox.blockInput = true;
            }

            base.Update();

            UpdateUI();

            if (OnlineManager.lobby == null) return;

            if (OnlineManager.lobby.isOwner)
            {
                SetCampaign(ExpeditionData.slugcatPlayer);
                expeditionGameMode.slugcatSelected = currentSelection;

                if (characterSelect != null)
                {
                    characterSelect.abandonButton.bumpBehav.greyedOut = false;
                    if (characterSelect.confirmExpedition?.buttonBehav is not null)
                        characterSelect.confirmExpedition.buttonBehav.greyedOut = false;
                }
            }
            else
            {
                currentSelection = expeditionGameMode.slugcatSelected;
                if (characterSelect != null)
                {
                    characterSelect.abandonButton.bumpBehav.greyedOut = true;
                    if (characterSelect.confirmExpedition is not null)
                        characterSelect.confirmExpedition.buttonBehav.greyedOut = !expeditionGameMode.canJoinGame;
                }
            }

            if (expeditionGameMode.needMenuSaveUpdate)
            {
                characterSelect?.UpdateSelectedSlugcat((characterSelect.menu as ExpeditionMenu).currentSelection);
                expeditionGameMode.needMenuSaveUpdate = false;
            }

            if (characterSelect != null && expeditionGameMode.needSlugUpdate)
            {
                expeditionGameMode.needSlugUpdate = false;
                characterSelect.UpdateSelectedSlugcat(expeditionGameMode.slugcatSelected);

                if (!OnlineManager.lobby.isOwner)
                {
                    for (int i = 0; i < characterSelect.slugcatButtons.Length; i++)
                    {
                        characterSelect.slugcatButtons[i].buttonBehav.greyedOut = expeditionGameMode.slugcatSelected != i;
                    }
                }
                else
                {
                    for (int i = 0; i < characterSelect.slugcatButtons.Length; i++)
                    {
                        bool greyout = !ExpeditionGame.unlockedExpeditionSlugcats.Contains(ExpeditionGame.playableCharacters[i]);
                        characterSelect.slugcatButtons[i].buttonBehav.greyedOut = greyout;
                    }
                }
            }
            if (isChatToggled)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    if (currentLogIndex < ChatLogManager.chatLog.Count - 1)
                    {
                        currentLogIndex++;
                        UpdateLogDisplay();
                    }
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    if (currentLogIndex > 0)
                    {
                        currentLogIndex--;
                        UpdateLogDisplay();
                    }
                }
            }
        }

        public static void pre_start()
        {
            if (OnlineManager.lobby != null)
            {
                if (OnlineManager.lobby.isOwner)
                {
                    ExpeditionOnlineMenu.expeditionGameMode.currentCampaign = Expedition.ExpeditionData.slugcatPlayer;
                }

                var expeditionGameMode = ExpeditionOnlineMenu.expeditionGameMode;
                for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
                {
                    expeditionGameMode.avatarSettings[i].playingAs = expeditionGameMode.currentCampaign;
                }

                for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
                {
                    expeditionGameMode.avatarSettings[i].currentColors = [.. PlayerGraphics.DefaultBodyPartColorHex(expeditionGameMode.avatarSettings[i].playingAs).Select(RWCustom.Custom.hexToColor)];
                }
            }
            if (MatchmakingManager.currentInstance is SteamMatchmakingManager steamMatchmakingManager)
                SteamMatchmaking.SetLobbyData(steamMatchmakingManager.lobbyID, MatchmakingManager.CAMPAIGN_KEY, expeditionGameMode.currentCampaign.value);
        }

        public static Menu.SlugcatSelectMenu.SaveGameData getSaveState()
        {
            return expeditionGameMode.menuSaveGameData;
        }

        public void SetCampaign(SlugcatStats.Name campaign)
        {
            if (expeditionGameMode.currentCampaign == campaign && expeditionGameMode.menuSaveState != null) return;
            expeditionGameMode.currentCampaign = campaign;

            SaveGameData sgd = MineForSaveData(RWCustom.Custom.rainWorld.processManager, campaign);

            if (sgd is not null)
                expeditionGameMode.menuSaveState = new StoryLobbyData.MenuSaveStateState(sgd);
            else
                expeditionGameMode.menuSaveState = null;

        }

        public override void ShutDownProcess()
        {
            isChatToggled = false;
            ResetChatInput();
            ChatTextBox.OnShutDownRequest -= ResetChatInput;
            ChatLogManager.MessageLogged -= OnMessageLogged;

            RainMeadow.DebugMe();
            var up = manager.upcomingProcess;
            if (up != ProcessManager.ProcessID.Game && up != RainMeadow.Ext_ProcessID.ExpeditionMenu && up != ExpeditionEnums.ProcessID.ExpeditionJukebox)
            {
                OnlineManager.LeaveLobby();
            }
            base.ShutDownProcess();
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "EXIT")
            {
                PlaySound(SoundID.MENU_Switch_Page_Out);
                global::Expedition.Expedition.coreFile.Save(runEnded: false);
                manager.musicPlayer?.FadeOutAllSongs(100f);
                manager.RequestMainProcessSwitch(RainMeadow.Ext_ProcessID.LobbySelectMenu);
                return;
            }
            
            base.Singal(sender, message);

            if (message == "LEFT")
            {
                if(currentPage == 3)
                    UpdateOnlinePage(2);
                else if(currentPage == 2)
                    UpdateOnlinePage(1);
            }
            if (message == "RIGHT")
            {
                if (currentPage == 2)
                    UpdateOnlinePage(3);
            }
            if (message == "NEW")
            {
                if (currentPage == 1)
                    UpdateOnlinePage(2);            
            }

            
        }
    }
}
