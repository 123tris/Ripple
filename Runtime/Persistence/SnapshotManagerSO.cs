using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ripple
{
    [RippleData("Persistence")]
    [CreateAssetMenu(menuName = "Ripple/Persistence/Snapshot Manager", fileName = "SnapshotManager")]
    public sealed class SnapshotManagerSO : RippleStackTraceSO
    {
        [SerializeField] private List<ScriptableObject> _trackedAssets = new();
        [SerializeField] private string _fileName = "ripple_snapshot.json";

        [System.Serializable]
        private struct Entry
        {
            public string Key;
            public string Json;
        }

        [System.Serializable]
        private struct Wrapper
        {
            public List<Entry> Entries;
        }

        public string FilePath => Path.Combine(Application.persistentDataPath, _fileName);

        public void Save()
        {
            var wrapper = new Wrapper { Entries = new List<Entry>() };
            foreach (var so in _trackedAssets)
            {
                if (so is ISnapshotable s)
                    wrapper.Entries.Add(new Entry { Key = s.SnapshotKey, Json = s.CaptureSnapshotJson() });
            }
            File.WriteAllText(FilePath, JsonUtility.ToJson(wrapper, true));
        }

        public void Load()
        {
            if (!File.Exists(FilePath)) return;
            var wrapper = JsonUtility.FromJson<Wrapper>(File.ReadAllText(FilePath));
            if (wrapper.Entries == null) return;

            var byKey = new Dictionary<string, string>();
            foreach (var e in wrapper.Entries) byKey[e.Key] = e.Json;

            foreach (var so in _trackedAssets)
            {
                if (so is ISnapshotable s && byKey.TryGetValue(s.SnapshotKey, out var json))
                    s.RestoreFromJson(json);
            }
        }
    }
}
