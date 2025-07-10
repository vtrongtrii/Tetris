using UnityEngine;
using UnityEngine.Tilemaps;
public enum Tetromino
{
    I, // Straight line
    J, // J shape
    L, // L shape
    O, // Square
    S, // S shape
    T, // T shape
    Z  // Z shape
}
[System.Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;
    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKicks { get; private set; }

    public void Initalize()
    {
        this.cells = Data.Cells[this.tetromino];
        this.wallKicks = Data.WallKicks[this.tetromino];
    }
}   