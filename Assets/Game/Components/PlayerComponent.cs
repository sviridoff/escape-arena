using EcsRx.Components;
using System;
using UnityEngine;

namespace Assets.Game.Components
{
    public class PlayerComponent : IComponent
    {
      public float fireRateTime;
      public float lastFireTime;
      public Boolean isDashing;
    }
}