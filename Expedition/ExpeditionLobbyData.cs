using Expedition;

using RainMeadow;

using System;
using System.Collections.Generic;

using static RainMeadow.OnlineResource;
using static RainMeadow.StoryLobbyData;

namespace RainMeadow
{
    public class ExpeditionLobbyData : OnlineResource.ResourceData
    {
        public override ResourceDataState MakeState(OnlineResource resource)
        {
            return new State(this, resource);
        }
        public class State : ResourceDataState
        {
            [OnlineField(nullable = true, group = "expeditiondata")]
            ExpeditionDataState? expeditionDataState;


            [OnlineField]
            SlugcatStats.Name currentCampain;
            [OnlineField(nullable = true, group = "menusave")]
            public MenuSaveStateState? currentMenuSaveState;
            [OnlineField]
            public int selectedSlugcat;

            [OnlineField]
            byte food; // 0bqqffffff 63 pips + 3/4 half
            [OnlineField]
            bool isInGame;
            [OnlineField]
            bool readyForWin;
            [OnlineField]
            bool hasSaveState;
            [OnlineField]
            public byte readyForTransition;

            [OnlineField(nullable = true)]
            public string? saveStateString;

            [OnlineField(group = "expeditiondata")]
            bool[] isChallengeCompleted;

            public State() { }
            public State(ExpeditionLobbyData expeditionLobbyData, OnlineResource onlineResource)
            {                
                ExpeditionGameMode expeditionGameMode = (onlineResource as Lobby).gameMode as ExpeditionGameMode;
                RainWorldGame currentGameState = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;

                PlayerState ps = currentGameState?.Players[0].state as PlayerState;

                food = (byte)((ps?.foodInStomach ?? 0) | ps?.quarterFoodPoints ?? 0 << 6);
                isInGame = RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame && RWCustom.Custom.rainWorld.processManager.upcomingProcess is null;
                readyForWin = expeditionGameMode.readyForWin;
                readyForTransition = (byte)expeditionGameMode.readyForTransition;
                saveStateString = expeditionGameMode.saveStateString;

                selectedSlugcat = expeditionGameMode.slugcatSelected;

                currentCampain = expeditionGameMode.currentCampaign;
                if (currentGameState?.session is StoryGameSession storySession)
                {
                    currentMenuSaveState = new MenuSaveStateState(storySession.saveState);
                    expeditionDataState = new ExpeditionDataState();
                    hasSaveState = saveStateString != null;
                }
                else
                {
                    hasSaveState = expeditionGameMode.hasSaveState;
                    if (currentMenuSaveState != expeditionGameMode.menuSaveState)
                    {
                        currentMenuSaveState = expeditionGameMode.menuSaveState;
                    }
                    expeditionDataState = new ExpeditionDataState();
                }

                if (ExpeditionOnlineMenu.expeditionGameMode is not null)
                {
                    if (ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted == null || ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted.Length != expeditionDataState.currentChallengeList.Count)
                    {
                        ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted = new bool[expeditionDataState.currentChallengeList.Count];
                    }
                    isChallengeCompleted = new bool[expeditionDataState.currentChallengeList.Count];
                    for (int i = 0; i < expeditionDataState.currentChallengeList.Count; i++)
                    {
                        isChallengeCompleted[i] = ExpeditionOnlineMenu.expeditionGameMode.isChallengeCompleted[i] = expeditionDataState.currentChallengeList[i].completed;
                    }
                }                
            }

            public override Type GetDataType()
            {
                return typeof(ExpeditionLobbyData);
            }

