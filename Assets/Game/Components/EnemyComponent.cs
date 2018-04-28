using EcsRx.Components;
using System;
using UniRx;
using UnityEngine;

namespace Assets.Game.Components
{
    public class EnemyComponent : IComponent
    {
        public LayerMask collisionMask;
        public Vector2 startPosition;
        public float speed;
        public float lastDestinationSetterTime;
        public float destinationSetterRateTime;
    }
}