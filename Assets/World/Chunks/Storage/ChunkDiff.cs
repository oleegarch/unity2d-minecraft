using System.Collections.Generic;
using UnityEngine;
using World.Blocks;
using World.Chunks;
using World.Chunks.Blocks.Storages;

namespace World.Storage
{
    public class ChunkDiff
    {
        public readonly ChunkIndex Index;
        public readonly byte Size;
        public readonly IBlockLayerStorage[] Layers;

        public ChunkDiff(ChunkIndex index, byte size)
        {
            Index = index;
            Size = size;

            Layers = new IBlockLayerStorage[]
            {
                new SparseBlockStorage(),  // BlockLayer.Main
                new SparseBlockStorage(),  // BlockLayer.Behind
                new SparseBlockStorage()   // BlockLayer.Front
            };
        }
    }
}