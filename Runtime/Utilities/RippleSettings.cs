using UnityEngine;

namespace Ripple
{
    public class RippleSettings : ScriptableObject
    {
        private const string SettingsKey = "com.metz.ripple.settings";

        [SerializeField] private bool _defaultClearListenersOnPlaymode = false;
        [SerializeField] private int _maxTraceHistoryCount = 25;
        [SerializeField] private bool _enableStackTraceCapture = true;

        public bool DefaultClearListenersOnPlaymode => _defaultClearListenersOnPlaymode;
        public int MaxTraceHistoryCount => _maxTraceHistoryCount;
        public bool EnableStackTraceCapture => _enableStackTraceCapture;

        private static RippleSettings _instance;

        public static RippleSettings Instance
        {
            get
            {
                if (_instance != null) return _instance;
#if UNITY_EDITOR
                if (UnityEditor.EditorBuildSettings.TryGetConfigObject(SettingsKey, out RippleSettings settings))
                {
                    _instance = settings;
                    return _instance;
                }
#endif
                _instance = CreateInstance<RippleSettings>();
                return _instance;
            }
        }

#if UNITY_EDITOR
        public static void SetInstance(RippleSettings settings)
        {
            _instance = settings;
            if (settings != null)
                UnityEditor.EditorBuildSettings.AddConfigObject(SettingsKey, settings, true);
        }
#endif
    }
}
