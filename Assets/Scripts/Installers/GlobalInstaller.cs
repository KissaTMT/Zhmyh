using Zenject;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IInput>().To<InputHandler>().AsSingle().NonLazy();
    }
}