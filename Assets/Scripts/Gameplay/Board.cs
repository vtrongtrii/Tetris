using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition = new Vector3Int(0, 8, 0);
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public TMP_Text scoreText;
    public int lineClear = 0;
    public int total_point = 0;
    public TMP_Text levelText;
    private int level = 1;
    public bool isGameOver { get; private set; } = false;

    public int[] levelUpPoints;

    public TMP_Text timerText;
    public float[] timesPerLevel;
    private float currentTime;
    [SerializeField] private GameObject previewPieceObject;

    public RectInt Bounds => new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        if (tilemap == null) Debug.LogError("Không tìm thấy Tilemap!");
        if (activePiece == null) Debug.LogError("Không tìm thấy Piece!");
        if (tetrominoes == null || tetrominoes.Length == 0) Debug.LogError("Mảng tetrominoes chưa được khởi tạo!");
        if (scoreText == null) Debug.LogWarning("scoreText chưa gán!");
        if (levelText == null) Debug.LogWarning("levelText chưa gán!");
        if (timerText == null) Debug.LogWarning("timerText chưa gán!");
        if (levelUpPoints == null || levelUpPoints.Length == 0) Debug.LogWarning("levelUpPoints chưa gán!");
        if (timesPerLevel == null || timesPerLevel.Length == 0) Debug.LogWarning("timesPerLevel chưa gán!");

        foreach (var t in tetrominoes)
        {
            t.Initialize();
        }
    }

    private void Start()
    {
        tilemap.ClearAllTiles();
        isGameOver = false;
        total_point = 0;
        lineClear = 0;
        level = 1;
        scoreText.text = "Score: 0";
        UpdateLevel();
        ResetTimer();
        SpawnPiece();
    }

    private void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            GameOver();
        }

        UpdateTimerDisplay();

        Challenge.RandomTetrominoEveryFewSecondsUpdate(() =>
        {
            Debug.Log("[Challenge] RandomTetrominoEveryFewSeconds: Đổi hình dạng Tetromino");

            Vector3Int currentPos = activePiece.position;
            int random = UnityEngine.Random.Range(0, tetrominoes.Length);
            TetrominoData newData = tetrominoes[random];

            Clear(activePiece);
            activePiece.data = newData;
            activePiece.rotationIndex = 0;
            activePiece.cells = new Vector3Int[4];
            activePiece.Initialize(this, currentPos, newData);

            if (IsValidPosition(activePiece, currentPos))
            {
                Set(activePiece);
            }
            else
            {
                Debug.LogWarning("Vị trí không hợp lệ, spawn lại ở spawnPosition");
                activePiece.position = spawnPosition;
                activePiece.Initialize(this, spawnPosition, newData);
                Set(activePiece);
            }
        });
    }

    public void SpawnPiece()
    {
        if (isGameOver) return;

        int random = UnityEngine.Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);
        activePiece.fallSpeed = Mathf.Max(0.1f, 1f - (level * 0.1f));

        if (IsValidPosition(activePiece, activePiece.position))
        {
            Set(activePiece);
        }
        else
        {
            Debug.LogError("Spawn position không hợp lệ. Game Over!");
            GameOver();
            return;
        }

        if (previewPieceObject != null)
        {
            bool hide = Challenge.ShouldHidePreview();
            previewPieceObject.SetActive(!hide);
            Debug.Log("[Challenge] HiddenPreview is active? " + hide);
        }
    }

    private void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Trò chơi kết thúc!");
        tilemap.ClearAllTiles();
        total_point = 0;
        lineClear = 0;
        level = 1;
        scoreText.text = "Score: 0";
        UpdateLevel();
        UpdateTimerDisplay();
        Invoke("RestartGame", 2f);
    }

    private void RestartGame()
    {
        isGameOver = false;
        tilemap.ClearAllTiles();
        ResetTimer();
        SpawnPiece();
    }

    public void Set(Piece piece)
    {
        foreach (var cell in piece.cells)
        {
            Vector3Int tilePosition = piece.position + cell;
            if (Bounds.Contains((Vector2Int)tilePosition))
                tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        foreach (var cell in piece.cells)
        {
            Vector3Int tilePosition = piece.position + cell;
            if (Bounds.Contains((Vector2Int)tilePosition))
                tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        foreach (var cell in piece.cells)
        {
            Vector3Int tilePosition = position + cell;
            if (!Bounds.Contains((Vector2Int)tilePosition) || tilemap.HasTile(tilePosition))
                return false;
        }
        return true;
    }

    // ClearLines, IsLineFull, LineClear, CountPoints, UpdateLevel,
    // UpdateTimerDisplay, ResetTimer, NextLevel giữ nguyên như cũ



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

        switch (linesClearedThisFrame)
        {
            case 1: points = 100; break;
            case 2: points = 300; break;
            case 3: points = 400; break;
            case 4: points = 500; break;
        }

        total_point += points; // Cộng điểm vào tổng điểm hiện tại của level

        if (scoreText != null)
        {
            scoreText.text = "Score: " + total_point.ToString();
        }

        // Kiểm tra xem người chơi đã đạt mốc điểm để lên cấp độ tiếp theo chưa
        // levelUpPoints[level - 1] sẽ là mốc điểm cho level hiện tại để lên level kế tiếp.
        // Đảm bảo không vượt quá kích thước mảng levelUpPoints
        if (level - 1 < levelUpPoints.Length)
        {
            if (total_point >= levelUpPoints[level - 1])
            {
                NextLevel();
            }
        }
        else
        {
            // Nếu đã vượt quá số level định nghĩa, có thể coi là đã hoàn thành tất cả các level
            // hoặc tiếp tục chơi ở level cuối cùng mà không lên cấp nữa.
        }
    }

    private void UpdateLevel()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level.ToString();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Định dạng thời gian để hiển thị (ví dụ: 00:00)
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Có thể thêm hiệu ứng màu sắc khi thời gian còn ít
            if (currentTime <= 10f && currentTime > 0f) // Ví dụ: 10 giây cuối
            {
                timerText.color = Color.red; // Đổi màu chữ sang đỏ
            }
            else
            {
                timerText.color = Color.white; // Trở lại màu trắng
            }
        }
    }

    private void ResetTimer()
    {
        // Đảm bảo mảng timesPerLevel đã được thiết lập và level không vượt quá giới hạn
        if (timesPerLevel != null && timesPerLevel.Length > 0 && level - 1 < timesPerLevel.Length)
        {
            currentTime = timesPerLevel[level - 1]; // Lấy thời gian tương ứng với level hiện tại
        }
        else
        {
            // Fallback nếu mảng không được thiết lập hoặc level vượt quá giới hạn
            Debug.LogWarning($"timesPerLevel chưa được thiết lập hoặc level {level} vượt quá số lượng thời gian đã định nghĩa. Sử dụng thời gian mặc định (ví dụ: 60s).");
            currentTime = 60f; // Giá trị mặc định an toàn
        }
        UpdateTimerDisplay();
    }

    private void NextLevel()
    {
        if (isGameOver) return;

        if (level >= levelUpPoints.Length + 1)
        {
            Debug.Log("Đã đạt cấp độ tối đa được định nghĩa! Tiếp tục chơi ở level này hoặc kết thúc game.");
            return;
        }

        level++;
        total_point = 0;

        if (scoreText != null)
        {
            scoreText.text = "Score: " + total_point.ToString();
        }
        Debug.Log($"Đạt cấp độ {level}! Điểm đã được reset. Chơi tiếp với tốc độ mới.");

        if (tilemap != null) tilemap.ClearAllTiles();

        if (activePiece != null)
        {
            if (tetrominoes != null && tetrominoes.Length > 0)
            {
                int randomIndex = Random.Range(0, tetrominoes.Length);
                activePiece.Initialize(this, spawnPosition, tetrominoes[randomIndex]);
                activePiece.fallSpeed = Mathf.Max(0.1f, 1f - (level * 0.1f));
            }
            else
            {
                Debug.LogError("Mảng tetrominoes rỗng hoặc không hợp lệ khi lên cấp, không thể spawn khối!");
                GameOver();
                return;
            }
        }

        UpdateLevel();
        ResetTimer();

        // ✅ GỌI LEVEL STAGE MANAGER để áp dụng thử thách nếu cần
        LevelStageManager.Instance.GoToNextStage(level);
    }

}