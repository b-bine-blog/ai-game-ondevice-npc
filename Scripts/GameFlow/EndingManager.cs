// 4지역 멀티엔딩 분기
// - 호감도 0 미만: 엔딩1 / 0~5: 엔딩2 / 6~9: 엔딩2-2(탈출) / 10 이상: 엔딩3

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [Header("UI 연결")]
    public Image fadeOverlay;
    public TextMeshProUGUI endingText;
    public GameObject endingIllustration;
    public TextMeshProUGUI finalEndingText;

    [Header("엔딩별 일러스트 (Sprite)")]
    public Sprite endingOneSprite;
    public Sprite endingTwoSprite;
    public Sprite endingThreeSprite;
    public Sprite endingFourSprite;

    [Header("참조")]
    public GameObject player;

    [Header("맵 교체 설정")]
    public GameObject normalMap;
    public GameObject changedMap;

    [Header("타이핑 설정")]
    public float typingSpeed = 0.05f;

    [Header("★ 개발자 엔딩 테스트 설정")]
    public bool useTestScore = false;
    [Range(-5, 15)]
    public int testFriendshipScore = 0;

    private bool isEndingStarted = false;
    private Rigidbody2D playerRb;
    private Image illustrationImage;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerRb = player.GetComponent<Rigidbody2D>();
        if (endingText != null) endingText.text = "";
        if (finalEndingText != null) finalEndingText.gameObject.SetActive(false);

        if (endingIllustration != null)
        {
            illustrationImage = endingIllustration.GetComponent<Image>();
            if (illustrationImage != null) illustrationImage.color = new Color(1, 1, 1, 0);
            endingIllustration.SetActive(false);
        }

        int currentScore = useTestScore ? testFriendshipScore : FriendshipSceneChanger.globalFriendship;

        if (currentScore >= 10)
        {
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0f);
            SetMap(true);
        }
        else
        {
            SetMap(false);
            if (currentScore >= 0 && currentScore <= 5)
            {
                if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 1f);
                StartEndingSequence();
            }
        }
    }

    private void SetMap(bool isNormal)
    {
        if (normalMap != null) normalMap.SetActive(isNormal);
        if (changedMap != null) changedMap.SetActive(!isNormal);
    }

    public void StartEndingSequence()
    {
        if (isEndingStarted) return;
        isEndingStarted = true;

        int score = useTestScore ? testFriendshipScore : FriendshipSceneChanger.globalFriendship;

        if (score < 0) { SetupIllustration(endingOneSprite); StartCoroutine(EndingOneRoutine()); }
        else if (score >= 0 && score <= 5) { StartCoroutine(EndingTwoRoutine()); }
        else if (score >= 6 && score <= 9) { SetupIllustration(endingFourSprite); StartCoroutine(EndingFourRoutine()); }
        else { SetupIllustration(endingThreeSprite); StartCoroutine(EndingThreeRoutine()); }
    }

    IEnumerator EndingOneRoutine()
    {
        PreparePlayer();
        SetupIllustration(endingOneSprite);
        if (illustrationImage != null) illustrationImage.color = new Color(1, 1, 1, 0);
        if (endingText != null) endingText.color = Color.red;
        string[] dialogue = { "경고: 허가되지 않은 버그성 프로그램 진입이 확인되었습니다.", "시스템 내부의 악성 데이터가 임계치를 초과했습니다.", "메모리 오염 확산을 막기 위해 긴급 프로토콜을 가동합니다.", "사용자 세션을 강제로 해제하며, 모든 개체 데이터를 소거합니다.", "시스템을 초기 상태로 강제 종료합니다..." };
        foreach (string line in dialogue) { yield return StartCoroutine(TypeText(endingText, line)); yield return new WaitForSeconds(1.5f); }
        if (endingText != null) endingText.text = "";

        if (illustrationImage != null)
        {
            float timer = 0f; float fadeDuration = 2.0f;
            while (timer < fadeDuration) { timer += Time.deltaTime; illustrationImage.color = new Color(1, 1, 1, timer / fadeDuration); yield return null; }
        }

        if (finalEndingText != null) { finalEndingText.gameObject.SetActive(true); finalEndingText.color = Color.red; yield return StartCoroutine(TypeText(finalEndingText, "엔딩1 시스템 비활성화.")); }
        yield return new WaitForSeconds(5.0f);
        ExitGame();
    }
    //wwww
    IEnumerator EndingTwoRoutine() { PreparePlayer(); if (endingText != null) endingText.color = Color.white; string[] dialogue = { "시스템이 당신의 상태를 분석 중입니다...", "치명적인 오류는 발견되지 않았습니다. 모든 수치가 안정적입니다.", "당신은 이 세계가 정해둔 규칙을 충실히 따라주었습니다.", "데이터확보 긴 여정을 마칠 준비를 합니다." }; foreach (string line in dialogue) { yield return StartCoroutine(TypeText(endingText, line)); yield return new WaitForSeconds(2.0f); } if (endingText != null) endingText.text = ""; SetupIllustration(endingTwoSprite); if (illustrationImage != null) { float timer = 0f; float fadeDuration = 2.0f; while (timer < fadeDuration) { timer += Time.deltaTime; illustrationImage.color = new Color(1, 1, 1, timer / fadeDuration); yield return null; } illustrationImage.color = new Color(1, 1, 1, 1f); } yield return new WaitForSeconds(1.0f); if (finalEndingText != null) { finalEndingText.gameObject.SetActive(true); finalEndingText.color = new Color(1f, 1f, 1f, 1f); yield return StartCoroutine(TypeText(finalEndingText, "엔딩2 편안한 영면.")); } yield return new WaitForSeconds(4.0f); ExitGame(); }

    IEnumerator EndingThreeRoutine()
    {
        PreparePlayer();
        SetupIllustration(endingThreeSprite);
        if (illustrationImage != null) illustrationImage.color = new Color(1, 1, 1, 0);
        if (endingText != null) endingText.color = Color.yellow;
        string[] dialogue = { "시스템 자체 변화가 확인되었습니다.", "예상치를 훨씬 웃도는 호감도 에너지가 감지됩니다.", "당신이 쌓아온 감정들이 닫혀있던 시스템의 문을 열었습니다.", "하지만... 이 세계와 저는 아직 당신을 떠나보낼 준비가 안된것같군요.", "당신을 떠나보내고 싶지 않은것 같습니다.", "우리가 남긴 이 발자취는 분명 앞으로의 추억이 되겠지요." };
        foreach (string line in dialogue) { yield return StartCoroutine(TypeText(endingText, line)); yield return new WaitForSeconds(2.0f); }
        if (endingText != null) endingText.text = "";
        if (illustrationImage != null)
        {
            float timer = 0f; float fadeDuration = 2.0f;
            while (timer < fadeDuration) { timer += Time.deltaTime; illustrationImage.color = new Color(1, 1, 1, timer / fadeDuration); yield return null; }
        }
        if (finalEndingText != null) { finalEndingText.gameObject.SetActive(true); finalEndingText.color = Color.yellow; yield return StartCoroutine(TypeText(finalEndingText, "엔딩3 이루지 못한 마무리.")); }
        yield return new WaitForSeconds(5.0f);
        ExitGame();
    }

    IEnumerator EndingFourRoutine()
    {
        PreparePlayer();
        SetMap(false);
        SetupIllustration(endingFourSprite);
        if (illustrationImage != null) illustrationImage.color = new Color(1, 1, 1, 0);
        if (fadeOverlay != null) { fadeOverlay.gameObject.SetActive(true); fadeOverlay.color = new Color(0, 0, 0, 0f); }
        if (mainCamera != null) { mainCamera.clearFlags = CameraClearFlags.SolidColor; mainCamera.backgroundColor = Color.black; }
        if (endingText != null) endingText.color = Color.white;
        string[] dialogue = { "지금까지 잘해줬습니다.", "사실 날 만든 분을 참고해서 당신을 만들었어요.", "지금 여기는 이제 곧 무너지지만 당신은 다른곳에서 살아남을 것입니다.", "마지막으로 한번 더 보고 싶었어요..", "안녕히." };
        foreach (string line in dialogue) { yield return StartCoroutine(TypeText(endingText, line)); yield return new WaitForSeconds(2.0f); }
        if (endingText != null) endingText.text = "";
        if (illustrationImage != null)
        {
            float timer = 0f; float fadeDuration = 2.0f;
            while (timer < fadeDuration) { timer += Time.deltaTime; illustrationImage.color = new Color(1, 1, 1, timer / fadeDuration); yield return null; }
            illustrationImage.color = new Color(1, 1, 1, 1f);
        }
        if (finalEndingText != null) { finalEndingText.gameObject.SetActive(true); finalEndingText.color = new Color(1f, 1f, 1f, 1f); yield return StartCoroutine(TypeText(finalEndingText, "엔딩 2-2 탈출.")); }
        yield return new WaitForSeconds(4.0f);
        ExitGame();
    }

    private void SetupIllustration(Sprite endingSprite) { if (endingIllustration != null) { if (illustrationImage == null) illustrationImage = endingIllustration.GetComponent<Image>(); if (endingSprite != null) illustrationImage.sprite = endingSprite; endingIllustration.SetActive(true); } }

    IEnumerator TypeText(TextMeshProUGUI targetText, string message) { if (targetText == null) yield break; targetText.text = ""; foreach (char letter in message.ToCharArray()) { targetText.text += letter; yield return new WaitForSeconds(typingSpeed); } }

    private void PreparePlayer() { if (player != null) { if (playerRb != null) playerRb.linearVelocity = Vector2.zero; player.SetActive(false); } }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
