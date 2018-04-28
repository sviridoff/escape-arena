using Assets.Game.Components;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Groups;
using EcsRx.Pools;
using EcsRx.Unity.Components;
using EcsRx.Unity.Systems;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets.Game.ViewResolvers
{
    public class HitScoreViewResolver : DefaultPooledViewResolverSystem
    {
        public override IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(HitScoreComponent),
                    typeof(ViewComponent)
                );
            }
        }

        private GameObject _canvas;

        public HitScoreViewResolver(
            IPoolManager poolManager,
            IEventSystem eventSystem,
            IInstantiator instantiator
        ) : base(
            poolManager,
            eventSystem,
            instantiator
        )
        {
            _canvas = GameObject.Find("Canvas");

            ViewPool.PreAllocate(10);
        }

        protected override GameObject ResolvePrefabTemplate()
        {
            return Resources.Load<GameObject>("Prefabs/HitScore");
        }

        protected override GameObject AllocateView(IEntity entity, IPool pool)
        {
            var hitScoreComponent = entity.GetComponent<HitScoreComponent>();
            var go = base.AllocateView(entity, pool);
            var text = go.GetComponent<Text>();

            text.text = hitScoreComponent.text;
            go.name = string.Format("HitScore_{0}", entity.Id);
            go.transform.SetParent(_canvas.transform, false);

            return go;
        }
    }
}
