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
    public class ExplosionViewResolver : DefaultPooledViewResolverSystem
    {
        public override IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(ViewComponent),
                    typeof(ExplosionComponent)
                );
            }
        }

        ExplosionViewResolver(
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
            return Resources.Load<GameObject>("Prefabs/Explosion");
        }

        protected override GameObject AllocateView(IEntity entity, IPool pool)
        {
            var explosionComponent = entity.GetComponent<ExplosionComponent>();
            var go = base.AllocateView(entity, pool);
            var position = new Vector3(
                explosionComponent.startPosition.x,
                explosionComponent.startPosition.y,
                ZIndex.explosion
            );

            go.transform.position = position;
            go.name = string.Format("Explosion_{0}", entity.Id);

            return go;
        }
    }
}
