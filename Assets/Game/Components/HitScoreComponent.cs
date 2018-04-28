using EcsRx.Components;
using EcsRx.Entities;
using System;
using UnityEngine;

namespace Assets.Game.Components
{
    public class HitScoreComponent : IComponent
    {
        public float elapsedTime;
        public float lifeTime;
        public Vector3 targetPosition;
        public Vector2 startPosition;
        public IEntity targetEntity;
        public float step;
        public string text;
    }
}