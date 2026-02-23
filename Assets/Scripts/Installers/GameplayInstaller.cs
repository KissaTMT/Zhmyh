using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public PlayerZhmyhBrian Player { get; private set; }
    [SerializeField] private Unit _unitPrefab;
    [SerializeField] private Cursor _cursor;
    public override void InstallBindings()
    {
        UnityEngine.Cursor.visible = false;

        Container.Bind<Cursor>().FromInstance(_cursor).AsSingle();

        var unit = Container.InstantiatePrefab(_unitPrefab);
        Player = Container.InstantiateComponent<PlayerZhmyhBrian>(unit);
        Player.name = "Player";
        Player.Init();
        Container.Bind<PlayerZhmyhBrian>().FromInstance(Player).AsSingle();
    }
}
