using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public static class RippleFactory
    {
        private static readonly Dictionary<Type, Type> eventTypes = new()
        {
            { typeof(bool), typeof(BoolEvent) },
            { typeof(float), typeof(FloatEvent) },
            { typeof(int), typeof(IntEvent) },
            { typeof(string), typeof(StringEvent) },
            { typeof(Vector2), typeof(Vector2Event) },
            { typeof(Vector3), typeof(Vector3Event) },
            { typeof(Color), typeof(ColorEvent) },
            { typeof(Quaternion), typeof(QuaternionEvent) },
            { typeof(AudioClip), typeof(AudioClipEvent) },
            { typeof(Transform), typeof(TransformEvent) },
            { typeof(void), typeof(VoidEventSO) },
        };

        private static readonly Dictionary<Type, Type> variableTypes = new()
        {
            { typeof(bool), typeof(BoolVariableSO) },
            { typeof(float), typeof(FloatVariableSO) },
            { typeof(int), typeof(IntVariableSO) },
            { typeof(string), typeof(StringVariableSO) },
            { typeof(Vector2), typeof(Vector2VariableSO) },
            { typeof(Vector3), typeof(Vector3VariableSO) },
            { typeof(Color), typeof(ColorVariableSO) },
            { typeof(GameObject), typeof(GameObjectVariableSO) },
        };

        private static readonly Dictionary<Type, Type> listenerTypes = new()
        {
            { typeof(bool), typeof(EventListenerBool) },
            { typeof(float), typeof(EventListenerFloat) },
            { typeof(int), typeof(EventListenerInt) },
            { typeof(string), typeof(EventListenerString) },
            { typeof(Vector2), typeof(EventListenerVector2) },
            { typeof(Vector3), typeof(EventListenerVector3) },
            { typeof(Color), typeof(EventListenerColor) },
            { typeof(AudioClip), typeof(EventListenerAudioClip) },
            { typeof(void), typeof(EventListenerVoid) },
        };

        public static GameEvent<T> CreateEvent<T>()
        {
            if (eventTypes.TryGetValue(typeof(T), out var concreteType))
                return (GameEvent<T>)Activator.CreateInstance(concreteType);
            return new GameEvent<T>();
        }

        public static VariableSO<T> CreateVariable<T>()
        {
            if (variableTypes.TryGetValue(typeof(T), out var concreteType))
                return (VariableSO<T>)Activator.CreateInstance(concreteType);
            return new VariableSO<T>();
        }

        public static EventListener<T> CreateEventListener<T>()
        {
            if (listenerTypes.TryGetValue(typeof(T), out var concreteType))
                return (EventListener<T>)Activator.CreateInstance(concreteType);
            return new EventListener<T>();
        }
    }
}
