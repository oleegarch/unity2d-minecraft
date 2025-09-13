using System;

namespace World.Rules
{
    public class WorldGlobalRules
    {
        // глобальное правило для определения: можем ли мы ломать блок позади на этих мировых координатам
        public Func<WorldPosition, bool> CanBreakBehindBlock;

        public void SetCanBreakBehindBlock(Func<WorldPosition, bool> func)
        {
            CanBreakBehindBlock = func;
        }
    }
}