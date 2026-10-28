// NPC '엘리스' 채팅 관리자 (온디바이스 LLM 연동의 중심)
// - 플레이어 메시지를 받아 '기본 설정 + 호감도별 말투 규칙 + 퍼즐 진행 상황 지시문'으로 시스템 프롬프트를 조립한 뒤 LLMAgent에 전달
// - 호감도 증감은 LLM이 판단하지 않고 코드가 키워드로 판별 (퍼즐 관련 첫 질문 시 +1)
// - 디버그 키: P = 호감도 10, O = 5, I = -2

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using LLMUnity;
using System.Collections;

public class ChatManager : MonoBehaviour
{
    // 싱글톤 패턴을 위한 Instance 추가
    public static ChatManager Instance;

    [Header("UI Elements")]
    public TMP_InputField userInput;
    public TextMeshProUGUI chatHistory;
    public Button sendButton;
    public ScrollRect scrollRect;

    [Header("Friendship & Puzzle Settings")]
    public TextMeshProUGUI clearCountText;

    public static bool hasAskedAboutPlatformPuzzle = false;
    public static bool hasAskedAboutBinaryPuzzle = false;

    [Header("AI Setting")]
    public LLMAgent llmCharacter;

    private string currentAiDisplayBody = "";
    private string totalChatLog = "";

