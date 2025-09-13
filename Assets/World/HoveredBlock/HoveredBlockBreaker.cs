using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using World.Blocks;
using World.Cameras;
using World.InputActions;
using World.Entities.Player;
using World.Chunks;
using World.HoveredBlock.Particles;

namespace World.HoveredBlock
{
    public class HoveredBlockBreaker : MonoBehaviour
    {
        [SerializeField] private CameraModeController _cameraModeController;
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private HoveredBlockObserver _blockHoveredObserver;
        [SerializeField] private SpriteRenderer _targetSpriteRenderer;
        [SerializeField] private Transform _breakingMask;
        [SerializeField] private BlockParticleSystemController _particlesController;
        [SerializeField] private PlayerAnimator _localPlayerAnimator;

        [Header("Settings")]
        [SerializeField] private float _cameraDistanceChangedForCancelBreaking = 0.5f;
        [SerializeField] private float _breakingHoldBefore = 0.2f;
        [SerializeField] private float _breakingMaskFinalScale = 1.35f;

        private Vector3 _startBreakingCameraPosition;
        private Coroutine _currentBreakingCoroutine;
        private bool _pointerPressing = false;

        public event Action<WorldPosition> OnBlockBreakAttempt;

        private void Awake()
        {
            _breakingMask.localScale = Vector3.zero;
        }
        private void OnEnable()
        {
            var actions = _inputManager.Controls.BlockBreaking;
            actions.PointerPress.performed += HandlePointerPress;
            actions.PointerPress.canceled += HandlePointerPress;
            actions.Enable();
            
            _cameraModeController.OnCameraModeChanged += HandleCameraModeChanged;
            _blockHoveredObserver.OnBlockHoveredChanged += HandleBlockHoveredChanged;
            _blockHoveredObserver.OnLimitedChanged += HandleLimitedChanged;

            // подписываемся на изменение позиции камеры только когда режим камеры включён на наблюдателя
            // так как в режиме наблюдателя нам требуется отменять процесс ломания блока при передвижении камеры
            // событиями движения курсором или тачами пользователя
            if (_cameraModeController.CameraMode == CameraMode.Spectator)
            {
                _cameraObserver.OnPositionChanged += HandleCameraPositionChanged;
            }
        }

        private void OnDisable()
        {
            var actions = _inputManager.Controls.BlockBreaking;
            actions.PointerPress.performed -= HandlePointerPress;
            actions.PointerPress.canceled -= HandlePointerPress;
            actions.Disable();

            _cameraModeController.OnCameraModeChanged -= HandleCameraModeChanged;
            _blockHoveredObserver.OnBlockHoveredChanged -= HandleBlockHoveredChanged;
            _blockHoveredObserver.OnLimitedChanged -= HandleLimitedChanged;
            _cameraObserver.OnPositionChanged -= HandleCameraPositionChanged;
        }

        private void HandleCameraModeChanged(CameraMode cameraMode)
        {
            _cameraObserver.OnPositionChanged -= HandleCameraPositionChanged;

            if (cameraMode == CameraMode.Spectator)
            {
                _cameraObserver.OnPositionChanged += HandleCameraPositionChanged;
            }
        }
        private void HandlePointerPress(InputAction.CallbackContext context)
        {
            _pointerPressing = context.performed;

            RestartBreaking();
        }
        private void HandleBlockHoveredChanged(WorldPosition worldPosition)
        {
            RestartBreaking();
        }
        private void HandleLimitedChanged(bool limited)
        {
            RestartBreaking();
        }
        private void HandleCameraPositionChanged(Vector3 newPosition)
        {
            float distance = Vector3.Distance(newPosition, _startBreakingCameraPosition);

            if (distance > _cameraDistanceChangedForCancelBreaking)
            {
                CancelBreaking();
            }
        }

        private void StartBreaking(WorldPosition worldPosition)
        {
            if (_currentBreakingCoroutine != null) return;

            _currentBreakingCoroutine = StartCoroutine(BreakingProcess(worldPosition));
        }
        private void CancelBreaking()
        {
            if (_currentBreakingCoroutine == null) return;

            StopCoroutine(_currentBreakingCoroutine);
            _currentBreakingCoroutine = null;
            _breakingMask.localScale = Vector3.zero;
            _particlesController.StopBlockBreaking();
            _localPlayerAnimator.StopBlockBreaking();
        }
        private void RestartBreaking()
        {
            CancelBreaking();

            if (!_blockHoveredObserver.ReachedLimitPosition && _pointerPressing)
            {
                StartBreaking(_blockHoveredObserver.HoveredPosition);
            }
        }

        private IEnumerator BreakingProcess(WorldPosition worldPosition)
        {
            _breakingMask.localScale = Vector3.zero;
            _startBreakingCameraPosition = _cameraObserver.GetPosition();

            float holdPassed = 0f;
            while (_breakingHoldBefore > holdPassed)
            {
                yield return null;

                holdPassed += Time.deltaTime;
            }

            Block hoveredBlock = _worldManager.Blocks.GetBreakable(worldPosition, out BlockLayer blockLayer);
            if (hoveredBlock.IsAir) yield break;

            BlockInfo hoveredInfo = _environment.BlockDatabase.Get(hoveredBlock.Id);

            _targetSpriteRenderer.color = hoveredInfo.OutlineColor;
            _particlesController.AnimateBlockBreaking(worldPosition, hoveredInfo);
            _localPlayerAnimator.AnimateBlockBreaking(worldPosition);

            float hardness = hoveredInfo.Hardness;
            float passed = 0f;
            while (passed < hardness)
            {
                yield return null;

                passed += Time.deltaTime;

                float progress = passed / hardness;
                float currentScale = progress * _breakingMaskFinalScale;

                _breakingMask.localScale = new Vector3(currentScale, currentScale, 1f);
            }

            OnBlockBreakAttempt?.Invoke(worldPosition);

            CancelBreaking();
        }
    }
}