            public override void ReadTo(ResourceData data, OnlineResource resource)
            {                
                Lobby lobby = (resource as Lobby);
                ExpeditionGameMode expedition = (lobby.gameMode as ExpeditionGameMode);
                expedition.currentCampaign = currentCampain;

                RainWorldGame currentGameState = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;
                if (currentGameState is not null)
                {
                    PlayerState ps = currentGameState.Players[0].state as PlayerState;

                    int _food = food & 63;
                    int _quarterfood = food >> 6;

                    ps.foodInStomach = _food;
                    ps.quarterFoodPoints = _quarterfood;
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
                    expeditionDataState.CreateSaveData(currentCampain);
                    // expedition.expeditionDataState = expeditionDataState;
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
        }
        public class ExpeditionDataState : Serializer.ICustomSerializable
        {
            public List<Challenge> currentChallengeList = new List<Challenge>();
            public List<string> activeUnlocks = new List<string>();

            public string activeMission;
            //  public List<Challenge> completedChallengeList = new List<Challenge>();

            public float challengeDifficulty = 0.5f;
            public string startingDen;
            public bool newGame = false;
            public bool validateQuests = true;
            public bool hasViewedManual = false;

            public bool devMode = false;
            public int saveSlot;
            public int[] ints;

            public int level;
            public int currentPoints;
            public int perkLimit = 1;
            public int totalPoints;
            public int totalChallengesCompleted;
            public int totalHiddenChallengesCompleted;
            public int totalWins;

            public string menuSong;

            public ExpeditionDataState()
            {            
                if (ExpeditionOnlineMenu.expeditionGameMode is not null)
                {
                    currentChallengeList = ExpeditionData.allChallengeLists[ExpeditionOnlineMenu.expeditionGameMode.currentCampaign];
                }
                activeUnlocks = ExpeditionGame.activeUnlocks;

                activeMission = ExpeditionData.activeMission;

                challengeDifficulty = ExpeditionData.challengeDifficulty;
                startingDen = ExpeditionData.startingDen;
                newGame = ExpeditionData.newGame;
                validateQuests = ExpeditionData.validateQuests;
                hasViewedManual = ExpeditionData.hasViewedManual;

                devMode = ExpeditionData.devMode;
                saveSlot = ExpeditionData.saveSlot;
                ints = ExpeditionData.ints;

                level = ExpeditionData.level;
                currentPoints = ExpeditionData.currentPoints;
                perkLimit = ExpeditionData.perkLimit;
                totalPoints = ExpeditionData.totalPoints;
                totalChallengesCompleted = ExpeditionData.totalHiddenChallengesCompleted;
                totalHiddenChallengesCompleted = ExpeditionData.totalHiddenChallengesCompleted;
                totalWins = ExpeditionData.totalWins;

                menuSong = ExpeditionData.menuSong;                
            }

            public void CreateSaveData(SlugcatStats.Name currentCampaign)
            {
                ExpeditionGame.allUnlocks[currentCampaign] = activeUnlocks;
                ExpeditionData.activeMission = activeMission;

                ExpeditionData.challengeDifficulty = challengeDifficulty;
                ExpeditionData.startingDen = startingDen;
                ExpeditionData.newGame = newGame;
                ExpeditionData.validateQuests = validateQuests;
                ExpeditionData.hasViewedManual = hasViewedManual;

                ExpeditionData.devMode = devMode;
                ExpeditionData.saveSlot = saveSlot;
                ExpeditionData.ints = ints;

                ExpeditionData.level = level;
                ExpeditionData.currentPoints = currentPoints;
                ExpeditionData.perkLimit = perkLimit;
                ExpeditionData.totalPoints = totalPoints;
                ExpeditionData.totalHiddenChallengesCompleted = totalChallengesCompleted;
                ExpeditionData.totalHiddenChallengesCompleted = totalHiddenChallengesCompleted;
                ExpeditionData.totalWins = totalWins;

                ExpeditionData.menuSong = menuSong;

                if (ExpeditionGame.expeditionComplete) return;

                var rwg = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;

                if (ExpeditionData.allChallengeLists[currentCampaign].Count != currentChallengeList.Count)
                {                    
                    ExpeditionData.allChallengeLists[currentCampaign] = currentChallengeList;// new List<Challenge>(currentChallengeList.Count);                    
                    // r3n: current challenge list here have null game and completed could be true, should I set game and call update description here and early return?
                    // return;
                }


                for (int i = 0; i < currentChallengeList.Count; i++)
                {
                    List<Challenge> challenge = ExpeditionData.allChallengeLists[currentCampaign];

                    bool oldCompleteState = challenge[i].completed;
                    bool needUpdate = false;

                    if (challenge[i].description != currentChallengeList[i].description) needUpdate = true;

                    challenge[i] = currentChallengeList[i];

                    if (rwg is not null)
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

            public void writeChallengeList(Serializer serializer, List<Challenge> challengeList)
            {                             
                serializer.writer.Write(challengeList.Count);                
                foreach (var item in challengeList)
                {
                    if (item is Expedition.AchievementChallenge)
                    {                        
                        serializer.writer.Write(0);
                        serializer.writer.Write((item as AchievementChallenge).ID.value); // 
                    }
                    else if (item is Expedition.CycleScoreChallenge)
                    {                     
                        serializer.writer.Write(1);
                        serializer.writer.Write((item as CycleScoreChallenge).increase); //
                        int len = (item as CycleScoreChallenge).killScores?.Length ?? -1;
                        serializer.writer.Write(len);
                        for (int i = 0; i < len; i++)
                        {
                            //   serializer.writer.Write((item as CycleScoreChallenge).killScores[i]);
                        }
                        serializer.writer.Write((item as CycleScoreChallenge).score);
                        serializer.writer.Write((item as CycleScoreChallenge).target);
                    }
                    else if (item is Expedition.EchoChallenge)
                    {                        
                        serializer.writer.Write(2);                     
                        serializer.writer.Write((item as EchoChallenge).ghost.value);
                    }
                    else if (item is Expedition.GlobalScoreChallenge)
                    {                        
                        serializer.writer.Write(3);                     
                        serializer.writer.Write((item as GlobalScoreChallenge).increase);
                        int len = (item as GlobalScoreChallenge).killScores?.Length ?? -1;
                        serializer.writer.Write(len);
                        for (int i = 0; i < len; i++)
                        {
                            //serializer.writer.Write((item as GlobalScoreChallenge).killScores[i]);
                        }
                        serializer.writer.Write((item as GlobalScoreChallenge).score);
                        serializer.writer.Write((item as GlobalScoreChallenge).target);
                    }
                    else if (item is Expedition.HuntChallenge)
                    {                        
                        serializer.writer.Write(4);                        
                        serializer.writer.Write((item as HuntChallenge).amount);
                        serializer.writer.Write((item as HuntChallenge).current);
                        serializer.writer.Write((item as HuntChallenge).target.value); //creatureTemplate.type to string                                
                    }
                    else if (item is Expedition.ItemHoardChallenge)
                    {                     
                        serializer.writer.Write(5);                        
                        serializer.writer.Write((item as ItemHoardChallenge).amount);
                        serializer.writer.Write((item as ItemHoardChallenge).target.value);
                    }
                    else if (item is Expedition.NeuronDeliveryChallenge)
                    {                        
                        serializer.writer.Write(6);                     
                        serializer.writer.Write((item as NeuronDeliveryChallenge).delivered);
                        serializer.writer.Write((item as NeuronDeliveryChallenge).neurons);
                    }
                    else if (item is Expedition.PearlDeliveryChallenge)
                    {                        
                        serializer.writer.Write(7);
                        serializer.writer.Write((item as PearlDeliveryChallenge).iterator);
                        serializer.writer.Write((item as PearlDeliveryChallenge).region);
                    }
                    else if (item is Expedition.PearlHoardChallenge)
                    {                     
                        serializer.writer.Write(8);                     
                        serializer.writer.Write((item as PearlHoardChallenge).amount);
                        serializer.writer.Write((item as PearlHoardChallenge).common);
                        serializer.writer.Write((item as PearlHoardChallenge).region);
                    }
                    else if (item is Expedition.PinChallenge)
                    {                        
                        serializer.writer.Write(9);
                        serializer.writer.Write((item as PinChallenge).current);
                        serializer.writer.Write((item as PinChallenge).target);

                        // int plen = (item as PinChallenge).pinList.Count;
                        // serializer.writer.Write(plen); // will create an 0 len array                        
                        // serializer.writer.Write((item as PinChallenge).spearList.Count); // will create an 0 len array
                    }
                    else if (item is Expedition.VistaChallenge)
                    {                        
                        serializer.writer.Write(10);
                        serializer.writer.Write((item as VistaChallenge).location.x);
                        serializer.writer.Write((item as VistaChallenge).location.y);
                        serializer.writer.Write((item as VistaChallenge).region);
                        serializer.writer.Write((item as VistaChallenge).room);
                    }
                    else // throw new InvalidProgrammerException($"hmmm, i dont recognize this challenge type: {item}");
                    {
                        // custom Challenge
                        serializer.writer.Write(10);
                    }
                    serializer.writer.Write(item.revealCheck);
                    serializer.writer.Write(item.revealCheckDelay);
                    serializer.writer.Write(item.completed);
                    serializer.writer.Write(item.description);
                    serializer.writer.Write(item.hidden);
                    serializer.writer.Write(item.revealed);
                }             
            }
            public void readChallengeList(Serializer serializer, out List<Challenge> challenge)
            {                
                int count2 = serializer.reader.ReadInt32();                
                challenge = new List<Challenge>(count2);
                for (int j = 0; j < count2; j++)
                {
                    int challengeType = serializer.reader.ReadInt32();
                    
                    Challenge c;
                    switch (challengeType)
                    {
                        case 0:
                        {

                            var _ID = (WinState.EndgameID)ExtEnumBase.Parse(typeof(WinState.EndgameID), serializer.reader.ReadString(), false);
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new AchievementChallenge()
                            {
                                ID = _ID,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            };
                            break;
                        }
                        case 1:
                        {

                            var _increase = serializer.reader.ReadInt32();

                            int ks = serializer.reader.ReadInt32();
                            int[] _killScores = null;
                            if (ks >= 0) _killScores = new int[ks];

                            var _score = serializer.reader.ReadInt32();
                            var _target = serializer.reader.ReadInt32();
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();
                            
                            c = new CycleScoreChallenge()
                            {

                                increase = _increase,
                                killScores = _killScores,
                                score = _score,
                                target = _target,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            };
                            break;
                        }
                        case 2:
                        {
                            var _ghost = (GhostWorldPresence.GhostID)ExtEnumBase.Parse(typeof(GhostWorldPresence.GhostID), serializer.reader.ReadString(), false);
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new EchoChallenge()
                            {
                                ghost = _ghost,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            };
                            break;
                        }
                        case 3:
                        {

                            var _increase = serializer.reader.ReadInt32();

                            int ks = serializer.reader.ReadInt32();
                            int[] _killScores = null;
                            if (ks >= 0) _killScores = new int[ks];

                            var _score = serializer.reader.ReadInt32();
                            var _target = serializer.reader.ReadInt32();
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();                            

                            c = new GlobalScoreChallenge()
                            {
                                increase = _increase,
                                killScores = _killScores,
                                score = _score,
                                target = _target,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            };
                            break;
                        }
                        case 4:
                        {
                            var _amount = serializer.reader.ReadInt32();
                            var _current = serializer.reader.ReadInt32();
                            var _target = (CreatureTemplate.Type)ExtEnumBase.Parse(typeof(CreatureTemplate.Type), serializer.reader.ReadString(), false);
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new HuntChallenge()
                            {
                                amount = _amount,
                                current = _current,
                                target = _target,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            };
                            break;
                        }
                        case 5:
                        {
                            var _amount = serializer.reader.ReadInt32();
                            var _target = (AbstractPhysicalObject.AbstractObjectType)ExtEnumBase.Parse(typeof(AbstractPhysicalObject.AbstractObjectType), serializer.reader.ReadString(), false);
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new ItemHoardChallenge()
                            {
                                amount = _amount,
                                target = _target,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            }; break;
                        }
                        case 6:
                        {
                            var _delivered = serializer.reader.ReadInt32();
                            var _neurons = serializer.reader.ReadInt32();
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new NeuronDeliveryChallenge()
                            {
                                delivered = _delivered,
                                neurons = _neurons,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            }; break;
                        }
                        case 7:
                        {
                            var _iterator = serializer.reader.ReadInt32();
                            var _region = serializer.reader.ReadString();
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new PearlDeliveryChallenge()
                            {
                                iterator = _iterator,
                                region = _region,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            }; break;
                        }
                        case 8:
                        {
                            var _amount = serializer.reader.ReadInt32();
                            var _common = serializer.reader.ReadBoolean();
                            var _region = serializer.reader.ReadString();
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new PearlHoardChallenge()
                            {
                                amount = _amount,
                                common = _common,
                                region = _region,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            }; break;
                        }
                        case 9:
                        {
                            var _current = serializer.reader.ReadInt32();
                            var _target = serializer.reader.ReadInt32();
                            // var _pinList = new List<Creature>(serializer.reader.ReadInt32());
                            // var _spearList = new List<Spear>(serializer.reader.ReadInt32());
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new PinChallenge()
                            {
                                current = _current,
                                target = _target,
                                // pinList = _pinList,
                                // spearList = _spearList,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            }; break;
                        }
                        case 10:
                        {

                            var _location = new UnityEngine.Vector2(serializer.reader.ReadSingle(), serializer.reader.ReadSingle());
                            var _region = serializer.reader.ReadString();
                            var _room = serializer.reader.ReadString();
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            c = new VistaChallenge()
                            {
                                location = _location,
                                region = _region,
                                room = _room,
                                revealCheck = _revealCheck,
                                revealCheckDelay = _revealCheckDelay,
                                completed = _completed,
                                description = _description,
                                hidden = _hidden,
                                revealed = _revealed,
                            }; break;
                        }
                        default:
                            throw new NotImplementedException($"challengeType: {challengeType}");
                    }
                    challenge.Add(c);
                }                
            }
            public void CustomSerialize(Serializer serializer)
            {                                                
                if (serializer.IsWriting)
                {
                    serializer.writer.Write(challengeDifficulty);//float              

                    writeChallengeList(serializer, currentChallengeList);
                    // active unlocks
                    var unlocksCount = activeUnlocks.Count;
                    serializer.writer.Write(unlocksCount);
                    for (int i = 0; i < unlocksCount; i++)
                    {
                        serializer.writer.Write(activeUnlocks[i]);
                    }
                    // ---
                    // complete challenge list
                    //writeChallengeList(serializer, completedChallengeList);
                    // ---
                    serializer.writer.Write(activeMission ?? "");

                    serializer.writer.Write(startingDen ?? ""); //string 
                    serializer.writer.Write(newGame);//bool 
                    serializer.writer.Write(validateQuests);//bool 
                    serializer.writer.Write(hasViewedManual);//bool 

                    serializer.writer.Write(devMode);//bool 
                    serializer.writer.Write(saveSlot);  //int 

                    serializer.writer.Write(level); // int  
                    serializer.writer.Write(currentPoints);  // int  
                    serializer.writer.Write(perkLimit);// int  
                    serializer.writer.Write(totalPoints);  // int  
                    serializer.writer.Write(totalChallengesCompleted); ; // int  
                    serializer.writer.Write(totalHiddenChallengesCompleted);  // int  
                    serializer.writer.Write(totalWins); // int  

                    serializer.writer.Write(menuSong ?? "");//string 

                }
                if (serializer.IsReading)
                {                    
                    challengeDifficulty = serializer.reader.ReadSingle();//float 

                    readChallengeList(serializer, out currentChallengeList);
                    
                    // active unlocks
                    var unlocksCount = serializer.reader.ReadInt32();
                    activeUnlocks = new List<string>(unlocksCount);
                    for (int i = 0; i < unlocksCount; i++)
                    {
                        activeUnlocks.Add(serializer.reader.ReadString());

                    }

                    string temp = serializer.reader.ReadString();//float 
                    activeMission = temp;// == "null" ? null : temp;


                    startingDen = serializer.reader.ReadString(); //string 
                    newGame = serializer.reader.ReadBoolean();//bool 
                    validateQuests = serializer.reader.ReadBoolean();//bool 
                    hasViewedManual = serializer.reader.ReadBoolean();//bool 
                                                                      //allEarnedPassages = serializer.reader.Read();// Dictionary<string, int> 
                    devMode = serializer.reader.ReadBoolean();//bool 
                    saveSlot = serializer.reader.ReadInt32(); //int 

                    level = serializer.reader.ReadInt32(); // int  
                    currentPoints = serializer.reader.ReadInt32(); ; // int  
                    perkLimit = serializer.reader.ReadInt32();// int  
                    totalPoints = serializer.reader.ReadInt32(); // int  
                    totalChallengesCompleted = serializer.reader.ReadInt32(); // int  
                    totalHiddenChallengesCompleted = serializer.reader.ReadInt32(); // int  
                    totalWins = serializer.reader.ReadInt32(); // int  

                    menuSong = serializer.reader.ReadString();//string 
                }               
            }

            //public static bool operator !=(ExpeditionDataState? left, ExpeditionDataState? right) => !(left == right);

            //public static bool operator ==(ExpeditionDataState? left, ExpeditionDataState? right)
            //{
            //    if (left is null) return right is null;
            //    if (right is null) return false;
            //    return left.Compare(right);
            //}
            //public bool Compare(ExpeditionDataState other)
            //{                
            //    var comparativo = true;
            //    r3n.Log($"comparativo begin");
            //    if (currentChallengeList.Count != other.currentChallengeList.Count) comparativo = false;
            //    r3n.Log($"challengeListCount: {currentChallengeList.Count} {comparativo}");
            //    if (activeUnlocks.Count != other.activeUnlocks.Count) comparativo = false;
            //    r3n.Log($"activeUnlocks: {activeUnlocks.Count} {comparativo}");

            //    for (int i = 0; i < currentChallengeList.Count; i++)
            //    {
            //        comparativo = comparativo && currentChallengeList[i].description == other.currentChallengeList[i].description;
            //        r3n.Log($"currentChallengeList[{i}].description: {currentChallengeList[i].description} {comparativo}");
            //        r3n.Log($"other.currentChallengeList[{i}].description: {other.currentChallengeList[i].description} {comparativo}");
            //        comparativo = comparativo && currentChallengeList[i].completed == other.currentChallengeList[i].completed;
            //        r3n.Log($"currentChallengeList[{i}].completed: {currentChallengeList[i].completed} {comparativo}");
            //        r3n.Log($"currentChallengeList[{i}].completed: {other.currentChallengeList[i].completed} {comparativo}");
            //    }

            //    for (int i = 0; i < activeUnlocks.Count; i++)
            //    {
            //        comparativo = comparativo && activeUnlocks[i] == other.activeUnlocks[i];
            //    }

            //    comparativo = comparativo &&
            ////(currentChallengeList == other.currentChallengeList) &&
            ////(activeUnlocks == other.activeUnlocks) &&
            //(activeMission == other.activeMission) &&
            ////(completedChallengeList == other.completedChallengeList) &&
            //(challengeDifficulty == other.challengeDifficulty) &&
            //(startingDen == other.startingDen) &&
            //(newGame == other.newGame) &&
            //(validateQuests == other.validateQuests) &&
            //(hasViewedManual == other.hasViewedManual) &&
            //(devMode == other.devMode) &&
            //(saveSlot == other.saveSlot) &&
            //(ints == other.ints) &&
            //(level == other.level) &&
            //(currentPoints == other.currentPoints) &&
            //(perkLimit == other.perkLimit) &&
            //(totalPoints == other.totalPoints) &&
            //(totalChallengesCompleted == other.totalChallengesCompleted) &&
            //(totalHiddenChallengesCompleted == other.totalHiddenChallengesCompleted) &&
            //(totalWins == other.totalWins) &&
            //(menuSong == other.menuSong);

            //    r3n.Log($"comparativo: {comparativo}");
            //    return comparativo;
            //}


        }

    }
}
