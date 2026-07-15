using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using Expedition;
using System.Runtime.ExceptionServices;
using Menu.Remix;
using Rewired;
using static Menu.SlugcatSelectMenu;

namespace RainMeadow
{
    public partial class ExpeditionOnlineMenu : ExpeditionMenu, IChatSubscriber
    {
        //List<Menu.Page> pages: SCENE SLUGCAT CHALLENGE PROGRESS
        int page;

        //public new CharacterSelectOnlinePage characterSelect;

        //public new ChallengeSelectOnlinePage challengeSelect;

        //public new ProgressionPage progressionPage;

        public static ExpeditionGameMode expeditionGameMode;
        //MenuObject slugcatSelected;

        public ExpeditionOnlineMenu(ProcessManager manager) : base(manager)
        {
            r3n.Log("ExpeditionOnlineMenu");
            //if (Expedition.Expedition.coreFile is not OnlineExpeditionCoreFile)
            //{
            //    r3n.Log("ExpeditionOnlineMenu - setting corefile");
            //    Expedition.Expedition.coreFile = new OnlineExpeditionCoreFile(Expedition.Expedition.coreFile.rainWorld);
            //}
            //  manager.
            expeditionGameMode = (ExpeditionGameMode)OnlineManager.lobby.gameMode;
            expeditionGameMode.Sanitize();


            //ExpeditionData.slugcatPlayer = SlugcatStats.Name.White;
            //currentSelection = 1;
            //if (!ExpeditionData.allChallengeLists.ContainsKey(ExpeditionData.slugcatPlayer))
            //{
            //    ExpeditionData.slugcatPlayer = SlugcatStats.Name.White;
            //}

            SetCampaign(ExpeditionData.slugcatPlayer);
            if (OnlineManager.lobby.isOwner)
            {
                expeditionGameMode.currentCampaign = ExpeditionData.slugcatPlayer;
                expeditionGameMode.saveToDisk = true;
                expeditionGameMode.slugcatSelected = currentSelection;
                //expeditionGameMode.requireCampaignSlugcat = false;
            }
            else
            {
                currentSelection = expeditionGameMode.slugcatSelected;
                expeditionGameMode.needSlugUpdate = true;
                //characterSelect.UpdateSelectedSlugcat(expeditionGameMode.slugcatSelected);

                //if (!OnlineManager.lobby.isOwner)
                //{
                //    for (int i = 0; i < characterSelect.slugcatButtons.Length; i++)
                //    {
                //        characterSelect.slugcatButtons[i].buttonBehav.greyedOut = expeditionGameMode.slugcatSelected != i;
                //    }
                //}

            }

            //csp = characterSelect;
            //chsp = challengeSelect;
            //pp = progressionPage;
        }



        //public new void InitMenuPages()
        //{
        //    r3n.Log("InitMenuPages");
        //    currentPage = 1;
        //    base.characterSelect = characterSelect = new CharacterSelectPage(this, pages[1], default(Vector2));
        //    r3n.Log($" - InitMenuPages - characterSelect: {characterSelect}");
        //    r3n.Log($" - InitMenuPages - base.characterSelect: {base.characterSelect}");
        //    pages[1].subObjects.Add(characterSelect);
        //    base.challengeSelect = challengeSelect = new ChallengeSelectPage(this, pages[2], default(Vector2));
        //    r3n.Log($" - InitMenuPages - challengeSelect: {challengeSelect}");
        //    r3n.Log($" - InitMenuPages - base.challengeSelect: {base.challengeSelect}");
        //    pages[2].subObjects.Add(challengeSelect);
        //    pages[2].pos.x += 1500f;
        //    base.progressionPage = progressionPage = new ProgressionPage(this, pages[3], default(Vector2));
        //    r3n.Log($" - InitMenuPages - progressionPage: {progressionPage}");
        //    r3n.Log($" - InitMenuPages - base.progressionPage: {base.progressionPage}");
        //    pages[3].subObjects.Add(progressionPage);
        //    pages[3].pos.x += 3000f;
        //    if (ExpeditionData.completedQuests.Count >= 75)
        //    {
        //        Custom.rainWorld.processManager.CueAchievement(RainWorld.AchievementID.Quests, 5f);
        //    }
        //}

        //public static void setMenu()
        //{

