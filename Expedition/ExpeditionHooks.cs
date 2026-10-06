using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Menu;
using Expedition;
using System.Text.RegularExpressions;

namespace RainMeadow
{
    public partial class RainMeadow
    {
        public void ExpeditionHooks()
        {
            On.Expedition.ExpeditionCoreFile.ExpeditionSaveFileName += ExpeditionCoreFile_ExpeditionSaveFileName;            
            On.Menu.SlugcatSelectMenu.MineForSaveData += SlugcatSelectMenu_MineForSaveData;
            On.PlayerProgression.IsThereASavedGame += PlayerProgression_IsThereASavedGame;

            On.Menu.ChallengeSelectPage.StartGame += ChallengeSelectPage_StartGame;
            On.Menu.CharacterSelectPage.LoadGame += CharacterSelectPage_LoadGame;
            IL.Menu.CharacterSelectPage.AbandonButton_OnPressDone += CharacterSelectPage_AbandonButton_OnPressDone;

            On.Expedition.Challenge.CompleteChallenge += Challenge_CompleteChallenge;

            On.Expedition.CycleScoreChallenge.CreatureKilled += CycleScoreChallenge_CreatureKilled;
            On.Expedition.GlobalScoreChallenge.CreatureKilled += GlobalScoreChallenge_CreatureKilled;
            On.Expedition.HuntChallenge.CreatureKilled += HuntChallenge_CreatureKilled;
            IL.Expedition.PinChallenge.Update += PinChallenge_Update;
            IL.RainWorldGame.Update += RainWorldGame_Update_GetChallengeIndex;
            IL.PlayerSessionRecord.AddKill += PlayerSessionRecord_AddKill_GetChallengeIndex;

            IL.ProcessManager.PreSwitchMainProcess += ProcessManager_PreSwitchMainProcess;

            On.Expedition.ExpeditionGame.SlowTimeTracker.Update += SlowTimeTracker_Update;
            On.Expedition.ExpeditionProgression.UnlockSprite += ExpeditionProgression_UnlockSprite;

            new Hook(typeof(ExpeditionGame).GetProperty(nameof(ExpeditionGame.activeUnlocks)).GetGetMethod(), ExpeditionGame_activeUnlocks);

            IL.Expedition.ExpeditionCoreFile.ToString += ExpeditionCoreFile_ToString;
            On.Expedition.ExpeditionCoreFile.FromString += unlockAllMeadowMusics;            
            On.Expedition.ExpeditionProgression.GetUnlockedSongs += ExpeditionProgression_GetUnlockedSongs;
            On.Expedition.PinChallenge.Reset += PinChallenge_Reset;
            On.Expedition.PinChallenge.ctor += PinChallenge_ctor;

            On.Expedition.ExpeditionCoreFile.FromString += ExpeditionCoreFile_FromString;
            On.Expedition.ExpeditionCoreFile.ToString += ExpeditionCoreFile_ToString1;
            On.Expedition.ExpeditionProgression.CheckUnlocked += ExpeditionProgression_CheckUnlocked;
            On.Expedition.CycleScoreChallenge.ToString += CycleScoreChallenge_ToString;
        }

        private string CycleScoreChallenge_ToString(On.Expedition.CycleScoreChallenge.orig_ToString orig, CycleScoreChallenge self)
        {
            if (isExpeditionMode(out _))
            {
                return string.Concat(new string[]
                {
                    "CycleScoreChallenge",
                    "~",                    
                    Menu.Remix.ValueConverter.ConvertToString<int>(self.target),
                    "><",
                    self.completed ? "1" : "0",
                    "><",
                    self.hidden ? "1" : "0",
                    "><",
                    self.revealed ? "1" : "0",
                    "><",
                    Menu.Remix.ValueConverter.ConvertToString<int>(self.score)
                    
                });
            }

            return orig(self);
        }

        private bool ExpeditionProgression_CheckUnlocked(On.Expedition.ExpeditionProgression.orig_CheckUnlocked orig, ProcessManager manager, SlugcatStats.Name slugcat)
        {

            if (isExpeditionMode(out _)) return true;
            return orig(manager, slugcat);
        }

