using Brains;
using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public PlayerUnitBrian PlayerUnitBrian { get; private set; }
    public PlayerZhmyhBrian PlayerBrain { get; private set; }
    [SerializeField] private Unit _unitPrefab;
    [SerializeField] private Cursor _cursor;
    public override void InstallBindings()
    {
        UnityEngine.Cursor.visible = false;

        Container.Bind<Cursor>().FromInstance(_cursor).AsSingle();

        var unit = Container.InstantiatePrefab(_unitPrefab);
        PlayerBrain = Container.InstantiateComponent<PlayerZhmyhBrian>(unit);
        PlayerBrain.name = "Player";
        PlayerBrain.Init();
        PlayerBrain.Transform.position = new Vector3(0, 1, 0);
        Container.Bind<PlayerZhmyhBrian>().FromInstance(PlayerBrain).AsSingle();

        //var unit = Container.InstantiatePrefab(_unitPrefab);
        //PlayerUnitBrian = Container.InstantiateComponent<PlayerUnitBrian>(unit);
        //PlayerUnitBrian.name = "Player";
        //PlayerUnitBrian.Init();
        //PlayerUnitBrian.Transform.position = new Vector3(0, 0, 0);
        //Container.Bind<PlayerUnitBrian>().FromInstance(PlayerUnitBrian).AsSingle();




        //unit = Container.InstantiatePrefab(_unitPrefab);
        //var AIBrain = Container.InstantiateComponent<AIZhmyhInvertedBrain>(unit);
        //AIBrain.name = "AI";
        //AIBrain.Init();
        //AIBrain.Transform.position = new Vector3(10, 0, 20);
        //Container.Bind<AIZhmyhInvertedBrain>().FromInstance(AIBrain).AsSingle();
    }
}
