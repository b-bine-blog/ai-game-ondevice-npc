// 몹 체력과 사망 처리

using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 4; // 4번 맞으면 죽음
    private int currentHealth;
    private bool isDead = false;
    private bool hasTriggeredCount = false;

    // ⭐ EnemyZoneTrigger에서 사용할 무적 상태 체크 변수
    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 🎯 [핵심] RedObjectZone 진입 시 에러를 해결해 줄 무적 설정 함수!!
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;

        if (isInvincible)
        {
            Debug.Log($"{gameObject.name}이(가) 제한 구역의 제어를 받아 무적 상태가 되었습니다.");
        }
    }

    // 💥 총알에 맞을 때마다 실행되는 함수
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // ⭐ 구역에 들어와서 무적 상태라면 공격 연산을 아예 무시(통과)합니다.
        if (isInvincible)
        {
            return;
        }

        currentHealth -= damage;

        // 콘솔 창에 남은 체력 글자를 띄웁니다.
        Debug.Log($"{gameObject.name}이(가) 공격받았습니다! 남은 체력: {currentHealth}");

        // 남은 체력이 1이 되는 순간 즉시 매니저 카운트를 올립니다.
        if (currentHealth == 1 && !hasTriggeredCount)
        {
            hasTriggeredCount = true;
            if (EnemyDeathManager.Instance != null)
            {
                EnemyDeathManager.Instance.MonsterDied();
            }
        }

        // 완전히 피가 다 닳으면 오브젝트 삭제
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttack") || collision.gameObject.name.Contains("Attack"))
        {
            TakeDamage(1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack") || collision.gameObject.name.Contains("Attack"))
        {
            TakeDamage(1);
        }
    }
}
