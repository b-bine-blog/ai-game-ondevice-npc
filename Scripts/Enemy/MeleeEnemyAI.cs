// 근접 몹 AI

using UnityEngine;

public class MeleeEnemyAI : MonoBehaviour
{
    [Header("능력치 설정")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 10f;

    [Header("체력 설정")]
    public int maxHealth = 3;
    private int currentHealth;

    private Transform player;
    private float lastAttackTime;
    private Rigidbody2D rb;

    // 1. 애니메이터를 제어할 변수 추가
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        // 2. 자식 오브젝트(enemy_idle_0)에 있는 Animator 컴포넌트를 가져옵니다.
        anim = GetComponentInChildren<Animator>();

        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if (target != null) player = target.transform;
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("🚨 플레이어를 찾을 수 없어! 플레이어 오브젝트의 Tag가 'Player'로 설정되어 있는지 확인해 줘.");
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            Attack();
        }
        else if (distance <= detectionRange)
        {
            // 2. 이 로그가 콘솔창에 미친듯이 뜬다면? -> 스크립트는 정상, 애니메이터 설정 문제!
            // 이 로그가 안 뜬다면? -> 거리가 너무 멀거나(detectionRange가 작음) 다른 문제!
            Debug.Log($"🏃 이동 중! 현재 거리: {distance}, 애니메이터 상태: {(anim != null ? "연결됨" : "연결안됨")}");
            MoveToPlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            // 3. 플레이어가 인식 범위 밖에 있어서 가만히 멈춰있을 때 (Idle 상태)
            if (anim != null) anim.SetBool("isMoving", false);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 피격! 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 사망");

        EnemyCounter counter = Object.FindFirstObjectByType<EnemyCounter>();

        if (counter != null)
        {
            counter.OnEnemyDeath();
        }
        else
        {
            Debug.LogWarning("씬에 EnemyCounter 오브젝트가 없습니다! 우호도 감소가 작동하지 않습니다.");
        }

        Destroy(gameObject);
    }

    void MoveToPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        transform.localScale = new Vector3(direction.x > 0 ? -5 : 5, 5, 5);

        // 4. 플레이어를 향해 이동 중일 때 (Move 상태)
        if (anim != null) anim.SetBool("isMoving", true);
    }

    void Attack()
    {
        rb.linearVelocity = Vector2.zero;

        if (anim != null) anim.SetBool("isMoving", false);

        // 쿨다운이 찼을 때만 공격 실행
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("플레이어 공격 애니메이션 재생!");
            if (anim != null) anim.SetTrigger("Attack");

            // --- [여기에 데미지 주는 코드를 추가합니다] ---
            // 이미 위쪽에서 player 변수(플레이어의 Transform)를 찾아두었으므로 바로 컴포넌트를 가져올 수 있습니다.
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"플레이어 체력을 {attackDamage} 깎았습니다!");
            }
            // ------------------------------------------

            lastAttackTime = Time.time;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
