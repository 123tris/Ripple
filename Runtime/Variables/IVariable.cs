namespace Ripple
{
    public interface IVariable<out T>
    {
        public T CurrentValue { get; }
    }
}