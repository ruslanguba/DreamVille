using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Audio;

namespace DreamVille.Tests.EditMode.Audio
{
    /// <summary>
    /// <see cref="ActorAudio"/> shares one AudioSource between jittered sounds (jump, footsteps) and plain
    /// one-shots (death, attack); a plain one-shot must not inherit the last jitter.
    /// </summary>
    public class ActorAudioTests
    {
        private GameObject _host;
        private AudioClip _clip;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("Actor");
            _clip = AudioClip.Create("Test", 441, 1, 44100, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_clip);
        }

        [Test]
        public void PlayDeath_AfterAJitteredJump_PlaysAtNeutralPitch()
        {
            var actor = _host.AddComponent<ActorAudio>();
            var serialized = new SerializedObject(actor);
            serialized.FindProperty("_jumpClip").objectReferenceValue = _clip;
            serialized.FindProperty("_deathClip").objectReferenceValue = _clip;
            serialized.FindProperty("_jumpPitchJitter").floatValue = 0.5f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var source = _host.GetComponent<AudioSource>();
            Random.InitState(1);
            actor.PlayJump();
            Assume.That(source.pitch, Is.Not.EqualTo(1f), "The seeded jump happened to land on a neutral pitch.");

            actor.PlayDeath();

            Assert.That(source.pitch, Is.EqualTo(1f));
        }
    }
}
