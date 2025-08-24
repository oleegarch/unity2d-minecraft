using UnityEngine;
using World.Blocks;
using World.Chunks;

namespace World.Entities.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Rigidbody2D _playerRb2;
        [SerializeField] private SpriteRenderer _playerSprite;

        private void Start()
        {
            _playerSprite.enabled = false;
            _playerRb2.simulated = false;
            _worldManager.OnVisibleChunksLoaded += SpawnPlayer;
        }
        private void OnDestroy()
        {
            _worldManager.OnVisibleChunksLoaded -= SpawnPlayer;
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
                    _playerSprite.enabled = true;
                    _playerRb2.simulated = true;
                    break;
                }

                y++;
            }
        }
    }
}