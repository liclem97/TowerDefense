using UnityEngine;


// 근접 공격을 하는 적 AI 클래스
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
    함수: AttackProcess
    기능: 
    1. Attack1 또는 Attack2의 트리거를 애니메이터에게 넘겨줌
    2. 메인 타겟의 체력 감소
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
    함수: Die
    기능: 
    1. 상태를 Die로 변경
    2. 플레이어에게 Coin을 줌
    3. 자신의 위치에서 폭발 이펙트 생성
    4. 애니메이터의 Death 트리거를 발동하고 애니메이터가 실행되는동안 움직이지 않도록 에이전트를 끔
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
    함수: UpdateAnimationSpeed
    기능: 에이전트의 속도를 애니메이터에게 전달
    *********************************************************************************************/
    private void UpdateAnimationSpeed()
    {
        float speed = agent.velocity.magnitude;
        enemyAnimator.SetFloat("Speed", speed);
    }
}
