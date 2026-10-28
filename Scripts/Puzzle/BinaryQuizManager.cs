// 이진수 퍼즐 관리자 (정답 시 호감도 +5, EnemyDeathManager에 'Binary' 완료 신호)

using UnityEngine;
using TMPro;

public class BinaryQuizManager : MonoBehaviour
{
    public TextMeshPro quizText;
    public TextMeshProUGUI clearCountText;
    public int maxNumber = 14;

    [Header("Puzzle Settings")]
    public BinaryBlock[] inputBlocks;

    private int currentAnswer;
    private int solvedCount = 0;
    private const int targetCount = 5;
    private bool isTransitioning = false;

    void Start()
    {
        solvedCount = 0;
        isTransitioning = false;

        // 게임 시작 시 이진수 퍼즐은 아직 미완료 상태이므로 false 전송
        if (EnemyDeathManager.Instance != null)
        {
            EnemyDeathManager.Instance.SetPuzzleCleared("Binary", false);
        }

        if (clearCountText != null)
        {
            clearCountText.text = FriendshipSceneChanger.globalFriendship.ToString();
        }

        GenerateNewQuiz();
    }

    public void GenerateNewQuiz()
    {
        if (solvedCount >= targetCount) return;

        isTransitioning = false;
        currentAnswer = Random.Range(1, maxNumber + 1);
        int num1 = Random.Range(0, currentAnswer + 1);
        int num2 = currentAnswer - num1;

        string binary1 = System.Convert.ToString(num1, 2).PadLeft(4, '0');
        string binary2 = System.Convert.ToString(num2, 2).PadLeft(4, '0');

        quizText.text = $"{binary1} + {binary2} = ?";
        ResetBlocks();
    }

    public void CheckAnswer()
    {
        if (isTransitioning || solvedCount >= targetCount) return;

        int playerSum = 0;
        if (inputBlocks.Length >= 4)
        {
            playerSum += inputBlocks[0].bitValue * 8;
            playerSum += inputBlocks[1].bitValue * 4;
            playerSum += inputBlocks[2].bitValue * 2;
            playerSum += inputBlocks[3].bitValue * 1;
        }

        if (playerSum == currentAnswer)
        {
            isTransitioning = true;
            solvedCount++;

            if (solvedCount >= targetCount)
            {
                OnAllPuzzlesSolved();
            }
            else
            {
                quizText.text = "CORRECT!";
                Invoke("GenerateNewQuiz", 0.8f);
            }
        }
    }

    void OnAllPuzzlesSolved()
    {
        isTransitioning = true;
        quizText.text = "SUCCESS!";

        // ⭐ 핵심: 이진수 퍼즐 완료 신호 전송
        if (EnemyDeathManager.Instance != null)
        {
            EnemyDeathManager.Instance.SetPuzzleCleared("Binary", true);
            Debug.Log("이진수 퍼즐 완료: 신호 전송됨");
        }

        if (clearCountText != null)
        {
            if (!int.TryParse(clearCountText.text, out int currentVal))
            {
                currentVal = 0;
            }

            int newVal = currentVal + 5;
            clearCountText.text = newVal.ToString();
            FriendshipSceneChanger.globalFriendship += 5;

            Debug.Log($"[퍼즐 완료] UI 점수: {currentVal} -> {newVal} / 전역 우호도: {FriendshipSceneChanger.globalFriendship}");
        }
    }

    void ResetBlocks()
    {
        foreach (var block in inputBlocks)
        {
            if (block != null) block.ResetBlock();
        }
    }
}
