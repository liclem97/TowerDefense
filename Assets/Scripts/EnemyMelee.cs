using UnityEngine;
using UnityEngine.AI;

public class EnemyMelee : DroneAI
{
    [Header("Animator")]
    [SerializeField] private Animator enemyAnimator;

    private void Start()
    {
        InitEnemy();     
    }

    void FixedUpdate()
    {        
        UpdateAnimationSpeed();
    }


    protected override void AttackProcess()
    {
        int attackNum = Random.Range(1, 3);
        enemyAnimator.SetTrigger($"Attack{attackNum}");
        if (target.TryGetComponent<MainTarget>(out MainTarget mainTarget))
        {
            mainTarget.OnTargetDamaged(attackPower);
        }
    }

    protected override void Die()
    {
        GameManager.Instance.AddPlayerCoin(enemyCoin);
        state = DroneState.Die;
        //explosion.position = transform.position;
        //expEffect.Play();
        //expAudio.Play();
        GameManager.Instance.SpawnExplosionParticle(transform);

        enemyAnimator.SetTrigger("Death");
        agent.enabled = false;
        Destroy(gameObject, 2.18f);
    }

    private void UpdateAnimationSpeed()
    {
        float speed = agent.velocity.magnitude;
        enemyAnimator.SetFloat("Speed", speed);
    }
}
