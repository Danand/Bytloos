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

        public T PassValue(T value)
        {
            Value = value;

            if (NeedsUpdate)
                NeedsUpdate = false;

            return Value;
        }
    }
}