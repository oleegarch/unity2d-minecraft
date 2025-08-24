using UnityEngine;
using World.Blocks;

namespace World.HoveredBlock.Particles
{
    public class BlockParticleSystemController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _breakingParticles;

        private ParticleSystemRenderer _breakingRenderer;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _breakingRenderer = _breakingParticles.GetComponent<ParticleSystemRenderer>();
            _mpb = new MaterialPropertyBlock();
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

            _breakingRenderer.GetPropertyBlock(_mpb);
            _mpb.SetTexture("_MainTex", texture);
            _breakingRenderer.SetPropertyBlock(_mpb);

            _breakingParticles.Play();
        }

        public void StopBlockBreaking()
        {
            _breakingParticles.Stop();
        }
    }
}