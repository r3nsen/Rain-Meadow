using Expedition;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainMeadow
{
    public static class ExpeditionRPC
    {

        [RPCMethod]
        public static void challengeCreaturePinned(int index, OnlineCreature crit)
        {
            if (!(RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)) return;
            if (!(ExpeditionData.challengeList[index] is PinChallenge)) throw new InvalidProgrammerException("not pin challenge");

            crit.creature.Realize();
            var cl = (PinChallenge)ExpeditionData.challengeList[index];
            if (!cl.pinList.Contains(crit.realizedCreature))
            {
                cl.pinList.Add(crit.realizedCreature);
                cl.current++;
                cl.UpdateDescription();
            }
        }

        [RPCMethod]
        public static void challengeCreatureKilled(int index, OnlineCreature crit, int pNum)
        {

            if (!(RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)) return;

            crit.creature.Realize();
            ExpeditionData.challengeList[index].CreatureKilled(crit.realizedCreature, pNum);            
        }

        [RPCMethod]
        public static void completeChallenge(int index)
        {
            if (!(RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)) return;
            ExpeditionData.challengeList[index].CompleteChallenge();
        }

        [RPCMethod]
        public static void SlowTimePerk()
        {
            if (!(RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)) return;
                
            for (int i = 0; i < ExpeditionGame.unlockTrackers.Count; i++)
            {
                if (ExpeditionGame.unlockTrackers[i] is ExpeditionGame.SlowTimeTracker stt)
                {
                    ((Player)stt.game.Players[0].realizedCreature).mushroomCounter = 100;
                    stt.cooldown = 10;
                }
            }
        }
    }
}
