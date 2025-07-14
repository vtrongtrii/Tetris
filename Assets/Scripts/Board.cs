
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public TMP_Text scoreText;
    public int lineClear = 0;
    public int total_point = 0;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initalize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        this.activePiece.Initialize(this, this.spawnPosition, data);

        if(IsValidPosition(this.activePiece, this.activePiece.position))
        {
          Set(this.activePiece);
        }
        else
        {
            GameOver();
            Debug.Log("Game Over!"); 
        }
    }

    private void GameOver()
    {
        // Xử lý game over, có thể dừng game hoặc reset lại
        Debug.Log("Game Over! Resetting the board.");
        this.tilemap.ClearAllTiles();
        SpawnPiece();
    }   

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.position + piece.cells[i];
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.position + piece.cells[i];
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = position + piece.cells[i];

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        // Xóa dòng hiện tại
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
          
        }
        lineClear++;
        CountPoints(lineClear);
        // Dồn các dòng trên xuống
        for (int y = row; y < bounds.yMax - 1; y++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int from = new Vector3Int(col, y + 1, 0);
                Vector3Int to = new Vector3Int(col, y, 0);
                TileBase tile = this.tilemap.GetTile(from);
                this.tilemap.SetTile(to, tile);
            }
        }

        // Xóa dòng trên cùng
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, bounds.yMax - 1, 0);
            this.tilemap.SetTile(position, null);
        }
    }

    private void CountPoints(int lines)
    {
        int point = 100;
        total_point = lines * point;

        if (scoreText != null)
        {
            scoreText.text = "Score: " + total_point.ToString();
        }

        if (total_point >= 1000) // hoặc mức bạn muốn kết thúc game
        {
            GameOver();
        }
    }
}
