// 엔딩 구역 트리거 신호를 EndingManager로 전달

using UnityEngine;

public class EndingTriggerRelay : MonoBehaviour
{
    public EndingManager endingManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (endingManager != null)
            {
                endingManager.StartEndingSequence();
            }
        }
    }
}
