using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    [Tooltip("Coloque aqui as Layers que representam o Chão/Paredes (Tilemap).")]
    public LayerMask groundLayer;
    
    [Header("Combat")]
    public int damageAmount = 1;
    [Tooltip("O quão alto o jogador vai ser arremessado quando pular na cabeça.")]
    public float bounceForce = 12f;

    private Rigidbody2D rb;
    private Collider2D coll;
    private bool movingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        // Garante que a física não fará o inimigo girar ou rolar no chão
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Aplica a velocidade horizontal e mantém a gravidade normal
        rb.linearVelocity = new Vector2((movingRight ? speed : -speed), rb.linearVelocity.y);

        // Pontos de onde medimos se a plataforma acabou ou se há parede na frente
        Vector2 originForGround = movingRight ? new Vector2(coll.bounds.max.x, coll.bounds.min.y) : new Vector2(coll.bounds.min.x, coll.bounds.min.y);
        Vector2 originForWall = movingRight ? new Vector2(coll.bounds.max.x, coll.bounds.center.y) : new Vector2(coll.bounds.min.x, coll.bounds.center.y);
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;

        // Visualização no modo Editor das "antenas" que o Inimigo usa pra enxergar
        Debug.DrawRay(originForGround, Vector2.down * 0.5f, Color.red);
        Debug.DrawRay(originForWall, direction * 0.2f, Color.blue);

        RaycastHit2D groundHit = Physics2D.Raycast(originForGround, Vector2.down, 0.5f, groundLayer);
        RaycastHit2D wallHit = Physics2D.Raycast(originForWall, direction, 0.2f, groundLayer);

        // Se NÃO há chão na frente (Hit é nulo) OU Há um objeto bloqueando a frente, vire!
        if (groundHit.collider == null || wallHit.collider != null)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        // Gira o sprite 180 graus sem alterar atributos de escala que estragam física
        transform.eulerAngles = new Vector3(0, movingRight ? 0 : 180, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se quem bateu tinha a Tag de Player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // Logica do Super Mario: Se a colisão aconteceu acima do centro ou barriga do inimigo, foi um pisão!
                float enemyCenterY = coll.bounds.center.y;
                float contactY = collision.contacts[0].point.y;

                if (contactY > enemyCenterY)
                {
                    // O jogador alcançou a cabeça! Rebater e matar o Inimigo
                    player.Bounce(bounceForce);
                    Die();
                }
                else
                {
                    // Bateram de frente ou de lado: Dano no Player.
                    player.TakeDamage(damageAmount);
                }
            }
        }
    }

    private void Die()
    {
        // Aqui mais para frente poderíamos criar particular ou animação. Por enquanto é só destruído
        Destroy(gameObject);
    }
}
