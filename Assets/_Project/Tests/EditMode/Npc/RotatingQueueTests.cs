using System;
using System.Collections.Generic;
using NUnit.Framework;

using DreamVille.Gameplay.Npc;

namespace DreamVille.Tests.EditMode.Npc
{
    /// <summary>Order rule of <see cref="RotatingQueue{T}"/>, the NPC queue's single source of order.</summary>
    public class RotatingQueueTests
    {
        [Test]
        public void Front_IsFirstMember()
        {
            RotatingQueue<string> queue = new RotatingQueue<string>(new[] { "a", "b", "c" });

            Assert.That(queue.Front, Is.EqualTo("a"));
        }

        [Test]
        public void RotateFrontToBack_MovesFrontToBackAndShiftsOthers()
        {
            RotatingQueue<string> queue = new RotatingQueue<string>(new[] { "a", "b", "c" });

            queue.RotateFrontToBack();

            Assert.That(Snapshot(queue), Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void RotateFrontToBack_CountTimes_RestoresOriginalOrder()
        {
            string[] members = { "a", "b", "c", "d", "e", "f" };
            RotatingQueue<string> queue = new RotatingQueue<string>(members);

            for (int i = 0; i < queue.Count; i++)
                queue.RotateFrontToBack();

            Assert.That(Snapshot(queue), Is.EqualTo(members));
        }

        [Test]
        public void Constructor_CopiesMembers()
        {
            List<string> source = new List<string> { "a", "b" };
            RotatingQueue<string> queue = new RotatingQueue<string>(source);

            source.Clear();
            source.Add("z");

            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(Snapshot(queue), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void Constructor_WithNoMembers_Throws()
        {
            Assert.Throws<ArgumentException>(() => new RotatingQueue<string>(Array.Empty<string>()));
        }

        private static string[] Snapshot(RotatingQueue<string> queue)
        {
            string[] snapshot = new string[queue.Count];
            for (int i = 0; i < queue.Count; i++)
                snapshot[i] = queue[i];

            return snapshot;
        }
    }
}
