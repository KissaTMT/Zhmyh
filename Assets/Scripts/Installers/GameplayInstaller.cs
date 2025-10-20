using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public PlayerUnitBrian Player { get; private set; }
    [SerializeField] private Unit _unitPrefab;
    [SerializeField] private Gniling _gnilling;
    [SerializeField] private Cursor _cursor;
    public override void InstallBindings()
    {
        UnityEngine.Cursor.visible = false;

        Container.Bind<Cursor>().FromInstance(_cursor).AsSingle();

        var unit = Container.InstantiatePrefab(_unitPrefab);
        Player = Container.InstantiateComponent<PlayerUnitBrian>(unit);
        Player.name = "Player";
        Player.Init();
        Container.Bind<PlayerUnitBrian>().FromInstance(Player).AsSingle();

        Container.InstantiateComponent<AIGnillingBrain>(_gnilling.gameObject).Init();
    }
}
