using System;
using UnityEngine;
using World.Blocks;
using World.Chunks;

namespace World
{
    public readonly struct WorldPosition : IEquatable<WorldPosition>
    {
        public readonly int x, y;

        public WorldPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Корректный модуль для отрицательных координат
        private static int Mod(int a, int b) => (a % b + b) % b;

        // Получить координаты чанка
        public ChunkIndex ToChunkIndex(int chunkSize) =>
            new ChunkIndex(Mathf.FloorToInt((float)x / chunkSize), Mathf.FloorToInt((float)y / chunkSize));

        // Получить координаты блока внутри чанка
        public BlockIndex ToBlockIndex(int chunkSize)
        {
            int localX = Mod(x, chunkSize);
            int localY = Mod(y, chunkSize);

            // Проверка безопасности при приведении к byte
            if (localX < 0 || localX > 255 || localY < 0 || localY > 255)
                throw new ArgumentOutOfRangeException($"BlockIndex out of byte range: ({localX}, {localY})");

            return new BlockIndex((byte)localX, (byte)localY);
        }

        public override string ToString() => $"WorldPosition({x}, {y})";

        public bool Equals(WorldPosition other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is WorldPosition cc && Equals(cc);

        public override int GetHashCode() => HashCode.Combine(x, y);

        public static bool operator ==(WorldPosition a, WorldPosition b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(WorldPosition a, WorldPosition b) => !(a == b);
    }
}