using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DroneAI : MonoBehaviour
{
    protected enum DroneState { Idle, Move, Attack, Damage, Die }
    //protected enum EnemyType { Drone, Melee }

    [Header("State Timers")]
    [SerializeField] protected float idleDelayTime = 2f;
    [SerializeField] protected float attackDelayTime = 2f;
    [SerializeField] protected float damageFreeze = 0.15f;

    [Header("Movement / Combat")]
    [SerializeField] protected float moveSpeed = 1f;
    [SerializeField] protected float attackRange = 3f;
    [SerializeField] protected float attackPower = 2f;    
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected LayerMask targetLayerMask;
    [SerializeField] private GameObject attackEffect;

    [Header("Stats")]
    [SerializeField] protected float hp;
    [SerializeField] protected float startHP = 10f;
    [SerializeField] protected int enemyCoin; // 적이 죽으면 플레이어가 획득하는 코인

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;

    [Header("Effect References")]
    [SerializeField] protected Transform explosion;
    [SerializeField] protected ParticleSystem expEffect;
    [SerializeField] protected AudioSource expAudio;
    [SerializeField] private Transform meshTransform;

    protected DroneState state = DroneState.Idle;
    //protected EnemyType enemyType = EnemyType.Drone;
    protected float currentTime;
    protected Transform target;
    protected NavMeshAgent agent;
    protected NavMeshObstacle obstacle;

    void Start()
    {
        InitEnemy();      
    }

    protected void InitEnemy()
    {
        //target = GameObject.Find("Target").transform; // 씬에서 타겟을 찾음
        target = GameManager.Instance.mainTarget.transform;
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();

        agent.enabled = false;      // 내비게이션을 할당 받고 바로 비활성화
        agent.speed = moveSpeed;    // moveSpeed로 이동속도 설정
        obstacle.enabled = false;   // 장애물 설정 끔

        hp = startHP;
        healthSlider.value = (hp / startHP) * 100;

        //if (explosion == null)
        //    explosion = GameObject.Find("SmallExplosionEffect").transform;
        //if (expEffect == null) expEffect = explosion.GetComponent<ParticleSystem>();
        //if (expAudio == null) expAudio = explosion.GetComponent<AudioSource>();
    }

    void Update()
    {
        switch (state)
        {
            case DroneState.Idle: Idle(); break;
            case DroneState.Move: Move(); break;
            case DroneState.Attack: Attack(); break;
            case DroneState.Damage: break;
            case DroneState.Die: break;
        }
    }

    void Idle()
    {
        currentTime += Time.deltaTime;   // 경과 시간 누적
        if (currentTime > idleDelayTime) // 경과 시간이 대기 시간을 초과함
        {
            agent.enabled = true;
            state = DroneState.Move;    //이동 상태로 전환
            if (target)
            {
                agent.SetDestination(target.position);   // 에이전트 목적지 설정
            }
        }
    }

    void Move()
    {
        // 타겟이 나의 공격 거리 안에 있음
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            state = DroneState.Attack;  // 상태를 공격으로 전환
            currentTime = 0f;   // 이동 중에는 경과시간이 누적되지 않음
        }
    }

    protected void Attack()
    {
        currentTime += Time.deltaTime;      // 경과 시간 누적
        if (Vector3.Distance(transform.position, target.position) <= attackRange) // 타겟이 나의 공격 거리 안에 있는 경우
        {
            agent.enabled = false;      // 공격 시 이동 중지
            obstacle.enabled = true;    // 다른 적들이 피하도록 obstacle 활성화

            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0f;  // 수직 방향 제거 → x축 회전 방지

            if (lookPos != Vector3.zero)  // 0 벡터 방지
            {
                Quaternion rot = Quaternion.LookRotation(lookPos);
                transform.rotation = rot;
            }    // 타겟을 바라봄

            if (currentTime > attackDelayTime)  // 2초에 한번 씩 공격 가능
            {
                AttackProcess();
                currentTime = 0f;       // 경과 시간 초기화
            }
        }
        else
        {
            agent.enabled = true;
            obstacle.enabled = false;
        }
    }

    protected virtual void AttackProcess()
    {
        Vector3 bulletDirection = (target.position - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, bulletDirection, out RaycastHit hitInfo, attackRange, targetLayerMask))
        {
            // TODO: 불릿 스폰 포인트에서 총알 발사 이펙트 재생
            //expEffect.Play();

            //explosion.forward = hitInfo.normal;
            //explosion.position = hitInfo.point;

            if (attackEffect)
            {
                GameObject spawnedEffect = Instantiate(attackEffect, hitInfo.point, Quaternion.identity);
                if (spawnedEffect)
                {
                    spawnedEffect.GetComponent<ParticleSystem>().Play();
                    Destroy(spawnedEffect, 3f);
                }
                //spawnedEffect.GetComponent<ParticleSystem>().Play();
            }

            Debug.DrawRay(bulletSpawnPoint.position, bulletDirection * attackRange, Color.red, 1f);
            if (hitInfo.transform.gameObject.TryGetComponent<MainTarget>(out MainTarget mainTarget))
            {
                mainTarget.OnTargetDamaged(attackPower);
            }            
        }
    }

    public virtual void OnDamageProcess(float amount)
    {
        if (state == DroneState.Die) return;

        hp -= amount; // 적의 체력 감소
        healthSlider.value = (hp / startHP) * 100;  // 체력 UI 갱신        

        if (hp > 0) // 죽지 않았으면 Damage 상태로 변경
        {
            //state = DroneState.Damage;
            //StartCoroutine(nameof(Damage));
        }
        else
        {
            Die();
        }
    }

    IEnumerator Damage()
    {
        agent.enabled = false;                                          // 길 찾기 중지
        obstacle.enabled = true;                                        // 다른 드론이 피해가도록 obstacle 활성화

        yield return new WaitForSeconds(damageFreeze);  // damageFreeze 시간동안 멈춤

        obstacle.enabled = false;   // obstacle 해제
        state = DroneState.Idle;    // Idle 상태로 변경
        currentTime = 0f;           // 경과 시간 초기화
    }

    protected virtual void Die()
    {
        GameManager.Instance.AddPlayerCoin(enemyCoin);
        state = DroneState.Die;

        GameManager.Instance.SpawnExplosionParticle(transform);
       // explosion.position = meshTransform.position;
       // expEffect.Play();
       // expAudio.Play();
        Destroy(gameObject);
    }
}
