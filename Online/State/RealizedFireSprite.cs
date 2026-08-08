
namespace RainMeadow
{
    public class RealizedFireSprite : RealizedPhysicalObjectState
    {
        [OnlineField]
        byte behavior;
        public RealizedFireSprite() { }
        public RealizedFireSprite(OnlinePhysicalObject onlineEntity) : base(onlineEntity) 
        {            
            var fireSprite = (Watcher.FireSprite)onlineEntity.apo.realizedObject;
            if(fireSprite.ai is not null)
                behavior = (byte)fireSprite.ai.behavior.index;         
        }

        public override void ReadTo(OnlineEntity onlineEntity)
        {            
            base.ReadTo(onlineEntity);
            var fireSprite = (Watcher.FireSprite)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;
            if (fireSprite.ai is not null)
                fireSprite.ai.behavior = behavior switch
                {
                    0 => Watcher.FireSpriteAI.Behavior.CollectEnergy,
                    2 => Watcher.FireSpriteAI.Behavior.ActivateRoot,
                    3 => Watcher.FireSpriteAI.Behavior.AvoidDanger,
                    4 => Watcher.FireSpriteAI.Behavior.Helping,
                    _ => Watcher.FireSpriteAI.Behavior.Idle,
                };         
        }
    }
}