        //        r3n.Log("firstTimeLoad");
        //        firstT = true;
        //        this.InitMenuPages();
        //        if (ExpeditionData.validateQuests)
        //        {
        //            ExpeditionData.validateQuests = false;
        //            ValidateQuestRewards();
        //        }

        //}




        public override void Update()
        {
            //  firstTimeLoad = true;
            //------------------------------------------------
            base.Update();



            //------------------------------------------------
            if (OnlineManager.lobby == null) return;

            if (OnlineManager.lobby.isOwner)
            {
                // r3n.Log($"expeditionGameMode.currentCampaign: {expeditionGameMode.currentCampaign} = ExpeditionData.slugcatPlayer: {ExpeditionData.slugcatPlayer}");
                //   expeditionGameMode.currentCampaign = ExpeditionData.slugcatPlayer;
                r3n.Log($"ExpeditionData.slugcatPlayer: {ExpeditionData.slugcatPlayer}");
                //if ((characterSelect.menu as ExpeditionMenu).currentSelection >= characterSelect?.slugcatButtons.Count())
                //{
                //    (characterSelect.menu as ExpeditionMenu).currentSelection = 1;
                //}
                SetCampaign(ExpeditionData.slugcatPlayer);
                expeditionGameMode.slugcatSelected = currentSelection;
                //if (characterSelect?.menu?.selectedObject != null)
                //{
                //r3n.Log($"[{expeditionGameMode}].[{expeditionGameMode?.slugcatSelected}] = [{characterSelect}].[{characterSelect?.menu}].[{characterSelect?.menu?.selectedObject}]");
                //      r3n.Log($"^ currentSelection: {(characterSelect.menu as ExpeditionMenu)?.currentSelection}, slugcat Selected: {expeditionGameMode?.slugcatSelected}");
                //      expeditionGameMode.slugcatSelected = (characterSelect.menu as ExpeditionMenu).currentSelection;
                //}
            }
            else
            {
                currentSelection = expeditionGameMode.slugcatSelected;
            }
            if (expeditionGameMode?.slugcatSelected == null) r3n.Log($"expeditionGameMode == null");
            if ((characterSelect?.menu as ExpeditionMenu) == null) r3n.Log($"(characterSelect.menu as ExpeditionMenu) == null");
            if (expeditionGameMode?.slugcatSelected != (characterSelect?.menu as ExpeditionMenu)?.currentSelection)
            {
                //if (expeditionGameMode?.slugcatSelected != null)
                //{
                //        r3n.Log($" v currentSelection: {(characterSelect.menu as ExpeditionMenu).currentSelection}, slugcat Selected: {expeditionGameMode.slugcatSelected}");
                //        (characterSelect.menu as ExpeditionMenu).currentSelection = expeditionGameMode.slugcatSelected;
                //}
            }
            if (expeditionGameMode.needMenuSaveUpdate)
            {
                //RWCustom.Custom.rainWorld.progression.currentSaveState = expeditionGameMode.menuSaveState.;
                r3n.Log($"characterSelect.menu.manager.rainWorld.progression.IsThereASavedGame({ExpeditionData.slugcatPlayer}) {characterSelect?.menu.manager.rainWorld.progression.IsThereASavedGame(ExpeditionData.slugcatPlayer)}");
                r3n.Log($"ExpeditionData.challengeList == null: {ExpeditionData.challengeList.Count}");
                //characterSelect.menu.manager.rainWorld.progression.IsThereASavedGame(ExpeditionData.slugcatPlayer) || ExpeditionData.challengeList == null)
                //characterSelect.UpdateChallengePreview();

                characterSelect?.UpdateSelectedSlugcat((characterSelect.menu as ExpeditionMenu).currentSelection);
                // characterSelect?.ClearStats();
                // characterSelect?.UpdateStats();
                // characterSelect?.UpdateChallengePreview();
                expeditionGameMode.needMenuSaveUpdate = false;
            }
            if (characterSelect != null)
                if (expeditionGameMode.needSlugUpdate)
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

                }
        }
        public static Menu.SlugcatSelectMenu.SaveGameData? getSaveState()
        {
            return expeditionGameMode.menuSaveGameData;
        }
        public void SetCampaign(SlugcatStats.Name campaign)
        {
            if (expeditionGameMode.currentCampaign == campaign) return;
            r3n.Log($"SetCampaign: {campaign}");
            expeditionGameMode.currentCampaign = campaign;

            SaveGameData sgd = MineForSaveData(RWCustom.Custom.rainWorld.processManager, campaign);

            if (sgd is not null)
                expeditionGameMode.menuSaveState = new StoryLobbyData.MenuSaveStateState(sgd);
            else
                expeditionGameMode.menuSaveState = null;
        }
        public override void ShutDownProcess()
        {
            RainMeadow.DebugMe();
            var up = manager.upcomingProcess;
            r3n.Log($" - ShutDownProcess - manager.upcomingProcess: {up}");
            if (up != ProcessManager.ProcessID.Game && up != RainMeadow.Ext_ProcessID.ExpeditionMenu && up != ExpeditionEnums.ProcessID.ExpeditionJukebox)
            {
                r3n.Log($" -- manager.upcomingProcess != ProcessManager.ProcessID.Game ({manager.upcomingProcess != ProcessManager.ProcessID.Game}) && manager.upcomingProcess != RainMeadow.Ext_ProcessID.ExpeditionMenu ({manager.upcomingProcess != RainMeadow.Ext_ProcessID.ExpeditionMenu})");
                OnlineManager.LeaveLobby();
            }
            base.ShutDownProcess();
        }

