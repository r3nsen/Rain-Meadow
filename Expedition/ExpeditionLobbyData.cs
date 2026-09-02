using Expedition;
using RainMeadow.Generics;
using System;
using System.Collections.Generic;
using System.Linq;

using static RainMeadow.OnlineResource;
using static RainMeadow.StoryLobbyData;

namespace RainMeadow;

public class ExpeditionLobbyData : OnlineResource.ResourceData
{
    public override ResourceDataState MakeState(OnlineResource resource)
    {
        return new State(this, resource);
    }

    public class State : ResourceDataState
    {
        [OnlineField]
        SlugcatStats.Name currentCampaign;
        
        [OnlineField]
        public int selectedSlugcat;

        [OnlineField]
        byte food; // 0bffffffqq
        
        [OnlineField]
        bool isInGame;
        
        [OnlineField]
        bool readyForWin;
        
        [OnlineField]
        bool hasSaveState;
        
        [OnlineField]
        public byte readyForTransition;

        [OnlineField]
        public int mushroomCounter;
        
        [OnlineFieldHalf]
        public float slowTimeCooldown;

        [OnlineFieldHalf]
        public float challengeDifficulty;
        
        [OnlineField]
        public bool newGame;
        
        [OnlineField]
        public bool validateQuests;

        [OnlineField(nullable = true)]
        public string? activeMission;
        
        [OnlineField(nullable = true)]
        public string? startingDen;

        [OnlineField(group = "saveString", nullable = true)]
        public string? saveStateString;

        [OnlineField(group = "menusave", nullable = true)]
        public MenuSaveStateState? currentMenuSaveState;

        [OnlineField(group = "expeditiondata")]
        bool[] isChallengeCompleted;

        [OnlineField(group = "expeditiondata")]
        DynamicOrderedStates<ChallengeState>? currentChallengeList;

        [OnlineField(nullable = true, group = "activeUnlocks")]
        public List<string> activeUnlocks = new List<string>();

        public State() { }
        public State(ExpeditionLobbyData expeditionLobbyData, OnlineResource onlineResource)
        {         
            ExpeditionGameMode expeditionGameMode = (ExpeditionGameMode)((Lobby)onlineResource).gameMode;
            RainWorldGame currentGameState = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;

            PlayerState ps = currentGameState?.Players[0].state as PlayerState;            

            food = (byte)(((ps?.foodInStomach ?? 0) << 2) | ps?.quarterFoodPoints ?? 0 & 3);
            mushroomCounter = (currentGameState?.Players[0].realizedCreature as Player)?.mushroomCounter ?? 0;

            for (int i = 0; i < ExpeditionGame.unlockTrackers.Count; i++)
            {
                if (ExpeditionGame.unlockTrackers[i] is ExpeditionGame.SlowTimeTracker stt)
                    slowTimeCooldown = stt.cooldown;
            }

            isInGame = RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame && RWCustom.Custom.rainWorld.processManager.upcomingProcess is null;
            readyForWin = expeditionGameMode.readyForWin;
            readyForTransition = (byte)expeditionGameMode.readyForTransition;
            saveStateString = expeditionGameMode.saveStateString;
            selectedSlugcat = expeditionGameMode.slugcatSelected;
            currentCampaign = expeditionGameMode.currentCampaign;

            // seems like currentCampaign could be null - oh, jukebox can set this to null too? i had a bug where host was on jukebox and client on infinite loop trying to join until esc
            if (currentCampaign is not null) // hmmm - ExpeditionOnlineMenu.expeditionGameMode is not null && ExpeditionOnlineMenu.expeditionGameMode.
            {
                List<ChallengeState> challengeStateList = new List<ChallengeState>();
                foreach (Challenge challenge in ExpeditionData.allChallengeLists[currentCampaign])
                {
                    challengeStateList.Add(GetChallengeState(challenge));
                }
                currentChallengeList = new DynamicOrderedStates<ChallengeState>(challengeStateList);
            }
            
            activeUnlocks = new List<string>(ExpeditionGame.activeUnlocks);
            challengeDifficulty = ExpeditionData.challengeDifficulty;                
            newGame = ExpeditionData.newGame;
            validateQuests = ExpeditionData.validateQuests;
            activeMission = ExpeditionData.activeMission;
            startingDen = ExpeditionData.startingDen;

            if (currentGameState?.session is StoryGameSession storySession)
            {
                currentMenuSaveState = new MenuSaveStateState(storySession.saveState);                
                hasSaveState = saveStateString != null;
            }
            else
            {
                hasSaveState = expeditionGameMode.hasSaveState;
                //if (currentMenuSaveState != expeditionGameMode.menuSaveState)
                //{
                currentMenuSaveState = expeditionGameMode.menuSaveState;
                //}                
            }

            if (ExpeditionOnlineMenu.expeditionGameMode is not null)
            {
                int cclCount = currentChallengeList.list.Count;

                if (ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted == null || ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted.Length != cclCount)
                {
                    ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted = new bool[cclCount];
                }

                isChallengeCompleted = new bool[cclCount];

                for (int i = 0; i < cclCount; i++)
                {
                    isChallengeCompleted[i] = ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted[i] = currentChallengeList.list[i].completed;
                }
            }
        }

