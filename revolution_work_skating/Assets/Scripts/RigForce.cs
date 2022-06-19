using UnityEngine;

public class RigForce : MonoBehaviour
{
    public float thrust = 1.0f;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        rb.AddForce(x*10.0f, 0, z*10.0f, ForceMode.Impulse);
        //Vector3 force = new Vector3(20.0f, 20.0f, 0.0f);
        //rb.AddForce(force, ForceMode.Impulse);
    }
}