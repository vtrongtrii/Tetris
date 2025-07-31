using UnityEngine;
using UnityEngine.SceneManagement; // Cần thiết để quản lý scene

public class MenuManager : MonoBehaviour
{
    // Hàm này sẽ được gọi khi nút "Play" được nhấp
    public void PlayGame()
    {
        // Tải scene game của bạn.
        // Thay thế "GameScene" bằng tên chính xác của scene game của bạn.
        // Đảm bảo scene "GameScene" đã được thêm vào Build Settings.
        SceneManager.LoadScene("Tetris"); // Ví dụ: tên scene game là "GameScene"
        Debug.Log("Chuyển sang scene Game!");
    }

    // Hàm này sẽ được gọi khi nút "Quit" được nhấp
    public void QuitGame()
    {
        // Thoát ứng dụng (chỉ hoạt động trong bản build, không hoạt động trong Unity Editor)
        Application.Quit();

        // Để kiểm tra trong Unity Editor, bạn có thể thêm một Debug.Log
        Debug.Log("Thoát trò chơi!");

        // Nếu bạn muốn dừng chơi trong Editor, bạn có thể dùng dòng này (chỉ dành cho Editor)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}