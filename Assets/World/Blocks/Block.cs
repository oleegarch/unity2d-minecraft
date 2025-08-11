namespace World.Blocks
{
    public readonly struct Block
    {
        public static readonly ushort AirId = 0;
        public static readonly Block Air = new Block(AirId);

        public readonly ushort Id;

        public Block(ushort id)
        {
            Id = id;
        }

        public readonly bool IsAir => Id == AirId;

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object obj) => obj is Block other && Id == other.Id;
        
        public static bool operator ==(Block a, Block b) => a.Id == b.Id;
        public static bool operator !=(Block a, Block b) => a.Id != b.Id;
    }
}