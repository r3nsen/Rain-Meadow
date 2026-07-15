using Expedition;

using IL;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using static RainMeadow.OnlineResource;
using static RainMeadow.OnlineState;
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
            [OnlineField]
            SlugcatStats.Name currentCampain;
            [OnlineField(nullable = true)]
            ExpeditionDataState expeditionDataState;
            [OnlineField(nullable = true)]
            public MenuSaveStateState? currentMenuSaveState;
            [OnlineField]
            public int selectedSlugcat;

            [OnlineField]
            byte food; // 0bqqffffff 63 pips + 3/4 half
            [OnlineField]
            bool isInGame;
            [OnlineField]
            bool readyForWin;
            [OnlineField(nullable = true)]
            public string? saveStateString;
            public State() { }
            public State(ExpeditionLobbyData expeditionLobbyData, OnlineResource onlineResource)
            {
                //    r3n.Log($" - State");
                ExpeditionGameMode expeditionGameMode = (onlineResource as Lobby).gameMode as ExpeditionGameMode;
                RainWorldGame currentGameState = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;

                PlayerState ps = currentGameState?.Players[0].state as PlayerState;
                //   r3n.Log($"ps.foodInStomach: {ps.foodInStomach} - ps.quarterFoodPoints: {ps.quarterFoodPoints}");
                food = (byte)((ps?.foodInStomach ?? 0) | ps?.quarterFoodPoints ?? 0 << 6);
                isInGame = RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame && RWCustom.Custom.rainWorld.processManager.upcomingProcess is null;
                readyForWin = expeditionGameMode.readyForWin;
                saveStateString = expeditionGameMode.saveStateString;
                r3n.Log($" - ctor saveStateString: {saveStateString}");

                selectedSlugcat = expeditionGameMode.slugcatSelected;
                //   r3n.Log($"food: {food}");

                currentCampain = expeditionGameMode.currentCampaign;
                if (currentGameState?.session is StoryGameSession)
                {
                    r3n.Log("session is StoryGameSession");
                    currentMenuSaveState = new MenuSaveStateState(RWCustom.Custom.rainWorld.progression.currentSaveState);
                    expeditionDataState = new ExpeditionDataState();
                    //      r3n.Log($" - in session expeditionDataState: {expeditionDataState}");
                }
                else
                {
                    r3n.Log($"session is not StoryGameSession - currentMenuSaveState != expeditionGameMode.menuSaveState: {currentMenuSaveState != expeditionGameMode.menuSaveState}");
                    r3n.Log($"session is not StoryGameSession - currentMenuSaveState: {currentMenuSaveState}");
                    r3n.Log($"session is not StoryGameSession - expeditionGameMode.menuSaveState: {expeditionGameMode.menuSaveState}");
                    r3n.Log($" - menuSaveState: {((expeditionGameMode.menuSaveState is null) ? ("null") : ("not null"))}");
                    r3n.Log($" - currentMenuSaveState: {((currentMenuSaveState is null) ? ("null") : ("not null"))}");
                    //if (currentMenuSaveState != expeditionGameMode.menuSaveState)
                    {
                        r3n.Log($" - currentMenuSaveState: {currentMenuSaveState}");
                        r3n.Log($" - expeditionGameMode.menuSaveState: {expeditionGameMode.menuSaveState}");
                        currentMenuSaveState = expeditionGameMode.menuSaveState;
                    }

                    //expeditionDataState = expeditionGameMode.expeditionDataState;
                    expeditionDataState = new ExpeditionDataState();
                    //     r3n.Log($" - expeditionDataState: {expeditionDataState}");
                }
            }

            public override Type GetDataType()
            {
                return typeof(ExpeditionLobbyData);
            }

            public override void ReadTo(ResourceData data, OnlineResource resource)
            {
                //      r3n.Log($" - ReadTo");

                var lobby = (resource as Lobby);
                var expedition = (lobby.gameMode as ExpeditionGameMode);
                expedition.currentCampaign = currentCampain;

                RainWorldGame currentGameState = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;
                if (currentGameState is not null)
                {
                    PlayerState ps = currentGameState.Players[0].state as PlayerState;

                    int _food = food & 63;
                    int _quarterfood = food >> 6;

                    ps.foodInStomach = _food;
                    ps.quarterFoodPoints = _quarterfood;

                    //   r3n.Log($"ps.foodInStomach: {ps.foodInStomach} - ps.quarterFoodPoints: {ps.quarterFoodPoints}");
                    //   r3n.Log($"food: {food}");
                }

                expedition.isInGame = isInGame;
                expedition.readyForWin = readyForWin;
                expedition.saveStateString = saveStateString;
                r3n.Log($" - readto saveStateString: {saveStateString}");
                expedition.hasSaveState = saveStateString != null;

                if (expedition.slugcatSelected != selectedSlugcat)
                {
                    expedition.slugcatSelected = selectedSlugcat;
                    expedition.needSlugUpdate = true;
                }

                if (expedition.expeditionDataState != expeditionDataState)
                {
                    //     r3n.Log($"readto ex.expeditionDataState: {((expedition.expeditionDataState is null)?("null"):("not null"))}");
                    //    r3n.Log($"readto expeditionDataState: {((expeditionDataState is null) ? ("null") : ("not null"))}");
                    expeditionDataState.CreateSaveData();
                    expedition.expeditionDataState = expeditionDataState;






                    //    r3n.Log($" ^ expeditionDataState: {expeditionDataState}");
                }
                if (expedition.menuSaveState != currentMenuSaveState)
                {
                    r3n.Log($"readto menuSaveState: {((expedition.menuSaveState is null) ? ("null") : ("not null"))}");
                    r3n.Log($"readto currentMenuSaveState: {((currentMenuSaveState is null) ? ("null") : ("not null"))}");
                    expedition.menuSaveState = currentMenuSaveState;
                    expedition.menuSaveGameData = expedition.menuSaveState?.CreateSaveData();
                    expedition.needMenuSaveUpdate = true;
                }
            }
        }
        public class ExpeditionDataState : Serializer.ICustomSerializable
        {

            //public SlugcatStats.Name[] allChallengeLists_keys = new SlugcatStats.Name[] { };
            //public List<Expedition.Challenge>[] allChallengeLists_values = new List<Expedition.Challenge>[] { };
            //public Dictionary<SlugcatStats.Name, List<Expedition.Challenge>> allChallengeLists = new Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>();
            List<Challenge> currentChallengeList = new List<Challenge>();
            string activeMission;
            //   public List<Expedition.Challenge> completedChallengeList = new List<Expedition.Challenge>();
            //   public Dictionary<string, string> allActiveMissions = new Dictionary<string, string>();
            //   public Dictionary<string, List<string>> requiredExpeditionContent = new Dictionary<string, List<string>>();
            public float challengeDifficulty = 0.5f;
            public string startingDen;
            public bool newGame = false;
            public bool validateQuests = true;
            public bool hasViewedManual = false;
            //   public Dictionary<string, int> allEarnedPassages = new Dictionary<string, int>();
            public bool devMode = false;
            public int saveSlot;
            public int[] ints;
            //   public SlugcatStats.Name slugcatPlayer = SlugcatStats.Name.White;
            //   public List<string> completedQuests = new List<string>();
            //   public List<string> completedMissions = new List<string>();
            //   public Dictionary<string, int> missionBestTimes = new Dictionary<string, int>();
            //   public List<string> unlockables = new List<string>();
            //   public List<string> newSongs = new List<string>();
            public int level;
            public int currentPoints;
            public int perkLimit = 1;
            public int totalPoints;
            public int totalChallengesCompleted;
            public int totalHiddenChallengesCompleted;
            public int totalWins;
            //  public Dictionary<string, int> slugcatWins = new Dictionary<string, int>();
            //  public Dictionary<string, int> challengeTypes = new Dictionary<string, int>();
            public string menuSong;

            public ExpeditionDataState()
            {
                if (ExpeditionOnlineMenu.expeditionGameMode is not null)
                    currentChallengeList = ExpeditionData.allChallengeLists[ExpeditionOnlineMenu.expeditionGameMode.currentCampaign]; // new Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>();

                activeMission = ExpeditionData.activeMission;
                //   public List<Expedition.Challenge> completedChallengeList = new List<Expedition.Challenge>();
                //   public Dictionary<string, string> allActiveMissions = new Dictionary<string, string>();
                //   public Dictionary<string, List<string>> requiredExpeditionContent = new Dictionary<string, List<string>>();
                challengeDifficulty = ExpeditionData.challengeDifficulty;
                startingDen = ExpeditionData.startingDen;
                newGame = ExpeditionData.newGame;
                validateQuests = ExpeditionData.validateQuests;
                hasViewedManual = ExpeditionData.hasViewedManual;
                //   public Dictionary<string, int> allEarnedPassages = new Dictionary<string, int>();
                devMode = ExpeditionData.devMode;
                saveSlot = ExpeditionData.saveSlot;
                ints = ExpeditionData.ints;
                //   public SlugcatStats.Name slugcatPlayer = SlugcatStats.Name.White;
                //   public List<string> completedQuests = new List<string>();
                //   public List<string> completedMissions = new List<string>();
                //   public Dictionary<string, int> missionBestTimes = new Dictionary<string, int>();
                //   public List<string> unlockables = new List<string>();
                //   public List<string> newSongs = new List<string>();
                level = ExpeditionData.level;
                currentPoints = ExpeditionData.currentPoints;
                perkLimit = ExpeditionData.perkLimit;
                totalPoints = ExpeditionData.totalPoints;
                totalChallengesCompleted = ExpeditionData.totalHiddenChallengesCompleted;
                totalHiddenChallengesCompleted = ExpeditionData.totalHiddenChallengesCompleted;
                totalWins = ExpeditionData.totalWins;
                //  public Dictionary<string, int> slugcatWins = new Dictionary<string, int>();
                //  public Dictionary<string, int> challengeTypes = new Dictionary<string, int>();
                menuSong = ExpeditionData.menuSong;

            }
            public void CreateSaveData()
            {

                if (ExpeditionOnlineMenu.expeditionGameMode is not null)
                    ExpeditionData.allChallengeLists[ExpeditionOnlineMenu.expeditionGameMode.currentCampaign] = currentChallengeList;
                ExpeditionData.activeMission = activeMission;
                //   public List<Expedition.Challenge> completedChallengeList = new List<Expedition.Challenge>();
                //   public Dictionary<string, string> allActiveMissions = new Dictionary<string, string>();
                //   public Dictionary<string, List<string>> requiredExpeditionContent = new Dictionary<string, List<string>>();
                ExpeditionData.challengeDifficulty = challengeDifficulty;
                ExpeditionData.startingDen = startingDen;
                ExpeditionData.newGame = newGame;
                ExpeditionData.validateQuests = validateQuests;
                ExpeditionData.hasViewedManual = hasViewedManual;
                //   public Dictionary<string, int> allEarnedPassages = new Dictionary<string, int>();
                ExpeditionData.devMode = devMode;
                ExpeditionData.saveSlot = saveSlot;
                ExpeditionData.ints = ints;
                //   public SlugcatStats.Name slugcatPlayer = SlugcatStats.Name.White;
                //   public List<string> completedQuests = new List<string>();
                //   public List<string> completedMissions = new List<string>();
                //   public Dictionary<string, int> missionBestTimes = new Dictionary<string, int>();
                //   public List<string> unlockables = new List<string>();
                //   public List<string> newSongs = new List<string>();
                ExpeditionData.level = level;
                ExpeditionData.currentPoints = currentPoints;
                ExpeditionData.perkLimit = perkLimit;
                ExpeditionData.totalPoints = totalPoints;
                ExpeditionData.totalHiddenChallengesCompleted = totalChallengesCompleted;
                ExpeditionData.totalHiddenChallengesCompleted = totalHiddenChallengesCompleted;
                ExpeditionData.totalWins = totalWins;
                //  public Dictionary<string, int> slugcatWins = new Dictionary<string, int>();
                //  public Dictionary<string, int> challengeTypes = new Dictionary<string, int>();
                ExpeditionData.menuSong = menuSong;
            }
            public void writeChallengeList(Serializer serializer, List<Challenge> challengeList)
            {

                serializer.writer.Write(challengeList.Count);
                r3n.Log($" - write: count {challengeList.Count}");
                foreach (var item in challengeList)
                {
                    //serializer.writer.Write(item.GetType());
                    if (item is Expedition.AchievementChallenge)
                    {
                        r3n.Log($" - write - challengeType 0");
                        r3n.Log($" - write - (item as AchievementChallenge).ID.Index: {(item as AchievementChallenge).ID.value}");
                        serializer.writer.Write(0);
                        serializer.writer.Write((item as AchievementChallenge).ID.value); // 
                    }
                    else if (item is Expedition.CycleScoreChallenge)
                    {
                        r3n.Log($" - write - challengeType 1");
                        r3n.Log($"read - increase {(item as CycleScoreChallenge).increase}");
                        r3n.Log($"read - killScores {(item as CycleScoreChallenge).killScores?.Length.ToString() ?? "null"}");
                        r3n.Log($"read - score {(item as CycleScoreChallenge).score}");
                        r3n.Log($"read - target {(item as CycleScoreChallenge).target}");
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
                        r3n.Log($" - write: challengeType 2");
                        serializer.writer.Write(2);
                        r3n.Log($" - write - ghost: {(item as EchoChallenge).ghost.value}");
                        serializer.writer.Write((item as EchoChallenge).ghost.value);
                    }
                    else if (item is Expedition.GlobalScoreChallenge)
                    {
                        r3n.Log($" - write: challengeType 3");
                        serializer.writer.Write(3);
                        r3n.Log($" - write - increase: {(item as GlobalScoreChallenge).increase}");
                        r3n.Log($" - write - killScores: {(item as GlobalScoreChallenge).killScores?.Length ?? -1}");
                        r3n.Log($" - write - score: {(item as GlobalScoreChallenge).score}");
                        r3n.Log($" - write - target: {(item as GlobalScoreChallenge).target}");
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
                        r3n.Log($" - write: challengeType 4");
                        serializer.writer.Write(4);
                        r3n.Log($" - write - amount: {(item as HuntChallenge).amount}");
                        r3n.Log($" - write - current: {(item as HuntChallenge).current}");
                        r3n.Log($" - write - target: {(item as HuntChallenge).target.value}");
                        serializer.writer.Write((item as HuntChallenge).amount);
                        serializer.writer.Write((item as HuntChallenge).current);
                        serializer.writer.Write((item as HuntChallenge).target.value); //creatureTemplate.type to string                                
                    }
                    else if (item is Expedition.ItemHoardChallenge)
                    {
                        r3n.Log($" - write: challengeType 5");
                        serializer.writer.Write(5);
                        r3n.Log($" - write - amount: {(item as ItemHoardChallenge).amount}");
                        r3n.Log($" - write - target: {(item as ItemHoardChallenge).target.value}");
                        serializer.writer.Write((item as ItemHoardChallenge).amount);
                        serializer.writer.Write((item as ItemHoardChallenge).target.value);
                    }
                    else if (item is Expedition.NeuronDeliveryChallenge)
                    {
                        r3n.Log($" - write: challengeType 6");
                        serializer.writer.Write(6);
                        r3n.Log($" - write - delivered: {(item as NeuronDeliveryChallenge).delivered}");
                        r3n.Log($" - write - neurons: {(item as NeuronDeliveryChallenge).neurons}");
                        serializer.writer.Write((item as NeuronDeliveryChallenge).delivered);
                        serializer.writer.Write((item as NeuronDeliveryChallenge).neurons);
                    }
                    else if (item is Expedition.PearlDeliveryChallenge)
                    {
                        r3n.Log($" - write: challengeType 7");
                        r3n.Log($" - write - iterator: {(item as PearlDeliveryChallenge).iterator}");
                        r3n.Log($" - write - region: {(item as PearlDeliveryChallenge).region}");
                        serializer.writer.Write(7);
                        serializer.writer.Write((item as PearlDeliveryChallenge).iterator);
                        serializer.writer.Write((item as PearlDeliveryChallenge).region);
                    }
                    else if (item is Expedition.PearlHoardChallenge)
                    {
                        r3n.Log($" - write: challengeType 8");
                        serializer.writer.Write(8);
                        r3n.Log($" - write - amount: {(item as PearlHoardChallenge).amount}");
                        r3n.Log($" - write - common: {(item as PearlHoardChallenge).common}");
                        r3n.Log($" - write - region: {(item as PearlHoardChallenge).region}");
                        serializer.writer.Write((item as PearlHoardChallenge).amount);
                        serializer.writer.Write((item as PearlHoardChallenge).common);
                        serializer.writer.Write((item as PearlHoardChallenge).region);
                    }
                    else if (item is Expedition.PinChallenge)
                    {
                        r3n.Log($" - write: challengeType 9");
                        r3n.Log($" - write - current: {(item as PinChallenge).current}");
                        r3n.Log($" - write - target: {(item as PinChallenge).target}");
                        r3n.Log($" - write - pinList: {(item as PinChallenge).pinList.Count}");
                        r3n.Log($" - write - spearList: {(item as PinChallenge).spearList.Count}");
                        serializer.writer.Write(9);
                        serializer.writer.Write((item as PinChallenge).current);
                        serializer.writer.Write((item as PinChallenge).target);
                        serializer.writer.Write((item as PinChallenge).pinList.Count); // will create an 0 len array
                        serializer.writer.Write((item as PinChallenge).spearList.Count); // will create an 0 len array

                        r3n.Log($"write - current {(item as PinChallenge).current}");
                        r3n.Log($"write - target {(item as PinChallenge).target}");
                        r3n.Log($"write - pinList {(item as PinChallenge).pinList}");
                        r3n.Log($"write - spearList {(item as PinChallenge).spearList}");


                    }
                    else if (item is Expedition.VistaChallenge)
                    {
                        r3n.Log($" - write: challengeType 10");
                        r3n.Log($" - write - location: ({(item as VistaChallenge).location.x},{(item as VistaChallenge).location.y})");
                        r3n.Log($" - write - region: ({(item as VistaChallenge).region}");
                        r3n.Log($" - write - room: ({(item as VistaChallenge).room}");

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



                    r3n.Log($"write - revealCheck {item.revealCheck}");
                    r3n.Log($"write - revealCheckDelay {item.revealCheckDelay}");
                    r3n.Log($"write - completed {item.completed}");
                    r3n.Log($"write - description {item.description}");
                    r3n.Log($"write - hidden {item.hidden}");
                    r3n.Log($"write - revealed {item.revealed}");

                }

            }
            public void readChallengeList(Serializer serializer, out List<Challenge> challenge)
            {
                //Challenge?[] challenge = new Challenge[count2];
                int count2 = serializer.reader.ReadInt32();
                r3n.Log($" - read: count: {count2}");
                challenge = new List<Challenge>(count2);
                for (int j = 0; j < count2; j++)
                {
                    int challengeType = serializer.reader.ReadInt32();
                    r3n.Log($" - read: challengeType: {challengeType}");
                    // revealCheck = false, revealCheckDelay = 0, completed = 0, description = "", hidden = false, revealed = false
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

                            r3n.Log($"read - ID: {_ID}");
                            r3n.Log($"read - revealCheck: {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay}");
                            r3n.Log($"read - completed: {_completed}");
                            r3n.Log($"read - description: {_description}");
                            r3n.Log($"read - hidden: {_hidden}");
                            r3n.Log($"read - revealed: {_revealed}");

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

                            r3n.Log($"read - increase {_increase}");
                            r3n.Log($"read - killScores {_killScores?.Length.ToString() ?? "null"}");
                            r3n.Log($"read - score {_score}");
                            r3n.Log($"read - target {_target}");
                            r3n.Log($"read - revealCheck {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay {_revealCheckDelay}");
                            r3n.Log($"read - completed {_completed}");
                            r3n.Log($"read - description {_description}");
                            r3n.Log($"read - hidden {_hidden}");
                            r3n.Log($"read - revealed {_revealed}");

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

                            r3n.Log($"read - ghost: {_ghost} ");
                            r3n.Log($"read - revealCheck: {_revealCheck} ");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay} ");
                            r3n.Log($"read - completed: {_completed} ");
                            r3n.Log($"read - description: {_description} ");
                            r3n.Log($"read - hidden: {_hidden} ");
                            r3n.Log($"read - revealed: {_revealed} ");

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
                            r3n.Log($"read - increase: {_increase}");
                            r3n.Log($"read - killScore: {_killScores}");
                            r3n.Log($"read - score: {_score}");
                            r3n.Log($"read - target: {_target}");
                            r3n.Log($"read - revealCheck: {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay}");
                            r3n.Log($"read - completed: {_completed}");
                            r3n.Log($"read - description: {_description}");
                            r3n.Log($"read - hidden: {_hidden}");
                            r3n.Log($"read - revealed: {_revealed}");

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

                            r3n.Log($"read - amount: {_amount}");
                            r3n.Log($"read - current: {_current}");
                            r3n.Log($"read - target: {_target}");
                            r3n.Log($"read - revealCheck: {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay}");
                            r3n.Log($"read - completed: {_completed}");
                            r3n.Log($"read - description: {_description}");
                            r3n.Log($"read - hidden: {_hidden}");
                            r3n.Log($"read - revealed: {_revealed}");

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

                            r3n.Log($"read - amount: {_amount}");
                            r3n.Log($"read - target: {_target}");
                            r3n.Log($"read - revealCheck: {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay}");
                            r3n.Log($"read - completed: {_completed}");
                            r3n.Log($"read - description: {_description}");
                            r3n.Log($"read - hidden: {_hidden}");
                            r3n.Log($"read - revealed: {_revealed}");

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

                            r3n.Log($"read - delivered: {_delivered}");
                            r3n.Log($"read - neurons: {_neurons}");
                            r3n.Log($"read - revealCheck: {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay}");
                            r3n.Log($"read - completed: {_completed}");
                            r3n.Log($"read - description: {_description}");
                            r3n.Log($"read - hidden: {_hidden}");
                            r3n.Log($"read - revealed: {_revealed}");

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

                            r3n.Log($"read - iterator: {_iterator} ");
                            r3n.Log($"read - region: {_region} ");
                            r3n.Log($"read - revealCheck: {_revealCheck} ");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay} ");
                            r3n.Log($"read - completed: {_completed} ");
                            r3n.Log($"read - description: {_description} ");
                            r3n.Log($"read - hidden: {_hidden} ");
                            r3n.Log($"read - revealed: {_revealed} ");
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

                            r3n.Log($"read - amount: {_amount}");
                            r3n.Log($"read - common: {_common}");
                            r3n.Log($"read - region: {_region}");
                            r3n.Log($"read - revealCheck: {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay: {_revealCheckDelay}");
                            r3n.Log($"read - completed: {_completed}");
                            r3n.Log($"read - description: {_description}");
                            r3n.Log($"read - hidden: {_hidden}");
                            r3n.Log($"read - revealed: {_revealed}");

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
                            var _pinList = new List<Creature>(serializer.reader.ReadInt32());
                            var _spearList = new List<Spear>(serializer.reader.ReadInt32());
                            var _revealCheck = serializer.reader.ReadBoolean();
                            var _revealCheckDelay = serializer.reader.ReadInt32();
                            var _completed = serializer.reader.ReadBoolean();
                            var _description = serializer.reader.ReadString();
                            var _hidden = serializer.reader.ReadBoolean();
                            var _revealed = serializer.reader.ReadBoolean();

                            r3n.Log($"read - current {_current}");
                            r3n.Log($"read - target {_target}");
                            r3n.Log($"read - pinList {_pinList}");
                            r3n.Log($"read - spearList {_spearList}");
                            r3n.Log($"read - revealCheck {_revealCheck}");
                            r3n.Log($"read - revealCheckDelay {_revealCheckDelay}");
                            r3n.Log($"read - completed {_completed}");
                            r3n.Log($"read - description {_description}");
                            r3n.Log($"read - hidden {_hidden}");
                            r3n.Log($"read - revealed {_revealed}");

                            c = new PinChallenge()
                            {
                                current = _current,
                                target = _target,
                                pinList = _pinList,
                                spearList = _spearList,
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

                            r3n.Log($"read - _location {_location}");
                            r3n.Log($"read - _region {_region}");
                            r3n.Log($"read - _room {_room}");
                            r3n.Log($"read - _revealCheck {_revealCheck}");
                            r3n.Log($"read - _revealCheckDelay {_revealCheckDelay}");
                            r3n.Log($"read - _completed {_completed}");
                            r3n.Log($"read - _description {_description}");
                            r3n.Log($"read - _hidden {_hidden}");
                            r3n.Log($"read - _revealed {_revealed}");

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
                //      r3n.Log($" - CustomSerialize");
                if (serializer.IsWriting)
                {
                    serializer.writer.Write(challengeDifficulty);//float 
                    r3n.Log($" - write: challengeDifficulty: {challengeDifficulty}");
                    //serializer.writer.Write(currentChallengeList.Count);
                    //foreach (var list in allChallengeLists)
                    // {
                    //   serializer.writer.Write(list.Key.ToString());
                    //   serializer.writer.Write(list.Value.Count);
                    writeChallengeList(serializer, currentChallengeList);
                    serializer.writer.Write(activeMission ?? "");
                    //}

                    //serializer.writer.Write(allChallengeLists); // Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>
                    //serializer.writer.Write(completedChallengeList); //List<Expedition.Challenge> 
                    //serializer.writer.Write(allActiveMissions);//Dictionary<string, string> 
                    //serializer.writer.Write(requiredExpeditionContent);//Dictionary<string, List<string>> 
                    serializer.writer.Write(startingDen ?? ""); //string 
                    serializer.writer.Write(newGame);//bool 
                    serializer.writer.Write(validateQuests);//bool 
                    serializer.writer.Write(hasViewedManual);//bool 
                                                             //serializer.writer.Write(allEarnedPassages);// Dictionary<string, int> 
                    serializer.writer.Write(devMode);//bool 
                    serializer.writer.Write(saveSlot);  //int 
                                                        //serializer.writer.Write(ints); ; //int[]
                                                        //serializer.writer.Write(slugcatPlayer);//SlugcatStats.Name 
                                                        //serializer.writer.Write(completedQuests);// List<string> 
                                                        //serializer.writer.Write(completedMissions);// List<string> 
                                                        //serializer.writer.Write(missionBestTimes);//Dictionary<string, int> 
                                                        //serializer.writer.Write(unlockables);// List<string> 
                                                        //serializer.writer.Write(newSongs);// List<string> 
                    serializer.writer.Write(level); // int  
                    serializer.writer.Write(currentPoints);  // int  
                    serializer.writer.Write(perkLimit);// int  
                    serializer.writer.Write(totalPoints);  // int  
                    serializer.writer.Write(totalChallengesCompleted); ; // int  
                    serializer.writer.Write(totalHiddenChallengesCompleted);  // int  
                    serializer.writer.Write(totalWins); // int  
                                                        //serializer.writer.Write(slugcatWins);// Dictionary<string, int> 
                                                        //serializer.writer.Write(challengeTypes);// Dictionary<string, int> 
                    serializer.writer.Write(menuSong ?? "");//string 

                }
                if (serializer.IsReading)
                {
                    r3n.Log($" - read: challengeDifficulty: {challengeDifficulty}");
                    challengeDifficulty = serializer.reader.ReadSingle();//float 

                    //allChallengeLists = serializer.reader.Read(); // Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>

                    //    allChallengeLists = new Dictionary<SlugcatStats.Name, List<Challenge>>(); serializer.writer.Write(allChallengeLists.Count);
                    //int count = serializer.reader.ReadInt32();
                    //for (int i = 0; i < count; i++)
                    //{
                    //    string name = serializer.reader.ReadString();
                    //    int count2 = serializer.reader.ReadInt32();
                    readChallengeList(serializer, out currentChallengeList);
                    r3n.Log($" - read - currentChallengeList len: {currentChallengeList.Count}");
                    string temp = serializer.reader.ReadString();//float 
                    activeMission = temp;// == "null" ? null : temp;

                    //    if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), name, false, out var enumBase))
                    //    {
                    //        allChallengeLists.Add((SlugcatStats.Name)enumBase, challenge);
                    //    }
                    //    else throw new InvalidProgrammerException("not a valid name here");
                    //}
                    //foreach (var list in allChallengeLists)
                    //{
                    //    serializer.writer.Write(list.Key.ToString());
                    //    serializer.writer.Write(list.Value.Count);
                    //    foreach (var item in list.Value)
                    //    {
                    //        serializer.writer.Write(item.revealCheck);
                    //        serializer.writer.Write(item.revealCheckDelay);
                    //        serializer.writer.Write(item.completed);
                    //        serializer.writer.Write(item.description);
                    //        serializer.writer.Write(item.hidden);
                    //        serializer.writer.Write(item.revealed);
                    //    }
                    //}

                    //completedChallengeList = serializer.reader.Read(); //List<Expedition.Challenge> 
                    //allActiveMissions = serializer.reader.Read();//Dictionary<string, string> 
                    //requiredExpeditionContent = serializer.reader.Read();//Dictionary<string, List<string>> 


                    startingDen = serializer.reader.ReadString(); //string 
                    newGame = serializer.reader.ReadBoolean();//bool 
                    validateQuests = serializer.reader.ReadBoolean();//bool 
                    hasViewedManual = serializer.reader.ReadBoolean();//bool 
                                                                      //allEarnedPassages = serializer.reader.Read();// Dictionary<string, int> 
                    devMode = serializer.reader.ReadBoolean();//bool 
                    saveSlot = serializer.reader.ReadInt32(); //int 
                                                              //ints = serializer.reader.ReadInt32(); //int[]
                                                              //slugcatPlayer = serializer.reader.Read();//SlugcatStats.Name 
                                                              //completedQuests = serializer.reader.Read();// List<string> 
                                                              //completedMissions = serializer.reader.Read();// List<string> 
                                                              //missionBestTimes = serializer.reader.Read(.);//Dictionary<string, int> 
                                                              //unlockables = serializer.reader.Read();// List<string> 
                                                              //newSongs = serializer.reader.Read();// List<string> 
                    level = serializer.reader.ReadInt32(); // int  
                    currentPoints = serializer.reader.ReadInt32(); ; // int  
                    perkLimit = serializer.reader.ReadInt32();// int  
                    totalPoints = serializer.reader.ReadInt32(); // int  
                    totalChallengesCompleted = serializer.reader.ReadInt32(); // int  
                    totalHiddenChallengesCompleted = serializer.reader.ReadInt32(); // int  
                    totalWins = serializer.reader.ReadInt32(); // int  
                                                               //slugcatWins = serializer.reader.Read();// Dictionary<string, int> 
                                                               //challengeTypes = serializer.reader.Read();// Dictionary<string, int> 
                    menuSong = serializer.reader.ReadString();//string 
                }
            }
            public class ChallengeState : Serializer.ICustomSerializable
            {
                public void CustomSerialize(Serializer serializer)
                {
                    throw new NotImplementedException();
                }
            }
        }

    }


    public static class ObjectSerialize
    {
        public static byte[] Serialize(this Object obj)
        {
            if (obj == null)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();

                binaryFormatter.Serialize(memoryStream, obj);

                var compressed = Compress(memoryStream.ToArray());
                return compressed;
            }
        }

        public static Object DeSerialize(this byte[] arrBytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                var decompressed = Decompress(arrBytes);

                memoryStream.Write(decompressed, 0, decompressed.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        public static byte[] Compress(byte[] input)
        {
            byte[] compressesData;

            using (var outputStream = new MemoryStream())
            {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    zip.Write(input, 0, input.Length);
                }

                compressesData = outputStream.ToArray();
            }

            return compressesData;
        }

        public static byte[] Decompress(byte[] input)
        {
            byte[] decompressedData;

            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new MemoryStream(input))
                {
                    using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outputStream);
                    }
                }

                decompressedData = outputStream.ToArray();
            }

            return decompressedData;
        }
    }
}
