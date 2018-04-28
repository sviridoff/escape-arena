using Assets.Game.Components;
using Assets.Game.Config;
using CnControls;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Game.Systems
{
    public class PlayerMovementSystem : IReactToGroupSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new Group(
                    typeof(PlayerComponent),
                    typeof(ViewComponent)
                );
            }
        }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor group)
        {
            return Observable.EveryUpdate().Select(x => group);
        }

        public void Execute(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;

            _move(go, entity);

            _rotate(go);
        }

        private void _move(GameObject go, IEntity entity)
        {
            var directionVector = new Vector2(
                CnInputManager.GetAxisRaw("DirectionHorizontal"),
                CnInputManager.GetAxisRaw("DirectionVertical")
            );

            Debug.Log(directionVector.x);

            if (directionVector.sqrMagnitude > 0.001f)
            {
                var speed = 12f;
                var directionVectorNormalized = directionVector.normalized;
                var rigidbody2d = go.GetComponent<Rigidbody2D>();

                rigidbody2d.AddForce(directionVectorNormalized * speed, ForceMode2D.Force);
            }
        }

        private void _rotate(GameObject go)
        {
            var rotationVector = new Vector2(
                CnInputManager.GetAxisRaw("RotationHorizontal"),
                CnInputManager.GetAxisRaw("RotationVertical")
            );

            if (rotationVector != Vector2.zero)
            {
                var speed = 12;
                var angle = Mathf.Atan2(rotationVector.x, rotationVector.y) * Mathf.Rad2Deg;
                var q = Quaternion.AngleAxis(angle, -Vector3.forward);

                go.transform.rotation = Quaternion.Slerp(
                    go.transform.rotation, q, Time.deltaTime * speed
                );
            }
        }

        private Boolean _canMove(Vector3 position)
        {
            var node = AstarPath.active.GetNearest(position).node;

            return node.Walkable;
        }
    }
}