    void Awake()
    {
        // 인스턴스 초기화
        Instance = this;
    }

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnClickSend);

        userInput.onSubmit.AddListener(delegate { OnClickSend(); });

        UpdateFriendshipUI();

        totalChatLog = "<color=#FFCC00>AI:</color> 진입 확인. 복구 모드 요청을 시작합니다. 도움이 필요하시면 /help 기능을 사용하실 수 있습니다.\n";
        chatHistory.text = totalChatLog;
    }

    public void OnClickSend()
    {
        string message = userInput.text.Trim();

        if (!string.IsNullOrEmpty(message) && llmCharacter != null)
        {
            totalChatLog += $"\n<color=#00FF00>나:</color> {message}\n\n<color=#FFCC00>AI:</color> ";
            chatHistory.text = totalChatLog;

            userInput.text = "";
            currentAiDisplayBody = "";

            // 1) 현재 호감도로 말투 규칙 결정
            int currentFriendship = FriendshipSceneChanger.globalFriendship;
            string toneInstruction = GetToneInstruction(currentFriendship);

            string messageToSendToLLM = message;

            bool justClearedBinary = false;
            bool justClearedPlatform = false;

            // 2) 실패 · 리셋 문의인지 키워드로 판별 (판단을 모델이 아니라 코드가 함)
            bool askedAboutFailure = message.Contains("실패") || message.Contains("리셋") || message.Contains("재시작") || message.Contains("막힘") || message.Contains("안돼");

            // 3) 퍼즐 관련 첫 질문이면 호감도 +1 (퍼즐마다 한 번만)
            if (message.Contains("이진수") || message.Contains("2진수") || message.Contains("문제"))
            {
                if (!hasAskedAboutBinaryPuzzle)
                {
                    IncreaseFriendship(1);
                    hasAskedAboutBinaryPuzzle = true;
                    justClearedBinary = true;
                }
            }
            else if (message.Contains("퍼즐") || message.Contains("발판") || message.Contains("순서"))
            {
                if (!hasAskedAboutPlatformPuzzle)
                {
                    IncreaseFriendship(1);
                    hasAskedAboutPlatformPuzzle = true;
                    justClearedPlatform = true;
                }
            }

            // 4) NPC 기본 설정. 발판 퍼즐 정답(1, 3, 2)은 PuzzleManager의 정답 순서와 같음
            string basePrompt = "너는 남쪽 길목에서 발판 퍼즐(정답 1, 3, 2)을 안내하는 상냥한 시스템 '엘리스'다. 존댓말만 써라,";
            string puzzleStatusInstruction = "";
            // 호감도 구간별 말투 · 문장 수 제한 (10 이상 / 5 이상 / 그 외)
            string GetToneInstruction(int friendship)
            {
                if (friendship >= 10)
                    return "플레이어를 반겨주는 상냥하고 다정한 존댓말 (절대 같은 말을 반복하거나 인사를 두 번 하지 말고, 딱 두 문장 이내로만 상냥하게 대답하세요)";
                else if (friendship >= 5)
                    return "차분하고 정중한 안내원의 존댓말 (구구절절 설명하지 말고, 오직 질문에 필요한 핵심만 딱 한 문장 혹은 두 문장으로만 짧고 깔끔하게 끝내세요)";
                else
                    return "말수가 극도로 적고 차가우며, 불필요한 설명이나 변명 없이 질문에 필요한 핵심만 딱 한 문장으로 짧게 끊어 치는 건조한 말투";
            }
            // 5) 퍼즐 진행 상황별 지시문: 모델이 상황을 추측하지 않도록 코드가 상태를 알려 줌
            if (hasAskedAboutBinaryPuzzle && hasAskedAboutPlatformPuzzle)
            {
                if (justClearedBinary || justClearedPlatform)
                {
                    string targetPuzzleName = justClearedPlatform ? "발판 퍼즐(정답 1, 3, 2)" : "이진수 퍼즐";
                    puzzleStatusInstruction = $" [중요: 플레이어가 방금 마지막 조건인 '{targetPuzzleName}'에 대해 물어보았습니다. 퍼즐 힌트를 제공하면서 '이로써 모든 조건이 충족되어 다음 구역으로 넘어가는 하얀색 통로가 개방되었다'는 안내를 자연스럽게 하세요.]";
                }
                else
                {
                    if (askedAboutFailure)
                    {
                        puzzleStatusInstruction = " [상황: 이미 통로는 개방되었지만 유저가 퍼즐 실패나 리셋에 대해 묻고 있습니다. '발판 퍼즐은 실패하면 즉시 재시작이 되지 않으니, 왼쪽 방향에 있는 이전 지역으로 돌아갔다 오셔야 리셋이 되어 다시 열립니다'라는 규칙을 정중하게 설명하고, 지금은 이미 하얀색 통로가 열렸으니 그냥 지나가셔도 된다고 안내하세요.]";
                    }
                    else
                    {
                        puzzleStatusInstruction = " [상황: 이미 모든 퍼즐 조건이 충족되어 다음 구역으로 가는 하얀색 통로가 활성화되어 있습니다. 통로가 열렸으니 어서 다음 지역으로 이동하라고 정중하게 안내하세요.]";
                    }
                }
            }
            else
            {
                if (askedAboutFailure || message.Contains("발판"))
                {
                    puzzleStatusInstruction = " [현재 상황: 다음 통로가 막혀있습니다. 특히 유저에게 '발판 퍼즐은 실패 시 그 자리에서 바로 재시작이 되지 않으므로, 왼쪽으로 이동하여 이전 지역으로 완전히 돌아갔다 오셔야 발판이 리셋되어 다시 열립니다'라는 인게임 규칙을 반드시 포함해서 설명하세요. 더불어 다음 구역으로 가려면 이진수 퍼즐과 발판 퍼즐을 모두 질문해야 한다는 점도 넌지시 언급하세요.]";
                }
                else
                {
                    puzzleStatusInstruction = " [현재 상황: 아직 퍼즐 조건이 부족하여 다음 지역 통로가 막혀있습니다. 플레이어가 다음 구역으로 넘어가려면 '이진수 퍼즐'과 '발판 퍼즐' 두 가지에 대해 모두 질문해야 한다고 자연스럽게 시스템 안내처럼 힌트를 주며 대답하세요.]";
                }
            }

            // 6) 최종 시스템 프롬프트 = 기본 설정 + 말투 규칙 + 상황 지시문
            llmCharacter.systemPrompt = $"{basePrompt} [현재 태도: {toneInstruction}]{puzzleStatusInstruction}";

            llmCharacter.Chat(messageToSendToLLM, HandleReply, ReplyCompleted);
            userInput.ActivateInputField();
        }
    }

    void IncreaseFriendship(int amount)
    {
        FriendshipSceneChanger.globalFriendship += amount;
        UpdateFriendshipUI();
        Debug.Log($"[채팅 힌트] 우호도 상승! 현재: {FriendshipSceneChanger.globalFriendship}");
    }

    // 다른 클래스(EnemyCounter)에서 호출할 수 있도록 public으로 변경
    public void UpdateFriendshipUI()
    {
        if (clearCountText != null)
        {
            clearCountText.text = FriendshipSceneChanger.globalFriendship.ToString();
        }
    }

    // 테스트용 단축키: 호감도를 바로 바꿔 엔딩 · 난이도 분기를 확인
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            FriendshipSceneChanger.globalFriendship = 10;
            UpdateFriendshipUI();
            if (llmCharacter != null) llmCharacter.ClearHistory();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            FriendshipSceneChanger.globalFriendship = 5;
            UpdateFriendshipUI();
            if (llmCharacter != null) llmCharacter.ClearHistory();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            FriendshipSceneChanger.globalFriendship = -2;
            UpdateFriendshipUI();
            if (llmCharacter != null) llmCharacter.ClearHistory();
        }
    }

    void HandleReply(string fullResponse)
    {
        currentAiDisplayBody = fullResponse;
        chatHistory.text = totalChatLog + currentAiDisplayBody;
        StartCoroutine(ScrollToBottom());
    }

    void ReplyCompleted()
    {
        totalChatLog += currentAiDisplayBody + "\n";
    }

    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }
}
