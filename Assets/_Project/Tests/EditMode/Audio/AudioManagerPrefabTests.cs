using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Audio;
using DreamVille.Bootstrap;

namespace DreamVille.Tests.EditMode.Audio
{
    /// <summary>
    /// The audio prefab that <see cref="ProjectInstaller"/> instantiates stays silent until <see cref="AudioPlayer"/>
    /// starts a track: no source plays on awake, and no source points at a clip or resource whose asset is gone (a
    /// missing reference loads as null, so nothing else would notice it).
    /// </summary>
    public class AudioManagerPrefabTests
    {
        private const string ProjectContextPrefabPath = "Assets/_Project/Resources/ProjectContext.prefab";

        private static readonly string[] ClipFields = { "m_audioClip", "m_Resource" };

        [Test]
        public void Sources_DoNotPlayOnAwakeAndReferenceNoMissingClip()
        {
            AudioSource[] sources = LoadAudioPlayerPrefab().GetComponentsInChildren<AudioSource>(true);
            Assert.That(sources, Is.Not.Empty, $"The {nameof(AudioPlayer)} prefab has no AudioSource.");

            var offenders = new List<string>();

            foreach (AudioSource source in sources)
            {
                if (source.playOnAwake)
                    offenders.Add($"{source.name}: Play On Awake is on");

                var serialized = new SerializedObject(source);
                foreach (string field in ClipFields)
                {
                    SerializedProperty reference = serialized.FindProperty(field);
                    if (reference.objectReferenceValue == null && reference.objectReferenceInstanceIDValue != 0)
                        offenders.Add($"{source.name}: {field} points at a missing asset");
                }
            }

            Assert.That(offenders, Is.Empty, string.Join("\n", offenders));
        }

        private static AudioPlayer LoadAudioPlayerPrefab()
        {
            var projectContext = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPrefabPath);
            Assert.That(projectContext, Is.Not.Null, $"{ProjectContextPrefabPath} is missing.");

            var installer = projectContext.GetComponentInChildren<ProjectInstaller>(true);
            Assert.That(installer, Is.Not.Null, $"{ProjectContextPrefabPath} has no {nameof(ProjectInstaller)}.");

            var prefab = new SerializedObject(installer).FindProperty("_audioPlayerPrefab").objectReferenceValue as AudioPlayer;
            Assert.That(prefab, Is.Not.Null, $"{nameof(ProjectInstaller)}._audioPlayerPrefab is not assigned.");

            return prefab;
        }
    }
}
