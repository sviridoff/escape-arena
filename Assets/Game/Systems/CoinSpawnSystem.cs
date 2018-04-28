using Assets.Game.Components;
using Assets.Game.Events;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Pools;
using EcsRx.Systems.Custom;
using EcsRx.Unity.Components;
using System;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class CoinSpawnSystem : EventReactionSystem<KillEvent> 
	{
        private IPool _pool;

		public CoinSpawnSystem (IEventSystem eventSystem, IPoolManager poolManager) : base(eventSystem)
		{
            _pool = poolManager.GetPool("");
		}

		public override void EventTriggered(KillEvent eventData)
		{
            var entity = eventData.target;

            if (!_isEnemy(entity))
            {
                return;
            }

            _spawnCoins(entity);
		}

        private Boolean _isEnemy(IEntity entity)
        {
            return entity.HasComponent<EnemyComponent>();
        }

        private void _spawnCoins(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var targetPosition = (Vector2)go.transform.position;
            var spawnCounts = UnityEngine.Random.Range(2, 5);

            for (var i = 0; i < spawnCounts; i++)
            {
                var offsetPosition = Vector2.Scale(UnityEngine.Random.onUnitSphere, Vector2.one);
                var coinEntity = _pool.CreateEntity();
                var coinComponent = new CoinComponent()
                {
                    startPosition = offsetPosition + targetPosition,
                    lifeTime = 6,
                    elapsedTime = 0,
                    elapsedBlinkTime = 0,
                    blinkTime = .5f,
                    score = 50
                };

                coinEntity.AddComponent(coinComponent);
                coinEntity.AddComponent(new ViewComponent());
            }
        }
	}
}

