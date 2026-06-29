using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using Expedition;

namespace RainMeadow
{
    public partial class ExpeditionOnlineMenu : ExpeditionMenu, IChatSubscriber
    {
        //List<Menu.Page> pages: SCENE SLUGCAT CHALLENGE PROGRESS
        int page;
        // characterSelect
        CharacterSelectPage csp;
        // challengeSelect
        ChallengeSelectPage chsp;
        // progressionPage
        ProgressionPage pp;
        ExpeditionGameMode expeditionGameMode;
        //MenuObject slugcatSelected;

        public ExpeditionOnlineMenu(ProcessManager manager) : base(manager)
        {
            expeditionGameMode = (ExpeditionGameMode)OnlineManager.lobby.gameMode;
            //csp = characterSelect;
            //chsp = challengeSelect;
            //pp = progressionPage;
        }

        public override void Update()
        {
            base.Update();
            if (OnlineManager.lobby == null) return;
            if (OnlineManager.lobby.isOwner)
            {
                //if (characterSelect?.menu?.selectedObject != null)
                //{
                //r3n.Log($"[{expeditionGameMode}].[{expeditionGameMode?.slugcatSelected}] = [{characterSelect}].[{characterSelect?.menu}].[{characterSelect?.menu?.selectedObject}]");
          //      r3n.Log($"^ currentSelection: {(characterSelect.menu as ExpeditionMenu)?.currentSelection}, slugcat Selected: {expeditionGameMode?.slugcatSelected}");
          //      expeditionGameMode.slugcatSelected = (characterSelect.menu as ExpeditionMenu).currentSelection;
                //}
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
        }
    }

    public class CharacterSelectOnlinePage : CharacterSelectPage
    {
        public CharacterSelectOnlinePage(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
        }

    }
}
