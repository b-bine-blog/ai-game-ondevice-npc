// 플레이어 이동 (가속 · 감속, 최고속도는 DifficultyManager가 설정)
// - 채팅 입력 중에는 이동을 막고, Space로 NPC 상호작용 · 채팅창 열기

using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float maxSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 20f;
    public Rigidbody2D rb;

    [Header("상호작용 설정")]
    public float interactRadius = 1.5f;
    public LayerMask npcLayer;
    public GameObject chatUI;

    private Vector2 movement;
    private Vector2 mousePos;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null &&
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
        {
            movement = Vector2.zero;
            UpdateAnimation();
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (anim != null)
        {
            bool isMoving = movement.magnitude > 0;
            anim.SetBool("isRunning", isMoving);
        }
    }

    void FixedUpdate()
    {
        Vector2 targetVelocity = movement.normalized * maxSpeed;
        float accelRate = (movement.magnitude > 0) ? acceleration : deceleration;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);

        if (mousePos.x < rb.position.x) spriteRenderer.flipX = true;
        else if (mousePos.x > rb.position.x) spriteRenderer.flipX = false;
    }

    void Interact()
    {
        Collider2D npcCollider = Physics2D.OverlapCircle(rb.position, interactRadius, npcLayer);

        if (npcCollider != null)
        {
            NPCInteractable npc = npcCollider.GetComponent<NPCInteractable>();
            if (npc != null) npc.OpenInteractionWindow();
        }
        else
        {
            if (chatUI != null)
            {
                chatUI.SetActive(true);
                TMP_InputField input = chatUI.GetComponentInChildren<TMP_InputField>();
                if (input != null) input.ActivateInputField();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
