using Menu;
using UnityEngine;
using Expedition;
using static Menu.SlugcatSelectMenu;
using System.Collections.Generic;
using RainMeadow.Generics;

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
            }
            else
            {
                currentSelection = expeditionGameMode.slugcatSelected;
                expeditionGameMode.needSlugUpdate = true;
            }

            // implement MatchmakingManager.OnPlayerListReceived += OnlineManager_OnPlayerListReceived;

            RMOverlayHUD.GetOverlay()?.DestroyChatHUD();
            textAnchor = RainMeadow.rainMeadowOptions.ChatTextDownscroll.Value
                ? ButtonScroller.TextAnchor.Bottom
                : ButtonScroller.TextAnchor.Top;

            SetupOnlineMenuItens();

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
                nullLobbyError = new NullLobbyError(this, pages[0], new Vector2((x - w) / 2, (y - h) / 2), new Vector2(w, h), Utils.Translate("Story lobby is null! Exiting..."), false);
                pages[0].subObjects.Add(nullLobbyError);
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
                        characterSelect.slugcatButtons[i].buttonBehav.greyedOut = false;
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
        }
    }
}
