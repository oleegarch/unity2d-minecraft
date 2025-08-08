using UnityEngine;
using UnityEngine.Rendering.Universal;
using World.Cameras;

namespace World.Background
{
    public class SkyController : MonoBehaviour
    {
        [SerializeField] private WorldController _worldController;
        [SerializeField] private WorldTime _worldTime;
        [SerializeField] private Transform _sun;
        [SerializeField] private Transform _moon;
        [SerializeField] private SpriteRenderer _sunRenderer;
        [SerializeField] private SpriteRenderer _moonRenderer;
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private Light2D _globalLight;
        [SerializeField] private Gradient _backgroundColorOverTime;
        [SerializeField] private Gradient _lightColorOverTime;
        [SerializeField] private float _celestialPixelSize = 256f;

        private Vector3 _pivotPosition;

        private void Awake()
        {
            SetPivotPosition(_cameraObserver.GetPosition());
            SetScaleByOrthoSize(_cameraObserver.GetSize());
        }
        private void OnEnable()
        {
            _worldController.OnWorldInited += OnWorldInited;
            _worldController.OnWorldDestroyed += OnWorldDestroyed;
            _worldTime.OnTimeChanged += OnTimeChanged;
        }
        private void OnDisable()
        {
            _worldController.OnWorldInited -= OnWorldInited;
            _worldController.OnWorldDestroyed -= OnWorldDestroyed;
            _worldTime.OnTimeChanged -= OnTimeChanged;
        }
        private void OnWorldInited()
        {
            _cameraObserver.OnPositionChanged += OnCameraPositionChanged;
            _cameraObserver.OnOrthographicSizeChanged += OnCameraSizeChanged;
        }
        private void OnWorldDestroyed()
        {
            _cameraObserver.OnPositionChanged -= OnCameraPositionChanged;
            _cameraObserver.OnOrthographicSizeChanged -= OnCameraSizeChanged;
        }

        private void OnTimeChanged(float dayRatio)
        {
            // вращение солнца и луны
            float angle = dayRatio * Mathf.PI * 2f;
            Vector3 sunOffset = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0f) * _cameraObserver.GetOrthographicWidth() * 1.5f;
            Vector3 moonOffset = new Vector3(-Mathf.Sin(angle + Mathf.PI), Mathf.Cos(angle + Mathf.PI), 0f) * _cameraObserver.GetOrthographicWidth() * 1.5f;
            _sun.position = _pivotPosition + sunOffset;
            _moon.position = _pivotPosition + moonOffset;

            // Плавное исчезновение под горизонтом
            float sunAlpha = Mathf.Clamp01(sunOffset.y);
            float moonAlpha = Mathf.Clamp01(moonOffset.y);
            SetAlpha(_sunRenderer, sunAlpha);
            SetAlpha(_moonRenderer, moonAlpha);

            // Цвет неба и глобального света
            float evalTime = Mathf.Sin(dayRatio * Mathf.PI);
            _cameraObserver.SetBackgroundColor(_backgroundColorOverTime.Evaluate(evalTime));
            _globalLight.color = _lightColorOverTime.Evaluate(evalTime);
        }

        private void OnCameraPositionChanged(Vector3 newPosition)
        {
            SetPivotPosition(newPosition);
        }
        private void OnCameraSizeChanged(float orthoSize)
        {
            SetScaleByOrthoSize(orthoSize);
        }

        private void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
        private void SetScaleByOrthoSize(float orthoSize)
        {
            float pixelsPerUnit = _sunRenderer.sprite.pixelsPerUnit;
            float screenHeight = Screen.height;
            float unitsPerPixel = orthoSize * 2f / screenHeight;
            float worldSize = _celestialPixelSize * unitsPerPixel;
            float scale = worldSize / (_sunRenderer.sprite.rect.height / pixelsPerUnit);
            _sun.localScale = Vector3.one * scale;
            _moon.localScale = Vector3.one * scale;
        }
        private void SetPivotPosition(Vector3 position)
        {
            position.z = 0f;
            _pivotPosition = position;
        }
    }
}