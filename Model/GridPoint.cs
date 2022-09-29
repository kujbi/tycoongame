using System;
using System.Drawing;

namespace Model
{
    /// <summary>
    /// A parkban a pozíció reprezenzálása
    /// </summary>
    public readonly struct GridPoint
    {
        /// <summary>
        /// Az oszlop száma
        /// </summary>
        public int Column { get; init; }
        
        /// <summary>
        /// A sor száma
        /// </summary>
        public int Row { get; init; }

        /// <summary>
        /// Inicializál egy új GridPoint példányt egy Point példány alapján
        /// </summary>
        /// <param name="point">A pont ami alapján létrehozzuk a példányt</param>
        public GridPoint(Point point) : this(row: point.Y, column: point.X) { }

        /// <summary>
        /// Inicializál egy új GridPont példányt a megadott sor és oszlop szerint.
        /// </summary>
        /// <param name="row">A sor száma</param>
        /// <param name="column">Az oszlop száma</param>
        /// <exception cref="ArgumentOutOfRangeException">Ha -1-nél kisebb számot adunk meg sor vagy oszlopnak</exception>
        public GridPoint(int row, int column)
        {
            if(column < -1) throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            if(row < -1) throw new ArgumentOutOfRangeException(nameof(row), "row must be >= -1");
            
            Column = column;
            Row = row;
        }

        /// <summary>
        /// Megmondja hogy a GridPoint a pályán van-e
        /// </summary>
        /// <param name="fieldSize">A pálya mérete</param>
        /// <returns></returns>
        public bool IsInField(int fieldSize)
        {
            return 0 <= Row && Row < fieldSize && 0 <= Column && Column < fieldSize;
        }

        
        public override bool Equals(object? obj)
        {
            if (obj is GridPoint gridPoint)
            {
                return Column == gridPoint.Column && Row == gridPoint.Row;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(GridPoint left, GridPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridPoint left, GridPoint right)
        {
            return !(left == right);
        }

        public static GridPoint operator +(GridPoint left, GridPoint right)
        {
            return new GridPoint(row: left.Row + right.Row, column: left.Column + right.Column);
        }
        
        public override string ToString()
        {
            return $"({Row}, {Column})";
        }
    }
}
