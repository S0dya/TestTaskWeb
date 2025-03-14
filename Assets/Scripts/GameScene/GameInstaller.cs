using Windows;
using Network;
using Zenject;

namespace GameScene
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<RequestHandler>().AsSingle();
            Container.Bind<RequestsQueue>().AsSingle();
        }
    }
}