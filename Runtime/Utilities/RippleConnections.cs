#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ripple
{
    public static class RippleConnections
    {
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

        public static void PopulateListeners(RippleStackTraceSO asset, List<ListenerRow> rows, bool includePrefabs)
        {
            rows.Clear();
            if (asset == null) return;

            foreach (var (target, method) in GetPersistentCalls(asset.SerializedResponse))
                rows.Add(ListenerRow.Serialized(target, method));

            foreach (var s in RippleConnectionRegistry.GetSubscribers(asset))
                rows.Add(ListenerRow.Runtime(s));

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                    CollectListeners(root, asset, scene.name, "Scene", rows);
            }

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

        private static IEnumerable<(Object target, string method)> GetPersistentCalls(UnityEventBase response)
        {
            if (response == null) yield break;
            for (int i = 0; i < response.GetPersistentEventCount(); i++)
                yield return (response.GetPersistentTarget(i), response.GetPersistentMethodName(i));
        }
    }
}
#endif
