using Assets.Game.Components;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class HitScoreSystem : IReactToGroupSystem
    {
        private IPool _pool;
        private Camera _camera;
        private RectTransform _canvasRectTransform;

        public HitScoreSystem(IPoolManager poolManager)
        {
            var canvas = GameObject.Find("Canvas");

            _pool = poolManager.GetPool("");
            _camera = Camera.main;
            _canvasRectTransform = canvas.GetComponent<RectTransform>();
        }

        public IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(HitScoreComponent),
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
            if (_canRemove(entity))
            {
                _remove(entity);
            }
            else
            {
                _move(entity);
            }
        }

        private void _move(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var rectTransform = go.GetComponent<RectTransform>();
            var hitScoreComponent = entity.GetComponent<HitScoreComponent>();
            var targetPosition = hitScoreComponent.targetPosition;
            var viewportPosition = _camera.WorldToViewportPoint(targetPosition);
            var screenPositionX = viewportPosition.x * _canvasRectTransform.sizeDelta.x - _canvasRectTransform.sizeDelta.x * 0.5f;
            var screenPositionY = viewportPosition.y * _canvasRectTransform.sizeDelta.y - _canvasRectTransform.sizeDelta.y * 0.5f;
            var screenPosition = new Vector2(
                screenPositionX + hitScoreComponent.startPosition.x,
                screenPositionY + hitScoreComponent.startPosition.y + hitScoreComponent.step
            );

            hitScoreComponent.step += Time.deltaTime * 30;
            rectTransform.anchoredPosition = screenPosition;
            rectTransform.localScale = new Vector3(1, 1, 1);
        }

        private Boolean _canRemove(IEntity entity)
        {
            var hitScoreComponent = entity.GetComponent<HitScoreComponent>();

            if (hitScoreComponent.targetEntity == null)
            {
                return true;
            }

            hitScoreComponent.elapsedTime += Time.deltaTime;

            if (hitScoreComponent.elapsedTime < hitScoreComponent.lifeTime)
            {
                return false;
            }

            return true;
        }

        private void _remove(IEntity entity)
        {
            _pool.RemoveEntity(entity);
        }
    }
}
