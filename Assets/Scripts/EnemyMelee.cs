using UnityEngine;


// ���� ������ �ϴ� �� AI Ŭ����
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

    /*********************************************************************************************
    �Լ�: AttackProcess
    ���: 
    1. Attack1 �Ǵ� Attack2�� Ʈ���Ÿ� �ִϸ����Ϳ��� �Ѱ���
    2. ���� Ÿ���� ü�� ����
    *********************************************************************************************/
    protected override void AttackProcess()
    {
        int attackNum = Random.Range(1, 3);
        enemyAnimator.SetTrigger($"Attack{attackNum}");
        if (target.TryGetComponent<MainTarget>(out MainTarget mainTarget))
        {
            mainTarget.OnTargetDamaged(attackPower);
        }
    }

    /*********************************************************************************************
    �Լ�: Die
    ���: 
    1. ���¸� Die�� ����
    2. �÷��̾�� Coin�� ��
    3. �ڽ��� ��ġ���� ���� ����Ʈ ����
    4. �ִϸ������� Death Ʈ���Ÿ� �ߵ��ϰ� �ִϸ����Ͱ� ����Ǵµ��� �������� �ʵ��� ������Ʈ�� ��
    *********************************************************************************************/
    protected override void Die()
    {
        state = DroneState.Die;

        GameManager.Instance.AddPlayerCoin(enemyCoin);
        GameManager.Instance.SpawnExplosionParticle(transform);

        enemyAnimator.SetTrigger("Death");
        agent.enabled = false;

        Destroy(gameObject, 2.18f);
    }

    /*********************************************************************************************
    �Լ�: UpdateAnimationSpeed
    ���: ������Ʈ�� �ӵ��� �ִϸ����Ϳ��� ����
    *********************************************************************************************/
    private void UpdateAnimationSpeed()
    {
        float speed = agent.velocity.magnitude;
        enemyAnimator.SetFloat("Speed", speed);
    }
}
