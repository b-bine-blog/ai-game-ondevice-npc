// 원거리 드론 몹 AI

using UnityEngine;

public class DroneEnemyAI : MonoBehaviour
{
    [Header("능력치 설정")]
    public float moveSpeed = 1f;       // 드론 이동 속도
    public float detectionRange = 8f;  // 탐지 사거리
    public float attackRange = 5f;     // 공격 사거리
    public float attackCooldown = 2f;  // 공격 쿨다운
    public float attackDamage = 10f;

    [Header("공격 차징(조준) 설정")]
    public float chargeTime = 1f;    // 멈춰서 조준(차징)하는 시간
    private bool isCharging = false;   // 현재 조준 중인지 확인
    private float chargeStartTime;     // 조준 시작 시간

    [Header("체력 설정")]
    public int maxHealth = 3;
    private int currentHealth;

    private Transform player;
    private float lastAttackTime;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();

        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if (target != null) player = target.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. 공격 준비(차징) 중일 때
        if (isCharging)
        {
            rb.linearVelocity = Vector2.zero; // 조준 중엔 무조건 정지

            if (Time.time >= chargeStartTime + chargeTime)
            {
                ExecuteAttack(distance);
            }
        }
        // 2. 공격 사거리 안일 때
        else if (distance <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCharging();
            }
            else
            {
                // 쿨다운 대기 중 정지
                rb.linearVelocity = Vector2.zero;
            }
        }
        // 3. 탐지 사거리 안일 때 (다가감)
        else if (distance <= detectionRange)
        {
            MoveToPlayer();
        }
        // 4. 사거리 밖 (대기)
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        rb.linearVelocity = Vector2.zero; // 이동 멈춤

        // 공격 애니메이션 실행 (Any State -> Attack으로 넘어감)
        if (anim != null) anim.SetTrigger("Attack");

        Debug.Log("드론 조준 시작! 삐빅...");
    }

    void ExecuteAttack(float currentDistance)
    {
        isCharging = false;
        lastAttackTime = Time.time;

        // 1.5초 후에도 플레이어가 사거리 안에 있으면 데미지 적용
        if (currentDistance <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"💥 드론 공격 적중! 데미지: {attackDamage}");
            }
        }
        else
        {
            Debug.Log("💨 플레이어가 사거리를 벗어나서 드론 공격 빗나감!");
        }
    }

    void MoveToPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // 이동 방향에 맞춰 뒤집기
        transform.localScale = new Vector3(direction.x > 0 ? -5 : 5, 5, 5);
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
        Debug.Log($"{gameObject.name} 파괴됨");

        EnemyCounter counter = Object.FindFirstObjectByType<EnemyCounter>();
        if (counter != null) counter.OnEnemyDeath();

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
