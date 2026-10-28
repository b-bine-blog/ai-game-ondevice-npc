// 포탈: 엔딩구역으로 이동

using UnityEngine;
using UnityEngine.SceneManagement;

public class ToEndingPortal : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string targetSceneName = "엔딩구역";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 태그 확인
        if (collision.CompareTag("Player"))
        {
            // 씬 이동 시도
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log("엔딩구역으로 이동합니다.");
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("이동할 씬 이름이 설정되지 않았습니다!");
            }
        }
    }
}
