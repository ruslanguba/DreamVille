using UnityEngine;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>
    /// Everything a receiver may know about one interaction. A readonly struct passed by <c>in</c>: no allocation per
    /// bullet hit, and adding a field later (a hit point, a direction) changes only the senders that fill it.
    /// </summary>
    public readonly struct InteractionContext
    {
        public InteractionContext(InteractionSource source, Transform instigator)
        {
            Source = source;
            Instigator = instigator;
        }

        public InteractionSource Source { get; }

        /// <summary>The bullet for a shot; the player's collector transform for a touch (pickups fly to it).</summary>
        public Transform Instigator { get; }

        public static InteractionContext Shot(Transform bullet) => new InteractionContext(InteractionSource.Shot, bullet);

        public static InteractionContext Touch(Transform collector) => new InteractionContext(InteractionSource.Touch, collector);
    }
}
