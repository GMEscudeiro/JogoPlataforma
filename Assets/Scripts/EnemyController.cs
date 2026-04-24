using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public LayerMask groundLayer;
    
    [Header("Combat")]
    public int damageAmount = 1;
    public float bounceForce = 5f;

    private Rigidbody2D rb;
    private Collider2D coll;
    private bool movingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        rb.linearVelocity = new Vector2((movingRight ? speed : -speed), rb.linearVelocity.y);

        Vector2 originForGround = movingRight ? new Vector2(coll.bounds.max.x, coll.bounds.min.y) : new Vector2(coll.bounds.min.x, coll.bounds.min.y);
        Vector2 originForWall = movingRight ? new Vector2(coll.bounds.max.x, coll.bounds.center.y) : new Vector2(coll.bounds.min.x, coll.bounds.center.y);
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;

        Debug.DrawRay(originForGround, Vector2.down * 0.5f, Color.red);
        Debug.DrawRay(originForWall, direction * 0.2f, Color.blue);

        RaycastHit2D groundHit = Physics2D.Raycast(originForGround, Vector2.down, 0.5f, groundLayer);
        RaycastHit2D wallHit = Physics2D.Raycast(originForWall, direction, 0.2f, groundLayer);

        if (groundHit.collider == null || wallHit.collider != null)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        transform.eulerAngles = new Vector3(0, movingRight ? 0 : 180, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                float enemyCenterY = coll.bounds.center.y;
                float contactY = collision.contacts[0].point.y;

                if (contactY > enemyCenterY)
                {
                    player.Bounce(bounceForce);
                    Die();
                }
                else
                {
                    player.TakeDamage(damageAmount);
                }
            }
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
