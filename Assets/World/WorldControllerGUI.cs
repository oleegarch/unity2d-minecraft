using UnityEngine;
using World.Cameras;
using World.Chunks;

namespace World
{
    public class WorldControllerGUI : MonoBehaviour
    {
        [SerializeField] private ChunksManager _chunksManager;
        [SerializeField] private CameraModeController _cameraModeController;
        [SerializeField] private WorldTime _worldTime;

        private int _popupIndex;
        private float _dayDurationSlider = 60f;
        private const float MinDayDuration = 1f;
        private const float MaxDayDuration = 900f;

        private void Start()
        {
            _popupIndex = (int)_cameraModeController.CameraMode;
            _dayDurationSlider = _worldTime.DayDuration;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 220));
            GUILayout.BeginVertical("box");
            GUILayout.Label("World Debug Controls", GUI.skin.label);

            // Chunk Manager Methods
            if (GUILayout.Button("Rerender All Chunks"))
            {
                _chunksManager.RerenderAll();
            }

            GUILayout.Space(10);

            // Game Mode Changing
            GUILayout.Label("Game Mode", GUI.skin.label);

            string[] modeNames = System.Enum.GetNames(typeof(CameraMode));
            Rect popupRect = GUILayoutUtility.GetRect(200, 20);
            int newIndex = GUI.SelectionGrid(popupRect, _popupIndex, modeNames, modeNames.Length);

            if (newIndex != _popupIndex)
            {
                _popupIndex = newIndex;
                CameraMode selectedMode = (CameraMode)_popupIndex;
                _cameraModeController.SetGameMode(selectedMode);
            }

            GUILayout.Space(10);

            // Day Duration Slider
            GUILayout.Label($"Day Duration: {_dayDurationSlider:F1} sec");
            float newSlider = GUILayout.HorizontalSlider(_dayDurationSlider, MinDayDuration, MaxDayDuration);

            if (Mathf.Abs(newSlider - _dayDurationSlider) > 0.01f)
            {
                _dayDurationSlider = newSlider;
                _worldTime.DayDuration = _dayDurationSlider;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}