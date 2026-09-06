using Menu;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using Steamworks;
using System.Linq;
using System.Globalization;

using static Menu.SlugcatSelectMenu;

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

        private bool jollyStarted;

        public ExpeditionOnlineMenu(ProcessManager manager) : base(manager)
        {
            playerSelectedSlugcats = new SlugcatStats.Name[4];
            SetupSelectableSlugcats();
            ID = OnlineManager.lobby.gameMode.MenuProcessId(); // conferir
            expeditionGameMode = (ExpeditionGameMode)OnlineManager.lobby.gameMode;
            expeditionGameMode.Sanitize();

            //SetCampaign(ExpeditionData.slugcatPlayer);
            if (OnlineManager.lobby.isOwner)
            {
                expeditionGameMode.currentCampaign = ExpeditionData.slugcatPlayer;
                expeditionGameMode.saveToDisk = true;
                expeditionGameMode.slugcatCampaingSelected = currentSelection;

                if (MatchmakingManager.currentInstance is SteamMatchmakingManager steamMatchmakingManager)
                    SteamMatchmaking.SetLobbyData(steamMatchmakingManager.lobbyID, MatchmakingManager.CAMPAIGN_KEY, "");
            }
            else
            {
                currentSelection = expeditionGameMode.slugcatCampaingSelected;
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
                expeditionGameMode.slugcatCampaingSelected = currentSelection;

                if (characterSelect != null)
                {
                    characterSelect.abandonButton.bumpBehav.greyedOut = false;
                    if (characterSelect.confirmExpedition?.buttonBehav is not null)
                        characterSelect.confirmExpedition.buttonBehav.greyedOut = false;
                }
            }
            else
            {
                currentSelection = expeditionGameMode.slugcatCampaingSelected;
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
                characterSelect.UpdateSelectedSlugcat(expeditionGameMode.slugcatCampaingSelected);

                if (!OnlineManager.lobby.isOwner)
                {
                    for (int i = 0; i < characterSelect.slugcatButtons.Length; i++)
                    {
                        characterSelect.slugcatButtons[i].buttonBehav.greyedOut = expeditionGameMode.slugcatCampaingSelected != i;
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
            if (!jollyStarted && false)
            {
                if (characterSelect != null)
                {
                    jollyStarted = true;
                    if (ModManager.JollyCoop)
                    {
                        new Vector2(50f, characterSelect.menu.manager.rainWorld.screenSize.y - 100f);
                        characterSelect.jollyToggleConfigMenu = new SymbolButton(characterSelect.menu, characterSelect, "coop", "JOLLY_TOGGLE_CONFIG", new UnityEngine.Vector2(440f, 550f));
                        characterSelect.jollyToggleConfigMenu.roundedRect.size = new UnityEngine.Vector2(50f, 50f);
                        characterSelect.jollyToggleConfigMenu.size = characterSelect.jollyToggleConfigMenu.roundedRect.size;
                        characterSelect.subObjects.Add(characterSelect.jollyToggleConfigMenu);
                        characterSelect.jollyPlayerCountLabel = new Menu.MenuLabel(characterSelect.menu, characterSelect, characterSelect.menu.Translate("Expedition-Players").Replace("<num_p>", Menu.Remix.ValueConverter.ConvertToString<int>(RWCustom.Custom.rainWorld.options.JollyPlayerCount)), characterSelect.jollyToggleConfigMenu.pos + new UnityEngine.Vector2(characterSelect.jollyToggleConfigMenu.size.x / 2f, -20f), UnityEngine.Vector2.zero, false, null);
                        characterSelect.jollyPlayerCountLabel.label.color = new UnityEngine.Color(0.7f, 0.7f, 0.7f);
                        characterSelect.subObjects.Add(characterSelect.jollyPlayerCountLabel);
                    }
                }
            }

            SetupSlugcatList();
            if (slugcatSelector != null)
            {
                slugcatSelector.Slug = PlayerSelectedSlugcat;
            }
        }

        public void pre_start()
        {
            if (OnlineManager.lobby != null)
            {
                if (OnlineManager.lobby.isOwner)
                {
                    ExpeditionOnlineMenu.expeditionGameMode.currentCampaign = Expedition.ExpeditionData.slugcatPlayer;
                }

                var expeditionGameMode = ExpeditionOnlineMenu.expeditionGameMode;

                var jollyallowed = false;// ModManager.JollyCoop;// && base.CheckJollyCoopAvailable(slugcatColorOrder[slugcatPageIndex]);
                expeditionGameMode.avatarCount = jollyallowed ? manager.rainWorld.options.JollyPlayerCount : 1;
                if (jollyallowed) PlayerGraphics.PopulateJollyColorArray(PlayerSelectedSlugcat);

                for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
                {
                    expeditionGameMode.avatarSettings[i].playingAs = expeditionGameMode.currentCampaign;
                    if (!expeditionGameMode.requireCampaignSlugcat && (playerSelectedSlugcats[i] is SlugcatStats.Name name))
                    {
                        expeditionGameMode.avatarSettings[i].playingAs = name;
                    }
                    //expeditionGameMode.avatarSettings[i].playingAs = expeditionGameMode.currentCampaign;             
                    expeditionGameMode.avatarSettings[i].currentColors = [.. PlayerGraphics.DefaultBodyPartColorHex(expeditionGameMode.avatarSettings[i].playingAs).Select(RWCustom.Custom.hexToColor)];

                    if (jollyallowed)
                    {
                        if (manager.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
                        {
                            expeditionGameMode.avatarSettings[i].currentColors = new List<Color>
                            {
                                manager.rainWorld.options.jollyPlayerOptionsArray[i].GetBodyColor(),
                                manager.rainWorld.options.jollyPlayerOptionsArray[i].GetFaceColor(),
                                manager.rainWorld.options.jollyPlayerOptionsArray[i].GetUniqueColor()
                            };

                        }
                        else if (manager.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)
                        {
                            if (i == 0)
                            {
                                expeditionGameMode.avatarSettings[i].currentColors = [.. PlayerGraphics.DefaultBodyPartColorHex(expeditionGameMode.avatarSettings[i].playingAs).Select(RWCustom.Custom.hexToColor)];
                            }
                            else
                            {
                                expeditionGameMode.avatarSettings[i].currentColors = new List<Color>
                            {
                                PlayerGraphics.JollyColor(i, 0),
                                PlayerGraphics.JollyColor(i, 1),
                                PlayerGraphics.JollyColor(i, 2)
                            };
                            }
                        }
                        else
                        {
                            expeditionGameMode.avatarSettings[i].currentColors = [.. PlayerGraphics.DefaultBodyPartColorHex(expeditionGameMode.avatarSettings[i].playingAs).Select(RWCustom.Custom.hexToColor)];
                        }
                        expeditionGameMode.avatarSettings[i].fakePup = manager.rainWorld.options.jollyPlayerOptionsArray[i].isPup;
                    }
                    else
                    {
                        // TODO: seperate custom colors for each avatar
                        RainMeadow.Debug($"currentColors: {expeditionGameMode.avatarSettings[i].currentColors} = expeditionGameMode.avatarSettings[{i}].playingAs: {expeditionGameMode.avatarSettings[i].playingAs}");
                        expeditionGameMode.avatarSettings[i].currentColors = manager.rainWorld.progression.GetCustomColors(expeditionGameMode.avatarSettings[i].playingAs); //abt colors, color config updates to campaign when required campaign is on. Client side, the host still needs to be in the menu to update it so they will notice the color config update
                        expeditionGameMode.avatarSettings[i].fakePup = true;
                    }
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

            if (message.StartsWith("MMFCUSTOMCOLOR"))
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                int num = int.Parse(message.Substring("MMFCUSTOMCOLOR".Length), NumberStyles.Any, CultureInfo.InvariantCulture);
                if (num == activeColorChooser)
                {
                    RemoveColorInterface();
                    PlaySound(SoundID.MENU_Remove_Level);
                }
                else
                {
                    activeColorChooser = num;
                    AddColorInterface();
                    PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                }
            }
            if (message == "DEFAULTCOL")
            {
                SlugcatStats.Name name = PlayerSelectedSlugcat;
                int index = activeColorChooser;
                manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][index] = colorInterface.defaultColors[activeColorChooser];
                float f = ValueOfSlider(hueSlider);
                float f2 = ValueOfSlider(satSlider);
                float f3 = ValueOfSlider(litSlider);
                SliderSetValue(hueSlider, f);
                SliderSetValue(satSlider, f2);
                SliderSetValue(litSlider, f3);
                PlaySound(SoundID.MENU_Remove_Level);
            }

            base.Singal(sender, message);

            if (message == "LEFT")
            {
                if (currentPage == 3)
                    UpdateOnlinePage(2);
                else if (currentPage == 2)
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
