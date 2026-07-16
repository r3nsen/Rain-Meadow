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
        //[RPCMethod]
        //public static void challengeCreatureKilled(int index, Creature crit,int pNum)
        //{
        //    if (!(RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)) return;
        //    ExpeditionData.challengeList[index].CreatureKilled(crit,pNum);
        //}

        [RPCMethod]
        public static void completeChallenge(int index)
        {
            if (!(RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)) return;
            r3n.Log($"run RPC completeChallenge {index}");
            ExpeditionData.challengeList[index].CompleteChallenge();
        }
    }
}
