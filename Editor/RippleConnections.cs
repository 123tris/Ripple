using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Ripple
{
    internal static class RippleConnections
    {
        public readonly struct Listener
        {
            public readonly string Kind;
            public readonly string Name;
            public readonly Object Target;

            public Listener(string kind, string name, Object target)
            {
                Kind = kind;
                Name = name;
                Target = target;
            }
        }

        public static List<Listener> Find(RippleStackTraceSO asset, bool includePrefabs)
        {
            var found = new List<Listener>();

            foreach (UnityEventBase response in asset.SerializedResponses)
                for (int i = 0; i < response.GetPersistentEventCount(); i++)
                {
                    Object target = response.GetPersistentTarget(i);
                    found.Add(new Listener("Response", (target ? NameOf(target) : "(missing)") + " → " + response.GetPersistentMethodName(i), target));
                }

            foreach (SubscriberInfo subscriber in RippleConnectionRegistry.GetSubscribers(asset))
                found.Add(Subscriber(subscriber));

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                    Collect(root, asset, "Scene", scene.name == "" ? "Untitled" : scene.name, found);
            }

            if (includePrefabs)
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Collect(AssetDatabase.LoadAssetAtPath<GameObject>(path), asset, "Prefab", path, found);
                }

            return found;
        }

        private static Listener Subscriber(SubscriberInfo subscriber)
        {
            object target = null;
            subscriber.Target?.TryGetTarget(out target);
            var owner = target as Object;
            return new Listener("Code", (owner ? NameOf(owner) : subscriber.DeclaringType) + " → " + subscriber.MethodName, owner);
        }

        private static string NameOf(Object owner) => $"{owner.name} ({owner.GetType().Name})";

        private static void Collect(GameObject root, RippleStackTraceSO asset, string kind, string scope, List<Listener> found)
        {
            foreach (IRippleListener listener in root.GetComponentsInChildren<IRippleListener>(true))
            {
                var component = (Component)listener;
                if (listener.Target == asset && !found.Exists(known => known.Target == component))
                    found.Add(new Listener(kind, $"{scope} / {HierarchyPath(component.transform)} ({component.GetType().Name})", component));
            }
        }

        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            for (Transform parent = transform.parent; parent; parent = parent.parent)
                path = parent.name + "/" + path;
            return path;
        }
    }
}
