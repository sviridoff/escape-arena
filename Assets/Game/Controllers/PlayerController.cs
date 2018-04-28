using Assets.Game.Components;
using EcsRx.Entities;
using EcsRx.Pools;
using EcsRx.Unity.Components;
using UnityEngine;

namespace Assets.Game.Controllers
{
    public class PlayerController
    {

        private IPool _pool;

        public PlayerController(IPoolManager poolManager)
        {
            _pool = poolManager.GetPool("");
        }

        public void start()
        {
            _createPlayer();
        }

        private void _createPlayer()
        {
            var entity = _pool.CreateEntity();
            var playerComponent = new PlayerComponent()
            {
		        lastFireTime = 0,
		        fireRateTime = 0.1f
            };
            var actorComponent = new ActorComponent()
            {
                health = 20,
                maxHealth = 100
            };

            entity.AddComponent(playerComponent);
            entity.AddComponent(actorComponent);
            entity.AddComponent(new ViewComponent());
        }
    }
}