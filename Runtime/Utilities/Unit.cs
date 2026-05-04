namespace Ripple
{
    /// <summary>Empty value type used to represent a void-typed generic parameter.</summary>
    public readonly struct Unit
    {
        public static readonly Unit Default = default;
        public override string ToString() => "()";
    }
}
