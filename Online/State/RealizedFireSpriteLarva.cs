using System.Runtime.CompilerServices;
using Watcher;

namespace RainMeadow
{
    [DeltaSupport(level = StateHandler.DeltaSupport.NullableDelta)]
    public class RealizedFireSpriteLarva : RealizedPhysicalObjectState
    {
    //    public static ConditionalWeakTable<BoxWorm.Larva, BoxWorm.LarvaHolder> themoddershavebeenlefttostarve = new();

        [OnlineField]
        byte bites = 3;
        [OnlineField]
        bool edible;

        [OnlineField(nullable = true)]
        // LarvaHolderState? holderState;

        public RealizedFireSpriteLarva() { }
        public RealizedFireSpriteLarva(OnlinePhysicalObject onlineEntity) : base(onlineEntity)
        {
            var larva = (BoxWorm.Larva)onlineEntity.apo.realizedObject;

            bites = (byte)larva.bites;
            edible = larva.edible;
            
            //if (HolderFromBoxWorm(larva) is BoxWorm.LarvaHolder holder)
            //{
            //    holderState = new(holder);
            //}
        }

        public override void ReadTo(OnlineEntity onlineEntity)
        {
            base.ReadTo(onlineEntity);
            var larva = (BoxWorm.Larva)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;

            larva.bites = bites;
            larva.edible = edible;
            
            //if (HolderFromBoxWorm(larva) is BoxWorm.LarvaHolder holder)
            //{
            //    holderState?.ReadTo(holder);
            //}
        }

        //public BoxWorm.LarvaHolder? HolderFromBoxWorm(BoxWorm.Larva larva)
        //{
        //    if (themoddershavebeenlefttostarve.TryGetValue(larva, out var holder))
        //    {
        //        return holder;
        //    }
        //    return null;
        //}
    }

    //[DeltaSupport(level = StateHandler.DeltaSupport.NullableDelta)]
    //public class LarvaHolderState : OnlineState
    //{
    //    [OnlineField]
    //    bool forceRelease;
    //    [OnlineField]
    //    bool retracted;
    //    [OnlineField]
    //    byte timeToDislodge;
        
    //    public LarvaHolderState() { }
    //    public LarvaHolderState(BoxWorm.LarvaHolder holder)//, int index)
    //    {            
    //        forceRelease = holder.forceRelease;
    //        retracted = holder.retracted;
    //        timeToDislodge = (byte)holder.timeToDislodge;
    //    }
    //    public void ReadTo(BoxWorm.LarvaHolder holder)
    //    {            
    //        holder.forceRelease = forceRelease;
    //        holder.retracted = retracted;
    //        holder.timeToDislodge.SetClamped(timeToDislodge);
    //    }
    //}
}
