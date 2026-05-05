using System;
using System.Runtime.CompilerServices;

namespace Ripple
{
    public interface IChannel
    {
        string Name { get; }
        int SubscriberCount { get; }
        Type PayloadType { get; }
    }

    public interface IChannel<T> : IChannel
    {
        IDisposable Subscribe(Action<T> handler);
        void Unsubscribe(Action<T> handler);

        void Invoke(T value,
            [CallerMemberName] string callerMember = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0);
    }
}
