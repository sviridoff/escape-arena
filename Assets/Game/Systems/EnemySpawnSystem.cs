using Assets.Game.Components;
using Assets.Game.Config;
using Assets.Game.Events;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class EnemySpawnSystem : IManualSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new EmptyGroup();
            }
        }

        private IPool _pool;
        private int _maxEnemies;
        private int _enemies;
        private IEventSystem _eventSystem;
        private List<IDisposable> _subscriptions;
        private IDisposable _subscription;

        public EnemySpawnSystem(IEventSystem eventSystem, IPoolManager poolManager)
        {
            _pool = poolManager.GetPool("");
            _maxEnemies = 8;
            _enemies = 0;
            _eventSystem = eventSystem;
            _subscriptions = new List<IDisposable>();
        }

        public void StartSystem(IGroupAccessor group)
        {
            _eventSystem.Receive<KillEvent>()
                .Where(x => _isEnemy(x.target))
                .Subscribe(x => _reduceEnemies())
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.STOP)
                .Subscribe(x =>
                {
                    _subscription.Dispose();
                    _resetEnemies();
                })
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.START)
                .Subscribe(x =>
                {
                    _subscription = Observable.FromCoroutine(_createEnemies)
                        .Subscribe();
                })
                .AddTo(_subscriptions);
        }

        public void StopSystem(IGroupAccessor group)
        {
            _subscriptions.DisposeAll();
        }

        private IEnumerator _createEnemies()
        {
            yield return new WaitForSeconds(2.5f);

            while (true)
            {
                while (!_canSpawn())
                {
                    yield return null;
                }

                var startPosition = Vector2.Scale(UnityEngine.Random.onUnitSphere, Vector2.one * 3);

                var spawn = _createSpawn(startPosition);

                yield return new WaitForSeconds(1);

                _removeSpawn(spawn);

                _createEnemy(startPosition);
            }
        }

        private void _resetEnemies()
        {
            _enemies = 0;
        }

        private Boolean _canSpawn()
        {
            if (_maxEnemies > _enemies)
            {
                _enemies += 1;

                return true;
            }

            return false;
        }

        private void _createEnemy(Vector2 startPosition)
        {
            var entity = _pool.CreateEntity();
            var actorComponent = new ActorComponent()
            {
                maxHealth = 20,
                health = 20,
                damage = 20
            };
            var enemyComponent = new EnemyComponent()
            {
                startPosition = startPosition,
                collisionMask = 1 << 9, // Player layer
                speed = 5,
                lastDestinationSetterTime = 0,
                destinationSetterRateTime = 0.5f
            };

            entity.AddComponent(actorComponent);
            entity.AddComponent(enemyComponent);
            entity.AddComponent(new ViewComponent());
        }

        private IEntity _createSpawn(Vector2 position)
        {
            var entity = _pool.CreateEntity();
            var spawnComponent = new SpawnComponent()
            {
                position = position
            };

            entity.AddComponent(spawnComponent);
            entity.AddComponent(new ViewComponent());

            return entity;
        }

        private void _removeSpawn(IEntity spawn)
        {
            _pool.RemoveEntity(spawn);
        }

        private Boolean _isEnemy(IEntity entity)
        {
            return entity.HasComponent<EnemyComponent>();
        }

        private void _reduceEnemies()
        {
            _enemies -= 1;
        }
    }
}
