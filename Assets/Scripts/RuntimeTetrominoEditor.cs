using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuntimeTetrominoEditor : MonoBehaviour
{
    public GameObject runtimeUI;
    public GameObject dropdownPrefab;

    private Tetromino currentType;

    void ShowRuntimeUI()
    {
        runtimeUI = new GameObject("RuntimeUI");
        runtimeUI.transform.SetParent(GameObject.Find("Canvas").transform);

        RectTransform rt = runtimeUI.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 100);
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
        ddRT.anchoredPosition = Vector2.zero;

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
    }

    void ApplyTetromino(Tetromino type)
    {
        var piece = FindObjectOfType<Piece>();
        if (piece != null)
        {
            TetrominoData newData = piece.board.tetrominoes[(int)type];
            newData.Initialize(); // Đúng chính tả

            piece.board.Clear(piece);
            piece.SetData(type, newData);
            piece.board.Set(piece);
            piece.AnimateTransformChange();

            Debug.Log($"✅ Đã đổi sang khối: {type}");
        }
        else
        {
            Debug.LogWarning("❗Không tìm thấy Piece để áp dụng khối mới.");
        }
    }

    public void ToggleRuntimeUI()
    {
        if (runtimeUI == null)
        {
            ShowRuntimeUI();
        }
        else
        {
            Destroy(runtimeUI);
            runtimeUI = null;
        }
    }
}
