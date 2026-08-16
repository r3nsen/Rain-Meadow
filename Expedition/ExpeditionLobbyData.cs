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

            [OnlineField]
            public int mushroomCounter;
            [OnlineFieldHalf]
            public float slowTimeCooldown;

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
                Lobby lobby = (Lobby)resource;
                ExpeditionGameMode expedition = (ExpeditionGameMode)lobby.gameMode;
                expedition.currentCampaign = currentCampain;

                //RainWorldGame currentGameState = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;
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

            public float challengeDifficulty = 0.5f;
            public string startingDen;
            public bool newGame = false;
            public bool validateQuests = true;
            
            public ExpeditionDataState()
            {            
                if (ExpeditionOnlineMenu.expeditionGameMode is not null && ExpeditionOnlineMenu.expeditionGameMode.currentCampaign is not null) // hmmm
                {
                    currentChallengeList = ExpeditionData.allChallengeLists[ExpeditionOnlineMenu.expeditionGameMode.currentCampaign]; // seems like currentCampaign could be null - oh, jukebox can set this to null too? i had a bug where host was on jukebox and client on infinite loop trying to join until esc
                }
                activeUnlocks = ExpeditionGame.activeUnlocks;

                activeMission = ExpeditionData.activeMission;

                challengeDifficulty = ExpeditionData.challengeDifficulty;
                startingDen = ExpeditionData.startingDen;
                newGame = ExpeditionData.newGame;
                validateQuests = ExpeditionData.validateQuests;                
            }

            public void CreateSaveData(SlugcatStats.Name currentCampaign)
            {
                ExpeditionGame.allUnlocks[currentCampaign] = activeUnlocks;
                ExpeditionData.activeMission = activeMission;

                ExpeditionData.challengeDifficulty = challengeDifficulty;
                ExpeditionData.startingDen = startingDen;
                ExpeditionData.newGame = newGame;
                ExpeditionData.validateQuests = validateQuests;
                
                if (ExpeditionGame.expeditionComplete) return; // r3n: in the win screen, current challenge list is used to set completed challenges, overriting it before complete challege setup crash the ending

                //var rwg = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;

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

            public void writeChallengeList(Serializer serializer, List<Challenge> challengeList)
            {                             
                serializer.writer.Write(challengeList.Count);                
                foreach (var item in challengeList)
                {
                    if (item is Expedition.AchievementChallenge)
                    {                        
                        serializer.writer.Write(0);
                        serializer.writer.Write(((AchievementChallenge)item).ID.value); // 
                    }
                    else if (item is Expedition.CycleScoreChallenge)
                    {                     
                        serializer.writer.Write(1);
                        serializer.writer.Write(((CycleScoreChallenge)item).increase); //
                        // int len = ((CycleScoreChallenge)item).killScores?.Length ?? -1;
                        // serializer.writer.Write(len);                        
                        serializer.writer.Write(((CycleScoreChallenge)item).score);
                        serializer.writer.Write(((CycleScoreChallenge)item).target);
                    }
                    else if (item is Expedition.EchoChallenge)
                    {                        
                        serializer.writer.Write(2);                     
                        serializer.writer.Write(((EchoChallenge)item).ghost.value);
                    }
                    else if (item is Expedition.GlobalScoreChallenge)
                    {                        
                        serializer.writer.Write(3);                     
                        serializer.writer.Write(((GlobalScoreChallenge)item).increase);
                        // int len = ((GlobalScoreChallenge)item).killScores?.Length ?? -1;
                        // serializer.writer.Write(len);
                        serializer.writer.Write(((GlobalScoreChallenge)item).score);
                        serializer.writer.Write(((GlobalScoreChallenge)item).target);
                    }
                    else if (item is Expedition.HuntChallenge)
                    {                        
                        serializer.writer.Write(4);                        
                        serializer.writer.Write(((HuntChallenge)item).amount);
                        serializer.writer.Write(((HuntChallenge)item).current);
                        serializer.writer.Write(((HuntChallenge)item).target.value); //creatureTemplate.type to string                                
                    }
                    else if (item is Expedition.ItemHoardChallenge)
                    {                     
                        serializer.writer.Write(5);                        
                        serializer.writer.Write(((ItemHoardChallenge)item).amount);
                        serializer.writer.Write(((ItemHoardChallenge)item).target.value);
                    }
                    else if (item is Expedition.NeuronDeliveryChallenge)
                    {                        
                        serializer.writer.Write(6);                     
                        serializer.writer.Write(((NeuronDeliveryChallenge)item).delivered);
                        serializer.writer.Write(((NeuronDeliveryChallenge)item).neurons);
                    }
                    else if (item is Expedition.PearlDeliveryChallenge)
                    {                        
                        serializer.writer.Write(7);
                        serializer.writer.Write(((PearlDeliveryChallenge)item).iterator);
                        serializer.writer.Write(((PearlDeliveryChallenge)item).region);
                    }
                    else if (item is Expedition.PearlHoardChallenge)
                    {                     
                        serializer.writer.Write(8);                     
                        serializer.writer.Write(((PearlHoardChallenge)item).amount);
                        serializer.writer.Write(((PearlHoardChallenge)item).common);
                        serializer.writer.Write(((PearlHoardChallenge)item).region);
                    }
                    else if (item is Expedition.PinChallenge)
                    {                        
                        serializer.writer.Write(9);
                        serializer.writer.Write(((PinChallenge)item).current);
                        serializer.writer.Write(((PinChallenge)item).target);
                    }
                    else if (item is Expedition.VistaChallenge)
                    {                        
                        serializer.writer.Write(10);
                        serializer.writer.Write(((VistaChallenge)item).location.x);
                        serializer.writer.Write(((VistaChallenge)item).location.y);
                        serializer.writer.Write(((VistaChallenge)item).region);
                        serializer.writer.Write(((VistaChallenge)item).room);
                    }
                    else
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

                            // int ks = serializer.reader.ReadInt32();
                            // int[] _killScores = null!;
                            // if (ks >= 0) _killScores = new int[ks];

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
                                // killScores = _killScores,
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

                            // int ks = serializer.reader.ReadInt32();
                            //int[] _killScores = null!;
                            //if (ks >= 0) _killScores = new int[ks];

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
                                // killScores = _killScores,
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
                }               
            }
        }
    }
}
