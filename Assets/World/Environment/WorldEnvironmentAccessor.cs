using System;
using System.Linq;
using R3;
using UnityEngine;
using World.Blocks;
using World.Blocks.Atlases;
using World.Chunks.Generator;
using World.Chunks.Storage;
using World.Entities;
using World.Items;

namespace World.Chunks
{
    public class WorldEnvironmentAccessor : MonoBehaviour
    {
        [SerializeField] private WorldEnvironment _environment;
        [SerializeField] private int _seed;

        public BehaviorSubject<bool> OnLoaded = new(false);
        public BehaviorSubject<string> OnWorldGeneratorChanged = new(null);

        public WorldEnvironment Environment => _environment;
        public BlockDatabase BlockDatabase => _environment.BlockDatabase;
        public BlockAtlasDatabase BlockAtlasDatabase => _environment.BlockAtlasDatabase;
        public ItemDatabase ItemDatabase => _environment.ItemDatabase;
        public ItemCategoryDatabase ItemCategoryDatabase => _environment.ItemCategoryDatabase;
        public EntityDatabase EntityDatabase => _environment.EntityDatabase;

        private string _currentWorldGeneratorName;
        private WorldGeneratorConfig _currentWorldGeneratorConfig;
        private IWorldGenerator _currentWorldGenerator;
        private IChunksStorage _currentChunksStorage;

        public WorldGeneratorConfig CurrentWorldGeneratorConfig => _currentWorldGeneratorConfig;
        public IWorldGenerator CurrentWorldGenerator => _currentWorldGenerator;
        public IChunksStorage CurrentChunksStorage => _currentChunksStorage;
        public string CurrentWorldGeneratorName
        {
            get
            {
                if (string.IsNullOrEmpty(_currentWorldGeneratorName))
                {
                    return _environment.DefaultWorldGeneratorName;
                }

                return _currentWorldGeneratorName;
            }
            set
            {
                if (!_environment.WorldGeneratorNames.Contains(value)) throw new OperationCanceledException($"WorldGenerator with name {value} not exists!");

                _currentWorldGeneratorName = value;
                _currentWorldGeneratorConfig = _environment.GetWorldGeneratorConfig(value);
                _currentWorldGenerator = _currentWorldGeneratorConfig.GetWorldGenerator(_environment, _seed);
                _currentChunksStorage = new ChunksStorage(_currentWorldGeneratorName, _seed);

                OnWorldGeneratorChanged.OnNext(_currentWorldGeneratorName);
            }
        }

        private void Awake()
        {
            CurrentWorldGeneratorName = _environment.DefaultWorldGeneratorName;
            
            OnLoaded.OnNext(true);
        }
        private void OnDestroy()
        {
            OnLoaded.Dispose();
            OnWorldGeneratorChanged.Dispose();
        }
    }
}