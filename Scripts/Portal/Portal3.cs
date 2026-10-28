// 포탈: 2지역 통로 개방 조건(EnemyDeathManager)을 확인한 뒤 분기점구역으로 이동

using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal3 : MonoBehaviour
{
    public string targetSceneName = "분기점구역";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 통로 개방 조건 확인 (EnemyDeathManager)
            // 몬스터 처치 완료(isEnemyCleared) 또는 (이진 퍼즐 완료 && 발판 퍼즐 완료)인 경우만 이동 허용
            bool canEnter = EnemyDeathManager.isEnemyCleared ||
                           (EnemyDeathManager.isBinaryPuzzleCleared && EnemyDeathManager.isPlatePuzzleCleared);

            if (canEnter)
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.Log("통로가 아직 활성화되지 않았습니다! 퍼즐을 풀거나 몬스터를 모두 처치하세요.");
            }
        }
    }
}