        private string formatCoreFile(string s)
        {
            string formatted = s.Replace("<expC>", "<expC>\n");
            formatted = formatted.Replace("<>", "<>\n\t");
            formatted = Regex.Replace(formatted, @"(UNLOCKS:|NEWSONGS:|QUESTS:|MISSIONS:)", "$1\n\t");

            formatted = Regex.Replace(formatted, @"(?<=\[CHALLENGES\]<expC>\n)(.*?)(?=\[END CHALLENGES\])",
            m => Regex.Replace(m.Value, @"^(.+)$", "\t$1", RegexOptions.Multiline));

            formatted = Regex.Replace(formatted, @"(?<=\[UNLOCKS\]<expC>\n)(.*?)(?=\[END UNLOCKS\])",
                m => Regex.Replace(m.Value, @"^(.+)$", "\t$1", RegexOptions.Multiline));

            formatted = Regex.Replace(formatted, @"(?<=\[PASSAGES\]<expC>\n)(.*?)(?=\[END PASSAGES\])",
                m => Regex.Replace(m.Value, @"^(.+)$", "\t$1", RegexOptions.Multiline));

            formatted = Regex.Replace(formatted, @"(?<=\[CONTENT\]<expC>\n)(.*?)(?=\[END CONTENT\])",
                m => Regex.Replace(m.Value, @"^(.+)$", "\t$1", RegexOptions.Multiline));

            formatted = Regex.Replace(formatted, @"(?<=\[ONLINEMENU\]<expC>\n)(.*?)(?=\[END ONLINEMENU\])",
                m => Regex.Replace(m.Value, @"^(.+)$", "\t$1", RegexOptions.Multiline));

            return formatted;
        }

        private string ExpeditionCoreFile_ToString1(On.Expedition.ExpeditionCoreFile.orig_ToString orig, ExpeditionCoreFile self)
        {
            var s = orig(self);
            if (isExpeditionMode(out _))
            {

                List<string> onlineData = new List<string>();
                onlineData.Add("[ONLINEDATA]");

                onlineData.Add($"CUSTOMCOLORS:{(ExpeditionOnlineCoreFIle.customColors? 1 : 0)}");
                onlineData.Add($"ISPUP:{(ExpeditionOnlineCoreFIle.isPup ? 1 : 0)}");

                onlineData.Add("[END ONLINEDATA]");

                string output = s + "<expC>" + string.Join("<expC>", onlineData.ToArray());

                RainMeadow.Debug($"\n{formatCoreFile(output)}");
                return output;
            }

            return s;
        }

        private void ExpeditionCoreFile_FromString(On.Expedition.ExpeditionCoreFile.orig_FromString orig, ExpeditionCoreFile self, string saveString)
        {
            orig(self, saveString);
            if (isExpeditionMode(out _))
            {

                string[] s = System.Text.RegularExpressions.Regex.Split(saveString, "<expC>");
                bool onlineData = false;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == "[ONLINEDATA]") onlineData = true;
                    if (s[i] == "[END ONLINEDATA]") break;

                    if (onlineData)
                    {
                        if (s[i].StartsWith("CUSTOMCOLORS:"))
                        {
                            ExpeditionOnlineCoreFIle.customColors = Regex.Split(s[i], ":")[1] == "1";
                        }
                        if (s[i].StartsWith("ISPUP:"))
                        {
                            ExpeditionOnlineCoreFIle.isPup = Regex.Split(s[i], ":")[1] == "1";
                        }
                    }
                }

