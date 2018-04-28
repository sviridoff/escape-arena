using Assets.Game.Components;
using Assets.Game.Events;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Groups;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using EcsRx.Unity.MonoBehaviours;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class CoinCollisionSystem : IReactToDataSystem<Collider2D>
    {
        public IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(CoinComponent),
                    typeof(ViewComponent)
                );
            }
        }

        private IPool _pool;
        private IEventSystem _eventSystem;

        public CoinCollisionSystem(IPoolManager poolManager, IEventSystem eventSystem)
        {
            _pool = poolManager.GetPool("");
            _eventSystem = eventSystem;
        }

        public IObservable<Collider2D> ReactToData(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            return go.OnTriggerEnter2DAsObservable();
        }

        public void Execute(IEntity entity, Collider2D collider)
        {
            if (_canDamage(collider))
            {
                _createHitScore(entity, collider);
                _publish(collider, entity);
                _pool.RemoveEntity(entity);
            }
        }

        private Boolean _canDamage(Collider2D collider)
        {
            return collider.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        private void _publish(Collider2D collider, IEntity entity)
        {
            var entityView = collider.gameObject.GetComponent<EntityView>();
            var sourceEntity = entityView.Entity;
            var scoreEvent = new ScoreEvent()
            {
                source = sourceEntity,
                target = entity
            };

            _eventSystem.Publish(scoreEvent);
        }

        private void _createHitScore(IEntity entity, Collider2D collider)
        {
            var coinComponent = entity.GetComponent<CoinComponent>();
            var go = collider.gameObject;

            var hitScoreEntity = _pool.CreateEntity();
            var hitScoreComponent = new HitScoreComponent()
            {
                elapsedTime = 0,
                lifeTime = 0.6f,
                targetPosition = go.transform.position,
                targetEntity = entity,
                step = 0,
                startPosition = new Vector2(0, 20),
                text =  string.Format("+{0}", coinComponent.score)
            };

            hitScoreEntity.AddComponent(hitScoreComponent);
            hitScoreEntity.AddComponent(new ViewComponent());
        }
    }
}
