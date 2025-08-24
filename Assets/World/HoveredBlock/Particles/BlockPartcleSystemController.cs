using UnityEngine;
using World.Blocks;

namespace World.HoveredBlock.Particles
{
    public class BlockParticleSystemController : MonoBehaviour
    {
        [SerializeField] private Material _instanceMaterial;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private ParticleSystemRenderer _psRenderer;

        private void Awake()
        {
            Stop();
        }

        // Применить текстуру частиц из BlockInfo к этой ParticleSystem.
        public void AnimateBlockBreaking(WorldPosition position, BlockInfo block)
        {
            transform.position = position.ToVector3Int();

            Texture2D texture = block.ParticleTexture;

            if (texture == null)
            {
                Debug.LogWarning($"BlockParticleSystemController: no particle texture or sprite texture available for block '{block.Name}'");
                return;
            }

            _instanceMaterial.mainTexture = texture;
            _psRenderer.material = _instanceMaterial;
            _particleSystem.Play();
        }
        public void Stop()
        {
            _particleSystem.Stop();
        }
    }
}