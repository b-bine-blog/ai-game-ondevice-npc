// 플레이어 체력 · 피격 효과음 · 게임오버 / 재시작

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬(맵)을 다시 시작하기 위해 필요합니다!

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;

    [Header("Game Over UI")]
    public GameObject gameOverPanel; // 1단계에서 만든 GameOverPanel을 넣을 공간

    // 👇 추가된 사운드 설정
    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioClip hitSound; // 맞을 때 나는 소리

    void Start()
    {
        // 게임이 시작될 때 시간 속도를 정상(1)으로 설정
        Time.timeScale = 1f;

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 시작할 때는 꺼두기
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        // 👇 맞았을 때 효과음 재생!
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("플레이어가 사망했습니다!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 게임 오버 창 띄우기
        }

        Time.timeScale = 0f; // 게임의 시간을 멈춰서 플레이어와 몹이 안 움직이게 만듭니다.
    }

    // [다시 시작] 버튼에 연결할 함수
    public void RestartGame()
    {
        // 현재 열려있는 씬(맵)을 처음부터 다시 로드합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
