using Expedition;

using Menu;
using MoreSlugcats;
using RainMeadow.Arena.ArenaOnlineGameModes.TeamBattle;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    public class ExpeditionGameMode : OnlineGameMode
    {
        public SlugcatCustomization[] avatarSettings;
        public int slugcatSelected;
        public ExpeditionLobbyData.ExpeditionDataState expeditionDataState;//ExpeditionData expeditionData;
        public ExpeditionGameMode(Lobby lobby) : base(lobby)
        {
            avatarSettings = new SlugcatCustomization[4];
            for (int i = 0; i < avatarSettings.Length; i++)
            {
                avatarSettings[i] = new SlugcatCustomization() { nickname = OnlineManager.mePlayer.id.name };
            }
        }

        public override ProcessManager.ProcessID MenuProcessId()
        {
            return RainMeadow.Ext_ProcessID.ExpeditionMenu;
        }

        public override void ConfigureAvatar(OnlineCreature onlineCreature)
        {
            int avatarsettings_index = 0;
            if (onlineCreature.abstractCreature.state is PlayerState state)
            {
                avatarsettings_index = state.playerNumber;
            }
            else RainMeadow.Error("No PlayerState for playerNumber");

            onlineCreature.AddData(avatarSettings[avatarsettings_index]);
            avatarSettings[avatarsettings_index].overlaySkin = AvatarData.ConfigureOverlay(onlineCreature);
        }

        public override void Customize(Creature creature, OnlineCreature oc)
        {
            if (oc.TryGetData<SlugcatCustomization>(out var data))
            {
                RainMeadow.Debug(oc);
                RainMeadow.creatureCustomizations.GetValue(creature,(c) => data);
            }
        }
        public override void ResourceAvailable(OnlineResource onlineResource)
        {
            base.ResourceAvailable(onlineResource);
            if (onlineResource is Lobby lobby)
            {
                lobby.AddData(new ExpeditionLobbyData());
            }
        }
    }

}
