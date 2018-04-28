using Assets.Game.Components;
using Assets.Game.Config;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Groups;
using EcsRx.Pools;
using EcsRx.Unity.Components;
using EcsRx.Unity.Systems;
using UnityEngine;
using Zenject;

namespace Assets.Game.ViewResolvers
{
    public class EnemyViewResolver : DefaultPooledViewResolverSystem
    {
        public override IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(ViewComponent),
                    typeof(EnemyComponent)
                );
            }
        }

        EnemyViewResolver(
            IPoolManager poolManager,
            IEventSystem eventSystem,
            IInstantiator instantiator
        ) : base(
            poolManager,
            eventSystem,
            instantiator
        )
        {
            ViewPool.PreAllocate(20);
        }

        protected override GameObject ResolvePrefabTemplate()
        {
            return Resources.Load<GameObject>("Prefabs/Enemy");
        }

        protected override GameObject AllocateView(IEntity entity, IPool pool)
        {
            var enemyComponent = entity.GetComponent<EnemyComponent>();
            var go = base.AllocateView(entity, pool);
            var position = new Vector3(
                enemyComponent.startPosition.x,
                enemyComponent.startPosition.y,
                ZIndex.enemy
            );
            
            go.transform.position = position;
            go.name = string.Format("Enemy_{0}", entity.Id);

            return go;
        }
    }
}
