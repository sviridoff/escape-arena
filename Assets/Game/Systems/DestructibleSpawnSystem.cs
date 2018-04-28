using Assets.Game.Components;
using Assets.Game.Events;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;
using Assets.Game.Config;

namespace Assets.Game.Systems
{
    public class DestructibleSpawnSystem : IManualSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new EmptyGroup();
            }
        }

        private IPool _pool;
        private List<IDisposable> _subscriptions;
        private Tilemap _destructiblesTilemap;
        private Tilemap _tilemapTilemap;
        private AstarPath _aStarAstarPath;
        private IEventSystem _eventSystem;

        public DestructibleSpawnSystem(IEventSystem eventSystem, IPoolManager poolManager)
        {
            _pool = poolManager.GetPool("");
            _subscriptions = new List<IDisposable>();

            _eventSystem = eventSystem;
        }

        public void StartSystem(IGroupAccessor group)
        {
            _eventSystem.Receive<ComponentRemovedEvent>()
                .Where(x => x.Component.GetType() == typeof(DestructibleComponent))
                .Subscribe(x => _removeDestructible((DestructibleComponent)x.Component))
                .AddTo(_subscriptions);

            _eventSystem.Receive<ComponentRemovedEvent>()
                .Where(x => x.Component.GetType() == typeof(DestructibleComponent))
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(s => {
                    AstarPath.active.UpdateGraphs(_destructiblesTilemap.localBounds);
                })
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.START)
                .Subscribe(x =>
                {
                    var destructiblesGo = GameObject.Find("Map/Destructibles");
                    _destructiblesTilemap = destructiblesGo.GetComponent<Tilemap>();
                    var tilemapGo = GameObject.Find("Map/Tilemap");
                    _tilemapTilemap = tilemapGo.GetComponent<Tilemap>();
                    var aStarGo = GameObject.Find("Map/A*");
                    _aStarAstarPath = aStarGo.GetComponent<AstarPath>();

                    _createDestructibles();
                })
                .AddTo(_subscriptions);
        }

        public void StopSystem(IGroupAccessor group)
        {
            _subscriptions.DisposeAll();
        }

        private void _createDestructibles()
        {
            foreach (var position in _destructiblesTilemap.cellBounds.allPositionsWithin)
            {
                if (_destructiblesTilemap.HasTile(position))
                {
                    _createDestructible(position);
                }
            }

            AstarPath.active.UpdateGraphs(_destructiblesTilemap.localBounds);
        }

        private void _createDestructible(Vector3Int position)
        {
            var entity = _pool.CreateEntity();
            var actorComponent = new ActorComponent()
            {
                health = 10
            };
            var destructibleComponent = new DestructibleComponent()
            {
                position = position
            };

            entity.AddComponent(actorComponent);
            entity.AddComponent(destructibleComponent);
        }

        private void _removeDestructible(DestructibleComponent destructibleComponent)
        {
            var position = destructibleComponent.position;

            var bounds = _destructiblesTilemap.GetBoundsLocal(position);

            _destructiblesTilemap.SetTile(position, null);
        }
    }
}
