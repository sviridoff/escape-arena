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
using UnityEngine.UI;

namespace Assets.Game.Systems
{
    public class CameraSystem : IManualSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new EmptyGroup();
            }
        }

        private IEventSystem _eventSystem;
        private List<IDisposable> _subscriptions;
        private IGroupAccessor _playersAccessor;
        private Camera _camera;
        private GameObject _go;
        private float _shakeDecay;
        private float _shakeIntensity;
        private Quaternion _originRotation;
        private Boolean _isPaused;

        public CameraSystem(IEventSystem eventSystem, IPoolManager poolManager)
        {
            _eventSystem = eventSystem;
            _playersAccessor = poolManager
                .CreateGroupAccessor(new Group(
                    typeof(PlayerComponent),
                    typeof(ViewComponent)
                ));
            _subscriptions = new List<IDisposable>();
            _camera = Camera.main;
            _isPaused = false;
        }

        public void StartSystem(IGroupAccessor group)
        {
            _eventSystem.Receive<DamageEvent>()
                .Where(x => _isPlayer(x.target))
                .Subscribe(x => _startShake())
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.PAUSE)
                .Subscribe(x => _isPaused = true)
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action != LevelActions.PAUSE)
                .Subscribe(x => _isPaused = false)
                .AddTo(_subscriptions);
            
            Observable.EveryUpdate()
                .Subscribe(x => _move())
                .AddTo(_subscriptions);
        }

        public void StopSystem(IGroupAccessor group)
        {
            _subscriptions.DisposeAll();
        }

        private void _move()
        {
            if (_isPaused)
            {
                return;
            }

            if (_go == null)
            {
                if (!_playersAccessor.Entities.Any())
                {
                    _stoptShake();

                    return;
                }

                var player = _playersAccessor.Entities.First();
                var viewComponent = player.GetComponent<ViewComponent>();
                _go = viewComponent.View.gameObject;
            }

            var positionNoZ = _camera.transform.position;
            positionNoZ.z = _go.transform.position.z;
            var targetDirection = _go.transform.position - positionNoZ;
            var interpolatedVelocity = targetDirection.magnitude * 12;
            var targetPosition = _camera.transform.position + (
                targetDirection.normalized * interpolatedVelocity * Time.deltaTime
            );

            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position, targetPosition, 0.25f
            );

            if (_canShake())
            {
                _shake();
            } else {
                _stoptShake();
            }
        }

        private Boolean _canShake()
        {
            return _shakeIntensity > 0;
        }

        private void _shake()
        {
            _camera.transform.position += UnityEngine.Random.insideUnitSphere * _shakeIntensity;
            _camera.transform.rotation = new Quaternion(
                _originRotation.x + UnityEngine.Random.Range(-_shakeIntensity, _shakeIntensity) * .2f,
                _originRotation.y + UnityEngine.Random.Range(-_shakeIntensity, _shakeIntensity) * .2f,
                _originRotation.z + UnityEngine.Random.Range(-_shakeIntensity, _shakeIntensity) * .2f,
                _originRotation.w + UnityEngine.Random.Range(-_shakeIntensity, _shakeIntensity) * .2f
            );
            _shakeIntensity -= _shakeDecay;
        }

        private void _startShake()
        {
            _originRotation = _camera.transform.rotation;
            _shakeIntensity = .08f;
            _shakeDecay = .002f;
        }

        private void _stoptShake()
        {
            _camera.transform.rotation = new Quaternion(0, 0, 0, 1);
            _shakeIntensity = 0;
        }

        private Boolean _isPlayer(IEntity entity)
        {
            return entity.HasComponent<PlayerComponent>();
        }
    }
}
