using Assets.Game.Components;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using CnControls;

namespace Assets.Game.Systems
{
    public enum DashState
    {
        Ready,
        Dashing,
        Cooldown
    }

    public class PlayerDashSystem : IReactToGroupSystem
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

        private Vector2 _originalVelocity;
        private DashState _dashState;
        private float _dashTimer;

        public PlayerDashSystem()
        {
            _dashState = DashState.Ready;
            _dashTimer = 0;
        }

        public IObservable<IGroupAccessor> ReactToGroup(IGroupAccessor group)
        {
            return Observable.EveryUpdate().Select(x => group);
        }

        public void Execute(IEntity entity)
        {
            switch (_dashState)
            {
                case DashState.Ready:
                    _ready(entity);

                    if (!_canMove(entity))
                    {
                        _stopMove(entity);
                    }

                    break;
                case DashState.Dashing:
                    _dashing(entity);

                    if (!_canMove(entity))
                    {
                        _stopMove(entity);
                    }

                    break;
                case DashState.Cooldown:
                    _cooldown(entity);

                    break;
            }
        }

        private void _ready(IEntity entity)
        {
            var isDashing = CnInputManager.GetButtonDown("Dash");

            if (isDashing)
            {
                var playerComponent = entity.GetComponent<PlayerComponent>();
                var viewComponent = entity.GetComponent<ViewComponent>();
                var go = viewComponent.View.gameObject;
                var rigidbody2D = go.GetComponent<Rigidbody2D>();
                var directionVector = new Vector2(
                    CnInputManager.GetAxisRaw("DirectionHorizontal"),
                    CnInputManager.GetAxisRaw("DirectionVertical")
                );
                var ghostEffect = go.GetComponent<GhostEffect>();

                directionVector = directionVector.sqrMagnitude > 0.001f
                    ? directionVector
                    : (Vector2)go.transform.up;
                _originalVelocity = rigidbody2D.velocity;
                rigidbody2D.velocity = directionVector.normalized * 3.5f;
                ghostEffect.ghostingEnabled = true;
                playerComponent.isDashing = true;
                _dashState = DashState.Dashing;
            }
        }

        private void _dashing(IEntity entity)
        {
            _dashTimer += Time.deltaTime;

            if (_dashTimer >= .3f)
            {
                var viewComponent = entity.GetComponent<ViewComponent>();
                var go = viewComponent.View.gameObject;
                var rigidbody2D = go.GetComponent<Rigidbody2D>();
                var ghostEffect = go.GetComponent<GhostEffect>();
                
                rigidbody2D.velocity = _originalVelocity;
                ghostEffect.ghostingEnabled = false;
            }

            // Wait .1 ms more.
            if (_dashTimer >= .4f)
            {
                var playerComponent = entity.GetComponent<PlayerComponent>();

                playerComponent.isDashing = false;
                _dashTimer = .1f;
                _dashState = DashState.Cooldown;
            }
        }

        private void _cooldown(IEntity entity)
        {
            _dashTimer -= Time.deltaTime;

            if (_dashTimer <= 0)
            {
                _dashTimer = 0;
                _dashState = DashState.Ready;
            }
        }

        private Boolean _canMove(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var rigidbody2D = go.GetComponent<Rigidbody2D>();
            var predictedPosition = new Vector2(
                go.transform.position.x,
                go.transform.position.y
            ) + rigidbody2D.velocity * Time.deltaTime;
            var node = AstarPath.active.GetNearest(predictedPosition).node;

            return node.Walkable;
        }

        private void _stopMove(IEntity entity)
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var go = viewComponent.View.gameObject;
            var rigidbody2D = go.GetComponent<Rigidbody2D>();

            rigidbody2D.velocity = Vector2.zero;
        }
    }
}

