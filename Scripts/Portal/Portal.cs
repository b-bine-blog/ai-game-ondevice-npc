// 포탈: 플레이어가 닿으면 시작구역으로 이동

using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    // 이동할 씬 이름 (현재는 아래 LoadScene에 "시작구역"을 직접 지정해 사용)
    public string targetSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 시작구역으로 이동
            SceneManager.LoadScene("시작구역");
        }
    }
}
