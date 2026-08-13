using System;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;
using Watcher;

namespace RainMeadow
{
    [DeltaSupport(level = StateHandler.DeltaSupport.NullableDelta)]
    public class RealizedBoxWormState : RealizedCreatureState
    {
        public static ConditionalWeakTable<Creature, RelationshipTracker.DynamicRelationship> creatureRelationship = new();
        //public static ConditionalWeakTable<BoxWorm.Larva, BoxWorm.LarvaHolder> themoddershavebeenlefttostarve = new();

        [OnlineField(nullable = true)]
        Generics.DynamicOrderedStates<LarvaHolderState> larvaHolders;        
        
        [OnlineField (group = "counters")] // we will try to change the attack to an rpc later
        int attackTimer;
        [OnlineField(group = "counters")]
        int releaseSteamTimer;
        [OnlineField]
        int steamAvailable;
        [OnlineField]
        byte behavior;
        [OnlineField(nullable = true)]
        OnlineCreature? attackTarget;

        [OnlineField]
        byte relationshipType;        
        [OnlineField]
        float intensity;

        public RealizedBoxWormState() { }

        public RealizedBoxWormState(OnlineCreature onlineCreature) : base(onlineCreature)
        {
            var boxWorm = (BoxWorm)onlineCreature.realizedCreature;
            
            attackTimer = boxWorm.attackTimer;
            releaseSteamTimer = boxWorm.releaseSteamTimer;
            steamAvailable = boxWorm.steamAvailable;

            System.Collections.Generic.List<LarvaHolderState> LarvaHolders = new();
            for (int i = 0; i < boxWorm.larvaHolders.Length; i++)
            {
                if (boxWorm.larvaHolders[i].hasLarva)
                {
                   LarvaHolders.Add(new LarvaHolderState(boxWorm.larvaHolders[i], i));
                }
            }
            larvaHolders = new Generics.DynamicOrderedStates<LarvaHolderState>(LarvaHolders);

            if (boxWorm.ai is not null)
            {
                behavior = (byte)boxWorm.ai.behavior.index;
                attackTarget = boxWorm.ai.AttackTarget?.representedCreature.GetOnlineCreature();
                RainMeadow.Info($"{this} ctor attackTarget: { attackTarget}");
                if (creatureRelationship.TryGetValue(boxWorm.abstractCreature.realizedCreature, out var drelationship))
                {
                    relationshipType = (byte)drelationship.currentRelationship.type.index;
                    intensity = drelationship.currentRelationship.intensity;
                }
            }

            
        }

