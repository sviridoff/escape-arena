using Assets.Game.Components;
using CnControls;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class PlayerFireSystem : IReactToGroupSystem
    {
        private IPool _pool;

        public PlayerFireSystem(IPoolManager poolManager)
        {
            _pool = poolManager.GetPool("");
        }

        public IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(PlayerComponent),
                    typeof(ViewComponent)
                );
            }
        }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor group)
        {
            return Observable.EveryUpdate().Select(x => group);
        }

        public void Execute(IEntity entity)
        {
            if (_canFire(entity))
            {
                _createBullet(entity);
            }
        }

        private Boolean _canFire(IEntity entity)
        {
            var playerComponent = entity.GetComponent<PlayerComponent>();
            var rotationVector = new Vector2(
                CnInputManager.GetAxisRaw("RotationHorizontal"),
                CnInputManager.GetAxisRaw("RotationVertical")
            );

            playerComponent.lastFireTime += Time.deltaTime;

            if (rotationVector == Vector2.zero)
            {
                return false;
            }

            if (playerComponent.lastFireTime < playerComponent.fireRateTime)
            {
                return false;
            }

            playerComponent.lastFireTime = 0;

            return true;
        }

        private void _createBullet(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var startPosition = go.transform.position + (go.transform.up * 0.25f);
            var bullet = _pool.CreateEntity();
            var enemyMask = 1 << LayerMask.NameToLayer("Enemy");
            var destructibleMask = 1 << LayerMask.NameToLayer("Destructible");
            var finalMask = enemyMask | destructibleMask;
            var bulletComponent = new BulletComponent()
            {
                startPosition = startPosition,
                rotation = _addDispersion(go.transform.rotation),
                elapsedTime = 0,
                lifeTime = .8f,
                collisionMask = finalMask
            };
            var actorComponent = new ActorComponent()
            {
                damage = 5
            };

            bullet.AddComponent(bulletComponent);
            bullet.AddComponent(actorComponent);
            bullet.AddComponent(new ViewComponent());
        }

        private Quaternion _addDispersion(Quaternion rotation)
        {
            var dispersion = UnityEngine.Random.Range(-6f, 6f);
            var rotationDispersion = Quaternion.Euler(0, 0, dispersion);

            return rotation * rotationDispersion;
        }
    }
}
