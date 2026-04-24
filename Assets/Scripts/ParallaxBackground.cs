using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    private float length, startpos;
    public GameObject cam;
    public float parallaxEffect;

    [Tooltip("Coloque 2 se você está usando duas imagens uma do lado da outra.")]
    public int numberOfImages = 2;

    void Start()
    {
        if (cam == null) cam = Camera.main.gameObject;
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        
        // Determina que parte do movimento da câmera ignoramos para o reset (loop)
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        
        // Determina quanto exatamente o fundo deve andar para acompanhar
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // Se a câmera passou de toda a largura da nossa imagem, teletransportamos a imagem
        // (multiplicado pelo numero de imagens que temos lado a lado para pular por todas elas)
        if (temp > startpos + length) 
        {
            startpos += length * numberOfImages;
        }
        else if (temp < startpos - length) 
        {
            startpos -= length * numberOfImages;
        }
    }
}
