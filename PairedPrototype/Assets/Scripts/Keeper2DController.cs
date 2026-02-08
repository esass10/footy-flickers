using UnityEngine;

public class Keeper2DController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 2f;
    public float top = 4f;
    public float bottom = -2f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Mathf.Abs(top - bottom);
        float pos = Mathf.PingPong(Time.time * speed, distance) + bottom;
        transform.position = new Vector3(transform.position.x, pos, transform.position.z);
    }
}
