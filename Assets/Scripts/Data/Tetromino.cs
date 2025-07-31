using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I,
    J,
    L,
    O,
    S,
    T,
    Z
}

[System.Serializable]
public class TetrominoData // Đảm bảo đây là 'class'
{
    public Tetromino tetromino;
    public Tile tile;
    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKicks { get; private set; }

    // Sửa lỗi chính tả từ Initalize thành Initialize
    public void Initialize() // <--- ĐÃ SỬA TẠI ĐÂY!
    {
        this.cells = (Vector2Int[])Data.Cells[tetromino].Clone(); // ✅ an toàn

        this.wallKicks = Data.WallKicks[this.tetromino];
    }
}