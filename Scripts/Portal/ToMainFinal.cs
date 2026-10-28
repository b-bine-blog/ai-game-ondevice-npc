// 포탈: 메인구역으로 강제 이동

using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMainFinal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 테스트용 강제 이동
            Debug.Log("강제로 메인구역 이동 시도!");
            SceneManager.LoadScene("메인구역");
        }
    }
}
