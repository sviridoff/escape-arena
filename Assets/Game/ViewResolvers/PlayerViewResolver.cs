using Assets.Game.Components;
using Assets.Game.Config;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Unity.Components;
using EcsRx.Unity.MonoBehaviours;
using EcsRx.Unity.Systems;
using UnityEngine;

namespace Assets.Game.ViewResolvers
{
    public class PlayerViewResolver : ViewResolverSystem
    {
        public PlayerViewResolver(IViewHandler viewHandler) : base(viewHandler)
        {
        }

        public override IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(PlayerComponent),
                    typeof(ViewComponent)
                );
            }
        }

        public override GameObject ResolveView(IEntity entity)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Player");
            var go = Object.Instantiate(prefab);
            var entityView = go.GetComponent<EntityView>();

            entityView.Entity = entity;
            go.transform.position = new Vector3(0, 0, ZIndex.player);
            go.name = "Player";

            return go;
        }
    }
}

