using UnityEngine;

public class Bomb : MonoBehaviour
{
    Transform explosion;
    ParticleSystem expEffect;
    AudioSource expAudio;

    public float range = 5f;
    public float maxDamage = 10f;

    private void Start()
    {
        //explosion = GameObject.Find("SmallExplosionEffect").transform;
        //expEffect = explosion.GetComponent<ParticleSystem>();
        //expAudio = explosion.GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        int layerMask = 1 << LayerMask.NameToLayer("Drone");

        Collider[] drones = Physics.OverlapSphere(transform.position, range, layerMask);

        foreach (Collider droneCollider in drones)
        {
            float distance = Vector3.Distance(transform.position, droneCollider.transform.position);
            float damageRatio = Mathf.Clamp01(1 - (distance / range));  // °Å¸® ºñ·Ê °¨¼è
            float damage = maxDamage * damageRatio;

            DroneAI hitDrone = droneCollider.GetComponent<DroneAI>();
            if (hitDrone != null)
            {
                hitDrone.OnDamageProcess(damage);
            }
        }

        //explosion.position = transform.position;
        //expEffect.Play();
        //expAudio.Play();
        GameManager.Instance.SpawnExplosionParticle(transform);

        Destroy(gameObject);
    }
}