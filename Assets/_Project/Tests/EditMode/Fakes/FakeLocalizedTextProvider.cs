using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written <see cref="ILocalizedTextProvider"/> double. By default it answers synchronously with
    /// "&lt;table&gt;/&lt;key&gt;" (see <see cref="TextFor"/>), like a preloaded table; in deferred mode every call
    /// stays pending until <see cref="CompletePending"/>. It ignores the cancellation token on purpose.
    /// </summary>
    public sealed class FakeLocalizedTextProvider : ILocalizedTextProvider
    {
        private readonly List<(UniTaskCompletionSource<string> Source, string Text)> _pending
            = new List<(UniTaskCompletionSource<string>, string)>();

        /// <summary>When true, calls return pending tasks that <see cref="CompletePending"/> finishes.</summary>
        public bool Deferred { get; set; }

        /// <summary>When set, every call fails with this exception.</summary>
        public Exception Failure { get; set; }

        public int CallCount { get; private set; }

        public static string TextFor(string table, string key) => $"{table}/{key}";

        public UniTask<string> GetTextAsync(LocalizedString text, CancellationToken cancellationToken)
        {
            CallCount++;

            if (Failure != null)
                return UniTask.FromException<string>(Failure);

            string resolved = TextFor(text.TableReference.TableCollectionName, text.TableEntryReference.Key);
            if (!Deferred)
                return UniTask.FromResult(resolved);

            var source = new UniTaskCompletionSource<string>();
            _pending.Add((source, resolved));
            return source.Task;
        }

        /// <summary>Finishes every pending call, including the ones that finishing the first ones triggers.</summary>
        public void CompletePending()
        {
            while (_pending.Count > 0)
            {
                (UniTaskCompletionSource<string> source, string text) = _pending[0];
                _pending.RemoveAt(0);
                source.TrySetResult(text);
            }
        }
    }
}
