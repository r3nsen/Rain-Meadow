using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Menu;
using Expedition;

namespace RainMeadow
{
    public partial class RainMeadow
    {
        public void ExpeditionHooks()
        {

            On.Expedition.ExpeditionCoreFile.ExpeditionSaveFileName += ExpeditionCoreFile_ExpeditionSaveFileName;
            On.Expedition.ExpeditionCoreFile.Load += ExpeditionCoreFile_Load;
            On.Menu.SlugcatSelectMenu.MineForSaveData += SlugcatSelectMenu_MineForSaveData;
            On.PlayerProgression.IsThereASavedGame += PlayerProgression_IsThereASavedGame;

            On.Menu.ChallengeSelectPage.StartGame += ChallengeSelectPage_StartGame;
            On.Menu.CharacterSelectPage.LoadGame += CharacterSelectPage_LoadGame;
            IL.Menu.CharacterSelectPage.AbandonButton_OnPressDone += CharacterSelectPage_AbandonButton_OnPressDone;

            On.Creature.Die += Creature_Die1;
            On.Expedition.Challenge.CompleteChallenge += Challenge_CompleteChallenge;

            On.Expedition.CycleScoreChallenge.CreatureKilled += CycleScoreChallenge_CreatureKilled;
            On.Expedition.GlobalScoreChallenge.CreatureKilled += GlobalScoreChallenge_CreatureKilled;
            On.Expedition.HuntChallenge.CreatureKilled += HuntChallenge_CreatureKilled;
            IL.Expedition.PinChallenge.Update += PinChallenge_Update;
            IL.RainWorldGame.Update += GetChallengeIndex;

            //new Hook(typeof(RainMeadow.RainMeadow).GetMethod("Options_GetSaveFileName_SavOrExp", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), Options_GetSaveFileName_SavOrExp);

            IL.ProcessManager.PreSwitchMainProcess += ProcessManager_PreSwitchMainProcess;

            On.Expedition.ExpeditionGame.SlowTimeTracker.Update += SlowTimeTracker_Update;
            On.Expedition.ExpeditionProgression.UnlockSprite += ExpeditionProgression_UnlockSprite;

            new Hook(typeof(ExpeditionGame).GetProperty(nameof(ExpeditionGame.activeUnlocks)).GetGetMethod(), ExpeditionGame_activeUnlocks);
            IL.Expedition.ExpeditionCoreFile.ToString += ExpeditionCoreFile_ToString;
            On.Expedition.ExpeditionCoreFile.FromString += unlockAllMeadowMusics;            
            On.Expedition.ExpeditionProgression.GetUnlockedSongs += ExpeditionProgression_GetUnlockedSongs;
        }

        private void unlockAllMeadowMusics(On.Expedition.ExpeditionCoreFile.orig_FromString orig, ExpeditionCoreFile self, string saveString)
        {
            orig(self, saveString);

            if (OnlineManager.lobby is not null)
            {
                var unlockedSongs = Expedition.ExpeditionProgression.GetUnlockedSongs();
                var cascenKey = unlockedSongs.FirstOrDefault(x => x.Value == "Cascen").Key; // r3n: cascen is so fire
                if (!Expedition.ExpeditionData.unlockables.Contains(cascenKey))
                {
                    int index = int.Parse(cascenKey.Split('-')[1]) - 2;
                    var meadowMusics = getMeadowMusics();
                    for (int i = 0; i < 34; i++)
                    {                        
                        Expedition.ExpeditionData.unlockables.Add("mus-" + (i + index));
                        Expedition.ExpeditionData.newSongs.Add("mus-" + (i + index));
                    }
                }
            }
        }
        List<string> getMeadowMusics()
        {
            List<string> meadowMusics = new List<string>(){
                    "403rings",
                    "71104",
                    "Cascen",
                    "DustAshWrong",
                    "Establish",
                    "Eyes_ Vain",
                    "Eyto",
                    "Folkada",
                    "Grasp",
                    "Gray Orange",
                    "Icy Parchment",
                    "indufor",
                    "Live more.",
                    "me",
                    "MTC",
                    "Nevertop Side",
                    "New and new",
                    "Ones",
                    "Pedal Petal",
                    "Porls",
                    "Purple Puff",
                    "Significance",
                    "Slightly Ill",
                    "Smoothed Ash",
                    "Soup",
                    "Swan ode",
                    "The Crewmate",
                    "tredjeplanen",
                    "Triptrap X",
                    "Trists",
                    "Void Genesis",
                    "Walked",
                    "Well Phoe",
                    "Woodback",
                };
            return meadowMusics;
        }
        private Dictionary<string, string> ExpeditionProgression_GetUnlockedSongs(On.Expedition.ExpeditionProgression.orig_GetUnlockedSongs orig)
        {
            var unlockedSongs = orig();
            var meadowMusics = getMeadowMusics();
            if (OnlineManager.lobby is not null)
            {
                int ulen = unlockedSongs.Count;
                for (int i = 0; i < meadowMusics.Count; i++)
                {
                    unlockedSongs["mus-" + Menu.Remix.ValueConverter.ConvertToString<int>(i + 1 + ulen)] = meadowMusics[i];
                }                
            }
            return unlockedSongs;
        }

