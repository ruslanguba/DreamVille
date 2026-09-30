using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

using DreamVille.Configs;
using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Guards spec 10 §5 C1–C4: every menu button in <c>MainMenu</c> and in the pause menu (as nested in
    /// <c>GameManager.prefab</c>, the asset the gameplay scenes instantiate) does something, every Inspector
    /// call still resolves, and the pause view keeps its references and the nested localization components.
    /// </summary>
    public class MenuWiringTests
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GameManagerPrefabPath = "Assets/_Project/Prefabs/GameManager.prefab";
        private const string CatalogAssetPath = "Assets/_Project/Settings/Levels/SceneCatalog.asset";
        private const int AddedLocalizeEventCount = 11;

        private static readonly Type[] ButtonOwnerViewTypes =
        {
            typeof(MainMenuView),
            typeof(LevelButtonView),
            typeof(PauseMenuView)
        };

        private string _originalScenePath;
        private GameObject _gameManager;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            _originalScenePath = activeScene.path;

            // Interactively a dirty scene just skips the fixture; on a build machine it is a real failure.
            const string dirtySceneMessage = "A dirty scene is open; save or discard changes before running this fixture.";
            if (Application.isBatchMode)
                Assert.That(activeScene.isDirty, Is.False, dirtySceneMessage);
            else
                Assume.That(!activeScene.isDirty, dirtySceneMessage);

            _gameManager = PrefabUtility.LoadPrefabContents(GameManagerPrefabPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (_gameManager != null)
                PrefabUtility.UnloadPrefabContents(_gameManager);

            if (string.IsNullOrEmpty(_originalScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else
                EditorSceneManager.OpenScene(_originalScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void MenuButtons_EveryPersistentCallResolves()
        {
            var failures = new List<string>();

            foreach (Button button in AllMenuButtons())
            {
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                {
                    string failure = DescribeUnresolvedCall(button, i);
                    if (failure != null)
                        failures.Add(failure);
                }
            }

            Assert.That(failures, Is.Empty, "Unresolved button calls:\n" + string.Join("\n", failures));
        }

        [Test]
        public void MenuButtons_EveryButtonHasAnAction()
        {
            List<Button> buttons = AllMenuButtons();
            HashSet<Button> boundInCode = ButtonsReferencedByViews();

            string[] deadButtons = buttons
                .Where(button => button.onClick.GetPersistentEventCount() == 0 && !boundInCode.Contains(button))
                .Select(PathOf)
                .ToArray();

            Assert.That(deadButtons, Is.Empty, "Buttons with neither an Inspector call nor a view binding:\n" + string.Join("\n", deadButtons));
        }

        [Test]
        public void MainMenu_LevelSelectView_ListsEveryCatalogLevelInOrder()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SceneCatalog>(CatalogAssetPath);
            Assume.That(catalog, Is.Not.Null, $"{CatalogAssetPath} is missing.");
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            LevelSelectView view = Object.FindObjectsByType<LevelSelectView>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();

            Assert.That(view.Levels, Is.EqualTo(catalog.Levels), "Level select entries must match SceneCatalog.Levels, in order.");
            Assert.That(catalog.Levels[0].UnlockedByDefault, Is.True, "New Game loads the first catalog level, so it must be unlocked by default.");
        }

        [Test]
        public void PauseMenuPrefab_ViewReferencesAreAssigned()
        {
            PauseMenuView view = _gameManager.GetComponentsInChildren<PauseMenuView>(true).Single();
            var serialized = new SerializedObject(view);
            string[] fields =
            {
                "_mainMenuPanel", "_settingsPanel", "_exitPanel", "_menuParent", "_comics",
                "_continueButton", "_restartButton", "_exitToMenuButton"
            };

            foreach (string field in fields)
            {
                Object reference = serialized.FindProperty(field).objectReferenceValue;

                Assert.That(reference, Is.Not.Null, $"PauseMenuView.{field} is not assigned.");
                Transform referenceTransform = reference is Component component ? component.transform : ((GameObject)reference).transform;
                Assert.That(referenceTransform.IsChildOf(view.transform), Is.True, $"PauseMenuView.{field} points outside the pause menu.");
            }

            GameObject canvasMenu = PrefabUtility.GetNearestPrefabInstanceRoot(view.gameObject);
            Assert.That(canvasMenu, Is.Not.Null, "The pause menu is no longer a nested CanvasMenu instance.");
            int addedLocalizeEvents = PrefabUtility.GetAddedComponents(canvasMenu)
                .Count(added => added.instanceComponent != null && added.instanceComponent.GetType().Name.StartsWith("Localize"));
            Assert.That(addedLocalizeEvents, Is.EqualTo(AddedLocalizeEventCount),
                "GameManager's CanvasMenu lost its added Localize*Event components (a prefab revert drops them).");
        }

        private List<Button> AllMenuButtons()
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            var buttons = new List<Button>(Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            buttons.AddRange(_gameManager.GetComponentsInChildren<Button>(true));

            Assume.That(buttons, Is.Not.Empty, "No menu buttons found.");
            return buttons;
        }

        private HashSet<Button> ButtonsReferencedByViews()
        {
            var views = new List<Component>();
            foreach (Type viewType in ButtonOwnerViewTypes)
            {
                views.AddRange(Object.FindObjectsByType(viewType, FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<Component>());
                views.AddRange(_gameManager.GetComponentsInChildren(viewType, true));
            }

            var buttons = new HashSet<Button>();
            foreach (Component view in views)
            {
                SerializedProperty property = new SerializedObject(view).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is Button button)
                        buttons.Add(button);
                }
            }

            return buttons;
        }

        private static string DescribeUnresolvedCall(Button button, int index)
        {
            Object target = button.onClick.GetPersistentTarget(index);
            string methodName = button.onClick.GetPersistentMethodName(index);
            string call = $"{PathOf(button)} call {index} ({methodName})";

            if (target == null)
                return $"{call}: no target.";
            if (string.IsNullOrEmpty(methodName))
                return $"{call}: no method.";

            var serialized = new SerializedObject(button);
            SerializedProperty mode = serialized.FindProperty($"m_OnClick.m_PersistentCalls.m_Calls.Array.data[{index}].m_Mode");
            Type argumentType = ArgumentTypeOf((PersistentListenerMode)mode.intValue);

            bool resolves = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(method => method.Name == methodName)
                .Any(method => Accepts(method, argumentType));

            return resolves ? null : $"{call}: {target.GetType().Name} has no public '{methodName}' for mode {(PersistentListenerMode)mode.intValue}.";
        }

        private static Type ArgumentTypeOf(PersistentListenerMode mode) => mode switch
        {
            PersistentListenerMode.Void => null,
            PersistentListenerMode.EventDefined => null,
            PersistentListenerMode.Object => typeof(Object),
            PersistentListenerMode.Int => typeof(int),
            PersistentListenerMode.Float => typeof(float),
            PersistentListenerMode.String => typeof(string),
            PersistentListenerMode.Bool => typeof(bool),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        private static bool Accepts(MethodInfo method, Type argumentType)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (argumentType == null)
                return parameters.Length == 0;

            return parameters.Length == 1
                && (parameters[0].ParameterType == argumentType
                    || (argumentType == typeof(Object) && typeof(Object).IsAssignableFrom(parameters[0].ParameterType)));
        }

        private static string PathOf(Component component)
        {
            Transform current = component.transform;
            string path = current.name;
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }
    }
}
