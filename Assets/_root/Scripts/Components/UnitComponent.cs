using UnityEngine;

namespace Components
{
    public class UnitComponent : IComponent
    {
        public bool Enabled => _enabled;
        public int RelativeTimeflow => relativeTimeflow;

        protected int relativeTimeflow = 1;

        private bool _enabled = true;

        public void Enable()
        {
            _enabled = true;
            OnEnable();
        }
        public void Disable()
        {
            _enabled = false;
            OnDisable();
        }
        public void SetTimeflow(int relative)
        {
            relativeTimeflow = (int)Mathf.Sign(relative);
        }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}
