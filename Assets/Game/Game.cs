using Assets.Game.Systems;
using Assets.Game.ViewResolvers;
using EcsRx.Unity;
using UnityEngine;

namespace Assets.Game
{
    public class Game : EcsRxApplication
    {
        protected override void ApplicationStarting()
        {
            RegisterBoundSystem<PlayerViewResolver>();
            RegisterBoundSystem<BulletViewResolver>();
            RegisterBoundSystem<EnemyViewResolver>();
            RegisterBoundSystem<HitScoreViewResolver>();
            RegisterBoundSystem<SpawnViewResolver>();
            RegisterBoundSystem<ExplosionViewResolver>();
            RegisterBoundSystem<CoinViewResolver>();

            RegisterBoundSystem<PlayerMovementSystem>();
            RegisterBoundSystem<PlayerDashSystem>();
            RegisterBoundSystem<EnemyMovementSystem>();
            RegisterBoundSystem<EnemyCollisionSystem>();
            RegisterBoundSystem<EnemySpawnSystem>();
            RegisterBoundSystem<BulletMovementSystem>();
            RegisterBoundSystem<BulletCollisionSystem>();
            RegisterBoundSystem<CoinCollisionSystem>();
            RegisterBoundSystem<CoinSpawnSystem>();
            RegisterBoundSystem<CoinSystem>();
            RegisterBoundSystem<PlayerFireSystem>();
            RegisterBoundSystem<DamageSystem>();
            RegisterBoundSystem<KillSystem>();
            RegisterBoundSystem<LevelSystem>();
            RegisterBoundSystem<HitScoreSystem>();
            RegisterBoundSystem<UISystem>();
            RegisterBoundSystem<CameraSystem>();
            RegisterBoundSystem<DestructibleSpawnSystem>();
        }

		protected override void ApplicationStarted()
		{
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;

            // Container.Resolve<CameraController>().start();
		}
    }
}