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
    }
}