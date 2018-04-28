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
    public class EnemyCollisionSystem : IReactToDataSystem<Collision2D>
    {
        public IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(EnemyComponent),
                    typeof(ViewComponent)
                );
            }
        }

        private IPool _pool;
        private IEventSystem _eventSystem;

        public EnemyCollisionSystem(IPoolManager poolManager, IEventSystem eventSystem)
        {
            _pool = poolManager.GetPool("");
            _eventSystem = eventSystem;
        }

        public IObservable<Collision2D> ReactToData(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;

            return go.OnCollisionEnter2DAsObservable();
        }

        public void Execute(IEntity entity, Collision2D collision)
        {
            var enemyComponent = entity.GetComponent<EnemyComponent>();

            if (_canDamage(enemyComponent.collisionMask, collision))
            {
                _publishDamage(entity, collision);
                _publishKill(collision, entity);
                _pool.RemoveEntity(entity);
            }
        }

        private Boolean _canDamage(LayerMask collisionMask, Collision2D collision)
        {
            if (_isDashing(collision))
            {
                return false;
            }

            var colliderMask = 1 << collision.gameObject.layer;

            return (collisionMask & colliderMask) > 0;
        }

        private Boolean _isDashing(Collision2D collision)
        {
            var entityView = collision.gameObject.GetComponent<EntityView>();

            if (entityView != null)
            {
                var entity = entityView.Entity;

                if (entity != null && entity.HasComponent<PlayerComponent>())
                {
                    var playerComponent = entity.GetComponent<PlayerComponent>();

                    return playerComponent.isDashing;
                }
            }

            return false;
        }

        private void _publishKill(Collision2D collision, IEntity entity)
        {
            var entityView = collision.gameObject.GetComponent<EntityView>();
            var sourceEntity = entityView.Entity;
            var killEvent = new KillEvent()
            {
                source = sourceEntity,
                target = entity
            };

            _eventSystem.Publish(killEvent);
        }

        private void _publishDamage(IEntity entity, Collision2D collision)
        {
            var entityView = collision.gameObject.GetComponent<EntityView>();
            var targetEntity = entityView.Entity;
            var damageEvent = new DamageEvent()
            {
                source = entity,
                target = targetEntity
            };

            _eventSystem.Publish(damageEvent);
        }
    }
}
