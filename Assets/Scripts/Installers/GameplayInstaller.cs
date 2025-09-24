using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public PlayerUnitBrian Player { get; private set; }
    [SerializeField] private Zhmyh _zhmyh;
    [SerializeField] private Cursor _cursor;
    public override void InstallBindings()
    {
        UnityEngine.Cursor.visible = false;

        Container.Bind<Cursor>().FromInstance(_cursor).AsSingle();

        var zhmyh = Container.InstantiatePrefab(_zhmyh);
        Player = Container.InstantiateComponent<PlayerUnitBrian>(zhmyh);
        Player.name = "Player";
        Player.Init();
        Container.Bind<PlayerUnitBrian>().FromInstance(Player).AsSingle();
    }
}
