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
    public class SpawnViewResolver : DefaultPooledViewResolverSystem
    {
        public override IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(ViewComponent),
                    typeof(SpawnComponent)
                );
            }
        }

        SpawnViewResolver(
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
            return Resources.Load<GameObject>("Prefabs/Spawn");
        }

        protected override GameObject AllocateView(IEntity entity, IPool pool)
        {
            var spawnComponent = entity.GetComponent<SpawnComponent>();
            var go = base.AllocateView(entity, pool);
            var position = new Vector3(
                spawnComponent.position.x,
                spawnComponent.position.y,
                ZIndex.spawn
            );
            
            go.transform.position = position;
            go.name = string.Format("Spawn_{0}", entity.Id);

            return go;
        }
    }
}
