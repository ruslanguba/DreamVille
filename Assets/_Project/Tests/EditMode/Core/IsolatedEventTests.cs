using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Core.Events;

namespace DreamVille.Tests.EditMode.Core
{
    /// <summary>
    /// <see cref="IsolatedEvent"/>: a throwing listener is logged once and never keeps the others from running.
    /// </summary>
    public class IsolatedEventTests
    {
        [Test]
        public void Raise_WhenAnEarlierListenerThrows_StillCallsTheLaterOnes()
        {
            var calls = new List<string>();
            Action listeners = () => throw new InvalidOperationException("listener failure");
            listeners += () => calls.Add("later");
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));

            IsolatedEvent.Raise(listeners);

            Assert.That(calls, Is.EqualTo(new[] { "later" }));
        }

        [Test]
        public void Raise_WhenAListenerThrows_LogsTheExceptionOnce()
        {
            Action<int> listeners = _ => throw new InvalidOperationException("listener failure");
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));

            IsolatedEvent.Raise(listeners, 1);

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Raise_WithArguments_PassesThemToEveryListener()
        {
            var calls = new List<string>();
            Action<int, string> listeners = (number, text) => calls.Add($"first:{number}:{text}");
            listeners += (number, text) => calls.Add($"second:{number}:{text}");

            IsolatedEvent.Raise(listeners, 3, "hit");

            Assert.That(calls, Is.EqualTo(new[] { "first:3:hit", "second:3:hit" }));
        }

        [Test]
        public void Raise_WithoutListeners_DoesNothing()
        {
            Action listeners = null;

            Assert.DoesNotThrow(() => IsolatedEvent.Raise(listeners));
        }
    }
}
