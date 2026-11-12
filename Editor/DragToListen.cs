using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ripple
{
    [InitializeOnLoad]
    internal static class DragToListen
    {
        static DragToListen()
        {
#if UNITY_6000_3_OR_NEWER
            DragAndDrop.AddDropHandlerV2(OnHierarchyDrop);
            DragAndDrop.AddDropHandlerV2(OnInspectorDrop);
#else
            DragAndDrop.AddDropHandler(OnHierarchyDrop);
            DragAndDrop.AddDropHandler(OnInspectorDrop);
#endif
        }

#if UNITY_6000_3_OR_NEWER
        private static DragAndDropVisualMode OnHierarchyDrop(EntityId id, HierarchyDropFlags mode, Transform parent, bool perform) =>
            OnHierarchyDrop(EditorUtility.EntityIdToObject(id), mode, perform);
#else
        private static DragAndDropVisualMode OnHierarchyDrop(int id, HierarchyDropFlags mode, Transform parent, bool perform) =>
            OnHierarchyDrop(EditorUtility.InstanceIDToObject(id), mode, perform);
#endif

        private static DragAndDropVisualMode OnHierarchyDrop(Object target, HierarchyDropFlags mode, bool perform) =>
            mode.HasFlag(HierarchyDropFlags.DropUpon) ? Drop(new[] { target }, perform) : DragAndDropVisualMode.None;

        private static DragAndDropVisualMode OnInspectorDrop(Object[] targets, bool perform) => Drop(targets, perform);

        private static DragAndDropVisualMode Drop(Object[] targets, bool perform)
        {
            GameObject[] gameObjects = targets.OfType<GameObject>().ToArray();
            var listeners = DragAndDrop.objectReferences
                .Select(asset => (asset, type: ListenerFor(asset)))
                .Where(listener => listener.type != null)
                .ToArray();
            if (Application.isPlaying || gameObjects.Length == 0 || listeners.Length == 0)
                return DragAndDropVisualMode.None;

            if (perform)
                foreach (GameObject gameObject in gameObjects)
                foreach ((Object asset, Type type) in listeners)
                    AddListener(gameObject, type, asset);

            return DragAndDropVisualMode.Link;
        }

        private static Type ListenerFor(Object asset)
        {
            if (asset is VoidEventSO) return typeof(EventListenerVoid);

            for (Type type = asset.GetType(); type != null; type = type.BaseType)
            {
                if (!type.IsGenericType) continue;
                Type definition = type.GetGenericTypeDefinition();
                Type listener = definition == typeof(GameEvent<>) ? typeof(EventListener<>)
                    : definition == typeof(VariableSO<>) ? typeof(VariableListener<>)
                    : null;
                if (listener != null)
                    return TypeCache.GetTypesDerivedFrom(listener.MakeGenericType(type.GetGenericArguments()))
                        .FirstOrDefault(candidate => !candidate.IsAbstract && !candidate.IsGenericType);
            }

            return null;
        }

        private static void AddListener(GameObject gameObject, Type type, Object asset)
        {
            var serialized = new SerializedObject(Undo.AddComponent(gameObject, type));
            (serialized.FindProperty("_event") ?? serialized.FindProperty("variable")).objectReferenceValue = asset;
            serialized.ApplyModifiedProperties();
        }
    }
}
