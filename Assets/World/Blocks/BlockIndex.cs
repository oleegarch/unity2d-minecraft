using System;
using World.Chunks;

namespace World.Blocks
{
    public readonly struct BlockIndex : IEquatable<BlockIndex>
    {
        public readonly byte x, y;

        public BlockIndex(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }

        public WorldPosition ToWorldPosition(ChunkIndex chunkIndex, byte chunkSize)
            => new WorldPosition(chunkIndex.x * chunkSize + x,
                              chunkIndex.y * chunkSize + y);

        public override string ToString() => $"BlockIndex({x}, {y})";

        public bool Equals(BlockIndex other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is BlockIndex bc && Equals(bc);

        public override int GetHashCode() => (x, y).GetHashCode();

        public static bool operator ==(BlockIndex a, BlockIndex b) => a.Equals(b);
        public static bool operator !=(BlockIndex a, BlockIndex b) => !a.Equals(b);

        public static BlockIndex Zero = new BlockIndex(0, 0);
    }
}