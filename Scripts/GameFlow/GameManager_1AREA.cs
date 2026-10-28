// 1지역 진행 관리자
// - 찾아야 할 글리치 수를 정하고 달성 여부를 확인
// - 호감도가 -2 이하로 추방되어 돌아오면 목표가 3개 → 5개로 늘어남 (페널티)

using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager_1AREA : MonoBehaviour
{
    public static GameManager_1AREA Instance;

    [Header("UI 설정")]
    public TextMeshProUGUI messageText;
    public float messageDuration = 3f;

    [Header("퀘스트 설정")]
    public int defaultRequiredGlitches = 3; // 기본 필요 개수
    public int penaltyExtraGlitches = 2;    // 호감도 페널티 시 추가되는 개수

    private int currentRequiredGlitches;    // 현재 퀘스트 목표 개수
    private int currentGlitchesFound = 0;
    public GameObject exitDoor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 시작할 때 호감도 수치를 체크하여 목표치를 설정합니다.
        SetupQuestDifficulty();
    }

    private void SetupQuestDifficulty()
    {
        // 글로벌 호감도 수치 가져오기
        int affinity = FriendshipSceneChanger.globalFriendship;

        // 호감도가 -2 이하이면 기본 3개 + 추가 페널티 만큼 목표량 증가
        if (affinity <= -2)
        {
            currentRequiredGlitches = defaultRequiredGlitches + penaltyExtraGlitches;
            Debug.Log($"호감도 페널티로 난이도 상승! 목표: {currentRequiredGlitches}개");
        }
        else
        {
            currentRequiredGlitches = defaultRequiredGlitches;
            Debug.Log($"기본 목표 설정: {currentRequiredGlitches}개");
        }
    }

    public void OnGlitchFound(string message)
    {
        currentGlitchesFound++;

        StopAllCoroutines();
        StartCoroutine(ShowMessageCoroutine(message));

        // 현재까지 찾은 글리치가 목표치에 도달했는지 확인합니다.
        if (currentGlitchesFound >= currentRequiredGlitches)
        {
            UnlockNextArea();
        }
    }

    private IEnumerator ShowMessageCoroutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        messageText.gameObject.SetActive(false);
    }

    private void UnlockNextArea()
    {
        Debug.Log("모든 글리치를 찾았습니다! 출구를 개방합니다.");

        StopAllCoroutines();
        StartCoroutine(ShowMessageCoroutine("어디선가 문이 열리는 소리가 들린다..."));

        if (exitDoor != null)
        {
            exitDoor.SetActive(true);
        }
    }
}
