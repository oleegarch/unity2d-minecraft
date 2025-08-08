using System.Collections;
using UnityEngine;
using World.Blocks;
using World.Chunks;

namespace World.Entities.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private ChunksManager _chunksManager;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Coroutine _waitCoroutine;

        private void Start()
        {
            _waitCoroutine = StartCoroutine(WaitForSpawnPlayer());
            _spriteRenderer.enabled = false;
        }
        private void OnDestroy()
        {
            if (_waitCoroutine != null)
                StopCoroutine(_waitCoroutine);
        }

        private IEnumerator WaitForSpawnPlayer()
        {
            yield return new WaitUntil(() => _chunksManager.Loaded);
            SpawnPlayer();
        }
        private void SpawnPlayer()
        {
            int y = 0;
            while (true)
            {
                Block block = _chunksManager.Block.Get(new WorldPosition(0, y));

                if (block.IsAir())
                {
                    _playerTransform.position = new Vector3(0, y, 0);
                    _spriteRenderer.enabled = true;
                    break;
                }

                y++;
            }
        }
    }
}