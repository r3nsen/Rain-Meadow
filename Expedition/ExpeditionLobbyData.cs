using Expedition;

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
            [OnlineField(nullable = true)]
            ExpeditionDataState expeditionDataState;
            public State() { }
            public State(ExpeditionLobbyData expeditionLobbyData, OnlineResource onlineResource)
            {
                r3n.Log($" - State");
                ExpeditionGameMode expeditionGameMode = (onlineResource as Lobby).gameMode as ExpeditionGameMode;
                RainWorldGame currentGameState = RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;

                if (currentGameState?.session is StoryGameSession)
                {
                    expeditionDataState = new ExpeditionDataState();
                    r3n.Log($" - in session expeditionDataState: {expeditionDataState}");
                }
                else
                {
                    //expeditionDataState = expeditionGameMode.expeditionDataState;
                    expeditionDataState = new ExpeditionDataState();
                    r3n.Log($" - expeditionDataState: {expeditionDataState}");
                }
            }

            public override Type GetDataType()
            {
                return typeof(ExpeditionLobbyData);
            }

            public override void ReadTo(ResourceData data, OnlineResource resource)
            {
                r3n.Log($" - ReadTo");
                var lobby = (resource as Lobby);
                var expedition = (lobby.gameMode as ExpeditionGameMode);
                if (expedition.expeditionDataState != expeditionDataState)
                {
                    expedition.expeditionDataState = expeditionDataState;
                    expeditionDataState.CreateSaveData();
                    r3n.Log($" ^ expeditionDataState: {expeditionDataState}");
                }
            }
        }
        public class ExpeditionDataState : Serializer.ICustomSerializable
        {

            //   public Dictionary<SlugcatStats.Name, List<Expedition.Challenge>> allChallengeLists = new Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>();
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
                r3n.Log($" - ExpeditionDataState");
                //challengeDifficulty = ExpeditionData.challengeDifficulty;
                //   public Dictionary<SlugcatStats.Name, List<Expedition.Challenge>> allChallengeLists = new Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>();
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
                r3n.Log($" - CreateSaveData");
            //    ExpeditionData.challengeDifficulty = challengeDifficulty;
                //   public Dictionary<SlugcatStats.Name, List<Expedition.Challenge>> allChallengeLists = new Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>();
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
            public void CustomSerialize(Serializer serializer)
            {
                r3n.Log($" - CustomSerialize");
                if (serializer.IsWriting)
                {
                  //  serializer.writer.Write(challengeDifficulty); // float
                    //serializer.writer.Write(allChallengeLists); // Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>
                    //serializer.writer.Write(completedChallengeList); //List<Expedition.Challenge> 
                    //serializer.writer.Write(allActiveMissions);//Dictionary<string, string> 
                    //serializer.writer.Write(requiredExpeditionContent);//Dictionary<string, List<string>> 
                    serializer.writer.Write(challengeDifficulty);//float 
                    serializer.writer.Write(startingDen??""); //string 
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

                    #region serializating
                    //byte[] s;
                    //serializer.writer.Write(challengeDifficulty);
                    //s = ObjectSerialize.Serialize(allChallengeLists);
                    //serializer.writer.Write(s); // Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>
                    //s = ObjectSerialize.Serialize(completedChallengeList);
                    //serializer.writer.Write(s); //List<Expedition.Challenge> 
                    //s = ObjectSerialize.Serialize(allActiveMissions);
                    //serializer.writer.Write(s);//Dictionary<string, string> 
                    //s = ObjectSerialize.Serialize(requiredExpeditionContent);
                    //serializer.writer.Write(s);//Dictionary<string, List<string>> 
                    //serializer.writer.Write(challengeDifficulty);//float 
                    //serializer.writer.Write(startingDen); ; //string 
                    //serializer.writer.Write(newGame);//bool 
                    //serializer.writer.Write(validateQuests);//bool 
                    //serializer.writer.Write(hasViewedManual);//bool 
                    //s = ObjectSerialize.Serialize(allEarnedPassages);
                    //serializer.writer.Write(s);// Dictionary<string, int> 
                    //serializer.writer.Write(devMode);//bool 
                    //serializer.writer.Write(saveSlot); ; //int 
                    //s = ObjectSerialize.Serialize(ints);
                    //serializer.writer.Write(s); //int[]
                    //s = ObjectSerialize.Serialize(slugcatPlayer);
                    //serializer.writer.Write(s);//SlugcatStats.Name 
                    //s = ObjectSerialize.Serialize(completedQuests);
                    //serializer.writer.Write(s);// List<string> 
                    //s = ObjectSerialize.Serialize(completedMissions);
                    //serializer.writer.Write(s);// List<string> 
                    //s = ObjectSerialize.Serialize(missionBestTimes);
                    //serializer.writer.Write(s);//Dictionary<string, int> 
                    //s = ObjectSerialize.Serialize(unlockables);
                    //serializer.writer.Write(s);// List<string> 
                    //s = ObjectSerialize.Serialize(newSongs);
                    //serializer.writer.Write(s);// List<string> 
                    //serializer.writer.Write(level); // int  
                    //serializer.writer.Write(currentPoints); ; // int  
                    //serializer.writer.Write(perkLimit);// int  
                    //serializer.writer.Write(totalPoints); ; // int  
                    //serializer.writer.Write(totalChallengesCompleted); ; // int  
                    //serializer.writer.Write(totalHiddenChallengesCompleted); ; // int  
                    //serializer.writer.Write(totalWins); // int  
                    //s = ObjectSerialize.Serialize(slugcatWins);
                    //serializer.writer.Write(s);// Dictionary<string, int> 
                    //s = ObjectSerialize.Serialize(challengeTypes);
                    //serializer.writer.Write(s);// Dictionary<string, int> 
                    //serializer.writer.Write(menuSong);//string 
                    #endregion
                }
                if (serializer.IsReading)
                {
                   // challengeDifficulty = serializer.reader.ReadSingle(); // float
                    //allChallengeLists = serializer.reader.Read(); // Dictionary<SlugcatStats.Name, List<Expedition.Challenge>>
                    //completedChallengeList = serializer.reader.Read(); //List<Expedition.Challenge> 
                    //allActiveMissions = serializer.reader.Read();//Dictionary<string, string> 
                    //requiredExpeditionContent = serializer.reader.Read();//Dictionary<string, List<string>> 
                    challengeDifficulty = serializer.reader.ReadSingle();//float 
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
