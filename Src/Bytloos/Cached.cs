using System;

namespace Bytloos
{
    internal class Cached<T>
    {
        public bool NeedsUpdate { get; private set; } = true;

        public T Value { get; private set; }

        public void MarkNeedsUpdate()
        {
            NeedsUpdate = true;
        }

        public T PassValue(Func<T> valueUpdating)
        {
            if (NeedsUpdate)
            {
                Value = valueUpdating.Invoke();
                NeedsUpdate = false;
            }

            return Value;
        }
    }
}