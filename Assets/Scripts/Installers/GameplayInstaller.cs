using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public Player Player { get; private set; }
    [SerializeField] private Zhmyh _zhmyh;
    public override void InstallBindings()
    {
        UnityEngine.Cursor.visible = false;

        var zhmyh = Container.InstantiatePrefab(_zhmyh);
        Player = Container.InstantiateComponent<Player>(zhmyh);
        Player.name = "Player";
        Player.Init();
        Container.Bind<Player>().FromInstance(Player).AsSingle();
    }
}
