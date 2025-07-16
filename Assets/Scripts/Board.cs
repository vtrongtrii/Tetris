using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; // Đảm bảo đã import TMPro

public class Board : MonoBehaviour
{
    // Biến lưu trữ Tilemap để quản lý lưới trò chơi
    public Tilemap tilemap { get; private set; }
    // Biến lưu trữ mảnh ghép đang hoạt động
    public Piece activePiece { get; private set; }
    // Mảng chứa dữ liệu các khối Tetromino (hình dạng, ô, v.v.), phải gán trong Inspector
    public TetrominoData[] tetrominoes;
    // Vị trí xuất hiện của mảnh ghép mới (mặc định là giữa đỉnh bảng)
    public Vector3Int spawnPosition = new Vector3Int(0, 8, 0);
    public Vector2Int boardSize = new Vector2Int(10, 20); // Kích thước bảng
    public TMP_Text scoreText;
    public int lineClear = 0;
    public int total_point = 0;
    public TMP_Text levelText;
    private int level = 1;
    public bool isGameOver { get; private set; } = false; // Đặt mặc định là false

    // Thuộc tính tính toán ranh giới của bảng chơi
    public RectInt Bounds
    {
        get
        {
            // Tính toán vị trí góc dưới bên trái của bảng
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        // Kiểm tra lỗi nếu các thành phần cần thiết không được tìm thấy
        if (tilemap == null)
        {
            Debug.LogError("Không tìm thấy Tilemap trong các thành phần con! Kiểm tra hierarchy.");
        }
        if (activePiece == null)
        {
            Debug.LogError("Không tìm thấy Piece trong các thành phần con! Kiểm tra hierarchy.");
        }
        if (tetrominoes == null || tetrominoes.Length == 0)
        {
            Debug.LogError("Mảng tetrominoes chưa được khởi tạo trong Inspector hoặc rỗng!");
        }
        if (scoreText == null) Debug.LogWarning("scoreText chưa được gán trong Inspector!");
        if (levelText == null) Debug.LogWarning("levelText chưa được gán trong Inspector!");

        // Khởi tạo dữ liệu cho tất cả các tetrominoes
        if (tetrominoes != null && tetrominoes.Length > 0)
        {
            for (int i = 0; i < tetrominoes.Length; i++)
            {
                tetrominoes[i].tetromino = (Tetromino)i;
                tetrominoes[i].Initialize(); // Đảm bảo đã sửa lỗi chính tả: Initalize -> Initialize
            }
        }
    }

    private void Start()
    {
        // Xóa toàn bộ Tilemap trước khi bắt đầu để đảm bảo không có tile thừa
        if (tilemap != null) tilemap.ClearAllTiles();

        isGameOver = false; // Đảm bảo trạng thái ban đầu là không game over
        total_point = 0;
        lineClear = 0;
        level = 1;
        if (scoreText != null) scoreText.text = "Score: 0";
        UpdateLevel(); // Cập nhật hiển thị cấp độ
        SpawnPiece(); // Bắt đầu trò chơi bằng cách spawn khối đầu tiên
    }

    public void SpawnPiece()
    {
        if (isGameOver) return; // Không spawn nếu game đã kết thúc
        if (tetrominoes == null || tetrominoes.Length == 0 || tilemap == null || activePiece == null)
        {
            Debug.LogError("tetrominoes, tilemap, hoặc activePiece là null khi cố gắng SpawnPiece! Kiểm tra hierarchy hoặc Inspector.");
            GameOver(); // Có thể coi là Game Over nếu không đủ thành phần cơ bản
            return;
        }

        // Chọn một tetromino ngẫu nhiên
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        // Khởi tạo mảnh ghép mới
        activePiece.Initialize(this, spawnPosition, data);
        activePiece.fallSpeed = Mathf.Max(0.1f, 1f - (level * 0.1f)); // Cập nhật tốc độ rơi theo level

        // Kiểm tra xem vị trí spawn có hợp lệ không
        if (IsValidPosition(activePiece, activePiece.position))
        {
            Set(activePiece); // Đặt mảnh ghép lên Tilemap
        }
        else
        {
            Debug.LogError($"Vị trí sinh không hợp lệ: {spawnPosition}. Trò chơi kết thúc!");
            GameOver(); // Nếu không thể spawn, trò chơi kết thúc
            return; // Rất quan trọng: Dừng hàm sau khi gọi GameOver
        }
    }

    private void GameOver()
    {
        if (isGameOver) return; // Đảm bảo chỉ gọi Game Over một lần
        isGameOver = true;
        Debug.Log("Trò chơi kết thúc!");

        // Ví dụ đơn giản: xóa bảng và reset điểm
        if (tilemap != null) tilemap.ClearAllTiles();

        // Reset điểm và level để chuẩn bị cho lần chơi tiếp theo
        total_point = 0;
        lineClear = 0;
        level = 1; // Reset level
        if (scoreText != null) scoreText.text = "Score: 0";
        UpdateLevel(); // Cập nhật hiển thị level về 1

        // Chờ 2 giây rồi khởi động lại trò chơi
        Invoke("RestartGame", 2f);
    }

    private void RestartGame()
    {
        isGameOver = false; // Đặt lại trạng thái trò chơi
        if (tilemap != null) tilemap.ClearAllTiles(); // Đảm bảo bảng trống trước khi chơi lại
        SpawnPiece(); // Bắt đầu trò chơi mới
    }

    public void Set(Piece piece)
    {
        if (piece == null || piece.cells == null || piece.data.tile == null || tilemap == null)
        {
            Debug.LogWarning("Piece, cells, tile, hoặc tilemap là null khi gọi Set!");
            return;
        }

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.position + piece.cells[i];
            // Luôn kiểm tra Bounds.Contains để tránh đặt tile ra ngoài Tilemap
            if (Bounds.Contains((Vector2Int)tilePosition))
            {
                tilemap.SetTile(tilePosition, piece.data.tile);
            }
            else
            {
                Debug.LogWarning($"Vị trí {tilePosition} nằm ngoài ranh giới khi Set, có thể do lỗi tính toán!");
            }
        }
    }

