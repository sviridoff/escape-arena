using Assets.Game.Controllers;
using Assets.Game.Systems;
using Assets.Game.ViewResolvers;
using EcsRx.Pools;
using Zenject;

namespace Assets.Game.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var poolManager = Container.Resolve<IPoolManager>();

            poolManager.CreatePool("");

            Container.Bind<PlayerViewResolver>()
                .ToSelf()
                .AsSingle();

            Container.Bind<BulletViewResolver>()
                .ToSelf()
                .AsSingle();

            Container.Bind<EnemyViewResolver>()
                .ToSelf()
                .AsSingle();

            Container.Bind<CoinViewResolver>()
                .ToSelf()
                .AsSingle();

            Container.Bind<HitScoreViewResolver>()
                .ToSelf()
                .AsSingle();

            Container.Bind<SpawnViewResolver>()
                .ToSelf()
                .AsSingle();

            Container.Bind<ExplosionViewResolver>()
                .ToSelf()
                .AsSingle();

            Container.Bind<PlayerMovementSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<PlayerDashSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<EnemyMovementSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<EnemyCollisionSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<CoinCollisionSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<EnemySpawnSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<CoinSpawnSystem>()
                .ToSelf()
                .AsSingle();
    
            Container.Bind<CoinSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<PlayerFireSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<BulletMovementSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<BulletCollisionSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<DamageSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<KillSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<UISystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<CameraSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<LevelSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<HitScoreSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<DestructibleSpawnSystem>()
                .ToSelf()
                .AsSingle();

            Container.Bind<PlayerController>()
                .AsSingle();
    
            Container.Bind<MapController>()
                .AsSingle();
        }
    }
}