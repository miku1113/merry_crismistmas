using UnityEngine;

public class SnowBall : MonoBehaviour
{
    public float fallSpeed = 2f;
    public float horizontalWind = 1f;
    public float destroyHeight = -10f;
    public float lifetime = 10f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
        transform.Translate(Vector3.right * horizontalWind * Time.deltaTime, Space.World);

        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}
