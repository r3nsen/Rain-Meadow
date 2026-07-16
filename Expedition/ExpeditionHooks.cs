using Expedition;

using Menu;

using Mono.Cecil.Cil;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainMeadow
{
    public partial class RainMeadow
    {
        public void ExpeditionHooks()
        {
            On.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud2;
            On.HUD.FoodMeter.GameUpdate += FoodMeter_GameUpdate;
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgression_GetOrInitiateSaveState1;
            On.Expedition.ExpeditionCoreFile.ExpeditionSaveFileName += ExpeditionCoreFile_ExpeditionSaveFileName;
            On.Expedition.ExpeditionCoreFile.Load += ExpeditionCoreFile_Load;
            On.PlayerProgression.ctor_RainWorld_bool_bool_string += PlayerProgression_ctor_RainWorld_bool_bool_string;
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgression_GetOrInitiateSaveState2;
            On.PlayerProgression.LoadProgression += PlayerProgression_LoadProgression;
            On.PlayerProgression.LoadGameState += PlayerProgression_LoadGameState;
            On.Menu.SlugcatSelectMenu.MineForSaveData += SlugcatSelectMenu_MineForSaveData;
            On.PlayerProgression.IsThereASavedGame += PlayerProgression_IsThereASavedGame;
            On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk1;
            //On.Player.AddFood += Player_AddFood1;
            //On.Player.AddQuarterFood += Player_AddQuarterFood1;
            //On.Player.SubtractFood += Player_SubtractFood1;
            IL.Menu.ExpeditionMenu.Update += ExpeditionMenu_Update;
            //     IL.Menu.ExpeditionMenu.SwitchBackground += ExpeditionMenu_SwitchBackground1;
            //  new Hook(typeof(Menu.ChallengeSelectPage).GetMethod(nameof(ChallengeSelectPage.StartGame)), typeof(ChallengeSelectOnlinePage).GetMethod(nameof(ChallengeSelectOnlinePage.StartGame)));
            On.Menu.ChallengeSelectPage.StartGame += ChallengeSelectPage_StartGame;
            On.Menu.CharacterSelectPage.LoadGame += CharacterSelectPage_LoadGame;
            IL.Menu.CharacterSelectPage.AbandonButton_OnPressDone += CharacterSelectPage_AbandonButton_OnPressDone; ;
            // debug
            On.Menu.CustomProgressionDialog.Singal += CustomProgressionDialog_Singal;
            On.Menu.CharacterSelectPage.UpdateChallengePreview += CharacterSelectPage_UpdateChallengePreview;
            On.Menu.CharacterSelectPage.UpdateStats += CharacterSelectPage_UpdateStats;
            On.Menu.CharacterSelectPage.Update += CharacterSelectPage_Update;
            On.Menu.CharacterSelectPage.ctor += CharacterSelectPage_ctor;
            On.ProcessManager.RequestMainProcessSwitch_ProcessID += ProcessManager_RequestMainProcessSwitch_ProcessID1;
            On.Menu.CharacterSelectPage.UpdateSelectedSlugcat += CharacterSelectPage_UpdateSelectedSlugcat;

            On.Creature.Die += Creature_Die1;
            On.Expedition.Challenge.CompleteChallenge += Challenge_CompleteChallenge;

            On.Expedition.CycleScoreChallenge.CreatureKilled += CycleScoreChallenge_CreatureKilled;
            On.Expedition.GlobalScoreChallenge.CreatureKilled += GlobalScoreChallenge_CreatureKilled;
            On.Expedition.HuntChallenge.CreatureKilled += HuntChallenge_CreatureKilled;
            IL.RainWorldGame.Update += RainWorldGame_Update2;
            //On.Expedition.PinChallenge.Update;


        }

        private void RainWorldGame_Update2(ILContext il)
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
                if (!self.completed)
                {
                    //rpc
                    if (isExpeditionMode(out var em))
                    {
                        int id = em.challengeIndex;
                        r3n.Log($"id == {id}");
                        if (id == -1)
                        {
                            r3n.Log($"oh no id == -1");
                            bool found = false;
                            // find id by description
                            for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
                            {
                                if (self.description == ExpeditionData.challengeList[i].description)
                                {
                                    if (found) throw new InvalidProgramException("not working, descriptions are equal ");
                                    id = i;
                                    r3n.Log($"id == {id}");
                                    found = true;
                                }
                            }
                            if (id == -1)
                            {
                                throw new InvalidProgramException("cannot find by descriptions");
                            }


                        }
                        if (!OnlineManager.lobby.isOwner && !em.isChallengeCompleted[id])
                        {
                            r3n.Log($"invokeRPC completeChallenge {id} - {em.isChallengeCompleted[id]}");
                            OnlineManager.lobby.owner.InvokeRPC(ExpeditionRPC.completeChallenge, id);
                        }
                        else orig(self);
                    }
                    else
                    {
                        orig(self);
                    }

                }
            }
        }

        private void HuntChallenge_CreatureKilled(On.Expedition.HuntChallenge.orig_CreatureKilled orig, Expedition.HuntChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is null)
            {
                orig(self, crit, playerNumber);
            }
            else
            {
                // rpc
                orig(self, crit, playerNumber);
            }
        }

        private void GlobalScoreChallenge_CreatureKilled(On.Expedition.GlobalScoreChallenge.orig_CreatureKilled orig, Expedition.GlobalScoreChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is null)
            {
                orig(self, crit, playerNumber);
            }
            else
            {
                // rpc
                orig(self, crit, playerNumber);
            }
        }

        private void CycleScoreChallenge_CreatureKilled(On.Expedition.CycleScoreChallenge.orig_CreatureKilled orig, Expedition.CycleScoreChallenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is null)
            {
                orig(self, crit, playerNumber);
            }
            else
            {
                // rpc
                orig(self, crit, playerNumber);
            }
        }

        private void Challenge_CreatureKilled(On.Expedition.Challenge.orig_CreatureKilled orig, Expedition.Challenge self, Creature crit, int playerNumber)
        {
            if (OnlineManager.lobby is null)
            {
                orig(self, crit, playerNumber);
            }
            else
            {
                // rpc
                orig(self, crit, playerNumber);
            }
        }

        private void Creature_Die1(On.Creature.orig_Die orig, Creature self)
        {
            r3n.Log($"{self} from {self.abstractCreature.GetOnlineCreature().owner} was slain");
            orig(self);
        }

        private void CharacterSelectPage_UpdateSelectedSlugcat(On.Menu.CharacterSelectPage.orig_UpdateSelectedSlugcat orig, CharacterSelectPage self, int num)
        {
            //try
            //{
            orig(self, num);
            //}
            //catch 
            //{
            //    r3n.Log($"Deu merda, {num} não é valido aqui");
            //    orig(self, 1);
            //}
        }

        private void ProcessManager_RequestMainProcessSwitch_ProcessID1(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if (ID == Expedition.ExpeditionEnums.ProcessID.ExpeditionMenu)
            {
                if (OnlineManager.lobby is not null)
                {
                    ID = RainMeadow.Ext_ProcessID.ExpeditionMenu;
                }
            }
            orig(self, ID);
        }

        private void CharacterSelectPage_AbandonButton_OnPressDone(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Menu.MenuObject>(nameof(Menu.MenuObject.menu)),
                x => x.MatchLdfld<MainLoopProcess>(nameof(MainLoopProcess.manager))//,
                // x => x.MatchLdsfld<Expedition.ExpeditionEnums.ProcessID>(nameof(Expedition.ExpeditionEnums.ProcessID.ExpeditionMenu))//,
                // x => x.MatchCallvirt<ProcessManager>(nameof(ProcessManager.RequestMainProcessSwitch))
                );
            c.Remove();
            c.Remove();
            c.EmitDelegate((ProcessManager manager) =>
            {

                ProcessManager.ProcessID id;
                if (OnlineManager.lobby is null) id = Expedition.ExpeditionEnums.ProcessID.ExpeditionMenu;
                else id = RainMeadow.Ext_ProcessID.ExpeditionMenu;
                manager.RequestMainProcessSwitch(id);
            });
        }

        private void CharacterSelectPage_ctor(On.Menu.CharacterSelectPage.orig_ctor orig, CharacterSelectPage self, Menu.Menu menu, MenuObject owner, UnityEngine.Vector2 pos)
        {

            //      r3n.Log("CharacterSelectPage_ctor");
            //      r3n.Log($" jolly: {ModManager.JollyCoop}");
            //      r3n.Log($" - self.jollyToggleConfigMenu: {self.jollyToggleConfigMenu is null}");

            orig(self, menu, owner, pos);
            //     r3n.Log($" - self.jollyToggleConfigMenu: {self.jollyToggleConfigMenu is null}");
        }


        private void CharacterSelectPage_Update(On.Menu.CharacterSelectPage.orig_Update orig, CharacterSelectPage self)
        {
            //r3n.Log($" - firstLoad: {self.firstLoad}");
            //r3n.Log($" - fullyLoaded: {(self.menu as Menu.ExpeditionMenu).fullyLoaded}");
            //r3n.Log($" - waitForSaveData: {self.waitForSaveData}");
            //r3n.Log($" - coreLoaded: {Expedition.Expedition.coreFile.coreLoaded}");
            //r3n.Log($" - progression: {self.menu?.manager?.rainWorld?.progression?.ToString() ?? "null"}");
            //r3n.Log($" - miscProgressionData: {self.menu?.manager?.rainWorld?.progression?.miscProgressionData?.ToString() ?? "null"}");
            //(self.owner.menu as ExpeditionMenu).currentSelection = 1;

            try
            {
                orig(self);
            }
            catch (Exception e)
            {
                r3n.Log($"ERROR: {e.Message} {e.StackTrace} {e.InnerException}");
            }
        }

        private void CharacterSelectPage_UpdateStats(On.Menu.CharacterSelectPage.orig_UpdateStats orig, CharacterSelectPage self)
        {
            //r3n.Log($"CharacterSelectPage_UpdateStats");
            //r3n.Log($" - firstLoad: {self.firstLoad}");
            //r3n.Log($" - fullyLoaded: {(self.menu as Menu.ExpeditionMenu).fullyLoaded}");
            //r3n.Log($" - waitForSaveData: {self.waitForSaveData}");
            //r3n.Log($" - coreLoaded: {Expedition.Expedition.coreFile.coreLoaded}");
            //r3n.Log($" - progression: {self.menu?.manager?.rainWorld?.progression?.ToString() ?? "null"}");
            //r3n.Log($" - miscProgressionData: {self.menu?.manager?.rainWorld?.progression?.miscProgressionData?.ToString() ?? "null"}");

            orig(self);
        }

        private void CharacterSelectPage_UpdateChallengePreview(On.Menu.CharacterSelectPage.orig_UpdateChallengePreview orig, CharacterSelectPage self)
        {
            //     r3n.Log($"CharacterSelectPage_UpdateChallengePreview");
            orig(self);
        }

        private void CustomProgressionDialog_Singal(On.Menu.CustomProgressionDialog.orig_Singal orig, CustomProgressionDialog self, MenuObject sender, string message)
        {
            //      r3n.Log($"CustomProgressionDialog_Singal - message: {message}");
            orig(self, sender, message);
        }

        private void HUD_InitSinglePlayerHud2(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);

            //if (isExpeditionMode(out var egameMode))
            //{
            //    self.AddPart(new OnlineHUD(self, cam, egameMode));
            //    self.AddPart(new SpectatorHud(self, cam));
            //    self.AddPart(new Pointing(self));

            //    if (MatchmakingManager.currentInstance.canSendChatMessages)
            //        self.AddPart(new ChatHud(self, cam));
            //}
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
                r3n.Log("StartGame");
                if (OnlineManager.lobby.isOwner)
                {
                    r3n.Log($" - StartGame - isOwner");
                    ExpeditionOnlineMenu.expeditionGameMode.currentCampaign = Expedition.ExpeditionData.slugcatPlayer;
                    r3n.Log($" - StartGame - current campain: {Expedition.ExpeditionData.slugcatPlayer}");
                }

                var expeditionGameMode = ExpeditionOnlineMenu.expeditionGameMode;
                for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
                {
                    expeditionGameMode.avatarSettings[i].playingAs = expeditionGameMode.currentCampaign;
                    r3n.Log($" - StartGame - expeditionGameMode.avatarSettings[{i}].playingAs: {expeditionGameMode.avatarSettings[i].playingAs}");
                }
                for (int i = 0; i < expeditionGameMode.avatarSettings.Length; i++)
                {
                    expeditionGameMode.avatarSettings[i].currentColors = [.. PlayerGraphics.DefaultBodyPartColorHex(expeditionGameMode.avatarSettings[i].playingAs).Select(RWCustom.Custom.hexToColor)];

                    r3n.Log($"avatarSettings[{i}].playingAs: {expeditionGameMode.avatarSettings[i].playingAs}");
                    r3n.Log($"avatarSettings[{i}].currentColors: {expeditionGameMode.avatarSettings[i].currentColors}");
                    expeditionGameMode.avatarSettings[i].currentColors.ForEach(x => r3n.Log($" - x: {x}"));

                }
            }
        }

        //private void ExpeditionMenu_SwitchBackground1(ILContext il)
        //{
        //    var c = new ILCursor(il);

        //}

        //private void ExpeditionMenu_SwitchBackground(On.Menu.ExpeditionMenu.orig_SwitchBackground orig, ExpeditionMenu self)
        //{
        //    if (self.scene == null)
        //    {
        //        return;
        //    }

        //    if (self.flatIllust)
        //    {
        //        self.manager.rainWorld.flatIllustrations = true;
        //    }

        //    self.scene.RemoveSprites();
        //    self.scene.RemoveSubObject(self.scene);
        //    self.scene = new InteractiveMenuScene(self, self.pages[0], self.currentScene);
        //    self.pages[0].subObjects.Add(self.scene);
        //    if (self.scene.depthIllustrations != null && self.scene.depthIllustrations.Count > 0)
        //    {
        //        int count = self.scene.depthIllustrations.Count;
        //        while (count-- > 0)
        //        {
        //            self.scene.depthIllustrations[count].sprite.MoveToBack();
        //        }
        //    }
        //    else
        //    {
        //        int count2 = self.scene.flatIllustrations.Count;
        //        while (count2-- > 0)
        //        {
        //            self.scene.flatIllustrations[count2].sprite.MoveToBack();
        //        }
        //    }

        //    self.characterSelect.ReloadSlugcatPortraits();
        //    self.pendingBackgroundChange = false;
        //    if (self.flatIllust)
        //    {
        //        self.manager.rainWorld.flatIllustrations = false;
        //    }
        //}

        private void ExpeditionMenu_Update(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);
            var skip = c.MarkLabel();
            c.GotoNext(MoveType.Before,
                x => x.MatchCall<Menu.ExpeditionMenu>(nameof(ExpeditionMenu.InitMenuPages))
                );
            c.Remove();
            c.EmitDelegate((ExpeditionMenu self) =>
            {
                if (OnlineManager.lobby is not null)
                {
                    if (self is ExpeditionOnlineMenu onlineSelf)
                        onlineSelf.InitMenuPages();
                }
                else
                {
                    r3n.Log("InitMenuPages");
                    self.InitMenuPages();
                }
            });
            c.GotoNext(MoveType.Before,
                x => x.MatchCall<Menu.ExpeditionMenu>(nameof(ExpeditionMenu.SwitchBackground))
                );
            c.Remove();
            c.EmitDelegate((ExpeditionMenu self) =>
            {
                r3n.Log($"Delegado - {OnlineManager.lobby}");
                if (OnlineManager.lobby is not null)
                {
                    r3n.Log($"{self}");
                    if (self is ExpeditionOnlineMenu onlineSelf)
                    {
                        r3n.Log($"onlineSelf.challengeSelect: {onlineSelf.challengeSelect}");
                        r3n.Log($"onlineSelf.characterSelect: {onlineSelf.characterSelect}");
                        r3n.Log($"onlineSelf.progressionPage: {onlineSelf.progressionPage}");
                        onlineSelf.SwitchBackground();
                    }
                }
                else
                {
                    r3n.Log("SwitchBackground");
                    self.SwitchBackground();
                }
            });

        }

        //private void Player_SubtractFood1(On.Player.orig_SubtractFood orig, Player self, int sub)
        //{
        //    r3n.Log(" expedition Player_SubtractFood1");
        //    if (OnlineManager.lobby is null)
        //    {
        //        orig(self, sub);
        //        return;
        //    }
        //    ChangeFood(() => orig(self, sub), self);
        //}

        //private void Player_AddQuarterFood1(On.Player.orig_AddQuarterFood orig, Player self)
        //{
        //    r3n.Log(" expedition Player_AddQuarterFood1");
        //    if (OnlineManager.lobby is null)
        //    {
        //        orig(self);
        //        return;
        //    }
        //    ChangeFood(() => orig(self), self);
        //}

        //private void Player_AddFood1(On.Player.orig_AddFood orig, Player self, int add)
        //{
        //    r3n.Log(" expedition Player_AddFood1");
        //    if (OnlineManager.lobby is null)
        //    {
        //        orig(self, add);
        //        return;
        //    }

        //    ChangeFood(() => orig(self, add), self);
        //}
        //private void ChangeFood(Action orig, Player self)
        //{
        //    //return;
        //    r3n.Log("ChangeFood");
        //    if (OnlineManager.lobby is null || !sUpdateFood)
        //    {
        //        r3n.Log(" - ChangeFood lobby is null");
        //        orig();
        //        return;
        //    }

        //    if (!OnlinePhysicalObject.map.TryGetValue(self.abstractPhysicalObject, out var onlineEntity))
        //    {
        //        r3n.Log($" - ChangeFood - no online entity: {self.abstractPhysicalObject}.");
        //        RainMeadow.Error("Player doesn't have OnlineEntity counterpart!!");
        //        orig();
        //        return;
        //    }

        //    if (!onlineEntity.isMine) return;
        //    r3n.Log(" - ChangeFood - is mine");

        //    var state = (isExpeditionMode(out _) && !self.isNPC) ? (PlayerState)self.abstractCreature.world.game.Players[0].state : (PlayerState)self.State;
        //    r3n.Log("isExpeditionMode(out _) && !self.isNPC ? (PlayerState)self.abstractCreature.world.game.Players[0].state : (PlayerState)self.State");
        //    r3n.Log($"{isExpeditionMode(out _)} && {!self.isNPC} ? {(PlayerState)self.abstractCreature.world.game.Players[0].state} : {(PlayerState)self.State}");
        //    r3n.Log($" - ChangeFood - state: {state}");
        //    var origFood = state.foodInStomach * 4 + state.quarterFoodPoints;
        //    r3n.Log($" - ChangeFood - origFood: {origFood}");

        //    orig();

        //    if (self.isNPC) return;
        //    r3n.Log($" - ChangeFood - not isNPC");
        //    if (!OnlineManager.lobby.isOwner && OnlineManager.lobby.gameMode is ExpeditionGameMode)
        //    {
        //        var newFood = state.foodInStomach * 4 + state.quarterFoodPoints;
        //        if (newFood != origFood)
        //        {
        //            r3n.Log($" - ChangeFood - invokeRCP ({(newFood - origFood)})");
        //            OnlineManager.lobby.owner.InvokeRPC(StoryRPCs.ChangeFood, (short)(newFood - origFood));
        //        }

        //    }

        //    // hack
        //    if (self.slugcatStats.malnourished && state.foodInStomach >= ((self.redsIllness != null) ? self.redsIllness.FoodToBeOkay : self.slugcatStats.maxFood))
        //    {
        //        if (self.redsIllness != null)
        //        {
        //            self.redsIllness.GetBetter();
        //            return;
        //        }
        //        if (!self.isSlugpup)
        //        {
        //            self.SetMalnourished(false);
        //        }
        //        if (self.playerState is MoreSlugcats.PlayerNPCState)
        //        {
        //            (self.playerState as MoreSlugcats.PlayerNPCState).Malnourished = false;
        //        }
        //    }
        //}


        private bool PlayerProgression_IsThereASavedGame(On.PlayerProgression.orig_IsThereASavedGame orig, PlayerProgression self, SlugcatStats.Name saveStateNumber)
        {
            if (OnlineManager.lobby is not null && isExpeditionMode(out _) && !OnlineManager.lobby.isOwner)
                return ExpeditionOnlineMenu.expeditionGameMode.hasSaveState;
            return orig(self, saveStateNumber);
        }

        private Menu.SlugcatSelectMenu.SaveGameData SlugcatSelectMenu_MineForSaveData(On.Menu.SlugcatSelectMenu.orig_MineForSaveData orig, ProcessManager manager, SlugcatStats.Name slugcat)
        {
            if (!isExpeditionMode(out _)) return orig(manager, slugcat);
            if (OnlineManager.lobby != null && !OnlineManager.lobby.isOwner)
            {
                return ExpeditionOnlineMenu.getSaveState();
            }
            r3n.Log("SlugcatSelectMenu_MineForSaveData");
            //r3n.Log($" - !manager.rainWorld.progression.IsThereASavedGame(slugcat): {!manager.rainWorld.progression.IsThereASavedGame(slugcat)}");
            r3n.Log($"manager.rainWorld.progression.currentSaveState != null ({manager.rainWorld.progression.currentSaveState != null}) && manager.rainWorld.progression.currentSaveState.saveStateNumber == slugcat({manager.rainWorld.progression.currentSaveState?.saveStateNumber == slugcat})");
            r3n.Log($"!manager.rainWorld.progression.HasSaveData {!manager.rainWorld.progression.HasSaveData}");

            r3n.Log($"SlugcatSelectMenu_MineForSaveData - manager: {manager} - slugcat: {slugcat}");
            var s = orig(manager, slugcat);

            if (s == null) return s;
            r3n.Log($" - karmaCap: {s.karmaCap}");
            r3n.Log($" - karma: {s.karma}");
            r3n.Log($" - karmaReinforced: {s.karmaReinforced}");
            r3n.Log($" - rippleLevel: {s.rippleLevel}");
            r3n.Log($" - shelterName: {s.shelterName}");
            r3n.Log($" - cycle: {s.cycle}");
            r3n.Log($" - hasGlow: {s.hasGlow}");
            r3n.Log($" - hasMark: {s.hasMark}");
            r3n.Log($" - redsExtraCycles: {s.redsExtraCycles}");
            r3n.Log($" - food: {s.food}");
            r3n.Log($" - redsDeath: {s.redsDeath}");
            r3n.Log($" - ascended: {s.ascended}");
            if (ModManager.MSC)
            {
                r3n.Log($" - altEnding: {s.altEnding}");
                r3n.Log($" - hasRobo: {s.hasRobo}");
                r3n.Log($" - pebblesEnergyTaken: {s.pebblesEnergyTaken}");
                r3n.Log($" - moonGivenRobe: {s.moonGivenRobe}");
            }

            if (ModManager.MMF)
            {
                r3n.Log($" - gameTimeAlive: {s.gameTimeAlive}");
                r3n.Log($" - gameTimeDead: {s.gameTimeDead}");
            }

            return s;
        }

        private bool PlayerProgression_SaveToDisk1(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
        {
            //if (!isExpeditionMode(out var egm) && !egm.saveToDisk) return false;
            r3n.Log(" - PlayerProgression_SaveToDisk1");
            return orig(self, saveCurrentState, saveMaps, saveMiscProg);
        }

        private SaveState PlayerProgression_LoadGameState(On.PlayerProgression.orig_LoadGameState orig, PlayerProgression self, string saveFilePath, RainWorldGame game, bool saveAsDeathOrQuit)
        {
            r3n.Log($"PlayerProgression_LoadGameState({self}, {saveFilePath}, {game}, {saveAsDeathOrQuit}) - saveFileDataInMemory: {self.saveFileDataInMemory}");
            return orig(self, saveFilePath, game, saveAsDeathOrQuit);
        }

        private void PlayerProgression_LoadProgression(On.PlayerProgression.orig_LoadProgression orig, PlayerProgression self)
        {
            r3n.Log($"PlayerProgression_LoadProgression({self}) - saveFileDataInMemory: {self.saveFileDataInMemory}");
            orig(self);
        }

        private SaveState PlayerProgression_GetOrInitiateSaveState2(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
        {
            r3n.Log($"PlayerProgression_GetOrInitiateSaveState2({self}, {saveStateNumber}, {game}, {setup}, {saveAsDeathOrQuit})");
            return orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
        }

        private void PlayerProgression_ctor_RainWorld_bool_bool_string(On.PlayerProgression.orig_ctor_RainWorld_bool_bool_string orig, PlayerProgression self, RainWorld rainWorld, bool tryLoad, bool saveAfterLoad, string overrideBaseDir)
        {
            r3n.Log($"PlayerProgression_ctor_RainWorld_bool_bool_string ({self}, {rainWorld}, {tryLoad}, {saveAfterLoad}, {overrideBaseDir})");
            orig(self, rainWorld, tryLoad, saveAfterLoad, overrideBaseDir);
        }

        private void ExpeditionCoreFile_Load(On.Expedition.ExpeditionCoreFile.orig_Load orig, Expedition.ExpeditionCoreFile self)
        {
            Expedition.ExpeditionCoreFile.CORE_FILE_DEFINITION.fileName = "online_expCore";
            orig(self);
        }

        private void ExpeditionCoreFile_LoadData(On.Expedition.ExpeditionCoreFile.orig_LoadData orig, Expedition.ExpeditionCoreFile self)
        {
            r3n.Log($"ExpeditionCoreFile_LoadData");
            orig(self);
        }

        private void ExpeditionCoreFile_ctor(On.Expedition.ExpeditionCoreFile.orig_ctor orig, Expedition.ExpeditionCoreFile self, RainWorld rainWorld)
        {
            r3n.Log($"ExpeditionCoreFile_ctor");
            orig(self, rainWorld);
        }

        private string ExpeditionCoreFile_ExpeditionSaveFileName(On.Expedition.ExpeditionCoreFile.orig_ExpeditionSaveFileName orig, Expedition.ExpeditionCoreFile self)
        {
            if (OnlineManager.lobby == null)
            {
                return orig(self);
            }
            r3n.Log($"ExpeditionSaveFileName: {self.rainWorld.options.saveSlot}");
            if (self.rainWorld.options.saveSlot >= 0)
            {
                return "online_expCore" + (self.rainWorld.options.saveSlot + 1);
            }

            return "online_expCore" + Math.Abs(self.rainWorld.options.saveSlot);



        }

        private SaveState PlayerProgression_GetOrInitiateSaveState1(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
        {
            return orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
        }

        public static bool isExpeditionMode(out ExpeditionGameMode gameMode)
        {
            gameMode = null;
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is ExpeditionGameMode sgm)
            {
                gameMode = sgm;
                return true;
            }
            return false;
        }

        private void FoodMeter_GameUpdate(On.HUD.FoodMeter.orig_GameUpdate orig, HUD.FoodMeter self)
        {
            try
            {
                //r3n.Log($"(orig: {orig}, food meter: {self})");
                orig(self);
            }
            catch (Exception e)
            {
                RainMeadow.Error(e);
                r3n.Log($"Error: {e.Message} {e.StackTrace} {e.InnerException}");
            }
        }

        private string ExpeditionGame_ExpeditionRandomStarts(On.Expedition.ExpeditionGame.orig_ExpeditionRandomStarts orig, RainWorld rainWorld, SlugcatStats.Name slug)
        {
            try
            {
                r3n.Log($"(rainWorld: {rainWorld}, slug: {slug})");
                return orig(rainWorld, slug);
            }
            catch (Exception e)
            {
                RainMeadow.Error(e);
                r3n.Log($"Error: {e.Message} {e.StackTrace} {e.InnerException}");
            }
            return "SU_S01";
        }
    }
}
