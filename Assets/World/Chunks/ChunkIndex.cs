using System;
using World.Blocks;

namespace World.Chunks
{
    public readonly struct ChunkIndex : IEquatable<ChunkIndex>
    {
        public readonly int x, y;

        public ChunkIndex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public WorldPosition ToWorldPosition(int chunkSize) => new WorldPosition(x * chunkSize, y * chunkSize);
        public WorldPosition ToWorldPosition(BlockIndex blockIndex, int chunkSize) 
            => new WorldPosition(x * chunkSize + blockIndex.x, y * chunkSize + blockIndex.y);

        public override string ToString() => $"ChunkIndex({x}, {y})";

        public bool Equals(ChunkIndex other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is ChunkIndex cc && Equals(cc);

        public override int GetHashCode() => (x, y).GetHashCode();

        public static bool operator ==(ChunkIndex a, ChunkIndex b) => a.Equals(b);
        public static bool operator !=(ChunkIndex a, ChunkIndex b) => !a.Equals(b);
    }
}