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

        }

        private SaveState PlayerProgression_GetOrInitiateSaveState1(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
        {
            if (OnlineManager.lobby == null)
            {
                return orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
            }
            if (RainMeadow.isStoryMode(out var storyGameMode))
            {
                RainMeadow.Debug("story: initiating save state!");
                if (self.currentSaveState == null && self.starvedSaveState != null && game != null && (!ModManager.MSC || game.manager.artificerDreamNumber == -1))
                {
                    //Custom.Log("LOADING STARVED STATE");
                    self.currentSaveState = self.starvedSaveState;
                    self.currentSaveState.deathPersistentSaveData.winState.ResetLastShownValues();
                    self.starvedSaveState = null;
                }

                if (self.currentSaveState != null && self.currentSaveState.saveStateNumber == saveStateNumber)
                {
                    if (saveAsDeathOrQuit)
                    {
                        self.SaveDeathPersistentDataOfCurrentState(saveAsIfPlayerDied: true, saveAsIfPlayerQuit: true);
                    }
                    RainMeadow.Debug("story: save state was not null and equals this save state number, returning save state!");
                    SaveStateHandler(self, storyGameMode, game);
                    return self.currentSaveState;
                }

                self.currentSaveState = new SaveState(saveStateNumber, self);


                if (self.saveFileDataInMemory == null || self.loadInProgress || !self.saveFileDataInMemory.Contains("save") || !setup.LoadInitCondition)
                {
                    self.currentSaveState.LoadGame("", game);
                }
                else
                {
                    // Returns `self.currentSaveState` or `null`
                    SaveState saveState = self.LoadGameState(null, game, saveAsDeathOrQuit);
                    if (saveState != null)
                    {
                        // Modifies `self.currentSaveState`, so `saveState` is invalid now
                        SaveStateHandler(self, storyGameMode, game);
                        return self.currentSaveState;
                    }

                    self.currentSaveState.LoadGame("", game);
                }


                if (saveAsDeathOrQuit)
                {
                    self.SaveDeathPersistentDataOfCurrentState(saveAsIfPlayerDied: true, saveAsIfPlayerQuit: true);
                }
                SaveStateHandler(self, storyGameMode, game);
                return self.currentSaveState;

            }
            return orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
        }

        public static bool isExpeditionMode(out StoryGameMode gameMode)
        {
            gameMode = null;
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode sgm)
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
                r3n.Log($"(orig: {orig}, food meter: {self})");
                orig(self);
            }
            catch (Exception e)
            {
                RainMeadow.Error(e);
                r3n.Log($"Error: {e.Message} {e.StackTrace} {e.InnerException}");
            }
        }

        private void HUD_InitSinglePlayerHud2(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            try
            {
                r3n.Log($"(self: {self}, cam: {cam})");
                orig(self, cam);
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
