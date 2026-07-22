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
    public partial class ExpeditionOnlineMenu : ExpeditionMenu//, IChatSubscriber
    {
        public static ExpeditionGameMode expeditionGameMode;

        public ExpeditionOnlineMenu(ProcessManager manager) : base(manager)
        {
            expeditionGameMode = (ExpeditionGameMode)OnlineManager.lobby.gameMode;
            expeditionGameMode.Sanitize();

            SetCampaign(ExpeditionData.slugcatPlayer);
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
        }

        public override void Update()
        {
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
                        else
                        {
                            for (int i = 0; i < characterSelect.slugcatButtons.Length; i++)
                            {
                                characterSelect.slugcatButtons[i].buttonBehav.greyedOut = false;
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
            expeditionGameMode.currentCampaign = campaign;

            SaveGameData sgd = MineForSaveData(RWCustom.Custom.rainWorld.processManager, campaign);

            if (sgd is not null)
                expeditionGameMode.menuSaveState = new StoryLobbyData.MenuSaveStateState(sgd);
            else
                expeditionGameMode.menuSaveState = null;
        }
        public override void ShutDownProcess()
        {
            //RainMeadow.DebugMe();
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
