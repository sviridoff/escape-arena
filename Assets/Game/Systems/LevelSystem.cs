using Assets.Game.Components;
using Assets.Game.Config;
using Assets.Game.Controllers;
using Assets.Game.Events;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class LevelSystem : IManualSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new EmptyGroup();
            }
        }

        private IPool _pool;
        private IEventSystem _eventSystem;
        private List<IDisposable> _subscriptions;
        private PlayerController _playerController;
        private GameObject _mapGo;
        private UnityEngine.Object _mapInstance;

        public LevelSystem(
            IEventSystem eventSystem,
            IPoolManager poolManager,
            PlayerController playerController
        )
        {
            _pool = poolManager.GetPool("");
            _eventSystem = eventSystem;
            _subscriptions = new List<IDisposable>();
            _playerController = playerController;
            _mapGo = Resources.Load<GameObject>("Prefabs/Maps/Map_1");
        }

        public void StartSystem(IGroupAccessor group)
        {
            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.PAUSE)
                .Subscribe(x => Time.timeScale = 0.00001f)
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.RESUME)
                .Subscribe(x => Time.timeScale = 1)
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.RESTART)
                .Subscribe(x => _restartLevel())
                .AddTo(_subscriptions);

            this.WaitForScene()
                .Subscribe(x =>
                {
                    _startLevel();
                });
        }

        public void StopSystem(IGroupAccessor group)
        {
            _subscriptions.DisposeAll();
        }

        private Boolean _isPlayer(IEntity entity)
        {
            return entity.HasComponent<PlayerComponent>();
        }

        private void _stopLevel()
        {
            var levelEvent = new LevelEvent()
            {
                action = LevelActions.STOP
            };

            _eventSystem.Publish(levelEvent);
            _pool.RemoveAllEntities();
        }

        private void _startLevel()
        {
            var levelEvent = new LevelEvent()
            {
                action = LevelActions.START
            };

            Time.timeScale = 1;
            _createMap();
            _playerController.start();
            _eventSystem.Publish(levelEvent);
        }

        private void _createMap()
        {
            if (_mapInstance != null)
            {
                GameObject.Destroy(_mapInstance);
            }

            _mapInstance = GameObject.Instantiate(_mapGo, Vector3.zero, Quaternion.identity);
            _mapInstance.name = "Map";
        }

        private void _restartLevel()
        {
            _stopLevel();
            _startLevel();
        }
    }
}
