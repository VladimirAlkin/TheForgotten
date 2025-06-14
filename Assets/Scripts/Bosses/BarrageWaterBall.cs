using UnityEngine;

public class BarrageFireball : MonoBehaviour
{
    [SerializeField] Vector2 startForceMinMax;
    [SerializeField] float turnSpeed = 0.5f;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 4f);
        rb.AddForce(transform.right * Random.Range(startForceMinMax.x, startForceMinMax.y), ForceMode2D.Impulse);
    }

    void Update()
    {
        var _dir = rb.velocity;

        if (_dir != Vector2.zero)
        {
            Vector3 _frontVector = Vector3.right;

            Quaternion _targetRotation = Quaternion.FromToRotation(_frontVector, _dir - (Vector2)transform.position);
            if (_dir.x > 0)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, turnSpeed);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180, transform.eulerAngles.z);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, turnSpeed);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            _other.GetComponent<PlayerController>().TakeDamage(Tamu.Instance.damage);
            if (PlayerController.Instance.pState.alive)
            {
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
            }
            Destroy(gameObject);
        }
    }
}
