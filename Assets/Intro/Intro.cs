using Assets.Intro.Systems;
using EcsRx.Unity;
using UnityEngine;

namespace Assets.Intro
{
    public class Intro : EcsRxApplication
    {
        protected override void ApplicationStarting()
        {
            RegisterBoundSystem<UISystem>();
        }

        protected override void ApplicationStarted()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }
    }
}