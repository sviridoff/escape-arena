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
    public class CoinViewResolver : DefaultPooledViewResolverSystem
    {
        public override IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(ViewComponent),
                    typeof(CoinComponent)
                );
            }
        }

        CoinViewResolver(
            IPoolManager poolManager,
            IEventSystem eventSystem,
            IInstantiator instantiator
        ) : base(
            poolManager,
            eventSystem,
            instantiator
        )
        {
            ViewPool.PreAllocate(15);
        }

        protected override GameObject ResolvePrefabTemplate()
        {
            return Resources.Load<GameObject>("Prefabs/Coin");
        }

        protected override GameObject AllocateView(IEntity entity, IPool pool)
        {
            var coinComponent = entity.GetComponent<CoinComponent>();
            var go = base.AllocateView(entity, pool);
            var position = new Vector3(
                coinComponent.startPosition.x,
                coinComponent.startPosition.y,
                ZIndex.coin
            );

            go.transform.position = position;
            go.name = string.Format("Coin_{0}", entity.Id);

            return go;
        }
    }
}
