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
        private InputAction _rotatePlacement;
        private InputAction _pause;
        private InputAction _subscribedPrimaryAction;
        private InputAction _subscribedInteract;
        private bool _ownsRuntimeActions;
        private long _primaryPressVersion;
        private long _primaryConsumedVersion = -1;
        private long _interactPressVersion;
        private long _interactConsumedVersion = -1;

        public InputActionAsset Actions => actions;

        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;

        public Vector2 Look => _look?.ReadValue<Vector2>() ?? Vector2.zero;

        public bool SprintHeld => _sprint?.IsPressed() ?? false;

        public bool PrimaryActionPressedThisFrame =>
            (_primaryAction?.WasPressedThisFrame() ?? false) &&
            _primaryConsumedVersion != _primaryPressVersion;

        public bool InteractPressedThisFrame =>
            (_interact?.WasPressedThisFrame() ?? false) &&
            _interactConsumedVersion != _interactPressVersion;

        public bool DropPressedThisFrame => _drop?.WasPressedThisFrame() ?? false;

        public bool RotatePlacementPressedThisFrame =>
            _rotatePlacement?.WasPressedThisFrame() ?? false;

        public bool PausePressedThisFrame => _pause?.WasPressedThisFrame() ?? false;

        public bool IsPointerLook => _look?.activeControl?.device is Pointer;

        public string InteractBindingPrompt => GetBindingPrompt(_interact, "E", "A");

        public string DropBindingPrompt => GetBindingPrompt(_drop, "G", "B");

        public string RotatePlacementBindingPrompt =>
            GetBindingPrompt(_rotatePlacement, "R", "Right Shoulder");

        public string PrimaryBindingPrompt => GetBindingPrompt(_primaryAction, "Mouse Left", "RT");

        public bool TryConsumeInteractPressThisFrame()
        {
            if (!InteractPressedThisFrame)
            {
                return false;
            }

            _interactConsumedVersion = _interactPressVersion;
            return true;
        }

        public bool TryConsumePrimaryActionPressThisFrame()
        {
            if (!PrimaryActionPressedThisFrame)
            {
                return false;
            }

            _primaryConsumedVersion = _primaryPressVersion;
            return true;
        }

        public void Configure(InputActionAsset inputActions)
        {
            if (inputActions == null)
            {
                throw new ArgumentNullException(nameof(inputActions));
            }

            if (!ReferenceEquals(actions, inputActions) || !_ownsRuntimeActions)
            {
                ReplaceActions(inputActions, Application.isPlaying);
            }

            _primaryPressVersion = 0;
            _primaryConsumedVersion = -1;
            _interactPressVersion = 0;
            _interactConsumedVersion = -1;
            CacheActions();
        }

        private void Awake()
        {
            if (Application.isPlaying && actions != null)
            {
                ReplaceActions(actions, cloneForRuntime: true);
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
            SetPrimaryActionSubscription(null);
            SetInteractSubscription(null);
            if (_ownsRuntimeActions && actions != null)
            {
                Destroy(actions);
            }
        }

        private void ReplaceActions(InputActionAsset source, bool cloneForRuntime)
        {
            InputActionAsset previousActions = actions;
            bool destroyPreviousActions = _ownsRuntimeActions && previousActions != null;

            _playerMap?.Disable();
            SetPrimaryActionSubscription(null);
            SetInteractSubscription(null);
            _playerMap = null;
            _move = null;
            _look = null;
            _primaryAction = null;
            _interact = null;
            _sprint = null;
            _drop = null;
            _rotatePlacement = null;
            _pause = null;

            if (cloneForRuntime)
            {
                string sourceName = source.name;
                actions = Instantiate(source);
                actions.name = sourceName;
                _ownsRuntimeActions = true;
            }
            else
            {
                actions = source;
                _ownsRuntimeActions = false;
            }

            if (destroyPreviousActions && !ReferenceEquals(previousActions, actions))
            {
                Destroy(previousActions);
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
            SetPrimaryActionSubscription(_primaryAction);
            _interact = _playerMap.FindAction(PlayerInputContract.Interact, true);
            SetInteractSubscription(_interact);
            _sprint = _playerMap.FindAction(PlayerInputContract.Sprint, true);
            _drop = _playerMap.FindAction(PlayerInputContract.Drop, true);
            _rotatePlacement = _playerMap.FindAction(PlayerInputContract.RotatePlacement, true);
            _pause = _playerMap.FindAction(PlayerInputContract.Pause, true);
            if (isActiveAndEnabled)
            {
                _playerMap.Enable();
            }
        }

        private void SetInteractSubscription(InputAction interact)
        {
            if (ReferenceEquals(_subscribedInteract, interact))
            {
                return;
            }

            if (_subscribedInteract != null)
            {
                _subscribedInteract.performed -= OnInteractPerformed;
            }

            _subscribedInteract = interact;
            if (_subscribedInteract != null)
            {
                _subscribedInteract.performed += OnInteractPerformed;
            }
        }

        private void SetPrimaryActionSubscription(InputAction primaryAction)
        {
            if (ReferenceEquals(_subscribedPrimaryAction, primaryAction))
            {
                return;
            }

            if (_subscribedPrimaryAction != null)
            {
                _subscribedPrimaryAction.performed -= OnPrimaryActionPerformed;
            }

            _subscribedPrimaryAction = primaryAction;
            if (_subscribedPrimaryAction != null)
            {
                _subscribedPrimaryAction.performed += OnPrimaryActionPerformed;
            }
        }

        private void OnPrimaryActionPerformed(InputAction.CallbackContext context)
        {
            if (_primaryPressVersion == long.MaxValue)
            {
                _primaryPressVersion = 0;
                _primaryConsumedVersion = -1;
                return;
            }

            _primaryPressVersion++;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (_interactPressVersion == long.MaxValue)
            {
                _interactPressVersion = 0;
                _interactConsumedVersion = -1;
                return;
            }

            _interactPressVersion++;
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