        private void ExpeditionCoreFile_ToString(ILContext il)
        {
            var c = new ILCursor(il);
            var skip = c.DefineLabel();
            // if (!this.runEnded && global::Expedition.ExpeditionGame.activeUnlocks != null && global::Expedition.ExpeditionGame.activeUnlocks.Count > 0)
            c.GotoNext(MoveType.Before,
                // x => x.MatchLdarg(0),
                x => x.MatchLdfld<ExpeditionCoreFile>("runEnded"),
                x => x.MatchBrtrue(out skip),

                x => x.MatchCall(typeof(ExpeditionGame).GetProperty(nameof(ExpeditionGame.activeUnlocks)).GetGetMethod()),
                x => x.MatchBrfalse(out _)
                );
            c.MoveAfterLabels();

            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate((ExpeditionCoreFile self, List<string> list) =>
            {
                if (OnlineManager.lobby is null || OnlineManager.lobby.isOwner) return false;

                if (!ExpeditionGame.allUnlocks.ContainsKey(ExpeditionData.slugcatPlayer))
                {
                    ExpeditionGame.allUnlocks[ExpeditionData.slugcatPlayer] = new List<string>();
                }

                List<string> activeUnlocks = ExpeditionGame.allUnlocks[ExpeditionData.slugcatPlayer];

                if (!self.runEnded && activeUnlocks != null && activeUnlocks.Count > 0)
                {
                    list.Add(ExpeditionData.slugcatPlayer.value + "#" + self.ActiveUnlocksString(activeUnlocks));
                }
                return true;
            });
            c.Emit(OpCodes.Brtrue, skip);
            c.Emit(OpCodes.Ldarg_0);
            // Info(il);
        }

        private List<string> ExpeditionGame_activeUnlocks(Func<List<string>> orig)
        {
            if (OnlineManager.lobby is null || OnlineManager.lobby.isOwner) return orig();
            return ExpeditionOnlineMenu.activeUnlocks;
        }
        private string ExpeditionProgression_UnlockSprite(On.Expedition.ExpeditionProgression.orig_UnlockSprite orig, string key, bool alwaysShow)
        {
            if (OnlineManager.lobby is null || OnlineManager.lobby.isOwner)
                return orig(key, alwaysShow);
            return orig(key, true);
        }

        private void SlowTimeTracker_Update(On.Expedition.ExpeditionGame.SlowTimeTracker.orig_Update orig, ExpeditionGame.SlowTimeTracker self)
        {
            if (OnlineManager.lobby is null || OnlineManager.lobby.isOwner)
            {
                orig(self);
                return;
            }

            if (self.cooldown > 0) return;

            for (int i = 0; i < self.game.Players.Count; i++)
            {
                if (self.game.Players[i].realizedCreature != null)
                {
                    Player player = (Player)self.game.Players[i].realizedCreature;

                    if (((player.input[0].mp && player.input[1].pckp) || (player.input[0].pckp && player.input[1].mp)) && self.cooldown <= 0f)
                    {
                        OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.SlowTimePerk);
                        break;
                    }
                }
            }
        }

