/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuntimeTetrominoEditor : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Toggle[] gridToggles; // 16 toggle theo thứ tự (0,0) -> (3,3)
    public Button saveButton;
    private GameObject runtimeUI; // Giữ tham chiếu để xoá sau
    public GameObject togglePrefab; // Prefab Toggle nhỏ
    public GameObject dropdownPrefab; // TMP_Dropdown prefab
    public GameObject buttonPrefab; // Button prefab

    private Tetromino currentType;

    void Start()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(Tetromino))));
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        saveButton.onClick.AddListener(SaveShape);

        currentType = (Tetromino)dropdown.value;
        LoadShape(currentType);
    }

    void OnDropdownChanged(int index)
    {
        currentType = (Tetromino)index;
        LoadShape(currentType);
    }

    void LoadShape(Tetromino type)
    {
        foreach (var t in gridToggles) t.isOn = false;

        Vector2Int[] shape = Data.Cells[type];
        foreach (var cell in shape)
        {
            int x = cell.x + 2;
            int y = cell.y + 2;
            if (x >= 0 && x < 4 && y >= 0 && y < 4)
                gridToggles[y * 4 + x].isOn = true;
        }
    }

    void SaveShape()
    {
        List<Vector2Int> newShape = new List<Vector2Int>();

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                int index = y * 4 + x;
                if (gridToggles[index].isOn)
                    newShape.Add(new Vector2Int(x - 2, y - 2));
            }
        }

        Data.SetShape(currentType, newShape.ToArray());

        var currentPiece = FindObjectOfType<Piece>();
        if (currentPiece != null && currentPiece.data.tetromino == currentType)
        {
            currentPiece.board.Clear(currentPiece);
            currentPiece.ApplyNewShape();
            currentPiece.board.Set(currentPiece);
            currentPiece.AnimateTransformChange(); // Tuỳ chọn hiệu ứng
        }

        Debug.Log($"Đã cập nhật khối {currentType} tại vị trí hiện tại.");
    }

    void ShowRuntimeUI()
    {
        runtimeUI = new GameObject("RuntimeUI");
        runtimeUI.transform.SetParent(GameObject.Find("Canvas").transform); // gắn vào canvas

        RectTransform rt = runtimeUI.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 300);
        runtimeUI.AddComponent<CanvasRenderer>();
        Image img = runtimeUI.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.5f);

        rt.anchoredPosition = Vector2.zero;

        // Thêm dropdown chọn khối
        GameObject dd = Instantiate(dropdownPrefab, runtimeUI.transform);
        dd.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        var dropdown = dd.GetComponent<TMP_Dropdown>();
        dropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(Tetromino))));
        dropdown.onValueChanged.AddListener((i) => { currentType = (Tetromino)i; LoadShape(currentType); });

        // Thêm 16 toggle dạng lưới
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                GameObject t = Instantiate(togglePrefab, runtimeUI.transform);
                RectTransform tr = t.GetComponent<RectTransform>();
                tr.anchoredPosition = new Vector2(x * 30 - 45, y * -30 + 45);
            }
        }

        // Thêm nút Save
        GameObject btn = Instantiate(buttonPrefab, runtimeUI.transform);
        btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);
        btn.GetComponentInChildren<TMP_Text>().text = "Save";
        btn.GetComponent<Button>().onClick.AddListener(SaveShape);
    }

}*/
