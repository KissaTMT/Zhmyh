namespace Components
{
    public interface ITickable : IComponent
    {
        public void Tick(float dt);
    }
}
