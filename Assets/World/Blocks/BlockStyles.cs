namespace World.Blocks
{
    public enum BlockLayer : byte
    {
        Main,
        Behind,
        Front
    }
    public struct BlockStyles
    {
        public bool IsBehind;
        public bool HasCollider;

        public static BlockStyles ForMain = new BlockStyles { IsBehind = false, HasCollider = true };
        public static BlockStyles ForBehind = new BlockStyles { IsBehind = true, HasCollider = false };
        public static BlockStyles ForFront = new BlockStyles { IsBehind = false, HasCollider = false };

        public static BlockStyles BehindLikeMain = new BlockStyles { IsBehind = false, HasCollider = false };

        public static BlockStyles[] ByLayer =
        {
            ForMain,
            ForBehind,
            ForFront
        };

        public override bool Equals(object obj)
        {
            if (obj is BlockStyles other)
            {
                return IsBehind == other.IsBehind && HasCollider == other.HasCollider;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return (IsBehind ? 1 : 0) | ((HasCollider ? 1 : 0) << 1);
        }

        public static bool operator ==(BlockStyles left, BlockStyles right) => left.Equals(right);
        public static bool operator !=(BlockStyles left, BlockStyles right) => !left.Equals(right);
    }
}