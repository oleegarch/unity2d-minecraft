using UnityEngine;
using World.Blocks;
using World.Chunks;

namespace World.Entities.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private WorldChunksCreator _worldChunksCreator;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Rigidbody2D _playerRb2;
        [SerializeField] private GameObject _playerGO;

        private void Start()
        {
            _playerRb2.simulated = false;
            _playerGO.SetActive(false);
            _worldChunksCreator.OnVisibleChunksLoaded += SpawnPlayer;
        }
        private void OnDestroy()
        {
            _worldChunksCreator.OnVisibleChunksLoaded -= SpawnPlayer;
        }

        private void SpawnPlayer()
        {
            int y = 0;
            while (true)
            {
                Block block = _worldManager.Blocks.Get(new WorldPosition(0, y));

                if (block.IsAir)
                {
                    _playerTransform.position = new Vector3(0, y, 0);
                    _playerRb2.simulated = true;
                    _playerGO.SetActive(true);
                    break;
                }

                y++;
            }
        }
    }
}