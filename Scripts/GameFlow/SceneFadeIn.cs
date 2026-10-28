// 씬 전환 페이드 인/아웃 (싱글톤)

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    // 어디서든 SceneFadeIn.Instance로 호출할 수 있게 싱글톤 처리
    public static SceneFadeIn Instance;

    public Image fadeImage;
    public float fadeDuration = 1.5f; // 페이드 시간(초)

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // [맵 진입] 씬이 시작되면 강제로 검은 화면으로 막고 시작
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1f);

        // 스르륵 밝아지기 실행
        StartFadeIn();
    }

    // ☀️ 화면을 스르륵 밝히고 싶을 때 부르는 함수
    public void StartFadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInEffect());
    }

    // 🌙 화면을 스르륵 어둡게 만들고 싶을 때 부르는 함수
    public void StartFadeOut(System.Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutEffect(onComplete));
    }

    // 페이드 인: 1.5초 동안 검은 화면의 투명도를 1 → 0으로
    IEnumerator FadeInEffect()
    {
        yield return new WaitForSeconds(0.1f);
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - (timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.gameObject.SetActive(false);
    }

    // 페이드 아웃: 투명도 0 → 1, 끝나면 전달받은 동작(씬 이동 등) 실행
    IEnumerator FadeOutEffect(System.Action onComplete)
    {
        fadeImage.gameObject.SetActive(true);
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1f);

        // 화면이 완전히 어두워진 뒤 씬 이동 등 후속 동작 실행
        if (onComplete != null) onComplete.Invoke();
    }
}
