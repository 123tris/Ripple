#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ripple
{
    /// <summary>
    /// Editor-only support for the per-asset "Connections" view on <see cref="RippleStackTraceSO"/>:
    /// enumerates the listeners of an event/variable across three sources — serialized UltEvent
    /// persistent calls, runtime dynamic subscribers (<see cref="RippleConnectionRegistry"/>), and
    /// design-time listener components in open scenes and (optionally) prefab assets.
    ///
    /// Lives in the runtime assembly (gated on UNITY_EDITOR) only so the inline Odin panel on the
    /// runtime <see cref="RippleStackTraceSO"/> can call it; the runtime assembly cannot reference
    /// the editor assembly. This mirrors the package's existing pattern of inline editor-only Odin
    /// UI on runtime ScriptableObjects.
    /// </summary>
    public static class RippleConnections
    {
        /// <summary>One row of the Listeners table. Rendered by Odin via [TableList].</summary>
        public sealed class ListenerRow
        {
            [TableColumnWidth(90, false)] public string Kind;
            public string Listener;

            [HideInTables, HideInInspector] public Object PingTarget;

            [Button("Ping"), ShowIf("@PingTarget != null"), TableColumnWidth(60, false)]
            private void Ping() => RippleConnections.Ping(PingTarget);

            public static ListenerRow Serialized(Object target, string method) => new()
            {
                Kind = "Serialized",
                Listener = (target ? target.name : "(missing target)") + " → " + (method ?? "?"),
                PingTarget = target,
            };

            public static ListenerRow Runtime(SubscriberInfo s)
            {
                object t = null;
                s.Target?.TryGetTarget(out t);
                var uo = t as Object;
                string listener = uo
                    ? uo.name + " → " + (s.MethodName ?? "?")
                    : (s.DeclaringType ?? "?") + " → " + (s.MethodName ?? "?")
                      + (s.Target != null ? " (collected)" : " (static)");
                return new ListenerRow { Kind = "Runtime", Listener = listener, PingTarget = uo };
            }

            public static ListenerRow Component(MonoBehaviour mb, string display, string kind) => new()
            {
                Kind = kind,
                Listener = display,
                PingTarget = mb,
            };
        }

        /// <summary>Rebuilds <paramref name="rows"/> with every known listener of <paramref name="asset"/>.</summary>
        public static void PopulateListeners(RippleStackTraceSO asset, List<ListenerRow> rows, bool includePrefabs)
        {
            rows.Clear();
            if (asset == null) return;

            // 1) Serialized UltEvent persistent calls wired directly on the asset's own response.
            foreach (var (target, method) in GetPersistentCalls(GetUltEvent(asset)))
                rows.Add(ListenerRow.Serialized(target, method));

            // 2) Runtime dynamic subscribers captured through AddListener/RemoveListener (Play mode).
            foreach (var s in RippleConnectionRegistry.GetSubscribers(asset))
                rows.Add(ListenerRow.Runtime(s));

            // 3) Design-time listener components in open scenes.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                    CollectListeners(root, asset, scene.name, "Scene", rows);
            }

            // 4) Design-time listener components living inside prefab assets (project-wide, opt-in).
            if (includePrefabs)
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (go) CollectListeners(go, asset, path, "Prefab", rows);
                }
            }
        }

        private static void CollectListeners(GameObject root, RippleStackTraceSO asset, string scope, string kind,
            List<ListenerRow> rows)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb is IRippleListener l && l.Target == asset)
                    rows.Add(ListenerRow.Component(mb, scope + " / " + HierarchyPath(mb.transform), kind));
            }
        }

        private static string HierarchyPath(Transform t)
        {
            string path = t.name;
            for (Transform p = t.parent; p != null; p = p.parent)
                path = p.name + "/" + path;
            return path;
        }

        public static void Ping(Object o)
        {
            if (!o) return;
            Selection.activeObject = o;
            EditorGUIUtility.PingObject(o);
        }

        // ---- UltEvents reflection (isolated + graceful-degrade) -------------------------------
        // VERIFY against the installed UltEvents version: the member that exposes persistent calls on
        // UltEvents.UltEventBase, and Target/MethodName on UltEvents.PersistentCall. If a name differs
        // these helpers simply return nothing rather than breaking compilation or throwing.

        private static readonly string[] UltEventFieldNames = { "response", "gameEvent", "OnValueChanged" };
        private static PropertyInfo _persistentCallsProp;
        private static FieldInfo _persistentCallsField;
        private static bool _persistentCallsResolved;

        private static object GetUltEvent(RippleStackTraceSO asset)
        {
            for (var t = asset.GetType(); t != null && t != typeof(object); t = t.BaseType)
            {
                foreach (string name in UltEventFieldNames)
                {
                    FieldInfo f = t.GetField(name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    object value = f?.GetValue(asset);
                    if (value != null) return value;
                }
            }
            return null;
        }

        private static IEnumerable<(Object target, string method)> GetPersistentCalls(object ultEvent)
        {
            var result = new List<(Object, string)>();
            if (ultEvent == null) return result;

            IEnumerable calls = GetPersistentCallList(ultEvent);
            if (calls == null) return result;

            foreach (object call in calls)
            {
                if (call == null) continue;
                System.Type ct = call.GetType();
                var target = ct.GetProperty("Target")?.GetValue(call) as Object;
                var method = ct.GetProperty("MethodName")?.GetValue(call) as string;
                result.Add((target, method));
            }
            return result;
        }

        private static IEnumerable GetPersistentCallList(object ultEvent)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            System.Type type = ultEvent.GetType();

            if (!_persistentCallsResolved)
            {
                _persistentCallsProp = type.GetProperty("PersistentCallsList", flags)
                                       ?? type.GetProperty("PersistentCalls", flags);
                if (_persistentCallsProp == null)
                    _persistentCallsField = type.GetField("_PersistentCalls", flags)
                                            ?? type.GetField("persistentCalls", flags);
                _persistentCallsResolved = true;
            }

            object raw = _persistentCallsProp != null ? _persistentCallsProp.GetValue(ultEvent)
                : _persistentCallsField?.GetValue(ultEvent);
            return raw as IEnumerable;
        }
    }
}
#endif
