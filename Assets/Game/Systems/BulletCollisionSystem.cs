using Assets.Game.Components;
using Assets.Game.Config;
using Assets.Game.Events;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using EcsRx.Unity.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Game.Systems
{
    public class BulletCollisionSystem : IReactToDataSystem<Collider2D>
    {
        public IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(BulletComponent),
                    typeof(ViewComponent)
                );
            }
        }

        private IPool _pool;
        private IEventSystem _eventSystem;
        private Tilemap _destructiblesTilemap;
        private IGroupAccessor _destructiblesAccessor;
        private List<IDisposable> _subscriptions;

        public BulletCollisionSystem(IPoolManager poolManager, IEventSystem eventSystem)
        {
            _pool = poolManager.GetPool("");
            _eventSystem = eventSystem;

            _destructiblesAccessor = poolManager
                .CreateGroupAccessor(new Group(
                    typeof(DestructibleComponent)
                ));
            _subscriptions = new List<IDisposable>();

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.START)
                .Subscribe(x =>
                {
                    var destructiblesGo = GameObject.Find("Map/Destructibles");
                    _destructiblesTilemap = destructiblesGo.GetComponent<Tilemap>();
                })
                .AddTo(_subscriptions);
        }

        public IObservable<Collider2D> ReactToData(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            return go.OnTriggerEnter2DAsObservable();
        }

        public void Execute(IEntity entity, Collider2D collider)
        {
            var bulletComponent = entity.GetComponent<BulletComponent>();

            if (_canDamage(bulletComponent.collisionMask, collider))
            {
                _publish(entity, collider);
            }

            if (_canRemove(bulletComponent.collisionMask, collider))
            {
                _pool.RemoveEntity(entity);
            }
        }

        private Boolean _canDamage(LayerMask collisionMask, Collider2D collider)
        {
            var colliderMask = 1 << collider.gameObject.layer;

            return (collisionMask & colliderMask) > 0;
        }

        private Boolean _canRemove(LayerMask collisionMask, Collider2D collider)
        {
            var colliderMask = 1 << collider.gameObject.layer;

            return (collisionMask & colliderMask) > 0;
        }

        private void _publish(IEntity entity, Collider2D collider)
        {
            var targetEntity = _getEntity(entity, collider);

            if (targetEntity == null)
            {
                return;
            }

            var damageEvent = new DamageEvent()
            {
                source = entity,
                target = targetEntity
            };
            _eventSystem.Publish(damageEvent);
        }

        private IEntity _getEntity(IEntity entity, Collider2D collider)
        {
            IEntity targetEntity = null;

            if (collider.gameObject.layer == LayerMask.NameToLayer("Destructible"))
            {
                targetEntity = _getDestructibleEntity(entity);
            }
            else
            {
                var entityView = collider.gameObject.GetComponent<EntityView>();
                targetEntity = entityView.Entity;
            }

            return targetEntity;
        }

        private IEntity _getDestructibleEntity(IEntity entity)
        {
            IEntity targetEntity = null;
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var forwardPosition = go.transform.position + (go.transform.up * 0.25f);
            var position = _destructiblesTilemap.WorldToCell(forwardPosition);

            if (_destructiblesTilemap.HasTile(position))
            {
                targetEntity = _destructiblesAccessor.Entities.Where(x =>
                {
                    var destructibleComponent = x.GetComponent<DestructibleComponent>();

                    if (destructibleComponent.position == position)
                    {
                        return true;
                    }

                    return false;
                })
                .First();
            }

            return targetEntity;
        }
    }
}
