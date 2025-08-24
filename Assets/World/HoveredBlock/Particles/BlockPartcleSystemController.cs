using UnityEngine;
using World.Blocks;

namespace World.HoveredBlock.Particles
{
    public class BlockParticleSystemController : MonoBehaviour
    {
        [SerializeField] private Material _instanceMaterial;
        [SerializeField] private ParticleSystem _breakingParticles;

        private ParticleSystemRenderer _breakingRenderer;

        private void Awake()
        {
            _breakingRenderer = _breakingParticles.GetComponent<ParticleSystemRenderer>();
        }
        private void Start()
        {
            StopBlockBreaking();
        }

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
            _breakingRenderer.material = _instanceMaterial;
            _breakingParticles.Play();
        }
        public void StopBlockBreaking()
        {
            _breakingParticles.Stop();
        }
    }
}