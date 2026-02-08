using UnityEngine;

public class Keeper2DController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 1.0f;
    public float top = 6f;
    public float bottom = 1f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Mathf.Abs(top-bottom);
        float back_and_forth = Mathf.PingPong(Time.time * speed, distance) + bottom;
        transform.position = new Vector3(transform.position.x, back_and_forth, transform.position.z);
    }
}
