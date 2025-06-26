using UnityEngine;

public class DroneBullet : MonoBehaviour
{
    [Header("Game Ref")]
    [SerializeField] private Collider bulletCollider;
    [SerializeField] private Rigidbody rb;

    [Header("Bullet Stat")]
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float bulletSpeed = 10f;

    private Transform target;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    private void FixedUpdate()
    {
        // target이 있으면 target까지 bulletSpeed로 rb 이동
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // other의 레이어가 tower가 아니면 return
        if (other.gameObject.layer != LayerMask.NameToLayer("Tower"))
            return;

        // other의 레이어가 tower이면 Tower.Instance.HP -= bulletDamage;
        Tower.Instance.HP -= bulletDamage;

        // 그리고 오브젝트 파괴
        Destroy(gameObject);
    }
}