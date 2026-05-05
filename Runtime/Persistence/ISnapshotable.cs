using System;

namespace Ripple
{
    public interface ISnapshotable
    {
        string SnapshotKey { get; }
        string CaptureSnapshotJson();
        void RestoreFromJson(string json);
    }
}
