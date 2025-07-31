using UnityEngine;
using System.Collections; // Cần dùng System.Collections cho IEnumerator

public class Piece : MonoBehaviour
{
 
    public Board board { get; private set; }
    public TetrominoData data { get; set; }
    public Vector3Int[] cells { get; set; }
    public Vector3Int position { get; set; }
    public int rotationIndex { get; set; } // Cho phép đọc và ghi từ ngoài
    private bool hasChanged = false;
    public float stepDelay = 1f;
    public float lockDelay = 0.5f;
    private float stepTime;
    private float lockTime;
    public float fallSpeed = 1f;

    // Hàm gọi khi khối mới spawn
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.hasChanged = false; // Đặt lại trạng thái khi khối mới spawn
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.fallSpeed = 1f;

        if (this.cells == null || this.cells.Length != data.cells.Length)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        if (board == null || board.isGameOver)
        {
            return;
        }

        board.Clear(this);
        this.lockTime += Time.deltaTime;

        // Xử lý input điều khiển
        if (Input.GetKeyDown(KeyCode.Q)) Rotate(-1);
        if (Input.GetKeyDown(KeyCode.E)) Rotate(1);
        if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.Space)) HardDrop();

        if (Time.time >= this.stepTime) Step();

        // Đổi khối bằng phím F (1 lần duy nhất)
        if (Input.GetKeyDown(KeyCode.F) && !hasChanged)
        {
            ChangeToRandomTetromino();
            hasChanged = true;
        }

        // Bật/tắt Runtime Tetromino Editor UI (cho phép sử dụng khi UI hiển thị)
        if (Input.GetKeyDown(KeyCode.C))
        {
            var editor = FindObjectOfType<RuntimeTetrominoEditor>();
            if (editor != null)
            {
                editor.ToggleRuntimeUI();
            }
        }

        board.Set(this);
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay / this.fallSpeed;

        if (!Move(Vector2Int.down))
        {
            if (this.lockTime >= this.lockDelay)
            {
                Lock();
            }
        }
        else
        {
            this.lockTime = 0f;
        }
    }

    private void HardDrop()
    {
        if (board == null || board.isGameOver) return;

        while (Move(Vector2Int.down)) continue;
        Lock();
    }

    private void Lock()
    {
        if (board == null || board.isGameOver) return;

        board.Set(this);
        board.ClearLines();

        // 🔔 Thêm dòng này để kích hoạt rung nếu luật thử thách là CameraShake
 

        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position + new Vector3Int(translation.x, translation.y, 0);

        if (board != null)
        {
            bool valid = board.IsValidPosition(this, newPosition);
            if (valid)
            {
                this.position = newPosition;
                this.lockTime = 0f;
            }
            return valid;
        }
        return false;
    }
    
    private void Rotate(int direction)
    {
        if (ChallengeRuleManager.Instance.currentRule == ChallengeRule.NoRotation)
        {
            Debug.Log("⛔ Không được xoay do luật thử thách.");
            return;
        }
        if (board == null || board.isGameOver) return;

        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (board != null && !TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
        else
        {
            this.lockTime = 0f;
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];
            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt(cell.x * Data.RotationMatrix[0] * direction + cell.y * Data.RotationMatrix[1] * direction);
                    y = Mathf.CeilToInt(cell.x * Data.RotationMatrix[2] * direction + cell.y * Data.RotationMatrix[3] * direction);
                    break;

                default:
                    x = Mathf.RoundToInt(cell.x * Data.RotationMatrix[0] * direction + cell.y * Data.RotationMatrix[1] * direction);
                    y = Mathf.RoundToInt(cell.x * Data.RotationMatrix[2] * direction + cell.y * Data.RotationMatrix[3] * direction);
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        if (board != null)
        {
            int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);
            for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
            {
                Vector2Int translation = this.data.wallKicks[wallKickIndex, i];
                if (Move(translation))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
        {
            wallKickIndex -= 1;
        }
        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        return (input - min + (max - min)) % (max - min) + min;
    }

    public void ApplyNewShape(Tetromino newType)
    {
        if (board == null || board.isGameOver) return;

        if (board != null)
        {
            this.data = board.tetrominoes[(int)newType];
            this.data.Initialize();

            if (this.cells == null || this.cells.Length != data.cells.Length)
            {
                this.cells = new Vector3Int[data.cells.Length];
            }

            for (int i = 0; i < data.cells.Length; i++)
            {
                this.cells[i] = (Vector3Int)data.cells[i];
            }

            this.rotationIndex = 0;
        }
    }

    public void SetData(Tetromino type, TetrominoData newData)
    {
        if (board == null || board.isGameOver) return;

        if (newData != null)
        {
            newData.tetromino = type;
            this.data = newData;
            this.rotationIndex = 0;

            if (this.cells == null || this.cells.Length != data.cells.Length)
                this.cells = new Vector3Int[data.cells.Length];

            for (int i = 0; i < data.cells.Length; i++)
            {
                this.cells[i] = (Vector3Int)data.cells[i];
            }
        }
    }

    public void AnimateTransformChange()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScale());
    }

    private IEnumerator AnimateScale()
    {
        Vector3 small = new Vector3(0.3f, 0.3f, 1f);
        Vector3 normal = Vector3.one;
        float t = 0;
        while (t < 1f)
        {
            transform.localScale = Vector3.Lerp(small, normal, t);
            t += Time.deltaTime * 5f;
            yield return null;
        }
        transform.localScale = normal;
    }

    private void ChangeToRandomTetromino()
    {
        if (board == null || board.isGameOver) return;

        if (board != null)
        {
            Tetromino current = this.data.tetromino;
            Tetromino newType;

            do
            {
                newType = (Tetromino)Random.Range(0, System.Enum.GetValues(typeof(Tetromino)).Length);
            } while (newType == current);

            TetrominoData newData = board.tetrominoes[(int)newType];
            if (newData != null)
            {
                newData.Initialize();

                board.Clear(this);

                this.data = newData;
                this.rotationIndex = 0;

                if (this.cells == null || this.cells.Length != data.cells.Length)
                {
                    this.cells = new Vector3Int[data.cells.Length];
                }
                for (int i = 0; i < data.cells.Length; i++)
                {
                    this.cells[i] = (Vector3Int)data.cells[i];
                }

                board.Set(this);

                AnimateTransformChange();
            }
        }
    }

    public void ChangeToTetrominoType(Tetromino newType)
    {
        if (board == null || board.isGameOver) return;

        if (board != null)
        {
            TetrominoData newData = board.tetrominoes[(int)newType];
            if (newData != null)
            {
                newData.Initialize();

                board.Clear(this);

                this.data = newData;
                this.rotationIndex = 0;

                this.cells = new Vector3Int[data.cells.Length];
                for (int i = 0; i < data.cells.Length; i++)
                {
                    this.cells[i] = (Vector3Int)data.cells[i];
                }

                board.Set(this);
                AnimateTransformChange();
                Debug.Log($"Khối đã đổi sang: {newType}");
            }
        }
    }

    public void Clear()
    {
        if (board != null)
        {
            board.Clear(this);
        }
        else
        {
            Debug.LogWarning("Board không được gán cho Piece khi gọi Clear!");
        }
    }
} 