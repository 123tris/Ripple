using System.Collections.Generic;

namespace Ripple
{
    public interface ICollectionVariable
    {
        int Count { get; }
        System.Type ItemType { get; }
        void ClearAll();
    }

    public interface ICollectionVariable<T> : ICollectionVariable
    {
        IReadOnlyList<T> Snapshot();
    }
}
