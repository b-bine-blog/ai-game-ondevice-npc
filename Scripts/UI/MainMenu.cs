// 타이틀 화면 시작 · 종료 버튼

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // 시작 버튼: 시작구역 씬으로 이동
        SceneManager.LoadScene("시작구역");
    }

    // 종료 버튼
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 클릭");
        Application.Quit();
    }
}
