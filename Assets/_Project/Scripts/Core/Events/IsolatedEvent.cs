using System;
using UnityEngine;

namespace DreamVille.Core.Events
{
    /// <summary>
    /// Raises an event so that each listener runs on its own: one that throws is logged and the rest still run.
    /// Used by every event with several independent listeners, where one failing listener must not stall a game
    /// flow (a bullet that never returns to its pool, a level that never completes, a dialogue stuck active).
    /// </summary>
    public static class IsolatedEvent
    {
        /// <summary>Invokes every listener of <paramref name="listeners"/>; null means no listeners.</summary>
        /// <param name="context">Log context when the failing listener is not itself a live Unity object.</param>
        public static void Raise(Action listeners, UnityEngine.Object context = null)
        {
            if (listeners == null)
                return;

            foreach (Delegate listener in listeners.GetInvocationList())
            {
                try
                {
                    ((Action)listener).Invoke();
                }
                catch (Exception exception)
                {
                    Log(exception, listener, context);
                }
            }
        }

        /// <inheritdoc cref="Raise(Action, UnityEngine.Object)"/>
        public static void Raise<T>(Action<T> listeners, T argument, UnityEngine.Object context = null)
        {
            if (listeners == null)
                return;

            foreach (Delegate listener in listeners.GetInvocationList())
            {
                try
                {
                    ((Action<T>)listener).Invoke(argument);
                }
                catch (Exception exception)
                {
                    Log(exception, listener, context);
                }
            }
        }

        /// <inheritdoc cref="Raise(Action, UnityEngine.Object)"/>
        public static void Raise<T1, T2>(Action<T1, T2> listeners, T1 first, T2 second, UnityEngine.Object context = null)
        {
            if (listeners == null)
                return;

            foreach (Delegate listener in listeners.GetInvocationList())
            {
                try
                {
                    ((Action<T1, T2>)listener).Invoke(first, second);
                }
                catch (Exception exception)
                {
                    Log(exception, listener, context);
                }
            }
        }

        private static void Log(Exception exception, Delegate listener, UnityEngine.Object context)
        {
            // Point the log at the listener that threw when it is a live Unity object (explicit check: ?? would skip
            // Unity's null for a destroyed one).
            var target = listener.Target as UnityEngine.Object;
            Debug.LogException(exception, target != null ? target : context);
        }
    }
}
