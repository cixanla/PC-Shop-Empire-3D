using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PCShopEmpire3D.Presentation.Input
{
    public interface IInputBindingSettingsStore
    {
        bool HasKey(string key);

        string GetString(string key);

        void SetString(string key, string value);

        void DeleteKey(string key);

        void Save();
    }

    /// <summary>
    /// Keeps rebind preferences separate from simulation save data.
    /// </summary>
    public static class InputBindingOverrideStore
    {
        public const string DefaultKey = "pse.input.binding-overrides.v1";

        public static void Save(
            InputActionAsset actions,
            IInputBindingSettingsStore store,
            string key = DefaultKey)
        {
            Require(actions, store, key);
            store.SetString(key, actions.SaveBindingOverridesAsJson());
            store.Save();
        }

        public static bool Load(
            InputActionAsset actions,
            IInputBindingSettingsStore store,
            string key = DefaultKey)
        {
            Require(actions, store, key);
            if (!store.HasKey(key))
            {
                return false;
            }

            actions.LoadBindingOverridesFromJson(store.GetString(key), true);
            return true;
        }

        public static void Reset(
            InputActionAsset actions,
            IInputBindingSettingsStore store,
            string key = DefaultKey)
        {
            Require(actions, store, key);
            actions.RemoveAllBindingOverrides();
            store.DeleteKey(key);
            store.Save();
        }

        private static void Require(
            InputActionAsset actions,
            IInputBindingSettingsStore store,
            string key)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A binding settings key is required.", nameof(key));
            }
        }
    }

    public sealed class PlayerPrefsInputBindingSettingsStore : IInputBindingSettingsStore
    {
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public string GetString(string key) => PlayerPrefs.GetString(key);

        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);

        public void Save() => PlayerPrefs.Save();
    }
}
