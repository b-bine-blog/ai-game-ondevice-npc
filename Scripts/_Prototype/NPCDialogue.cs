// [초기 시안] LLM 도입 전 고정 대사 NPC (최종 빌드에서는 사용하지 않음)

using UnityEngine;
using UnityEngine.UI;
using LLMUnity;
using System.Text.RegularExpressions;

public class NPCDialogue : MonoBehaviour
{
    [Header("연결 부품들")]
    public LLMCharacter llmCharacter;
    public InputField playerInput;
    public Button sendButton;
    public Text npcText;

    [Header("플레이어 상태")]
    public int playerGold = 100;
    public int reputation = 0;
    public int meetCount = 0;

    [Header("구매 확인 UI")]
    public GameObject purchasePanel;
    public Text purchaseMsgText;     // 💡 NEW: 팝업창 안의 질문 텍스트
    public Button buyButton;
    public Button cancelButton;

    private int currentFinalPrice = 50;
    private string rawAIResponse = "";
    private string pendingItem = "";
    private int pendingPrice = 0;

    void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        buyButton.onClick.AddListener(OnConfirmBuy);
        cancelButton.onClick.AddListener(OnCancelBuy);

        purchasePanel.SetActive(false);
        meetCount = PlayerPrefs.GetInt("Bolter_MeetCount", 0);
        _ = llmCharacter.Chat("안녕", null, () => Debug.Log("[시스템] AI 로딩 완료!"));
    }

    void Update()
    {
        // 개발자 테스트용: 키보드 F12 키를 누르면 친밀도 초기화!
        if (Input.GetKeyDown(KeyCode.F12))
        {
            PlayerPrefs.DeleteKey("Bolter_MeetCount"); // 하드디스크에서 기록 삭제
            meetCount = 0;                             // 현재 메모리에서도 0으로 변경
            Debug.Log("<color=#00FFFF>[시스템] 삐빅- 볼터의 기억을 지웠습니다. (친밀도 0)</color>");
        }
    }

    public void OnSendClicked()
    {
        string message = playerInput.text;
        if (string.IsNullOrEmpty(message)) return;

        npcText.text = "볼터가 인상을 쓰며 무언가 말하려 합니다...";
        playerInput.text = "";
        sendButton.interactable = false;
        rawAIResponse = "";
        purchasePanel.SetActive(false);

        string relationInfo = (meetCount == 0) ? "초면임. 아는 척 안 함." : $"구면임. 친밀도 {meetCount}.";

        int basePrice = 50;
        string repContext = (reputation >= 50) ? $"명성 높아 {30}G로 할인." : (reputation <= -50) ? $"명성 낮아 {100}G로 바가지." : $"정가 {50}G.";
        currentFinalPrice = (reputation >= 50) ? 30 : (reputation <= -50) ? 100 : basePrice;
        string affordStatus = (playerGold >= currentFinalPrice) ? "구매 가능" : "구매 불가";

        // 💡 NEW: AI가 맘대로 구매창을 띄우지 못하게 하는 절대 규칙 추가!
        string systemRule = "[절대 규칙: 플레이어가 명확하게 물건을 '사겠다', '구매하겠다', '달라'고 요구할 때만 문장 맨 끝에 [구매제안:아이템이름] 태그를 달아라. 단순한 인사나 잡담에는 절대 이 태그를 달지 마라.]";

        // AI에게 규칙과 상황을 묶어서 전달합니다.
        string ragMessage = $"{systemRule}\n[상태: 골드 {playerGold}G, 관계: {relationInfo}, 가격: {repContext}, 상점: {affordStatus}]\n플레이어: {message}";

        meetCount++;
        PlayerPrefs.SetInt("Bolter_MeetCount", meetCount);
        PlayerPrefs.Save();

        _ = llmCharacter.Chat(ragMessage, HandleReplyStream, ReplyCompleted);
    }

    void HandleReplyStream(string text)
    {
        rawAIResponse = text;
        string filteredText = Regex.Replace(rawAIResponse, @"\[구매제안:.*?\]", "");
        npcText.text = filteredText.Trim();
    }

    void ReplyCompleted()
    {
        Match match = Regex.Match(rawAIResponse, @"\[구매제안:(.*?)\]");
        if (match.Success && playerGold >= currentFinalPrice)
        {
            pendingItem = match.Groups[1].Value;
            pendingPrice = currentFinalPrice;

            // 💡 팝업창 안의 텍스트를 아이템 이름과 가격으로 바꿔줍니다!
            if (purchaseMsgText != null)
            {
                purchaseMsgText.text = $"'{pendingItem}'을(를)\n{pendingPrice}G에 구매하시겠습니까?";
            }

            purchasePanel.SetActive(true);
        }
        sendButton.interactable = true;
    }

    void OnConfirmBuy()
    {
        playerGold -= pendingPrice;
        purchasePanel.SetActive(false);
        npcText.text += $"\n\n<color=#00FF00>[시스템: 구매 완료! 남은 골드: {playerGold}G]</color>";
        Debug.Log($"[시스템] '{pendingItem}' 획득! (지불: {pendingPrice}G / 남은 골드: {playerGold}G)");
    }

    void OnCancelBuy()
    {
        purchasePanel.SetActive(false);
        npcText.text += "\n\n<color=#FF0000>[시스템: 구매를 취소했습니다.]</color>";
        Debug.Log("[시스템] 구매를 취소했습니다.");
    }
}
