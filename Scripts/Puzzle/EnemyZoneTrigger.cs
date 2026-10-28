// 숨은 퍼즐: 몹을 특정 구역으로 유도하면 발동하는 트리거

using UnityEngine;

public class EnemyZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 들어온 오브젝트의 태그가 "Enemy"이거나, 이름에 "Enemy" 또는 "Square"가 포함되어 있다면
        if (collision.CompareTag("Enemy") || collision.gameObject.name.Contains("Enemy") || collision.gameObject.name.Contains("Square"))
        {
            Debug.Log($"🚨 {collision.gameObject.name} 구역 진입! 작동 정지 및 무적 고정 상태가 됩니다.");

            // 1. 이동 AI 스크립트 강제 정지 (더 이상 플레이어를 추적하지 않음)
            MonoBehaviour enemyAI = collision.gameObject.GetComponent("MeleeEnemyAI") as MonoBehaviour;
            if (enemyAI != null)
            {
                enemyAI.enabled = false;
            }

            // 2. 물리 속도 0으로 만들고 Kinematic으로 고정 (플레이어가 밀어도 밀리지 않음)
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // 3. 애니메이션 멈추기 (그 자리에 프레임 얼려버리기)
            Animator anim = collision.gameObject.GetComponent<Animator>();
            if (anim != null)
            {
                anim.speed = 0f;
            }

            // 4. 피격 안 되게 만들기 (EnemyHealth 스크립트 무적 처리)
            EnemyHealth health = collision.gameObject.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.SetInvincible(true);
            }

            // ==========================================================
            // 🎯 [핵심] 기존 처치 조건에 더해, 구역 가두기 성공 시에도 하얀색 타일맵 발동!
            // ==========================================================
            Debug.Log("🎉 몹을 구역에 가두는 데 성공했습니다! 타일맵을 하얀색으로 변경합니다.");

            if (EnemyDeathManager.Instance != null)
            {
                // 기존에 몹이 체력이 다 닳아 죽었을 때 카운트를 올리고 타일맵을 하얗게 바꾸던 
                // 그 똑같은 핵심 제어 함수를 여기서도 강제로 트리거(실행)해 줍니다!
                EnemyDeathManager.Instance.MonsterDied();
            }
        }
    }
}