                RainMeadow.Debug($"\n{formatCoreFile(saveString)}");
            }
        }

        private void PinChallenge_ctor(On.Expedition.PinChallenge.orig_ctor orig, PinChallenge self)
        {
            orig(self);
            if (isExpeditionMode(out var ex))
            {
                if (OnlineManager.lobby.isOwner)
                {
                    ex.pinChallenge_PinList = new List<OnlineCreature>();
                }
            }
        }

        private void PinChallenge_Reset(On.Expedition.PinChallenge.orig_Reset orig, PinChallenge self)
        {
            orig(self);
            if (isExpeditionMode(out var ex))
            {
                ex.pinChallenge_PinList = new List<OnlineCreature>();
            }

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
            try
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
            }
            catch (Exception e)
            {
                Error($"Error while IL hooking : {e}");
            }
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
            try
            {
                var c = new ILCursor(il);

                // if (global::ModManager.Expedition && ID == global::ProcessManager.ProcessID.MainMenu && this.rainWorld.options.saveSlot < 0)

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
            catch (Exception e)
            {
                Error($"Error while IL hooking : {e}");
            }
        }

        private void PinChallenge_Update(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                var skip = c.DefineLabel();

                // if (this.spearList[k].stuckInObject != null && this.spearList[k].stuckInObject is global::Creature && this.spearList[k].stuckInWall != null && !this.pinList.Contains(this.spearList[k].stuckInObject as global::Creature))

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
                    if (isExpeditionMode(out var ex))
                    {
                        getChallengeID(self, out var id);

                        var stuckInSpear = (Creature)self.spearList[index].stuckInObject;

                        if (stuckInSpear.abstractCreature.GetOnlineCreature() is OnlineCreature onlineCrit)
                        {
                            RainMeadow.Debug($"creature: {onlineCrit}, killer: {onlineCrit.abstractCreature.realizedCreature.killTag}, owner: {onlineCrit.owner}");

                            if (OnlineManager.lobby.isOwner)
                            {
                                ex.pinChallenge_PinList.Add(onlineCrit);
                                return false;
                            }
                            OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreaturePinned, id, onlineCrit);
                            self.pinList.Add(stuckInSpear);
                        }
                        return true;
                    }
                    return false;
                });
                c.Emit(OpCodes.Brtrue, skip);
            }
            catch (Exception e)
            {
                Error($"Error while IL hooking : {e}");
            }
        }

        //r3n: hack - hook on RainWorldGame.Update and get the current index of the for loop that check and calls ExpeditionData.challengeList[i].Update
        private void RainWorldGame_Update_GetChallengeIndex(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // for (int num13 = 0; num13 < global::Expedition.ExpeditionData.challengeList.Count; num13++)
                //      ^^^^^^^^^^^^^

                c.GotoNext(MoveType.After,
                    x => x.MatchLdcI4(0),
                    x => x.MatchStloc(24),
                    x => x.MatchBr(out _)
                );

                c.MoveAfterLabels();

                c.Emit(OpCodes.Ldloc, 24);
                c.EmitDelegate((int index) =>
                {
                    if (OnlineManager.lobby is not null && isExpeditionMode(out var em))
                    {
                        em.challengeIndex = index;
                    }
                });

                // for (int num13 = 0; num13 < global::Expedition.ExpeditionData.challengeList.Count; num13++)
                //                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

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
            catch (Exception e)
            {
                Error($"Error while IL hooking : {e}");
            }
        }

        private void PlayerSessionRecord_AddKill_GetChallengeIndex(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // for (int j = 0; j < global::Expedition.ExpeditionData.challengeList.Count; j++)
                //      ^^^^^^^^^

                c.GotoNext(MoveType.After,
                    x => x.MatchLdcI4(0),
                    x => x.MatchStloc(1),
                    x => x.MatchBr(out _)
                );

                c.MoveAfterLabels();

                c.Emit(OpCodes.Ldloc, 1);
                c.EmitDelegate((int index) =>
                {
                    if (OnlineManager.lobby is not null && isExpeditionMode(out var em))
                    {
                        em.challengeIndex = index;
                    }
                });

                // for (int j = 0; j < global::Expedition.ExpeditionData.challengeList.Count; j++)
                //                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                c.GotoNext(MoveType.After,
                    x => x.MatchLdloc(1),
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
            catch (Exception e)
            {
                Error($"Error while IL hooking : {e}");
            }
        }

        private void Challenge_CompleteChallenge(On.Expedition.Challenge.orig_CompleteChallenge orig, Expedition.Challenge self)
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

        bool getChallengeID(Challenge self, out int id)
        {
            id = -1;
            if (isExpeditionMode(out var em))
            {
                id = em.challengeIndex;
                if (id == -1)
                {
                    RainMeadow.Debug("challengeIndex is -1, searching challenge by description");
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
            if (OnlineManager.lobby is not null)
            {
                if (self.completed || self.game == null || crit == null) return;
                if (crit.abstractCreature.GetOnlineCreature() is not OnlineCreature onlineCrit) return;
                if (!onlineCrit.isMine) return;

                RainMeadow.Info($"creature: {onlineCrit}, killer: {crit.abstractCreature.realizedCreature.killTag}, owner: {onlineCrit.owner}");

                if (OnlineManager.lobby.isOwner)
                {
                    RainMeadow.Info("HuntChallenge - Player " + (OnlineManager.lobby.owner).ToString() + " killed " + onlineCrit);
                    orig(self, crit, playerNumber);
                    return;
                }

                getChallengeID(self, out var id);

                CreatureTemplate.Type type = crit.abstractCreature.creatureTemplate.type;
                if (self.target == type || (self.target == CreatureTemplate.Type.DaddyLongLegs && type == CreatureTemplate.Type.BrotherLongLegs && (crit as DaddyLongLegs).colorClass))
                    OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreatureKilled, id, onlineCrit, playerNumber);
            }
            else
            {
                orig(self, crit, playerNumber);
            }
        }

        private void GlobalScoreChallenge_CreatureKilled(On.Expedition.GlobalScoreChallenge.orig_CreatureKilled orig, Expedition.GlobalScoreChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is not null)
            {
                if (self.completed || self.game == null || crit == null) return;
                if (crit.abstractCreature.GetOnlineCreature() is not OnlineCreature onlineCrit) return;
                if (!onlineCrit.isMine) return;

                RainMeadow.Info($"creature: {onlineCrit}, killer: {crit.abstractCreature.realizedCreature.killTag}, owner: {onlineCrit.owner}");

                if (OnlineManager.lobby.isOwner)
                {
                    RainMeadow.Info("GlobalScroeChallenge - Player " + (OnlineManager.lobby.owner).ToString() + " killed " + onlineCrit);
                    orig(self, crit, playerNumber);
                    return;
                }

                getChallengeID(self, out var id);
                CreatureTemplate.Type type = crit.abstractCreature.creatureTemplate.type;
                    
                if (type != null && ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type) is ChallengeTools.ExpeditionCreature globalExpeditionCrit)
                    OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreatureKilled, id, onlineCrit, playerNumber);
            }
            else
            {
                orig(self, crit, playerNumber);
            }
        }

        private void CycleScoreChallenge_CreatureKilled(On.Expedition.CycleScoreChallenge.orig_CreatureKilled orig, Expedition.CycleScoreChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is not null)
            {
                if (self.completed || self.game == null || crit == null) return;
                if (crit.abstractCreature.GetOnlineCreature() is not OnlineCreature onlineCrit) return;
                if (!onlineCrit.isMine) return;

                RainMeadow.Info($"creature: {onlineCrit}, killer: {crit.abstractCreature.realizedCreature.killTag}, owner: {onlineCrit.owner}");

                if (OnlineManager.lobby.isOwner)
                {
                    RainMeadow.Info("CycleScoreChallenge - Player " + (OnlineManager.lobby.owner).ToString() + " killed " + onlineCrit);
                    orig(self, crit, playerNumber);
                    return;
                }

                getChallengeID(self, out var id);
                CreatureTemplate.Type type = crit.abstractCreature.creatureTemplate.type;

                if (type != null && ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type) is ChallengeTools.ExpeditionCreature globalExpeditionCrit)
                    OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.challengeCreatureKilled, id, onlineCrit, playerNumber);
            }
            else
            {
                orig(self, crit, playerNumber);
            }
        }

        private void CharacterSelectPage_AbandonButton_OnPressDone(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // this.menu.manager.RequestMainProcessSwitch(global::Expedition.ExpeditionEnums.ProcessID.ExpeditionMenu);

                c.GotoNext(MoveType.Before,
                    x => x.MatchLdsfld<ExpeditionEnums.ProcessID>(nameof(ExpeditionEnums.ProcessID.ExpeditionMenu)),
                    x => x.MatchCallvirt<ProcessManager>(nameof(ProcessManager.RequestMainProcessSwitch))
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
            catch (Exception e)
            {
                Error($"Error while IL hooking : {e}");
            }
        }

        private void CharacterSelectPage_LoadGame(On.Menu.CharacterSelectPage.orig_LoadGame orig, CharacterSelectPage self)
        {
            if(self.menu is ExpeditionOnlineMenu om) om.pre_start();
            orig(self);
        }

        private void ChallengeSelectPage_StartGame(On.Menu.ChallengeSelectPage.orig_StartGame orig, ChallengeSelectPage self)
        {
            if (self.menu is ExpeditionOnlineMenu om) om.pre_start();
            orig(self);
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
                    return orig(self, saveStateNumber);
                    em.hasSaveState = orig(self, saveStateNumber);
                    return em.hasSaveState;
                }
            }
            return orig(self, saveStateNumber);
        }

        private Menu.SlugcatSelectMenu.SaveGameData SlugcatSelectMenu_MineForSaveData(On.Menu.SlugcatSelectMenu.orig_MineForSaveData orig, ProcessManager manager, SlugcatStats.Name slugcat)
        {
            if (!isExpeditionMode(out var em)) return orig(manager, slugcat);

            if (OnlineManager.lobby != null && !OnlineManager.lobby.isOwner)
                return em.menuSaveGameData;

            var sgd = orig(manager, slugcat);

            //em.SetSaveData(sgd);

            return sgd;
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
