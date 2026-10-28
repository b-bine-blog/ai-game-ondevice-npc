// 발판 퍼즐 관리자
// - 정답 순서 1 → 3 → 2 (인덱스 0, 2, 1). NPC 프롬프트의 힌트와 같은 값
// - 성공: 호감도 +1, 'Plate' 완료 신호 / 실패: 호감도 -1. 0.5초 뒤 발판 초기화

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    public TextMeshProUGUI statusText;
    public List<PressurePlate> plates = new List<PressurePlate>();

    private int[] correctOrder = { 0, 2, 1 };
    private int stepCount = 0;
    private bool isWrong = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OnTileStepped(int steppedIndex)
    {
        if (stepCount < correctOrder.Length)
        {
            if (steppedIndex != correctOrder[stepCount]) isWrong = true;
            stepCount++;
            if (stepCount == correctOrder.Length) UpdateFinalResult();
        }
    }

    private void UpdateFinalResult()
    {
        int scoreChange = 0;

        // 성공 시
        if (!isWrong)
        {
            scoreChange = 1;
            if (EnemyDeathManager.Instance != null)
                EnemyDeathManager.Instance.SetPuzzleCleared("Plate", true);
        }
        // 실패 시
        else
        {
            scoreChange = -1;
            if (EnemyDeathManager.Instance != null)
                EnemyDeathManager.Instance.SetPuzzleCleared("Plate", false);
        }

        if (statusText != null)
        {
            int.TryParse(statusText.text, out int currentVal);
            statusText.text = (currentVal + scoreChange).ToString();
        }
        FriendshipSceneChanger.globalFriendship += scoreChange;

        Invoke("ResetPuzzle", 0.5f);
    }

    public void ResetPuzzle()
    {
        stepCount = 0;
        isWrong = false;
        foreach (var plate in plates) if (plate != null) plate.isSteppedOn = false;
    }
}
