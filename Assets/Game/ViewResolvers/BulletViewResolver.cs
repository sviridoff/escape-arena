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
    public class BulletViewResolver : DefaultPooledViewResolverSystem
    {
        public override IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(ViewComponent),
                    typeof(BulletComponent)
                );
            }
        }

        BulletViewResolver(
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
            return Resources.Load<GameObject>("Prefabs/Bullet");
        }

        protected override GameObject AllocateView(IEntity entity, IPool pool)
        {
            var bulletComponent = entity.GetComponent<BulletComponent>();
            var go = base.AllocateView(entity, pool);
            var position = new Vector3(
                bulletComponent.startPosition.x,
                bulletComponent.startPosition.y,
                ZIndex.bullet
            );
            
            go.transform.position = position;
            go.transform.rotation = bulletComponent.rotation;
            go.name = string.Format("Bullet_{0}", entity.Id);

            return go;
        }
    }
}
