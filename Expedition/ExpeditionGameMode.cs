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
    public class ExpeditionGameMode : StoryGameMode//OnlineGameMode//
    {
        public int slugcatSelected;
        public bool needSlugUpdate = false;
        public bool hasSaveState;
        //public bool isInGame;        
        //public bool readyForWin;

        //// these are synced by StoryLobbyData
        //public bool isInGame = false;
        //public bool changedRegions = false;
        //public bool readyForWin = false;
        //public enum ReadyForTransition : byte
        //{
        //    Closed,
        //    MeetRequirement,
        //    Opening,
        //    Crossed,
        //}
        //public ReadyForTransition readyForTransition = ReadyForTransition.Closed;
        //public bool friendlyFire = false;
        //public string? defaultDenPos;
        //public string? region = null;
        public SlugcatStats.Name currentCampaign;
        //public SlugcatStats.Name preferredSlug;
        //public bool requireCampaignSlugcat;
        //public string? saveStateString;
        //public bool lastWarpIsEcho = false;

        //// TODO: split these out for other gamemodes to reuse (see Story/StoryMenuHelpers for methods)
        //public Dictionary<string, bool> storyBoolRemixSettings;
        //public Dictionary<string, float> storyFloatRemixSettings;
        //public Dictionary<string, int> storyIntRemixSettings;

        ////public SlugcatCustomization[] avatarSettings;
        //public int avatarCount { get; set; } = 1;

        //public StoryClientSettingsData storyClientData;
        //public Watcher.WarpPoint.WarpPointData? myLastWarp = null; //yeah watcher gonna watch
        //public string? myLastDenPos = null;
        //public bool hasSheltered = false;
        //public float rippleLevel;

        //public List<AbstractCreature> pups;

        //public StoryLobbyData.MenuSaveStateState? menuSaveState;
        //public SlugcatSelectMenu.SaveGameData? menuSaveGameData;
        //public bool needMenuSaveUpdate = false;


      //  public bool saveToDisk = true;

        //public void Sanitize()
        //{
        //    hasSheltered = false;
        //    isInGame = false;
        //    changedRegions = false;
        //    readyForWin = false;
        //    readyForTransition = ReadyForTransition.Closed;
        //    defaultDenPos = null;
        //    myLastWarp = null;
        //    myLastDenPos = null;
        //    lastWarpIsEcho = false;
        //    region = null;
        //    saveStateString = null;
        //    pups = new();
        //    storyClientData?.Sanitize();
        //    rippleLevel = 0.0f;
        //    this.ResetOverWorld();

        //}
        //
        public SlugcatCustomization[] avatarSettings;
        //public int slugcatSelected;
        public ExpeditionLobbyData.ExpeditionDataState expeditionDataState;//ExpeditionData expeditionData;
        public ExpeditionGameMode(Lobby lobby) : base(lobby)
        {
            avatarSettings = new SlugcatCustomization[4];
            for (int i = 0; i < avatarSettings.Length; i++)
            {
                avatarSettings[i] = new SlugcatCustomization() { nickname = OnlineManager.mePlayer.id.name };
            }
        }

        public override bool AllowedInMode(PlacedObject item)
        {
            return true;
        }
        public override bool ShouldLoadCreatures(RainWorldGame game, WorldSession worldSession)
        {
            //return false;
            if (OnlineManager.mePlayer.isActuallySpectating)
            {
                return false;
            }
            return worldSession.owner == null || worldSession.isOwner;
        }
        public override bool ShouldSpawnRoomItems(RainWorldGame game, RoomSession roomSession)
        {
            //return false;
            if (OnlineManager.mePlayer.isActuallySpectating)
            {
                return false;
            }
            return roomSession.owner == null || roomSession.isOwner;
        }
        public override bool ShouldRegisterAPO(OnlineResource resource, AbstractPhysicalObject apo)
        {
            //return false;
            return true;
        }
        public override bool ShouldSyncAPOInRoom(RoomSession rs, AbstractPhysicalObject apo)
        {
            //return false;
            return true;
        }
        public override bool ShouldSyncAPOInWorld(WorldSession ws, AbstractPhysicalObject apo)
        {
            //return false;
            return true;
        }
        public override SlugcatStats.Name GetStorySessionPlayer(RainWorldGame self)
        {
            return currentCampaign;
        }
        public override SlugcatStats.Name LoadWorldAs(RainWorldGame game)
        {
            return currentCampaign;
        }
        public override SlugcatStats.Timeline LoadWorldIn(RainWorldGame game)
        {
            return game?.GetStorySession.saveState.currentTimelinePosition ?? SlugcatStats.SlugcatToTimeline(currentCampaign); // need to understand what this do
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
            //base.ResourceAvailable(onlineResource);
            RainMeadow.Debug(onlineResource);
            if (onlineResource is Lobby lobby)
            {
                lobby.AddData(new ExpeditionLobbyData());
            }
        }
       
        public override AbstractCreature SpawnAvatar(RainWorldGame self, WorldCoordinate location)
        {
            try
            {
                r3n.Log($"SpawnAvatar");
                AbstractCreature creature;
                AbstractCreature mainAvatar = null;
                for (int i = 0; i < self.StoryPlayerCount; i++)
                {
                    r3n.Log($" - SpawnAvatar - count: {self.StoryPlayerCount}");
                    creature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, i));
                    r3n.Log($"- SpawnAvatar - creature state: {creature.state}");
                    creature.state = new PlayerState(creature, i, avatarSettings[i].playingAs, false) { isPup = avatarSettings[i].fakePup }; // look at avatarSettings
                    r3n.Log($"- SpawnAvatar - creature state: {creature.state}, {creature.state.GetType()}, {creature.state is null}");
                    r3n.Log($"- SpawnAvatar - creature state: {new PlayerState(creature, i, avatarSettings[i].playingAs, false) { isPup = avatarSettings[i].fakePup }} - new PlayerState({creature}, {i}, avatarSettings[{i}].playingAs: {avatarSettings[i].playingAs}, false) {{{avatarSettings[i].fakePup}}}");
                    self.world.GetAbstractRoom(creature.pos.room).AddEntity(creature);
                    self.session.AddPlayer(creature);
                    r3n.Log($"- SpawnAvatar - creature: {creature}");
                    r3n.Log($"- SpawnAvatar - creature state: {creature.state}");
                    if (i == 0) mainAvatar = creature;
                }
                if (mainAvatar is null) throw new InvalidProgrammerException("Main avatar is null");
                return mainAvatar;
            }
            catch (Exception e)
            {
                r3n.Log($"ERROR: {e.Message} {e.StackTrace} {e.InnerException}");
                throw new InvalidProgrammerException("youre dumb");
            }
            
        }
    }

}
        