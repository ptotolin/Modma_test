using UnityEngine;

public class PlayerController : MonoBehaviour
{
   [SerializeField] private float maxSpeed;
   [SerializeField] private float acceleration;
   [SerializeField] private float decceleration;

    private Transform curTransform;
    private Vector2 speed;

    private void Awake()
    {
        curTransform = transform;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) {
            speed.x -= acceleration * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            speed.x += acceleration * Time.deltaTime;
        }
        else {
            var sign = Mathf.Sign(speed.x);
            speed.x =  (Mathf.Abs(speed.x) - decceleration * Time.deltaTime) * sign;
        }
        
        if (Input.GetKey(KeyCode.UpArrow)) {
            speed.y += acceleration * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            speed.y -= acceleration * Time.deltaTime;
        }
        else {
            var sign = Mathf.Sign(speed.y);
            speed.y =  (Mathf.Abs(speed.y) - decceleration * Time.deltaTime) * sign;
        }

        speed.x = Mathf.Clamp(speed.x, -maxSpeed, maxSpeed);
        speed.y = Mathf.Clamp(speed.y, -maxSpeed, maxSpeed);
        curTransform.position += new Vector3( speed.x, speed.y, 0) * Time.deltaTime;
    }
}
