using Assets.Game.Components;
using Assets.Game.Events;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Pools;
using EcsRx.Systems.Custom;
using EcsRx.Unity.Components;
using System.Collections;
using UnityEngine;
using UniRx;

namespace Assets.Game.Systems
{
    public class KillSystem : EventReactionSystem<KillEvent>
    {
        private IPool _pool;

        public KillSystem(
            IEventSystem eventSystem,
            IPoolManager poolManager
        ) : base(eventSystem)
        {
            _pool = poolManager.GetPool("");
        }

        public override void EventTriggered(KillEvent eventData)
        {
            var source = eventData.source;

            _createExplosion(source);
        }

        private void _createExplosion(IEntity source)
        {
            if (!source.HasComponent<ViewComponent>()) {
                return;
            }

            var viewComponent = source.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var startPosition = go.transform.position;

            var explosionComponent = new ExplosionComponent()
            {
                startPosition = startPosition
            };
            var entity = _pool.CreateEntity();

            entity.AddComponent(explosionComponent);
            entity.AddComponent(new ViewComponent());

            MainThreadDispatcher.StartCoroutine(_removeExplosion(entity));
        }

        private IEnumerator _removeExplosion(IEntity entity)
        {
            yield return new WaitForSeconds(0.8f);

            _pool.RemoveEntity(entity);
        }
    }
}
