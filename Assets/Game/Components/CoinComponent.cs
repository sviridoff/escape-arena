using EcsRx.Components;
using UnityEngine;

namespace Assets.Game.Components
{
    public class CoinComponent : IComponent
    {
        public Vector2 startPosition;
        public float elapsedTime;
        public float lifeTime;
        public float elapsedBlinkTime;
        public float blinkTime;
        public int score;
    }
}