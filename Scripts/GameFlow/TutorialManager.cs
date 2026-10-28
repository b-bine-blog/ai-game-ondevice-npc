// 튜토리얼 진행 단계 저장 (static이라 씬이 바뀌어도 유지)

using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // static 변수라 씬이 바뀌어도 값이 초기화되지 않음
    // 0: 시작 전, 1: 진행 중, 2: 미션 완료(보고 대기), 3: 튜토리얼 완료
    public static int tutorialState = 0;
}
