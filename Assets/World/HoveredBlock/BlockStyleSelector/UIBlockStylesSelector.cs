using UnityEngine;
using System.Collections.Generic;
using World.Blocks;
using World.Chunks;
using World.Cameras;

namespace World.HoveredBlock.BlockStylesSelector
{
    public class UIBlockStyleSelector : MonoBehaviour
    {
        [SerializeField] private ChunksManager _chunksManager;
        [SerializeField] private HoveredBlockPicker _hoveredBlockPicker;
        [SerializeField] private GameObject _blockStylePrefab;
        [SerializeField] private CameraObserver _cameraObserver;

        private List<UIBlockStyles> _items = new();
        private WorldPosition _startedWorldPosition;

        public void StartSelecting(WorldPosition worldPosition)
        {
            DestroyUIs();

            Vector3 screenPos = _cameraObserver.Camera.WorldToScreenPoint(worldPosition.ToVector3Int());
            transform.position = screenPos;
            _startedWorldPosition = worldPosition;

            Block selectedBlock = _hoveredBlockPicker.SelectedBlock;
            BlockInfo selectedInfo = _chunksManager.BlockDatabase.Get(selectedBlock.Id);
            int index = 0;
            foreach (BlockPlacementVariant styles in selectedInfo.AvailablePlacements)
            {
                GameObject prefab = Instantiate(_blockStylePrefab, transform);
                UIBlockStyles uiStyles = prefab.GetComponent<UIBlockStyles>();
                uiStyles.Initialize(selectedInfo, styles, index);
                uiStyles.Centering(selectedInfo.AvailablePlacements.Length);
                _items.Add(uiStyles);
                index++;
            }

            gameObject.SetActive(true);
        }
        public BlockPlacementVariant? Select()
        {
            gameObject.SetActive(false);

            foreach (UIBlockStyles uiStyles in _items)
            {
                if (uiStyles.IsHovered)
                {
                    return uiStyles.Variant;
                }
            }

            return null;
        }

        public WorldPosition GetStartedWorldPosition()
        {
            return _startedWorldPosition;
        }

        private void DestroyUIs()
        {
            foreach (UIBlockStyles uiStyles in _items)
            {
                DestroyImmediate(uiStyles.gameObject);
            }

            _items.Clear();
        }
    }
}