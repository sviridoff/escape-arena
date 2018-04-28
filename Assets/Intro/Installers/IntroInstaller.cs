using Assets.Intro.Systems;
using Zenject;

namespace Assets.Intro.Installers
{
    public class IntroInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<UISystem>()
                .ToSelf()
                .AsSingle();
        }
    }
}