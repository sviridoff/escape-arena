using EcsRx.Components;
using System;
using UnityEngine;

namespace Assets.Game.Components
{
    public class BulletComponent : IComponent
    {
        public Vector2 startPosition;
        public Quaternion rotation;
        public float elapsedTime;
        public float lifeTime;
        public LayerMask collisionMask;
    }
}