        //{
        //    get
        //    {
        //        if (rainWorld.options.saveSlot >= 0)
        //        {
        //            return "expCore" + (rainWorld.options.saveSlot + 1);
        //        }

        //        return "expCore" + Math.Abs(rainWorld.options.saveSlot);
        //    }
        //}

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

    //public class OnlineExpeditionCoreFile : ExpeditionCoreFile
    //{
    //    public OnlineExpeditionCoreFile(RainWorld rainWorld) : base(rainWorld)
    //    {
    //        r3n.Log("OnlineExpeditionCoreFile");
    //        CORE_FILE_DEFINITION.fileName = "online_expCore";
    //    }
    //    //public new string ExpeditionSaveFileName()
    //    //{
    //    //    r3n.Log($"ExpeditionSaveFileName: {rainWorld.options.saveSlot}");
    //    //    if (rainWorld.options.saveSlot >= 0)
    //    //    {
    //    //        return "online_expCore" + (rainWorld.options.saveSlot + 1);
    //    //    }

    //    //    return "online_expCore" + Math.Abs(rainWorld.options.saveSlot);
    //    //}
    //}



    public class CharacterSelectOnlinePage : CharacterSelectPage
    {
        public CharacterSelectOnlinePage(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            r3n.Log("YAYAYAYAYAYAYAYAYAY");
            if (ModManager.JollyCoop)
            {
                new Vector2(50f, menu.manager.rainWorld.screenSize.y - 100f);
                jollyToggleConfigMenu = new SymbolButton(menu, this, "coop", "JOLLY_TOGGLE_CONFIG", new Vector2(440f, 550f));
                jollyToggleConfigMenu.roundedRect.size = new Vector2(50f, 50f);
                jollyToggleConfigMenu.size = jollyToggleConfigMenu.roundedRect.size;
                subObjects.Add(jollyToggleConfigMenu);
                jollyPlayerCountLabel = new MenuLabel(menu, this, menu.Translate("Expedition-Players").Replace("<num_p>", ValueConverter.ConvertToString(Custom.rainWorld.options.JollyPlayerCount)), jollyToggleConfigMenu.pos + new Vector2(jollyToggleConfigMenu.size.x / 2f, -20f), Vector2.zero, bigText: false);
                jollyPlayerCountLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
                subObjects.Add(jollyPlayerCountLabel);
            }
        }
    }
    public class ChallengeSelectOnlinePage : ChallengeSelectPage
    {
        public ChallengeSelectOnlinePage(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            r3n.Log("YOOOOOOOOOOOOOOOOOOO");
        }



        //public void StartGame()
        //{
        //    r3n.Log("StartGame");
        //    //if (OnlineManager.lobby.isOwner)
        //    //{                
        //    //    ExpeditionOnlineMenu.expeditionGameMode.currentCampaign = ExpeditionData.slugcatPlayer;
        //    //}

        //    var expeditionGameMode = ExpeditionOnlineMenu.expeditionGameMode;
        //    for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
        //    {
        //        expeditionGameMode.avatarSettings[i].playingAs = expeditionGameMode.currentCampaign;
        //    }

        //    base.StartGame();
        //}
    }
}
