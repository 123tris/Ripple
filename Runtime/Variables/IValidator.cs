namespace Ripple
{
    public interface IValidator<T>
    {
        bool Validate(T candidate, out T corrected);
    }
}
