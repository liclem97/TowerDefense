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
        // target�� ������ target���� bulletSpeed�� rb �̵�
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // other�� ���̾ tower�� �ƴϸ� return
        if (other.gameObject.layer != LayerMask.NameToLayer("Tower"))
            return;

        // other�� ���̾ tower�̸� Tower.Instance.HP -= bulletDamage;
        Tower.Instance.HP -= bulletDamage;

        // �׸��� ������Ʈ �ı�
        Destroy(gameObject);
    }
}