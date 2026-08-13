using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PCShopEmpire3D.Presentation.Input
{
    /// <summary>
    /// Caches the Input System contract once and exposes frame values to gameplay presentation.
    /// </summary>
    public sealed class PlayerInputAdapter : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;

        private InputActionMap _playerMap;
        private InputAction _move;
        private InputAction _look;
        private InputAction _primaryAction;
        private InputAction _interact;
        private InputAction _sprint;
        private InputAction _drop;
        private InputAction _pause;
        private bool _ownsRuntimeActions;

        public InputActionAsset Actions => actions;

        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;

        public Vector2 Look => _look?.ReadValue<Vector2>() ?? Vector2.zero;

        public bool SprintHeld => _sprint?.IsPressed() ?? false;

        public bool PrimaryActionPressedThisFrame => _primaryAction?.WasPressedThisFrame() ?? false;

        public bool InteractPressedThisFrame => _interact?.WasPressedThisFrame() ?? false;

        public bool DropPressedThisFrame => _drop?.WasPressedThisFrame() ?? false;

        public bool PausePressedThisFrame => _pause?.WasPressedThisFrame() ?? false;

        public bool IsPointerLook => _look?.activeControl?.device is Pointer;

        public string InteractBindingPrompt => GetBindingPrompt(_interact, "E", "A");

        public string DropBindingPrompt => GetBindingPrompt(_drop, "G", "B");

        public void Configure(InputActionAsset inputActions)
        {
            actions = inputActions != null
                ? inputActions
                : throw new ArgumentNullException(nameof(inputActions));
            CacheActions();
        }

        private void Awake()
        {
            if (Application.isPlaying && actions != null)
            {
                string sourceName = actions.name;
                actions = Instantiate(actions);
                actions.name = sourceName;
                _ownsRuntimeActions = true;
            }

            CacheActions();
        }

        private void OnEnable()
        {
            CacheActions();
            _playerMap?.Enable();
        }

        private void OnDisable()
        {
            _playerMap?.Disable();
        }

        private void OnDestroy()
        {
            if (_ownsRuntimeActions && actions != null)
            {
                Destroy(actions);
            }
        }

        private void CacheActions()
        {
            if (actions == null)
            {
                return;
            }

            _playerMap = actions.FindActionMap(PlayerInputContract.PlayerMap, true);
            _move = _playerMap.FindAction(PlayerInputContract.Move, true);
            _look = _playerMap.FindAction(PlayerInputContract.Look, true);
            _primaryAction = _playerMap.FindAction(PlayerInputContract.PrimaryAction, true);
            _interact = _playerMap.FindAction(PlayerInputContract.Interact, true);
            _sprint = _playerMap.FindAction(PlayerInputContract.Sprint, true);
            _drop = _playerMap.FindAction(PlayerInputContract.Drop, true);
            _pause = _playerMap.FindAction(PlayerInputContract.Pause, true);
            if (isActiveAndEnabled)
            {
                _playerMap.Enable();
            }
        }

        private static string GetBindingPrompt(
            InputAction action,
            string keyboardFallback,
            string gamepadFallback)
        {
            if (action == null)
            {
                return $"{keyboardFallback} / {gamepadFallback}";
            }

            string keyboard = null;
            string gamepad = null;
            for (int index = 0; index < action.bindings.Count; index++)
            {
                InputBinding binding = action.bindings[index];
                if (binding.isComposite || binding.isPartOfComposite || string.IsNullOrEmpty(binding.effectivePath))
                {
                    continue;
                }

                string display = action.GetBindingDisplayString(index);
                if (binding.groups?.Contains(PlayerInputContract.KeyboardAndMouseScheme) == true)
                {
                    keyboard ??= display;
                }
                else if (binding.groups?.Contains(PlayerInputContract.GamepadScheme) == true)
                {
                    gamepad ??= display;
                }
            }

            return $"{keyboard ?? keyboardFallback} / {gamepad ?? gamepadFallback}";
        }
    }
}