    public void Clear(Piece piece)
    {
        if (piece == null || piece.cells == null || tilemap == null)
        {
            Debug.LogWarning("Piece, cells, hoặc tilemap là null khi gọi Clear!");
            return;
        }

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.position + piece.cells[i];
            // Kiểm tra Bounds.Contains trước khi xóa để tránh lỗi
            if (Bounds.Contains((Vector2Int)tilePosition))
            {
                tilemap.SetTile(tilePosition, null); // Xóa tile
            }
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        if (piece == null || piece.cells == null || tilemap == null) return false;

        RectInt bounds = Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = position + piece.cells[i];

            // 1. Kiểm tra xem ô có nằm ngoài ranh giới bảng không
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // 2. Kiểm tra xem ô đó đã có tile khác chiếm giữ chưa
            // Khi gọi IsValidPosition từ Piece, Piece đó đã được Clear khỏi bảng,
            // nên không cần lo lắng về việc nó tự va chạm với chính nó.
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        if (tilemap == null) return;

        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesClearedThisFrame = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row); // Xóa dòng
                linesClearedThisFrame++;
                // Không tăng 'row' ở đây vì sau khi xóa, dòng trên sẽ rơi xuống và cần được kiểm tra lại
            }
            else
            {
                row++; // Chỉ tăng 'row' nếu dòng hiện tại không đầy
            }
        }

        if (linesClearedThisFrame > 0)
        {
            CountPoints(linesClearedThisFrame); // Tính điểm
        }
    }

    private bool IsLineFull(int row)
    {
        if (tilemap == null) return false;
        RectInt bounds = Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        if (tilemap == null) return;
        RectInt bounds = Bounds;

        // Xóa tất cả các tile trong dòng hiện tại
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Di chuyển tất cả các dòng phía trên xuống 1 đơn vị
        for (int y = row; y < bounds.yMax - 1; y++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int from = new Vector3Int(col, y + 1, 0);
                Vector3Int to = new Vector3Int(col, y, 0);
                TileBase tile = tilemap.GetTile(from); // Lấy tile từ dòng trên
                tilemap.SetTile(to, tile); // Đặt tile đó xuống dòng hiện tại
            }
        }

        // Đảm bảo dòng trên cùng nhất (sau khi các dòng khác đã rơi xuống) được xóa trống
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, bounds.yMax - 1, 0);
            tilemap.SetTile(position, null);
        }
    }

    private void CountPoints(int linesClearedThisFrame)
    {
        int points = 0;
        // Không có bonusMultiplier cho đồng màu ở đây

        switch (linesClearedThisFrame)
        {
            case 1: points = 100; break;
            case 2: points = 300; break;
            case 3: points = 500; break;
            case 4: points = 800; break;
        }

        total_point += points; // Chỉ cộng điểm cơ bản
        lineClear += linesClearedThisFrame; // Cập nhật số dòng đã xóa

        if (scoreText != null)
        {
            scoreText.text = "Score: " + total_point.ToString();
        }

        // Lên cấp dựa trên điểm
        if (total_point >= 500) // Ví dụ: Cần 500 điểm để lên cấp
        {
            NextLevel();
        }
    }

    private void UpdateLevel()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level.ToString();
        }
    }

    private void NextLevel()
    {
        if (isGameOver) return; // Không lên cấp nếu game đã kết thúc

        level++;
        total_point = 0; // Reset điểm để cấp độ mới bắt đầu từ 0 điểm
        lineClear = 0;
        if (scoreText != null) scoreText.text = "Score: 0";
        Debug.Log($"Đạt cấp độ {level}! Đặt lại bảng chơi.");

        // Xóa toàn bộ Tilemap để bảng trống cho cấp độ mới
        if (tilemap != null) tilemap.ClearAllTiles();

        // Đảm bảo activePiece được reset và spawn khối mới
        if (activePiece != null)
        {
            if (tetrominoes != null && tetrominoes.Length > 0)
            {
                int randomIndex = Random.Range(0, tetrominoes.Length);
                activePiece.Initialize(this, spawnPosition, tetrominoes[randomIndex]);
                activePiece.fallSpeed = Mathf.Max(0.1f, 1f - (level * 0.1f)); // Cập nhật tốc độ rơi
            }
            else
            {
                Debug.LogError("Mảng tetrominoes rỗng hoặc không hợp lệ khi lên cấp, không thể spawn khối!");
                GameOver(); // Nếu không có khối để spawn, coi như Game Over
                return;
            }
        }
        UpdateLevel();
    }
}