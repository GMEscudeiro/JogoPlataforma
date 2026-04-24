using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    [Tooltip("Altura mínima (negativa) que o jogador pode cair antes de perder vida.")]
    public float fallBoundaryY = -15f;

    public float climbSpeed = 4f;
    public LayerMask ladderLayer;

    [Header("Collectibles")]
    public int coinsCollected = 0;
    private int totalCoinsInScene;
    [Tooltip("Arraste o componente de Texto (Text - TextMeshPro) das Moedas aqui")]
    public TextMeshProUGUI coinsText;

    [Header("Health & Interface")]
    public int maxLives = 3;
    public int currentLives;
    [Tooltip("Arraste das 3 imagens de Coração da sua Interface de dentro da Unity para cá")]
    public GameObject[] heartsUI;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private CapsuleCollider2D coll;

    private Vector2 moveInput;
    private bool isClimbing;
    private bool isTouchingLadder;
    private float defaultGravity;
    private Vector3 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<CapsuleCollider2D>();
    }

    private void Start()
    {
        startPosition = transform.position;

        // Como não herdamos mais as vidas, toda vez que a cena carregar recomeçamos com o máximo
        currentLives = maxLives;
        
        defaultGravity = rb.gravityScale;
        totalCoinsInScene = GameObject.FindGameObjectsWithTag("Coin").Length;

        UpdateHeartsUI();
        UpdateCoinsUI();
    }

    private void UpdateHeartsUI()
    {
        if (heartsUI == null) return;

        for (int i = 0; i < heartsUI.Length; i++)
        {
            if (heartsUI[i] != null)
            {
                // Se a vida atual for maior que o numero do coração na lista, ele aparece, senão ele some.
                heartsUI[i].SetActive(i < currentLives);
            }
        }
    }

    private void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = coinsCollected.ToString() + " / " + totalCoinsInScene.ToString();
        }
    }

    public void TakeDamage(int damage)
    {
        currentLives -= damage;
        UpdateHeartsUI();
        Debug.Log("Você tomou dano! Vidas: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
    }

    public void Bounce(float bounceForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
    }

    private void Die()
    {
        SceneManager.LoadScene("Derrota");
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

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
        if (transform.position.y < fallBoundaryY)
        {
            RespawnFromFall();
        }

        CheckLadder();

        if (isTouchingLadder && Mathf.Abs(moveInput.y) > 0.1f)
        {
            isClimbing = true;
        }

        if (!isTouchingLadder)
        {
            isClimbing = false;
        }

        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true;
        }

        bool isWalking = Mathf.Abs(moveInput.x) > 0.01f;
        animator.SetBool("isWalking", isWalking);
    }

    private void CheckLadder()
    {
        Collider2D ladderColl = Physics2D.OverlapBox(coll.bounds.center, coll.bounds.size, 0f, ladderLayer);
        isTouchingLadder = (ladderColl != null);
    }

    private void RespawnFromFall()
    {
        TakeDamage(1);
        
        if (currentLives > 0)
        {
            rb.linearVelocity = Vector2.zero; 
            transform.position = startPosition;
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, moveInput.y * climbSpeed);
        }
        else
        {
            rb.gravityScale = defaultGravity;
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se esbarramos em algo que tem a TAG "Coin"
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coinsCollected++;
            UpdateCoinsUI();

            if (coinsCollected >= totalCoinsInScene && totalCoinsInScene > 0)
            {
                WinGame();
            }
        }
    }

    private void WinGame()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Fase1")
        {
            SceneManager.LoadScene("Fase2");
        }
        else if (sceneName == "Fase2")
        {
            SceneManager.LoadScene("Vitoria");
        }
        else 
        {
             Debug.Log("Você venceu! (Configure as cenas Fase1 e Fase2 corretamente)");
        }
    }
}
