using System;

namespace World.Inventories
{
    public sealed class ChestInventory : Inventory
    {
        public int Columns { get; }
        public int Rows { get; }

        public ChestInventory(int columns, int rows) : base(columns * rows)
        {
            if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
            if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
            Columns = columns;
            Rows = rows;
        }

        public int IndexAt(int column, int row)
        {
            if (column < 0 || column >= Columns) throw new ArgumentOutOfRangeException(nameof(column));
            if (row < 0 || row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
            return row * Columns + column;
        }
    }
}