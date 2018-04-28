using Assets.Game.Components;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using System;
using UniRx;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class BulletMovementSystem : IReactToGroupSystem
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

        public BulletMovementSystem(IPoolManager poolManager)
        {
            _pool = poolManager.GetPool("");
        }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor group)
        {
            return Observable.EveryUpdate().Select(x => group);
        }

        public void Execute(IEntity entity)
        {
            _move(entity);
            

            if (_canRemove(entity))
            {
                _remove(entity);
            }
        }

        private Boolean _canRemove(IEntity entity)
        {
            var bulletComponent = entity.GetComponent<BulletComponent>();

            bulletComponent.elapsedTime += Time.deltaTime;

            if (bulletComponent.elapsedTime < bulletComponent.lifeTime)
            {
                return false;
            }

            return true;
        }

        private void _move(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var nextPosition = go.transform.up * 6 * Time.deltaTime;

            go.transform.position += nextPosition;
        }

        private void _remove(IEntity entity)
        {
             _pool.RemoveEntity(entity);
        }
    }
}
