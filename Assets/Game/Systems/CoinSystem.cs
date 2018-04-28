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
    public class CoinSystem : IReactToGroupSystem
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

        public CoinSystem(IPoolManager poolManager)
        {
            _pool = poolManager.GetPool("");
        }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor group)
        {
            return Observable.EveryUpdate().Select(x => group);
        }

        public void Execute(IEntity entity)
        {
            if (_canToggleBlink(entity))
            {
                _toggleBlink(entity);
            }

            if (_canRemove(entity))
            {
                _remove(entity);
            }
        }

        private void _toggleBlink(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var spriteRenderer = go.GetComponent<SpriteRenderer>();

            spriteRenderer.enabled = !spriteRenderer.enabled;
        }

        private Boolean _canToggleBlink(IEntity entity)
        {
            var coinComponent = entity.GetComponent<CoinComponent>();

            if (coinComponent.elapsedTime < coinComponent.lifeTime * .4)
            {
                return false;
            }

            coinComponent.elapsedBlinkTime += Time.deltaTime;

            if (coinComponent.elapsedBlinkTime > coinComponent.blinkTime)
            {
                coinComponent.elapsedBlinkTime = 0;
                coinComponent.blinkTime = coinComponent.blinkTime > .1f
                    ? coinComponent.blinkTime * .8f
                    : .1f;

                return true;
            }

            return false;
        }

        private Boolean _canRemove(IEntity entity)
        {
            var coinComponent = entity.GetComponent<CoinComponent>();

            coinComponent.elapsedTime += Time.deltaTime;

            if (coinComponent.elapsedTime < coinComponent.lifeTime)
            {
                return false;
            }

            return true;
        }

        private void _remove(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var spriteRenderer = go.GetComponent<SpriteRenderer>();

            spriteRenderer.enabled = !spriteRenderer.enabled;

            _pool.RemoveEntity(entity);
        }
    }
}
