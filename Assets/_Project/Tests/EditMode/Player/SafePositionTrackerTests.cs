using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Player;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary><see cref="SafePositionTracker"/> save timing while grounded on safe ground.</summary>
    public class SafePositionTrackerTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _createdObjects)
                Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        private T Track<T>(T asset) where T : Object
        {
            _createdObjects.Add(asset);
            return asset;
        }

        [Test]
        public void Tick_OnFirstSafeFrame_SavesImmediately()
        {
            var config = Track(PlayerConfig.CreateForTests(safePositionInterval: 0.3f));
            var tracker = new SafePositionTracker(config);
            var position = new Vector3(1f, 2f, 3f);

            tracker.Tick(0.01f, true, position);

            Assert.That(tracker.LastSafePosition, Is.EqualTo(position));
        }

        [Test]
        public void Tick_WhileSafe_SavesOncePerInterval()
        {
            var config = Track(PlayerConfig.CreateForTests(safePositionInterval: 0.5f));
            var tracker = new SafePositionTracker(config);
            Vector3 previous = tracker.LastSafePosition;
            int saveCount = 0;

            for (int i = 1; i <= 5; i++)
            {
                tracker.Tick(0.25f, true, new Vector3(i, 0f, 0f));
                if (tracker.LastSafePosition != previous)
                {
                    saveCount++;
                    previous = tracker.LastSafePosition;
                }
            }

            Assert.That(saveCount, Is.EqualTo(3));
        }

        [Test]
        public void Tick_WhileUnsafe_KeepsLastSafePositionAndDoesNotRunTimer()
        {
            var config = Track(PlayerConfig.CreateForTests(safePositionInterval: 0.5f));
            var tracker = new SafePositionTracker(config);
            var safePosition = new Vector3(1f, 0f, 0f);
            tracker.Tick(0.25f, true, safePosition);

            tracker.Tick(1000f, false, new Vector3(99f, 99f, 99f));
            Assert.That(tracker.LastSafePosition, Is.EqualTo(safePosition));

            tracker.Tick(0.25f, true, new Vector3(2f, 0f, 0f));
            Assert.That(tracker.LastSafePosition, Is.EqualTo(safePosition));

            var finalPosition = new Vector3(3f, 0f, 0f);
            tracker.Tick(0.25f, true, finalPosition);
            Assert.That(tracker.LastSafePosition, Is.EqualTo(finalPosition));
        }
    }
}
