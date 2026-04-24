using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    [Tooltip("Layer que representa o chão para podermos pular.")]
    public LayerMask groundLayer;

    [Header("Climbing Settings")]
    public float climbSpeed = 4f;
    [Tooltip("Layer que agrupa somente as escadas.")]
    public LayerMask ladderLayer;

    [Header("Health & Combat")]
    public int maxLives = 3;
    public int currentLives;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private CapsuleCollider2D coll;

    private Vector2 moveInput;
    private bool isClimbing;
    private bool isTouchingLadder;
    private float defaultGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<CapsuleCollider2D>();
    }

    private void Start()
    {
        currentLives = maxLives;
        defaultGravity = rb.gravityScale;
    }

    public void TakeDamage(int damage)
    {
        currentLives -= damage;
        Debug.Log("Você tomou dano! Vidas: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
    }

    public void Bounce(float bounceForce)
    {
        // Força impulsionada pra cima quando pula no inimigo
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
    }

    private void Die()
    {
        Debug.Log("GAME OVER! Reiniciando cena...");
        // Reinicia a fase caso as vidas acabem
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // This method is called by the PlayerInput component via "Send Messages"
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Chamado pelo PlayerInput (New Input System) quando apertamos o botão de Pulo
    public void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private bool IsGrounded()
    {
        // Dispara um raio minúsculo só pra checar se estamos colados no chão
        Vector2 origin = new Vector2(coll.bounds.center.x, coll.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }

    private void Update()
    {
        // Checa se tem alguma escada atrás de nós o tempo todo
        CheckLadder();

        // Só começa a escalar DE FATO se estivermos na escada e apertarmos pra cima (W) ou pra baixo (S)
        if (isTouchingLadder && Mathf.Abs(moveInput.y) > 0.1f)
        {
            isClimbing = true;
        }

        // Se sair da escada perdemos a capacidade de escalar
        if (!isTouchingLadder)
        {
            isClimbing = false;
        }

        // Flip sprite based on movement direction
        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true;
        }

        // Update Animator (we assume a boolean parameter "isWalking" exists in the Animator)
        bool isWalking = Mathf.Abs(moveInput.x) > 0.01f;
        animator.SetBool("isWalking", isWalking);
    }

    private void CheckLadder()
    {
        // Verifica se há algo da layer 'ladderLayer' cruzando nosso corpo
        Collider2D ladderColl = Physics2D.OverlapBox(coll.bounds.center, coll.bounds.size, 0f, ladderLayer);
        isTouchingLadder = (ladderColl != null);
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            // Tira a gravidade (para não escorregar pela corda) e aplica velocidade Cima/Baixo/Esquerda/Direita
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, moveInput.y * climbSpeed);
        }
        else
        {
            // Devolve a gravidade e anda normalmente
            rb.gravityScale = defaultGravity;
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }
}