        public override void ReadTo(OnlineEntity onlineEntity)
        {
            base.ReadTo(onlineEntity);
            if (((OnlinePhysicalObject)onlineEntity).apo.realizedObject is not BoxWorm boxWorm) return;

            boxWorm.attackTimer.SetClamped(attackTimer);
            boxWorm.releaseSteamTimer.SetClamped(releaseSteamTimer);
            boxWorm.steamAvailable.SetClamped(steamAvailable);
            
            for (int i = 0; i < larvaHolders?.list?.Count; i++)
            {
                int index = larvaHolders.list[i].index;
                larvaHolders.list[i].ReadTo(boxWorm.larvaHolders[index]);
            }

            if (boxWorm.ai is not null)
            {
                //var newbehavior = behavior switch
                //{
                //    1 => Watcher.BoxWormAI.Behavior.ApproachEnemy,
                //    2 => Watcher.BoxWormAI.Behavior.Attack,
                //    3 => Watcher.BoxWormAI.Behavior.ReturnHome,
                //    _ => Watcher.BoxWormAI.Behavior.Dormant
                //};

                //setBehavior(boxWorm, newbehavior);
                if (attackTarget.abstractCreature != boxWorm.ai.AttackTarget.representedCreature)
                {
                    try
                    {
                        //boxWorm.ai.preyTracker.AddPrey(boxWorm.ai.tracker.RepresentationForCreature(attackTarget.abstractCreature, true));
                        var relationship = boxWorm.ai.tracker.RepresentationForCreature(attackTarget.abstractCreature, true);
                        if (creatureRelationship.TryGetValue(boxWorm.abstractCreature.realizedCreature, out var drelationship))
                        {
                            var rt = relationshipType switch {
                                0 => CreatureTemplate.Relationship.Type.DoesntTrack,
                                1 => CreatureTemplate.Relationship.Type.Ignores,
                                2 => CreatureTemplate.Relationship.Type.Eats,
                                3 => CreatureTemplate.Relationship.Type.Afraid,
                                4 => CreatureTemplate.Relationship.Type.StayOutOfWay,
                                5 => CreatureTemplate.Relationship.Type.AgressiveRival,
                                6 => CreatureTemplate.Relationship.Type.Attacks,
                                7 => CreatureTemplate.Relationship.Type.Uncomfortable,
                                8 => CreatureTemplate.Relationship.Type.Antagonizes,
                                9 => CreatureTemplate.Relationship.Type.PlaysWith,
                                10 => CreatureTemplate.Relationship.Type.SocialDependent,
                                11 => CreatureTemplate.Relationship.Type.Pack,
                            };

                            drelationship.rt.SortCreatureIntoModule(drelationship, new CreatureTemplate.Relationship(rt,intensity));
                        }
                    }
                    catch (Exception e)
                    {
                        RainMeadow.Error(e);
                    }
                    RainMeadow.Info($"{this} readto attackTarget: {attackTarget}");
                }
            }
        }
        void setBehavior(BoxWorm boxworm, BoxWormAI.Behavior newBehavior)
        {
            if (newBehavior == boxworm.ai.behavior) return;            

            BoxWormAI boxwormai = boxworm.ai;
            boxwormai.ThisCreature.ReleaseAllGrabbers();
            if (boxwormai.behavior == BoxWormAI.Behavior.Dormant || (boxwormai.behavior == BoxWormAI.Behavior.ApproachEnemy && newBehavior == BoxWormAI.Behavior.Attack))
            {
                boxwormai.ThisCreature.transitionTimer.Reset();
            }
            if (newBehavior == BoxWormAI.Behavior.ApproachEnemy)
            {
                boxwormai.memoryCounter.Add(400);
            }
            if (newBehavior == BoxWormAI.Behavior.Attack)
            {
                boxwormai.ThisCreature.attackTimer.Reset();
            }
            if (newBehavior == Watcher.BoxWormAI.Behavior.Dormant)
            {
                boxwormai.ThisCreature.helpCallTimer.Reset();
                boxwormai.ThisCreature.releaseSteamTimer.Reset();
            }
            boxwormai.behavior = newBehavior;
            boxwormai.Lag();            
        }
    }

    [DeltaSupport(level = StateHandler.DeltaSupport.NullableDelta)]
    public class LarvaHolderState : OnlineState
    {
        [OnlineField(nullable = true)]
        public OnlineEntity.EntityId onlineLarvaID;
        [OnlineField]
        public byte index;

        // [OnlineField]
        // bool forceRelease;
        // [OnlineField]
        // bool retracted;
        [OnlineField]
        byte timeToDislodge;

        public LarvaHolderState() { }
        public LarvaHolderState(BoxWorm.LarvaHolder holder, int index)
        {
            this.index = (byte)index;

            // forceRelease = holder.forceRelease;
            // retracted = holder.retracted;
            timeToDislodge = (byte)holder.timeToDislodge;
            if (holder.abstractLarva?.GetOnlineObject() is OnlinePhysicalObject opo)
                onlineLarvaID = opo.id;
        }
        public void ReadTo(BoxWorm.LarvaHolder holder)
        {
            // holder.forceRelease = forceRelease;
            // holder.retracted = retracted;            
            holder.timeToDislodge.SetClamped(timeToDislodge);            
            if (onlineLarvaID.FindEntity() is not OnlinePhysicalObject onlineLarva) return;            
            if (onlineLarva.apo?.realizedObject is not Watcher.BoxWorm.Larva larva) return;
            if (holder.abstractLarva != larva.abstractPhysicalObject)
            {                
                holder.abstractLarva = (BoxWorm.Larva.AbstractLarva)onlineLarva.apo;
                holder.hasLarva = true;
            }
        }
    }
}
