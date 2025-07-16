using UnityEngine;
using System.Collections; // Cần dùng System.Collections cho IEnumerator

public class Piece : MonoBehaviour
{
    // Tham chiếu đến board (bàn chơi)
    public Board board { get; private set; }

    // Dữ liệu Tetromino hiện tại (kiểu khối, tile, cells...)
    public TetrominoData data { get; private set; }

    // Các ô tạo nên khối (vị trí tương đối)
    public Vector3Int[] cells { get; private set; }

    // Vị trí tuyệt đối của khối trong tilemap
    public Vector3Int position { get; private set; }

    // Chỉ số vòng quay (0,1,2,3)
    public int rotationIndex { get; private set; }

    // Khối đã được đổi bằng phím F chưa? (chỉ 1 lần duy nhất)
    private bool hasChanged = false;

    // Thời gian delay giữa mỗi lần rơi và khóa
    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    // Thời gian kiểm soát rơi và khóa
    private float stepTime;
    private float lockTime;

    // Tốc độ rơi (thêm để tương thích với NextLevel)
    public float fallSpeed = 1f;

    // Hàm gọi khi khối mới spawn
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.hasChanged = false;
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.fallSpeed = 1f; // Tốc độ rơi ban đầu (sẽ được cập nhật theo level từ Board)

        if (this.cells == null || this.cells.Length != data.cells.Length) // Đảm bảo mảng cells được khởi tạo lại nếu kích thước thay đổi
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
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) // <--- ĐÃ THÊM: Kiểm tra trạng thái isGameOver từ Board
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

        // Rơi tự động theo thời gian
        // Điều chỉnh stepDelay dựa trên fallSpeed (tốc độ rơi)
        if (Time.time >= this.stepTime) Step();

        // Đổi khối bằng phím F (1 lần duy nhất)
        if (Input.GetKeyDown(KeyCode.F) && !hasChanged)
        {
            ChangeToRandomTetromino();
            hasChanged = true;
        }

        // Bật/tắt Runtime Tetromino Editor UI (nếu có)
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
        // Điều chỉnh tốc độ rơi theo fallSpeed của Piece
        this.stepTime = Time.time + this.stepDelay / this.fallSpeed;

        // Cố gắng di chuyển xuống. Nếu không thể di chuyển, bắt đầu khóa.
        if (!Move(Vector2Int.down))
        {
            if (this.lockTime >= this.lockDelay)
            {
                Lock();
            }
        }
        else
        {
            // Nếu di chuyển được, reset lockTime
            this.lockTime = 0f;
        }
    }

    private void HardDrop()
    {
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) return;

        while (Move(Vector2Int.down)) continue; // Di chuyển xuống cho đến khi không thể nữa
        Lock(); // Khóa khối ngay lập tức
    }

    private void Lock()
    {
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) return;

        board.Set(this); // Đặt khối vào Tilemap
        board.ClearLines(); // Xóa các dòng đầy
        board.SpawnPiece(); // Tạo khối mới
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
                this.lockTime = 0f; // Đặt lại thời gian khóa khi di chuyển hợp lệ
            }
            return valid;
        }
        return false;
    }

    private void Rotate(int direction)
    {
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) return;

        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (board != null && !TestWallKicks(this.rotationIndex, direction))
        {
            // Nếu không tìm được vị trí hợp lệ sau khi xoay và kick, quay lại trạng thái ban đầu
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction); // Xoay ngược lại để khôi phục cells
        }
        else
        {
            this.lockTime = 0f; // Reset lockTime nếu xoay thành công
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
                    // Tetromino I và O xoay quanh tâm 0.5, 0.5
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt(cell.x * Data.RotationMatrix[0] * direction + cell.y * Data.RotationMatrix[1] * direction);
                    y = Mathf.CeilToInt(cell.x * Data.RotationMatrix[2] * direction + cell.y * Data.RotationMatrix[3] * direction);
                    break;

                default:
                    // Các tetromino khác xoay quanh tâm 0, 0
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

            // Duyệt qua các điểm offset trong bảng Wall Kicks
            for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
            {
                Vector2Int translation = this.data.wallKicks[wallKickIndex, i];
                if (Move(translation))
                {
                    return true; // Nếu tìm được vị trí hợp lệ, trả về true
                }
            }
        }
        return false; // Không tìm được vị trí hợp lệ sau khi thử tất cả wall kicks
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        // Chuyển đổi chỉ số quay và hướng quay thành chỉ số trong mảng wallKicks
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0) // Nếu quay ngược chiều kim đồng hồ
        {
            wallKickIndex -= 1;
        }
        // Đảm bảo chỉ số nằm trong phạm vi của mảng wallKicks
        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        // Hàm bao bọc giá trị để đảm bảo nằm trong min-max
        return (input - min + (max - min)) % (max - min) + min;
    }

    public void ApplyNewShape(Tetromino newType)
    {
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) return;

        if (board != null)
        {
            this.data = board.tetrominoes[(int)newType];
            this.data.Initialize(); // Sửa lỗi chính tả: Initalize -> Initialize

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

    // Phương thức SetData (dành cho Runtime Editor)
    public void SetData(Tetromino type, TetrominoData newData)
    {
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) return;

        if (newData != null) // Đã sửa lỗi CS0019 bằng cách đảm bảo TetrominoData là class
        {
            newData.tetromino = type; // Đặt lại kiểu tetromino cho newData
            this.data = newData;
            this.rotationIndex = 0;

            // Đảm bảo cells được cập nhật theo newData
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
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) return;

        if (board != null)
        {
            Tetromino current = this.data.tetromino;
            Tetromino newType;

            // Chọn một loại tetromino ngẫu nhiên khác với loại hiện tại
            do
            {
                newType = (Tetromino)Random.Range(0, System.Enum.GetValues(typeof(Tetromino)).Length);
            } while (newType == current);

            TetrominoData newData = board.tetrominoes[(int)newType];
            if (newData != null) // Kiểm tra null an toàn (vì TetrominoData giờ là class)
            {
                newData.Initialize(); // Sửa lỗi chính tả: Initalize -> Initialize

                board.Clear(this); // Xóa khối cũ khỏi bảng

                this.data = newData;
                this.rotationIndex = 0;

                // Cập nhật các ô của khối mới
                if (this.cells == null || this.cells.Length != data.cells.Length)
                {
                    this.cells = new Vector3Int[data.cells.Length];
                }
                for (int i = 0; i < data.cells.Length; i++)
                {
                    this.cells[i] = (Vector3Int)data.cells[i];
                }

                board.Set(this); // Đặt khối mới vào bảng

                AnimateTransformChange(); // Chạy animation
            }
        }
    }

    // Phương thức để đổi sang một loại tetromino cụ thể (dành cho bên ngoài gọi)
    public void ChangeToTetrominoType(Tetromino newType)
    {
        // Dừng mọi hoạt động nếu board không tồn tại hoặc game đã kết thúc
        if (board == null || board.isGameOver) return;

        if (board != null)
        {
            TetrominoData newData = board.tetrominoes[(int)newType];
            if (newData != null) // Kiểm tra null an toàn
            {
                newData.Initialize(); // Sửa lỗi chính tả: Initalize -> Initialize

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