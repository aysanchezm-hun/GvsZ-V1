using System.Collections.Generic;
using UnityEngine;

namespace LabSW.Core
{
    public class BoardState
    {
        public const int BoardSize = 8;
        private readonly CellState[,] grid = new CellState[BoardSize, BoardSize];

        public BoardState()
        {
            InitializeBoard();
        }

        // Determina si una casilla es blanca jugable en este MVP
        public static bool IsWhiteCell(int x, int y)
        {
            return (x + y) % 2 == 0;
        }

        public static bool IsValidBounds(int x, int y)
        {
            return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
        }

        public void InitializeBoard()
        {
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    grid[x, y] = CellState.Empty;
                }
            }

            // 4 Zorras en casillas blancas de la Fila 0 (Y = 0)
            grid[0, 0] = CellState.Fox;
            grid[2, 0] = CellState.Fox;
            grid[4, 0] = CellState.Fox;
            grid[6, 0] = CellState.Fox;

            // 1 Gallina en casilla blanca de la Fila 7 (Y = 7)
            grid[7, 7] = CellState.Chicken;
        }

        public CellState GetCell(int x, int y) => IsValidBounds(x, y) ? grid[x, y] : CellState.Empty;
        public CellState GetCell(Vector2Int pos) => GetCell(pos.x, pos.y);

        public void SetCell(int x, int y, CellState state)
        {
            if (IsValidBounds(x, y)) grid[x, y] = state;
        }

        public Vector2Int GetChickenPosition()
        {
            for (int y = 0; y < BoardSize; y++)
                for (int x = 0; x < BoardSize; x++)
                    if (grid[x, y] == CellState.Chicken) return new Vector2Int(x, y);
            return new Vector2Int(-1, -1);
        }

        public List<Vector2Int> GetFoxPositions()
        {
            List<Vector2Int> foxes = new List<Vector2Int>();
            for (int y = 0; y < BoardSize; y++)
                for (int x = 0; x < BoardSize; x++)
                    if (grid[x, y] == CellState.Fox) foxes.Add(new Vector2Int(x, y));
            return foxes;
        }

        public bool ExecuteMove(int fromX, int fromY, int toX, int toY)
        {
            CellState p = GetCell(fromX, fromY);
            if (p == CellState.Empty) return false;
            SetCell(fromX, fromY, CellState.Empty);
            SetCell(toX, toY, p);
            return true;
        }
    }
}