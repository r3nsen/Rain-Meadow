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

            var cl = ExpeditionData.challengeList[index] as PinChallenge;
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
            
            var cl = ExpeditionData.challengeList[index];
            cl.CreatureKilled(crit.realizedCreature, pNum);            
        }

        [RPCMethod]
        public static void completeChallenge(int index)
        {
            if (!(RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)) return;
            r3n.Log($"run RPC completeChallenge {index}");
            ExpeditionData.challengeList[index].CompleteChallenge();
        }
    }
}
