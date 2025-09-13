using UnityEngine;
using World.Blocks;
using World.Cameras;
using World.Chunks;
using World.HoveredBlock;

namespace World
{
    public class WorldControllerGUI : MonoBehaviour
    {
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private WorldChunksCreator _worldChunksCreator;
        [SerializeField] private WorldModeController _worldModeController;
        [SerializeField] private CameraModeController _cameraModeController;
        [SerializeField] private WorldTime _worldTime;
        [SerializeField] private HoveredBlockPicker _blockPicker;

        private int _cameraModePopupIndex;
        private int _worldModePopupIndex;
        private float _dayDurationSlider = 60f;
        private const float MinDayDuration = 1f;
        private const float MaxDayDuration = 900f;

        private void Start()
        {
            _cameraModePopupIndex = (int)_cameraModeController.CameraMode;
            _worldModePopupIndex = (int)_worldModeController.WorldMode;
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
                _worldChunksCreator.RerenderAll();
            }

            GUILayout.Space(10);

            // Camera Mode Changing
            GUILayout.Label("Camera Mode", GUI.skin.label);

            string[] cameraModeNames = System.Enum.GetNames(typeof(CameraMode));
            Rect cameraModePopupRect = GUILayoutUtility.GetRect(200, 20);
            int cameraModeNewIndex = GUI.SelectionGrid(cameraModePopupRect, _cameraModePopupIndex, cameraModeNames, cameraModeNames.Length);

            if (cameraModeNewIndex != _cameraModePopupIndex)
            {
                _cameraModePopupIndex = cameraModeNewIndex;
                _cameraModeController.SetCameraMode((CameraMode)_cameraModePopupIndex);
            }

            GUILayout.Space(10);

            // World Mode Changing
            GUILayout.Label("World Mode", GUI.skin.label);

            string[] worldModeNames = System.Enum.GetNames(typeof(WorldMode));
            Rect worldModePopupRect = GUILayoutUtility.GetRect(200, 20);
            int worldModeNewIndex = GUI.SelectionGrid(worldModePopupRect, _worldModePopupIndex, worldModeNames, worldModeNames.Length);

            if (worldModeNewIndex != _worldModePopupIndex)
            {
                _worldModePopupIndex = worldModeNewIndex;
                _worldModeController.SetWorldMode((WorldMode)_worldModePopupIndex);
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

            GUILayout.Space(10);

            // Current picked block
            ushort blockId = _blockPicker.SelectedBlock.Id;
            BlockLayer blockLayer = _blockPicker.SelectedLayer;
            GUILayout.Label($"Picked block: {_environment.BlockDatabase.Get(blockId).Name}(Id:{blockId},Layer:{blockLayer})");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}