        private void ProcessManager_PreSwitchMainProcess(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchLdsfld<ProcessManager.ProcessID>(nameof(ProcessManager.ProcessID.MainMenu)),
                x => x.MatchCall("ExtEnum`1<ProcessManager/ProcessID>", "op_Equality")
                );

            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((bool isMainMenu, ProcessManager.ProcessID process) =>
            {
                return isMainMenu || process == Ext_ProcessID.LobbySelectMenu;

            });
        }


        private void PinChallenge_Update(ILContext il)
        {
            var c = new ILCursor(il);
            var skip = c.DefineLabel();
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Expedition.PinChallenge>(nameof(Expedition.PinChallenge.pinList)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Expedition.PinChallenge>(nameof(Expedition.PinChallenge.spearList)),
                x => x.MatchLdloc(2),
                x => x.MatchCallvirt(typeof(List<Spear>).GetMethod("get_Item")),
                x => x.MatchLdfld<Spear>(nameof(Spear.stuckInObject)),
                x => x.MatchIsinst<Creature>(),
                x => x.MatchCallvirt(typeof(List<Creature>).GetMethod("Contains")),
                x => x.MatchBrtrue(out skip)
                );
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_2);
            c.EmitDelegate((PinChallenge self, int index) =>
            {
                if (OnlineManager.lobby is null) return false;
                if (OnlineManager.lobby.isOwner) return false;
                if (isExpeditionMode(out var em))
                {
                    getChallengeID(self, out var id);

                    var stuckInSpear = (Creature)self.spearList[index].stuckInObject;

                    if (stuckInSpear.abstractCreature.GetOnlineCreature() is OnlineCreature onlineCrit)
                    {
                        OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreaturePinned, id, onlineCrit);
                        self.pinList.Add(stuckInSpear);
                    }
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, skip);
        }

        //r3n: hack - hook on RainWorldGame.Update and get the current index of the for loop that check and calls ExpeditionData.challengeList[i].Update
        private void GetChallengeIndex(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(24),
                x => x.MatchBr(out _));
            c.Emit(OpCodes.Ldloc, 24);
            c.EmitDelegate((int index) =>
            {
                if (OnlineManager.lobby is not null && isExpeditionMode(out var em))
                {
                    em.challengeIndex = index;
                }
            });

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(24),
            x => x.MatchCall("Expedition.ExpeditionData", "get_challengeList"),
            x => x.MatchCallvirt(typeof(List<Challenge>).GetProperty("Count").GetGetMethod()),
            x => x.MatchBlt(out _)
        );
            c.EmitDelegate(() =>
            {
                if (OnlineManager.lobby is not null && isExpeditionMode(out var em))
                {
                    em.challengeIndex = -1;
                }
            });
        }

        private void Challenge_CompleteChallenge(On.Expedition.Challenge.orig_CompleteChallenge orig, Expedition.Challenge self)
        {
            if (OnlineManager.lobby is null)
                orig(self);
            else
            {
                if (isExpeditionMode(out var em))
                {
                    if (!self.completed)
                    {

                        if (!OnlineManager.lobby.isOwner && getChallengeID(self, out var id) && !em.isChallengeCompleted[id])
                        {
                            OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.completeChallenge, id);
                        }
                        else orig(self);
                    }
                    return;
                }
                orig(self);
            }
        }

        bool getChallengeID(Expedition.Challenge self, out int id)
        {
            id = -1;
            if (isExpeditionMode(out var em))
            {
                id = em.challengeIndex;
                if (id == -1)
                {
                    bool found = false;
                    // find id by description
                    for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
                    {
                        if (self.description == ExpeditionData.challengeList[i].description)
                        {
                            if (found) throw new InvalidProgramException("not working, descriptions are equal "); // r3n: can ocour if try to complete challenge just calling the CompleteChallenge method and more than one challenge are hidden
                            id = i;
                            found = true;
                        }
                    }
                    if (id == -1)
                    {
                        throw new InvalidProgramException("cannot find by descriptions");
                    }
                }
            }
            return (id != -1);
        }

        private void HuntChallenge_CreatureKilled(On.Expedition.HuntChallenge.orig_CreatureKilled orig, Expedition.HuntChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is not null && !OnlineManager.lobby.isOwner)
            {
                getChallengeID(self, out var id);

                if (crit.abstractCreature.GetOnlineCreature() is OnlineCreature onlineCrit)
                    OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreatureKilled, id, onlineCrit, playerNumber);
            }
            else
            {
                orig(self, crit, playerNumber);
            }
        }

        private void GlobalScoreChallenge_CreatureKilled(On.Expedition.GlobalScoreChallenge.orig_CreatureKilled orig, Expedition.GlobalScoreChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is not null && !OnlineManager.lobby.isOwner)
            {
                getChallengeID(self, out var id);

                if (crit.abstractCreature.GetOnlineCreature() is OnlineCreature onlineCrit)
                    OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreatureKilled, id, onlineCrit, playerNumber);
            }
            else
            {
                orig(self, crit, playerNumber);
            }
        }

        private void CycleScoreChallenge_CreatureKilled(On.Expedition.CycleScoreChallenge.orig_CreatureKilled orig, Expedition.CycleScoreChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is not null && !OnlineManager.lobby.isOwner)
            {
                getChallengeID(self, out var id);

                if (crit.abstractCreature.GetOnlineCreature() is OnlineCreature onlineCrit)
                    OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreatureKilled, id, onlineCrit, playerNumber);
            }
            else
            {
                orig(self, crit, playerNumber);
            }
        }

        private void Creature_Die1(On.Creature.orig_Die orig, Creature self)
        {
            // r3n: want to use this to inform host all the client kills, this then will be use to sync and separate kills per client on sleep and end screen, also will need to save this info on the savefile
            orig(self);
        }

        private void CharacterSelectPage_AbandonButton_OnPressDone(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Menu.MenuObject>(nameof(Menu.MenuObject.menu)),
                x => x.MatchLdfld<MainLoopProcess>(nameof(MainLoopProcess.manager))
                );
            c.Remove(); // Ldsfld Expedition.ExpeditionEnums.ProcessID.ExpeditionMenu
            c.Remove(); // Callvirt ProcessManager.RequestMainProcessSwitch
            c.EmitDelegate((ProcessManager manager) =>
            {

                ProcessManager.ProcessID id;
                if (OnlineManager.lobby is null) id = Expedition.ExpeditionEnums.ProcessID.ExpeditionMenu;
                else id = Ext_ProcessID.ExpeditionMenu;
                manager.RequestMainProcessSwitch(id);
            });
        }

        private void CharacterSelectPage_LoadGame(On.Menu.CharacterSelectPage.orig_LoadGame orig, CharacterSelectPage self)
        {
            pre_start();
            orig(self);
        }

        private void ChallengeSelectPage_StartGame(On.Menu.ChallengeSelectPage.orig_StartGame orig, ChallengeSelectPage self)
        {
            pre_start();
            orig(self);
        }

        void pre_start()
        {
            if (OnlineManager.lobby != null)
            {
                if (OnlineManager.lobby.isOwner)
                {
                    ExpeditionOnlineMenu.expeditionGameMode.currentCampaign = Expedition.ExpeditionData.slugcatPlayer;
                }

                var expeditionGameMode = ExpeditionOnlineMenu.expeditionGameMode;
                for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
                {
                    expeditionGameMode.avatarSettings[i].playingAs = expeditionGameMode.currentCampaign;
                }

                for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
                {
                    expeditionGameMode.avatarSettings[i].currentColors = [.. PlayerGraphics.DefaultBodyPartColorHex(expeditionGameMode.avatarSettings[i].playingAs).Select(RWCustom.Custom.hexToColor)];
                }
            }
        }

        private bool PlayerProgression_IsThereASavedGame(On.PlayerProgression.orig_IsThereASavedGame orig, PlayerProgression self, SlugcatStats.Name saveStateNumber)
        {
            if (OnlineManager.lobby is not null && isExpeditionMode(out var em))
            {
                if (!OnlineManager.lobby.isOwner)
                {
                    return em.hasSaveState;
                }
                else
                {
                    em.hasSaveState = orig(self, saveStateNumber);
                    return em.hasSaveState;
                }
            }
            return orig(self, saveStateNumber);
        }

        private Menu.SlugcatSelectMenu.SaveGameData SlugcatSelectMenu_MineForSaveData(On.Menu.SlugcatSelectMenu.orig_MineForSaveData orig, ProcessManager manager, SlugcatStats.Name slugcat)
        {
            if (!isExpeditionMode(out _)) return orig(manager, slugcat);

            if (OnlineManager.lobby != null && !OnlineManager.lobby.isOwner)
                return ExpeditionOnlineMenu.getSaveState();

            return orig(manager, slugcat);
        }
        private void ExpeditionCoreFile_Load(On.Expedition.ExpeditionCoreFile.orig_Load orig, Expedition.ExpeditionCoreFile self)
        {
            Expedition.ExpeditionCoreFile.CORE_FILE_DEFINITION.fileName = "online_expCore";
            orig(self);
        }

        private string ExpeditionCoreFile_ExpeditionSaveFileName(On.Expedition.ExpeditionCoreFile.orig_ExpeditionSaveFileName orig, Expedition.ExpeditionCoreFile self)
        {
            if (OnlineManager.lobby == null)
            {
                return orig(self);
            }
            if (self.rainWorld.options.saveSlot >= 0)
            {
                return "online_expCore" + (self.rainWorld.options.saveSlot + 1);
            }

            return "online_expCore" + Math.Abs(self.rainWorld.options.saveSlot);
        }

        public static bool isExpeditionMode(out ExpeditionGameMode gameMode)
        {
            gameMode = null!;
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is ExpeditionGameMode sgm)
            {
                gameMode = sgm;
                return true;
            }
            return false;
        }
    }
}
