using UnityEngine;

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

    // Hàm gọi khi khối mới spawn
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.hasChanged = false; // reset quyền đổi khối
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        if (this.cells == null)
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
        this.board.Clear(this); // xóa khối khỏi tilemap để update

        this.lockTime += Time.deltaTime;

        // 🔁 Xử lý input điều khiển
        if (Input.GetKeyDown(KeyCode.Q)) Rotate(-1);
        if (Input.GetKeyDown(KeyCode.E)) Rotate(1);
        if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.Space)) HardDrop();

        // Rơi tự động theo thời gian
        if (Time.time >= this.stepTime) Step();

        // Nhấn F để đổi khối (1 lần duy nhất)  
        if (Input.GetKeyDown(KeyCode.F) && !hasChanged)
        {
            ChangeToRandomTetromino();
            hasChanged = true;
        }


        /*if (Input.GetKeyDown(KeyCode.C))
        {
            if (runtimeUI == null)
            {
                ShowRuntimeUI();
            }
            else
            {
                Destroy(runtimeUI);
            }
        }*/

        this.board.Set(this); // vẽ lại khối
    }

    // Rơi từng bước
    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;
        Move(Vector2Int.down);

        // Nếu đã chạm đáy hoặc khối khác
        if (this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }

    // Rơi hết mức
    private void HardDrop()
    {
        while (Move(Vector2Int.down)) continue;
    }

    //Khóa khối, kiểm tra dòng và spawn khối mới
    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    // Di chuyển khối, kiểm tra hợp lệ
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position + new Vector3Int(translation.x, translation.y, 0);

        bool valid = this.board.IsValidPosition(this, newPosition);
        if (valid)
        {
            this.position = newPosition;
            this.lockTime = 0f;
        }
        return valid;
    }

    // Xoay khối trái/phải
    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        // Thử wall kick nếu xoay không hợp lệ
        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    // Áp dụng ma trận xoay cho từng cell
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
                    // xoay chính giữa với I/O
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

    // Thử wall kick sau xoay
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];
            if (Move(translation)) return true;
        }
        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0) wallKickIndex -= 1;
        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    // Đảm bảo rotation nằm trong khoảng
    private int Wrap(int input, int min, int max)
    {
        return (input - min + (max - min)) % (max - min) + min;
    }

    // Áp dụng lại shape từ Data.Cells
    /*public void ApplyNewShape()
    {
        this.data.Initalize();
        if (this.cells.Length != data.cells.Length)
            this.cells = new Vector3Int[data.cells.Length];

        for (int i = 0; i < data.cells.Length; i++)
            this.cells[i] = (Vector3Int)data.cells[i];
    }*/

    // Gây hiệu ứng khi đổi khối
    public void AnimateTransformChange()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScale());
    }

    private System.Collections.IEnumerator AnimateScale()
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

    // Đổi sang khối khác ngẫu nhiên (chỉ 1 lần)
    private void ChangeToRandomTetromino()
    {
        Tetromino current = this.data.tetromino;
        Tetromino newType;

        do
        {
            newType = (Tetromino)Random.Range(0, System.Enum.GetValues(typeof(Tetromino)).Length);
        } while (newType == current);

        TetrominoData newData = board.tetrominoes[(int)newType];
        newData.Initalize();

        board.Clear(this);

        this.data = newData;
        this.rotationIndex = 0;

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }

        board.Set(this);

        AnimateTransformChange(); // 💥 Gọi hiệu ứng co giãn sau khi đổi
    }

}
