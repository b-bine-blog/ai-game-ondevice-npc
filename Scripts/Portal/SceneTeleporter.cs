// 포탈: 지정한 씬으로 이동하면서 튜토리얼 단계(1 → 2)를 갱신

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("이동할 씬 이름을 정확히 입력하세요.")]
    public string targetSceneName = "메인구역";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 부딪힌 대상이 플레이어인지 확인 (태그: "Player")
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[포탈] {targetSceneName} 씬으로 이동합니다.");
            if (TutorialManager.tutorialState == 1)
            {
                TutorialManager.tutorialState = 2;
                Debug.Log("시스템: [튜토리얼 목표 달성] 이전 화면의 NPC에게 돌아가 보고하세요.");
            }
            // 2. 씬 이름이 비어 있지 않은지 확인
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("이동할 씬 이름이 설정되지 않았습니다. Inspector를 확인하세요.");
            }
        }
    }
}
