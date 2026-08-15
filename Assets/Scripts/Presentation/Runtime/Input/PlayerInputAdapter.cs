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
        private InputAction _subscribedDrop;
        private InputAction _subscribedRotatePlacement;
        private InputActionMap _subscribedDeviceMap;
        private bool _ownsRuntimeActions;
        private bool _usesGamepadPrompts;
        private long _primaryPressVersion;
        private long _primaryConsumedVersion = -1;
        private long _interactPressVersion;
        private long _interactConsumedVersion = -1;
        private long _dropPressVersion;
        private long _dropConsumedVersion = -1;
        private long _rotatePressVersion;
        private long _rotateConsumedVersion = -1;

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

        public bool DropPressedThisFrame =>
            (_drop?.WasPressedThisFrame() ?? false) &&
            _dropConsumedVersion != _dropPressVersion;

        public bool RotatePlacementPressedThisFrame =>
            (_rotatePlacement?.WasPressedThisFrame() ?? false) &&
            _rotateConsumedVersion != _rotatePressVersion;

        public bool PausePressedThisFrame => _pause?.WasPressedThisFrame() ?? false;

        public bool IsPointerLook => _look?.activeControl?.device is Pointer;

        public bool UsesGamepadPrompts => _usesGamepadPrompts;

        public string InteractBindingPrompt => GetBindingPrompt(_interact, "E", "A");

        public string DropBindingPrompt => GetBindingPrompt(_drop, "G", "B");

        public string RotatePlacementBindingPrompt =>
            GetBindingPrompt(_rotatePlacement, "R", "RB");

        public string PrimaryBindingPrompt => GetBindingPrompt(_primaryAction, "LMB", "RT");

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

        public bool TryConsumeDropPressThisFrame()
        {
            if (!DropPressedThisFrame)
            {
                return false;
            }

            _dropConsumedVersion = _dropPressVersion;
            return true;
        }

        public bool TryConsumeRotatePlacementPressThisFrame()
        {
            if (!RotatePlacementPressedThisFrame)
            {
                return false;
            }

            _rotateConsumedVersion = _rotatePressVersion;
            return true;
        }

        public void DrainGameplayPressesThisFrame()
        {
            TryConsumePrimaryActionPressThisFrame();
            TryConsumeInteractPressThisFrame();
            TryConsumeDropPressThisFrame();
            TryConsumeRotatePlacementPressThisFrame();
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
            _dropPressVersion = 0;
            _dropConsumedVersion = -1;
            _rotatePressVersion = 0;
            _rotateConsumedVersion = -1;
            _usesGamepadPrompts = false;
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
            SetDeviceMapSubscription(null);
            SetPrimaryActionSubscription(null);
            SetInteractSubscription(null);
            SetDropSubscription(null);
            SetRotatePlacementSubscription(null);
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
            SetDeviceMapSubscription(null);
            SetPrimaryActionSubscription(null);
            SetInteractSubscription(null);
            SetDropSubscription(null);
            SetRotatePlacementSubscription(null);
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
            SetDeviceMapSubscription(_playerMap);
            _move = _playerMap.FindAction(PlayerInputContract.Move, true);
            _look = _playerMap.FindAction(PlayerInputContract.Look, true);
            _primaryAction = _playerMap.FindAction(PlayerInputContract.PrimaryAction, true);
            SetPrimaryActionSubscription(_primaryAction);
            _interact = _playerMap.FindAction(PlayerInputContract.Interact, true);
            SetInteractSubscription(_interact);
            _sprint = _playerMap.FindAction(PlayerInputContract.Sprint, true);
            _drop = _playerMap.FindAction(PlayerInputContract.Drop, true);
            SetDropSubscription(_drop);
            _rotatePlacement = _playerMap.FindAction(PlayerInputContract.RotatePlacement, true);
            SetRotatePlacementSubscription(_rotatePlacement);
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

        private void SetDropSubscription(InputAction dropAction)
        {
            if (ReferenceEquals(_subscribedDrop, dropAction))
            {
                return;
            }

            if (_subscribedDrop != null)
            {
                _subscribedDrop.performed -= OnDropPerformed;
            }

            _subscribedDrop = dropAction;
            if (_subscribedDrop != null)
            {
                _subscribedDrop.performed += OnDropPerformed;
            }
        }

        private void SetRotatePlacementSubscription(InputAction rotateAction)
        {
            if (ReferenceEquals(_subscribedRotatePlacement, rotateAction))
            {
                return;
            }

            if (_subscribedRotatePlacement != null)
            {
                _subscribedRotatePlacement.performed -= OnRotatePlacementPerformed;
            }

            _subscribedRotatePlacement = rotateAction;
            if (_subscribedRotatePlacement != null)
            {
                _subscribedRotatePlacement.performed += OnRotatePlacementPerformed;
            }
        }

        private void SetDeviceMapSubscription(InputActionMap playerMap)
        {
            if (ReferenceEquals(_subscribedDeviceMap, playerMap))
            {
                return;
            }

            if (_subscribedDeviceMap != null)
            {
                _subscribedDeviceMap.actionTriggered -= OnPlayerActionTriggered;
            }

            _subscribedDeviceMap = playerMap;
            if (_subscribedDeviceMap != null)
            {
                _subscribedDeviceMap.actionTriggered += OnPlayerActionTriggered;
            }
        }

        private void OnPlayerActionTriggered(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed || context.control == null)
            {
                return;
            }

            if (context.control.device is Gamepad)
            {
                _usesGamepadPrompts = true;
            }
            else if (context.control.device is Keyboard || context.control.device is Mouse)
            {
                _usesGamepadPrompts = false;
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

        private void OnDropPerformed(InputAction.CallbackContext context)
        {
            if (_dropPressVersion == long.MaxValue)
            {
                _dropPressVersion = 0;
                _dropConsumedVersion = -1;
                return;
            }

            _dropPressVersion++;
        }

        private void OnRotatePlacementPerformed(InputAction.CallbackContext context)
        {
            if (_rotatePressVersion == long.MaxValue)
            {
                _rotatePressVersion = 0;
                _rotateConsumedVersion = -1;
                return;
            }

            _rotatePressVersion++;
        }

        private string GetBindingPrompt(
            InputAction action,
            string keyboardFallback,
            string gamepadFallback)
        {
            string fallback = _usesGamepadPrompts
                ? gamepadFallback
                : keyboardFallback;
            if (action == null)
            {
                return fallback;
            }

            string bindingGroup = _usesGamepadPrompts
                ? PlayerInputContract.GamepadScheme
                : PlayerInputContract.KeyboardAndMouseScheme;
            for (int index = 0; index < action.bindings.Count; index++)
            {
                InputBinding binding = action.bindings[index];
                if (binding.isComposite ||
                    binding.isPartOfComposite ||
                    string.IsNullOrEmpty(binding.effectivePath) ||
                    binding.groups?.Contains(bindingGroup) != true)
                {
                    continue;
                }

                string display = action.GetBindingDisplayString(index);
                if (!string.IsNullOrWhiteSpace(display))
                {
                    return display;
                }
            }

            return fallback;
        }
    }
}
