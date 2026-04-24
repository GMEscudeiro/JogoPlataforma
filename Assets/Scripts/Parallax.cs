using UnityEngine;

public class Parallax : MonoBehaviour
{

    private float length;
    public float parallaxEffect;

    void Start()
    {
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        transform.position += Vector3.left * Time.deltaTime * parallaxEffect;
        if(transform.position.x < -length ) {
            transform.position = new Vector3(length, transform.position.y, transform.position.z);
        }
    }
}
