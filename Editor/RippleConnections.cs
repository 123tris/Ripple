using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ripple
{
    internal static class RippleConnections
    {
        [Serializable]
        public class Listener
        {
            public string Kind;
            public string Name;
            public Object Target;
        }

        public static void Find(RippleStackTraceSO asset, bool includePrefabs, List<Listener> found)
        {
            found.Clear();

            foreach (UnityEventBase response in asset.SerializedResponses)
                for (int i = 0; i < response.GetPersistentEventCount(); i++)
                {
                    Object target = response.GetPersistentTarget(i);
                    found.Add(new Listener { Kind = "Response", Name = (target ? NameOf(target) : "(missing)") + " → " + response.GetPersistentMethodName(i), Target = target });
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
        }

        private static Listener Subscriber(SubscriberInfo subscriber)
        {
            object target = null;
            subscriber.Target?.TryGetTarget(out target);
            var owner = target as Object;
            return new Listener { Kind = "Code", Name = (owner ? NameOf(owner) : subscriber.DeclaringType) + " → " + subscriber.MethodName, Target = owner };
        }

        private static string NameOf(Object owner) => $"{owner.name} ({owner.GetType().Name})";

        private static void Collect(GameObject root, RippleStackTraceSO asset, string kind, string scope, List<Listener> found)
        {
            foreach (IRippleListener listener in root.GetComponentsInChildren<IRippleListener>(true))
            {
                var component = (Component)listener;
                if (listener.Target == asset && !found.Exists(known => known.Target == component))
                    found.Add(new Listener { Kind = kind, Name = $"{scope} / {HierarchyPath(component.transform)} ({component.GetType().Name})", Target = component });
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
