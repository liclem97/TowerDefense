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

        // 반지름 5인 구에 오버랩 된 Drone 레이어 마스크를 가진 게임 오브젝트를 모두 찾음
        Collider[] drones = Physics.OverlapSphere(transform.position, range, layerMask);

        foreach (Collider droneCollider in drones)  // 배열 순회
        {
            float distance = Vector3.Distance(transform.position, droneCollider.transform.position);    // 폭탄이 터진 위치와 적 위치의 거리를 구함
            float damageRatio = Mathf.Clamp01(1 - (distance / range));                                  // 거리 비례 데미지 감소율
            float damage = maxDamage * damageRatio;                                                     // 최대 대미지 * 감소율

            DroneAI hitDrone = droneCollider.GetComponent<DroneAI>();                                   // DroneAI 컴포넌트가 있는지 검사
            if (hitDrone)
            {
                hitDrone.OnDamageProcess(damage);                                                       // 적 대미지
            }
        }

        GameManager.Instance.SpawnExplosionParticle(transform);                                         // 게임 매니저에 있는 폭발 이펙트 함수 호출
        Destroy(gameObject);
    }
}