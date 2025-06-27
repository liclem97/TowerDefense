using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Transform explosion;
    private ParticleSystem expEffect;
    private AudioSource expAudio;

    public float range = 5f;
    public float maxDamage = 10f;


    private void OnCollisionEnter(Collision collision)
    {
        int layerMask = 1 << LayerMask.NameToLayer("Drone");

        // ������ 5�� ���� ������ �� Drone ���̾� ����ũ�� ���� ���� ������Ʈ�� ��� ã��
        Collider[] drones = Physics.OverlapSphere(transform.position, range, layerMask);

        foreach (Collider droneCollider in drones)  // �迭 ��ȸ
        {
            float distance = Vector3.Distance(transform.position, droneCollider.transform.position);    // ��ź�� ���� ��ġ�� �� ��ġ�� �Ÿ��� ����
            float damageRatio = Mathf.Clamp01(1 - (distance / range));                                  // �Ÿ� ��� ������ ������
            float damage = maxDamage * damageRatio;                                                     // �ִ� ����� * ������

            DroneAI hitDrone = droneCollider.GetComponent<DroneAI>();                                   // DroneAI ������Ʈ�� �ִ��� �˻�
            if (hitDrone)
            {
                hitDrone.OnDamageProcess(damage);                                                       // �� �����
            }
        }

        GameManager.Instance.SpawnExplosionParticle(transform);                                         // ���� �Ŵ����� �ִ� ���� ����Ʈ �Լ� ȣ��
        Destroy(gameObject);
    }
}