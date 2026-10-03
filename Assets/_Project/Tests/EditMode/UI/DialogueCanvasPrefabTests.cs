using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.UI.Dialogues;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Guards spec 11 B3: the dialogue canvas prefab keeps every view reference, and its Next button reaches the
    /// presenter only through the code binding — no Inspector call into a project script (ADR 0008 rule).
    /// </summary>
    public class DialogueCanvasPrefabTests
    {
        private const string CanvasPrefabPath = "Assets/_Project/Prefabs/UI/DialogueCanvas.prefab";

        private GameObject _canvas;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _canvas = AssetDatabase.LoadAssetAtPath<GameObject>(CanvasPrefabPath);
            Assume.That(_canvas, Is.Not.Null, $"{CanvasPrefabPath} is missing.");
        }

        [Test]
        public void DialogueView_ReferencesAreAssigned()
        {
            var view = _canvas.GetComponent<DialogueView>();
            Assert.That(view, Is.Not.Null, "The canvas root has no DialogueView.");

            var unassigned = new List<string>();
            SerializedProperty property = new SerializedObject(view).GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.name != "m_Script" && property.objectReferenceValue == null)
                    unassigned.Add(property.name);
            }

            Assert.That(unassigned, Is.Empty, "DialogueView references not assigned: " + string.Join(", ", unassigned));
        }

        [Test]
        public void Buttons_HaveNoPersistentCallsToProjectScripts()
        {
            var offenders = new List<string>();

            foreach (Button button in _canvas.GetComponentsInChildren<Button>(true))
            {
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                {
                    Object target = button.onClick.GetPersistentTarget(i);
                    string targetNamespace = target != null ? target.GetType().Namespace ?? string.Empty : string.Empty;
                    if (targetNamespace.StartsWith("DreamVille"))
                        offenders.Add($"{button.name} -> {target.GetType().Name}.{button.onClick.GetPersistentMethodName(i)}");
                }
            }

            Assert.That(offenders, Is.Empty, "Buttons must reach project scripts through a view binding: " + string.Join(", ", offenders));
        }
    }
}
