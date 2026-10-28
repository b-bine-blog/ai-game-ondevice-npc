// 2지역 통로 개방 조건 관리자
// - 몹 3마리 처치 OR (이진수 퍼즐 완료 AND 발판 퍼즐 완료) 이면 다음 통로 개방

using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyDeathManager : MonoBehaviour
{
    public static EnemyDeathManager Instance { get; private set; }

    [Header("설정")]
    public int targetDeathCount = 3;
    private int currentDeathCount = 0;

    [Header("연결할 타일맵")]
    public Tilemap targetTilemap;
    public Color startColor = Color.black;

    // 퍼즐 상태 관리
    public static bool isEnemyCleared = false;
    public static bool isBinaryPuzzleCleared = false;
    public static bool isPlatePuzzleCleared = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 몬스터 처치 시 호출
    public void MonsterDied()
    {
        if (isEnemyCleared) return;
        currentDeathCount++;
        if (currentDeathCount >= targetDeathCount)
        {
            isEnemyCleared = true;
            CheckConditions();
        }
    }

    // 외부 퍼즐에서 호출할 신호 (Binary, Plate)
    public void SetPuzzleCleared(string puzzleName, bool cleared)
    {
        if (puzzleName == "Binary") isBinaryPuzzleCleared = cleared;
        else if (puzzleName == "Plate") isPlatePuzzleCleared = cleared;
        CheckConditions();
    }

    private void CheckConditions()
    {
        // 몬스터 3마리 처치 완료 OR (이진 퍼즐 완료 AND 발판 퍼즐 완료)
        bool isAnyConditionMet = isEnemyCleared || (isBinaryPuzzleCleared && isPlatePuzzleCleared);

        if (targetTilemap != null)
        {
            targetTilemap.color = isAnyConditionMet ? Color.white : startColor;
        }
    }
}
