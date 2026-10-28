// 인트로 문장을 한 글자씩 출력 (한 번 본 뒤에는 건너뜀)

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public Text introText;           // 화면 중앙 텍스트
    public string[] introLines;      // 출력할 인트로 문장들
    public float typingSpeed = 0.1f; // 한 글자 출력 간격(초)
    public float displayTime = 2f;   // 한 문장을 보여 주는 시간(초)

    void Start()
    {
        // 인트로를 이미 본 경우(GameMenu에 기록) 건너뜀
        if (GameMenu.Instance != null && GameMenu.Instance.isIntroPlayed)
        {
            // 텍스트를 숨기고 바로 종료
            introText.gameObject.SetActive(false);
            return;
        }

        // 페이드 인 시간에 맞춰 1초 뒤 시작
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(1.0f);

        foreach (string line in introLines)
        {
            introText.gameObject.SetActive(true);
            introText.text = "";

            foreach (char letter in line.ToCharArray())
            {
                introText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(displayTime);
            introText.text = "";
        }

        introText.gameObject.SetActive(false);

        // 모든 문장을 출력하면 '인트로 재생 완료'로 기록
        if (GameMenu.Instance != null)
        {
            GameMenu.Instance.isIntroPlayed = true;
        }
    }
}
