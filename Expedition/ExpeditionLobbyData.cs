using Expedition;
using RainMeadow.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        [OnlineField(group = "expeditiondata", polymorphic = true)]
        ChallengeState[] currentChallengeList;

        [OnlineField(group = "activeUnlocks", nullable = true)]
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

            if (currentCampaign is not null)
            {
                if (ExpeditionOnlineMenu.challengeListStrings is null || 
                    ExpeditionOnlineMenu.challengeListStrings.Count != ExpeditionData.allChallengeLists[currentCampaign].Count ||
                    !ExpeditionOnlineMenu.challengeListStrings.SequenceEqual(ExpeditionData.allChallengeLists[currentCampaign].Select(x => x.ToString())))
                {
                    ExpeditionOnlineMenu.challengeListStrings = ExpeditionData.allChallengeLists[currentCampaign].Select(x => x.ToString()).ToList();

                    List<ChallengeState> challengeStateList = new List<ChallengeState>();
                    foreach (Challenge challenge in ExpeditionData.allChallengeLists[currentCampaign])
                    {
                        challengeStateList.Add(GetChallengeState(challenge));
                    }
                    ExpeditionOnlineMenu.currentChallengeList = challengeStateList.ToArray();
                }
            }

            RainMeadow.Debug("allChallengeLists"    + $"size: {ExpeditionData.allChallengeLists[currentCampaign].Count} - "               + string.Join(", ", ExpeditionData.allChallengeLists[currentCampaign].Select(n => $"{n} ")));
            RainMeadow.Debug("currentChallengeList" + $"size: {ExpeditionOnlineMenu.currentChallengeList.Length} - " + string.Join(", ", ExpeditionOnlineMenu.currentChallengeList.Select(n => $"{n} ")));
            RainMeadow.Debug("challengeListStrings" + $"size: {ExpeditionOnlineMenu.challengeListStrings.Count} - "      + string.Join(", ", ExpeditionOnlineMenu.challengeListStrings.Select(n => $"{n} ")));

            if (ExpeditionOnlineMenu.activeUnlocks is null || !ExpeditionGame.activeUnlocks.SequenceEqual(ExpeditionOnlineMenu.activeUnlocks))
            {
                ExpeditionOnlineMenu.activeUnlocks = new List<string>(ExpeditionGame.activeUnlocks);
            }

            currentChallengeList = ExpeditionOnlineMenu.currentChallengeList;
            activeUnlocks = ExpeditionOnlineMenu.activeUnlocks;
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
                currentMenuSaveState = expeditionGameMode.menuSaveState;              
                hasSaveState = expeditionGameMode.hasSaveState;                
            }

            if (ExpeditionOnlineMenu.expeditionGameMode is not null)
            {
                int cclCount = currentChallengeList.Length;

                if (ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted == null || ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted.Length != cclCount)
                {
                    ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted = new bool[cclCount];
                }

                isChallengeCompleted = new bool[cclCount];

                for (int i = 0; i < cclCount; i++)
                {
                    isChallengeCompleted[i] = ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted[i] = currentChallengeList[i].completed;
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
                PlayerState ps = (PlayerState)currentGameState.Players[0].state; // TODO: add jolly support

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

            ExpeditionData.challengeDifficulty = challengeDifficulty;                
            ExpeditionData.newGame = newGame;
            ExpeditionData.validateQuests = validateQuests;
            ExpeditionData.activeMission = activeMission;
            ExpeditionData.startingDen = startingDen;
            
            ExpeditionOnlineMenu.activeUnlocks = activeUnlocks;                

            if (!ExpeditionGame.expeditionComplete)
            {
                if (!ExpeditionData.allChallengeLists.ContainsKey(currentCampaign) || ExpeditionData.allChallengeLists[currentCampaign].Count != currentChallengeList.Length)
                {
                    List<Challenge> challengeList = new List<Challenge>();
                    foreach (ChallengeState challengeState in currentChallengeList)
                    {
                        var _challenge = challengeState.GetChallenge;                            
                        challengeList.Add(_challenge);
                    }
                    ExpeditionData.allChallengeLists[currentCampaign] = challengeList;
                }

                List<Challenge> challenge = ExpeditionData.allChallengeLists[currentCampaign];

                RainMeadow.Debug("currentChallengeList" + $"size: {currentChallengeList.Length} - " + string.Join(", ", currentChallengeList.Select(n => $"{n} ")));
                for (int i = 0; i < ExpeditionData.allChallengeLists[currentCampaign].Count; i++)
                {
                    RainMeadow.Debug($" - [{i}]");
                    var cc = ExpeditionData.allChallengeLists[currentCampaign][i];
                    Type t = cc.GetType();
                    FieldInfo[] fields = t.GetFields();

                    foreach (var field in fields)
                    {
                        var value = field.GetValue(cc);
                        RainMeadow.Debug($" - [{t}]");
                        RainMeadow.Debug($" - field: {field}");
                        RainMeadow.Debug($" - value: {value}");
                    }
                    // RainMeadow.Debug($" - {ExpeditionData.allChallengeLists[currentCampaign][i].GetType()} ");
                    // RainMeadow.Debug($" -- { ExpeditionData.allChallengeLists[currentCampaign][i]} ");
                }
                // RainMeadow.Debug("currentChallengeList" + $"size: {ExpeditionData.allChallengeLists[currentCampaign].Count} - " + string.Join(", ", ExpeditionData.allChallengeLists[currentCampaign].Select(n => $"{n} ")));

                if (ExpeditionOnlineMenu.challengeListStrings is null || ExpeditionOnlineMenu.challengeListStrings.Count != currentChallengeList.Length)//challenge.Count)
                {
                    string[] challengeStrings = new string[currentChallengeList.Length];
                    //for (int i = 0; i < challenge.Count; i++) 
                    //{
                    //    challengeStrings[i] = challenge[i].ToString();
                    //}
                    ExpeditionOnlineMenu.challengeListStrings = new List<string>(challengeStrings);
                }

                for (int i = 0; i < currentChallengeList.Length; i++)
                {

                    if (challenge[i].GetType() != currentChallengeList[i].ChallengeType)
                    {
                        challenge[i] = currentChallengeList[i].GetChallenge;
                    }

                    bool oldCompleteState = challenge[i].completed;
                    bool needUpdate = false;

                    currentChallengeList[i].ReadTo(challenge[i]);

                    if (ExpeditionOnlineMenu.challengeListStrings[i] != challenge[i].ToString())
                    {
                        ExpeditionOnlineMenu.challengeListStrings[i] = challenge[i].ToString();
                        needUpdate = true;
                    }

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

        public ChallengeState GetChallengeState(Challenge challenge) // modders can hook this to custom challenges
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

            throw new NotImplementedException($"challenge type: {challenge.GetType()} not implemented");
        }

        public override Type GetDataType()
        {
            return typeof(ExpeditionLobbyData);
        }
    }
}
