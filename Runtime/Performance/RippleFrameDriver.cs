using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public sealed class RippleFrameDriver : MonoBehaviour
    {
        private static RippleFrameDriver _instance;
        private static readonly List<ICoalescing> _coalescing = new();

        public static void Register(ICoalescing target)
        {
            EnsureInstance();
            if (!_coalescing.Contains(target)) _coalescing.Add(target);
        }

        public static void Unregister(ICoalescing target) => _coalescing.Remove(target);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { _instance = null; _coalescing.Clear(); }

        private static void EnsureInstance()
        {
            if (_instance != null) return;
            if (!Application.isPlaying) return;
            var go = new GameObject("[RippleFrameDriver]");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<RippleFrameDriver>();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _coalescing.Count; i++)
                _coalescing[i].FlushFrame();
        }
    }

    public interface ICoalescing
    {
        void FlushFrame();
    }
}