        public override void ReadTo(ResourceData data, OnlineResource resource)
        {
            Lobby lobby = (Lobby)resource;
            ExpeditionGameMode expedition = (ExpeditionGameMode)lobby.gameMode;
            expedition.currentCampaign = currentCampaign;

            if (RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame currentGameState)
            {
                PlayerState ps = (PlayerState)currentGameState.Players[0].state; // TODO: jolly support

                int _food = food >> 2;
                int _quarterfood = food & 3;

                ps.foodInStomach = _food;
                ps.quarterFoodPoints = _quarterfood;

                if ((currentGameState?.Players[0].realizedCreature is Player player))
                {
                    player.mushroomCounter = mushroomCounter;
                    player.AddFood(0);
                }

                for (int i = 0; i < ExpeditionGame.unlockTrackers.Count; i++)
                {
                    if (ExpeditionGame.unlockTrackers[i] is ExpeditionGame.SlowTimeTracker stt)
                        stt.cooldown = slowTimeCooldown;
                }
            }

            expedition.isInGame = isInGame;
            expedition.readyForWin = readyForWin;
            expedition.readyForTransition = (StoryGameMode.ReadyForTransition)readyForTransition;
            expedition.saveStateString = saveStateString;

            expedition.hasSaveState = hasSaveState;

            if (expedition.slugcatSelected != selectedSlugcat)
            {
                expedition.slugcatSelected = selectedSlugcat;
                expedition.needSlugUpdate = true;
            }

            // if (expedition.expeditionDataState != expeditionDataState)
            {
                ExpeditionData.challengeDifficulty = challengeDifficulty;                
                ExpeditionData.newGame = newGame;
                ExpeditionData.validateQuests = validateQuests;
                ExpeditionData.activeMission = activeMission;
                ExpeditionData.startingDen = startingDen;
            
                ExpeditionOnlineMenu.activeUnlocks = activeUnlocks;                

                if (!ExpeditionGame.expeditionComplete)
                {
                    if (!ExpeditionData.allChallengeLists.ContainsKey(currentCampaign) || ExpeditionData.allChallengeLists[currentCampaign].Count != currentChallengeList.list.Count)
                    {
                        List<Challenge> challengeList = new List<Challenge>();
                        foreach (ChallengeState challengeState in currentChallengeList.list)
                        {
                            var _challenge = challengeState.GetChallenge;                            
                            challengeList.Add(_challenge);
                        }
                        ExpeditionData.allChallengeLists[currentCampaign] = challengeList;
                    }

                    List<Challenge> challenge = ExpeditionData.allChallengeLists[currentCampaign];
                    for (int i = 0; i < currentChallengeList.list.Count; i++)
                    {

                        if (challenge[i].GetType() != currentChallengeList.list[i].ChallengeType)
                        {
                            challenge[i] = currentChallengeList.list[i].GetChallenge;
                        }

                        bool oldCompleteState = challenge[i].completed;
                        bool needUpdate = false;

                        if (challenge[i].description != currentChallengeList.list[i].description) needUpdate = true;

                        currentChallengeList.list[i].ReadTo(challenge[i]);

                        if (RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame rwg)
                        {
                            challenge[i].game = rwg;
                            bool newCompleteState = challenge[i].completed;

                            if (oldCompleteState != newCompleteState && newCompleteState)
                            {
                                needUpdate = false;
                                challenge[i].completed = oldCompleteState;
                                challenge[i].CompleteChallenge();
                            }

                            if (needUpdate) challenge[i].UpdateDescription();
                        }
                    }
                }                
            }
            if (expedition.menuSaveState != currentMenuSaveState)
            {
                expedition.menuSaveState = currentMenuSaveState;
                expedition.menuSaveGameData = expedition.menuSaveState?.CreateSaveData();
                expedition.needMenuSaveUpdate = true;
            }

            if (ExpeditionOnlineMenu.expeditionGameMode is not null)
            {
                ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted = isChallengeCompleted;
            }
        }

        public virtual ChallengeState GetChallengeState(Challenge challenge)
        {
            if (challenge is AchievementChallenge) return new AchievementChallengeState(challenge);
            if (challenge is CycleScoreChallenge) return new CycleScoreChallengeState(challenge);
            if (challenge is EchoChallenge) return new EchoChallengeState(challenge);
            if (challenge is GlobalScoreChallenge) return new GlobalScoreChallengeState(challenge);
            if (challenge is HuntChallenge) return new HuntChallengeState(challenge);
            if (challenge is ItemHoardChallenge) return new ItemHoardChallengeState(challenge);
            if (challenge is NeuronDeliveryChallenge) return new NeuronDeliveryChallengeState(challenge);
            if (challenge is PearlDeliveryChallenge) return new PearlDeliveryChallengeState(challenge);
            if (challenge is PearlHoardChallenge) return new PearlHoardChallengeState(challenge);
            if (challenge is PinChallenge) return new PinChallengeState(challenge);
            if (challenge is VistaChallenge) return new VistaChallengeState(challenge);

            throw new NotImplementedException();       
        }

        public override Type GetDataType()
        {
            return typeof(ExpeditionLobbyData);
        }
    }
}
