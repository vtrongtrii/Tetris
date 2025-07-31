using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuntimeTetrominoEditor : MonoBehaviour
{
    public GameObject runtimeUI;
    public GameObject dropdownPrefab;
    [SerializeField] private float uiTimer = 5f; // Thời gian đếm ngược mặc định, hiển thị trong Inspector
    public TMP_Text timerText; // Tham chiếu đến TMP_Text có sẵn trong Hierarchy
    private float currentTime; // Thời gian còn lại
    private Tetromino currentType;

    void ShowRuntimeUI()
    {
        if (runtimeUI != null) return; // Ngăn tạo UI mới nếu đã tồn tại

        runtimeUI = new GameObject("RuntimeUI");
        runtimeUI.transform.SetParent(GameObject.Find("Canvas").transform);

        RectTransform rt = runtimeUI.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 150); // Tăng chiều cao để chứa text
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        runtimeUI.AddComponent<CanvasRenderer>();
        Image img = runtimeUI.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.5f);

        // Dropdown
        GameObject dd = Instantiate(dropdownPrefab, runtimeUI.transform);
        RectTransform ddRT = dd.GetComponent<RectTransform>();
        ddRT.anchoredPosition = new Vector2(0, 20); // Điều chỉnh vị trí để tránh chồng lấp với text

        var dropdown = dd.GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(Tetromino))));

        dropdown.onValueChanged.AddListener((i) =>
        {
            currentType = (Tetromino)i;
            ApplyTetromino(currentType);
        });

        // Gọi khối mặc định ban đầu
        currentType = (Tetromino)dropdown.value;
        ApplyTetromino(currentType);

        currentTime = uiTimer; // Bắt đầu đếm ngược
        if (timerText != null) timerText.gameObject.SetActive(true); // Hiển thị text nếu đã gán
    }

    void ApplyTetromino(Tetromino type)
    {
        var piece = FindObjectOfType<Piece>();
        if (piece != null && runtimeUI != null) // Chỉ áp dụng khi UI còn hiển thị
        {
            TetrominoData newData = piece.board.tetrominoes[(int)type];
            newData.Initialize();

            piece.board.Clear(piece);
            piece.SetData(type, newData);
            piece.board.Set(piece);
            piece.AnimateTransformChange();

            Debug.Log($"✅ Đã đổi sang khối: {type}");
            currentTime = uiTimer; // Reset thời gian khi thay đổi thành công
        }
        else
        {
            Debug.LogWarning("❗Không tìm thấy Piece hoặc UI đã bị đóng, không thể áp dụng khối mới.");
        }
    }

    void Update()
    {
        if (runtimeUI != null && timerText != null)
        {
            currentTime -= Time.deltaTime;
            timerText.text = "Time Left: " + Mathf.Ceil(currentTime).ToString() + "s"; // Cập nhật thời gian
            if (currentTime <= 0)
            {
                Destroy(runtimeUI);
                runtimeUI = null;
                timerText.gameObject.SetActive(false); // Ẩn text khi UI đóng
                Debug.Log("⏰ Thời gian đếm ngược kết thúc, UI đã tự động đóng.");
            }
        }
    }

    public void ToggleRuntimeUI()
    {
        if (runtimeUI == null)
        {
            ShowRuntimeUI();
            Debug.Log("✅ UI RuntimeTetrominoEditor đã được bật với thời gian đếm ngược: " + uiTimer + " giây.");
        }
        else
        {
            Destroy(runtimeUI);
            runtimeUI = null;
            if (timerText != null) timerText.gameObject.SetActive(false); // Ẩn text khi đóng thủ công
            Debug.Log("✅ UI RuntimeTetrominoEditor đã bị đóng thủ công.");
        }
    }
}