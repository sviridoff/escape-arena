using Assets.Game.Components;
using CnControls;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using EcsRx.Unity.MonoBehaviours;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Pathfinding;

namespace Assets.Game.Systems
{
    public class EnemyMovementSystem : IReactToGroupSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(EnemyComponent),
                    typeof(ViewComponent)
                );
            }
        }

        private IGroupAccessor _playersAccessor;

        public EnemyMovementSystem(IPoolManager poolManager)
        {
            _playersAccessor = poolManager
                .CreateGroupAccessor(new Group(
                    typeof(PlayerComponent),
                    typeof(ViewComponent)
                ));
        }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor group)
        {
            return Observable.EveryUpdate().Select(x => group);
        }

        public void Execute(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var enemyComponent = entity.GetComponent<EnemyComponent>();
            var go = viewComponent.View.gameObject;
            var aiDestinationSetter = go.GetComponent<AIDestinationSetter>();
            var seeker = go.GetComponent<Seeker>();

            enemyComponent.lastDestinationSetterTime += Time.deltaTime;

            if (enemyComponent.lastDestinationSetterTime < enemyComponent.destinationSetterRateTime)
            {
                return;
            }

            enemyComponent.lastDestinationSetterTime = 0;

            if (!_playersAccessor.Entities.Any())
            {
                return;
            }

            var player = _playersAccessor.Entities.First();
            var playerViewComponent = player.GetComponent<ViewComponent>();
            var playerGO = playerViewComponent.View.gameObject;

            aiDestinationSetter.target = playerGO.transform;
        }
    }
}

