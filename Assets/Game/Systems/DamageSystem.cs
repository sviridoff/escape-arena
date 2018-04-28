using Assets.Game.Components;
using Assets.Game.Events;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Pools;
using EcsRx.Systems.Custom;
using EcsRx.Unity.Components;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class DamageSystem : EventReactionSystem<DamageEvent>
    {
        private IPool _pool;
        private IEventSystem _eventSystem;

        public DamageSystem(
            IEventSystem eventSystem,
            IPoolManager poolManager
        ) : base(eventSystem)
        {
            _pool = poolManager.GetPool("");
            _eventSystem = eventSystem;
        }

        public override void EventTriggered(DamageEvent eventData)
        {
            var source = eventData.source;
            var target = eventData.target;

            var targetActorComponent = target.GetComponent<ActorComponent>();
            var sourceActorComponent = source.GetComponent<ActorComponent>();

            targetActorComponent.health = targetActorComponent.health - sourceActorComponent.damage;

            _createHitScore(source, target);

            if (targetActorComponent.health <= 0)
            {
                targetActorComponent.health = 0;

                _publishKill(source, target);
                _pool.RemoveEntity(target);
            }
        }

        private void _publishKill(IEntity source, IEntity target)
        {
            var killEvent = new KillEvent()
            {
                source = source,
                target = target
            };
            _eventSystem.Publish(killEvent);
        }

        private void _createHitScore(IEntity source, IEntity target)
        {
            var sourceActorComponent = source.GetComponent<ActorComponent>();
            var sourcePosition = Vector3.zero;

            var viewComponent = source.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            sourcePosition = go.transform.position;

            var entity = _pool.CreateEntity();
            var hitScoreComponent = new HitScoreComponent()
            {
                elapsedTime = 0,
                lifeTime = 0.6f,
                targetPosition = sourcePosition,
                targetEntity = source,
                step = 0,
                startPosition = new Vector2(0, 20),
                text = sourceActorComponent.damage.ToString()
            };

            entity.AddComponent(hitScoreComponent);
            entity.AddComponent(new ViewComponent());
        }
    }
}
