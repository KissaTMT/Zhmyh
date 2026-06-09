namespace Components
{
    public class UnitComponent : IComponent
    {
        public bool Enabled => _enabled;

        private bool _enabled;

        public void Enable()
        {
            _enabled = true;
        }
        public void Disable()
        {
            _enabled = false;
        }
    }
}
