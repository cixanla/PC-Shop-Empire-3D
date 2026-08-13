using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Presentation.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay
{
    public sealed class PlayerInputContractTests
    {
        [Test]
        public void GameplayInputAssetHasOnlyTheLockedPrototypeActionsAndSchemes()
        {
            InputActionAsset actions = LoadActions();
            InputActionMap player = actions.FindActionMap(PlayerInputContract.PlayerMap, true);
            string[] expectedActions =
            {
                PlayerInputContract.Move,
                PlayerInputContract.Look,
                PlayerInputContract.PrimaryAction,
                PlayerInputContract.Interact,
                PlayerInputContract.Sprint,
                PlayerInputContract.Drop,
                PlayerInputContract.Pause
            };

            Assert.That(player.actions.Select(action => action.name), Is.EquivalentTo(expectedActions));
            Assert.That(actions.FindActionMap(PlayerInputContract.UiMap, true), Is.Not.Null);
            Assert.That(
                actions.controlSchemes.Select(scheme => scheme.name),
                Is.EquivalentTo(new[]
                {
                    PlayerInputContract.KeyboardAndMouseScheme,
                    PlayerInputContract.GamepadScheme
                }));
            Assert.That(player.actions.All(action => string.IsNullOrEmpty(action.interactions)), Is.True);
            Assert.That(player.FindAction(PlayerInputContract.Interact, true).interactions, Is.Empty);
        }

        [Test]
        public void StableActionIdsAndEssentialBindingsArePreserved()
        {
            InputActionMap player = LoadActions().FindActionMap(PlayerInputContract.PlayerMap, true);

            Assert.That(
                player.FindAction(PlayerInputContract.Move, true).id.ToString(),
                Is.EqualTo("351f2ccd-1f9f-44bf-9bec-d62ac5c5f408"));
            Assert.That(
                player.FindAction(PlayerInputContract.Look, true).id.ToString(),
                Is.EqualTo("6b444451-8a00-4d00-a97e-f47457f736a8"));
            Assert.That(
                player.FindAction(PlayerInputContract.PrimaryAction, true).id.ToString(),
                Is.EqualTo("6c2ab1b8-8984-453a-af3d-a3c78ae1679a"));
            Assert.That(
                player.FindAction(PlayerInputContract.Interact, true).id.ToString(),
                Is.EqualTo("852140f2-7766-474d-8707-702459ba45f3"));
            AssertBinding(player, PlayerInputContract.PrimaryAction, "<Mouse>/leftButton");
            AssertBinding(player, PlayerInputContract.PrimaryAction, "<Gamepad>/rightTrigger");
            AssertBinding(player, PlayerInputContract.Interact, "<Keyboard>/e");
            AssertBinding(player, PlayerInputContract.Interact, "<Gamepad>/buttonSouth");
            AssertBinding(player, PlayerInputContract.Drop, "<Keyboard>/g");
            AssertBinding(player, PlayerInputContract.Drop, "<Gamepad>/buttonEast");
            AssertBinding(player, PlayerInputContract.Pause, "<Keyboard>/escape");
            AssertBinding(player, PlayerInputContract.Pause, "<Gamepad>/start");
            AssertBinding(player, PlayerInputContract.Look, "<Mouse>/delta");
            Assert.That(
                player.FindAction(PlayerInputContract.Look, true).bindings.Any(
                    binding => binding.path == "<Pointer>/delta"),
                Is.False);
        }

        [Test]
        public void BindingOverridesRoundTripAndResetOutsideSimulationSave()
        {
            InputActionAsset source = LoadActions();
            InputActionAsset actions = UnityEngine.Object.Instantiate(source);
            var store = new MemorySettingsStore();
            try
            {
                InputAction interact = actions.FindActionMap(PlayerInputContract.PlayerMap, true)
                    .FindAction(PlayerInputContract.Interact, true);
                int keyboardBinding = FindBinding(interact, "<Keyboard>/e");
                interact.ApplyBindingOverride(keyboardBinding, "<Keyboard>/f");

                InputBindingOverrideStore.Save(actions, store);
                interact.RemoveBindingOverride(keyboardBinding);
                Assert.That(interact.bindings[keyboardBinding].effectivePath, Is.EqualTo("<Keyboard>/e"));

                Assert.That(InputBindingOverrideStore.Load(actions, store), Is.True);
                Assert.That(interact.bindings[keyboardBinding].effectivePath, Is.EqualTo("<Keyboard>/f"));

                InputBindingOverrideStore.Reset(actions, store);
                Assert.That(interact.bindings[keyboardBinding].effectivePath, Is.EqualTo("<Keyboard>/e"));
                Assert.That(store.HasKey(InputBindingOverrideStore.DefaultKey), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        private static InputActionAsset LoadActions()
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                PlayerInputContract.AssetPath);
            Assert.That(actions, Is.Not.Null);
            return actions;
        }

        private static void AssertBinding(InputActionMap map, string actionName, string expectedPath)
        {
            InputAction action = map.FindAction(actionName, true);
            Assert.That(action.bindings.Any(binding => binding.path == expectedPath), Is.True,
                $"{actionName} is missing {expectedPath}.");
        }

        private static int FindBinding(InputAction action, string path)
        {
            for (int index = 0; index < action.bindings.Count; index++)
            {
                if (string.Equals(action.bindings[index].path, path, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            throw new InvalidOperationException($"Binding not found: {path}");
        }

        private sealed class MemorySettingsStore : IInputBindingSettingsStore
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

            public bool HasKey(string key) => _values.ContainsKey(key);

            public string GetString(string key) => _values[key];

            public void SetString(string key, string value) => _values[key] = value;

            public void DeleteKey(string key) => _values.Remove(key);

            public void Save()
            {
            }
        }
    }
}
