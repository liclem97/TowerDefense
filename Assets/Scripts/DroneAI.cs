using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DroneAI : MonoBehaviour
{
    protected enum DroneState { Idle, Move, Attack, Damage, Die }   // 드론 상태 열거형

    [Header("State Timers")]
    [SerializeField] protected float idleDelayTime = 2f;            // Idle에 있는 시간
    [SerializeField] protected float attackDelayTime = 2f;          // 공격 딜레이 타임
    [SerializeField] protected float damageFreeze = 0.05f;          // 공격 당할 시 멈춤 시간

    [Header("Movement / Combat")]
    [SerializeField] protected float moveSpeed = 1f;                // 이동속도
    [SerializeField] protected float attackRange = 3f;              // 공격거리
    [SerializeField] protected float attackPower = 2f;              // 공격 대미지
    [SerializeField] protected Transform bulletSpawnPoint;          // 총알이 나가는 위치
    [SerializeField] protected LayerMask targetLayerMask;           // 목표의 레이어 마스크
    [SerializeField] private GameObject attackEffect;               // 공격 이펙트

    [Header("Stats")]
    [SerializeField] protected float hp;                            // 체력
    [SerializeField] protected float startHP = 10f;                 // 시작 체력
    [SerializeField] protected int enemyCoin;                       // 죽을 시 플레이어가 획득하는 코인

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;                   // 체력바 UI

    [Header("Effect References")]
    [SerializeField] protected Transform explosion;                 // 폭발 이펙트
    [SerializeField] protected ParticleSystem expEffect;
    [SerializeField] protected AudioSource expAudio;
    [SerializeField] private Transform meshTransform;               // 공중에 떠있는 매쉬("드론 전용")

    protected DroneState state = DroneState.Idle;                   // 상태 저장 변수
    protected float currentTime;                                    // 경과 시간 저장 변수
    protected Transform target;                                     // 타겟의 위치
    protected NavMeshAgent agent;                                   // AI 이동
    protected NavMeshObstacle obstacle;                             // AI 장애물

    void Start()
    {
        InitEnemy();
    }

    /*********************************************************************************************
    함수: InitEnemy
    기능: 적 스폰 시 필요한 요소를 초기화 함
    *********************************************************************************************/
    protected void InitEnemy()
    {
        target = GameManager.Instance.mainTarget.transform;
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();

        agent.enabled = false;      // 내비게이션을 할당 받고 바로 비활성화
        agent.speed = moveSpeed;    // moveSpeed로 이동속도 설정
        obstacle.enabled = false;   // 장애물 설정 끔

        hp = startHP;
        healthSlider.value = (hp / startHP) * 100;
    }

    void Update()
    {   
        // DroneState에 따른 함수 호출
        switch (state)
        {
            case DroneState.Idle: Idle(); break;
            case DroneState.Move: Move(); break;
            case DroneState.Attack: Attack(); break;
            case DroneState.Damage: break;
            case DroneState.Die: break;
        }
    }


    /*********************************************************************************************
    함수: Idle
    기능: 일정 시간이 지나면 에이전트 활성화, 상태를 Move로 전환하고 목적지를 설정
    *********************************************************************************************/
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

    /*********************************************************************************************
    함수: Move
    기능: 목적지까지의 이동, 공격 거리 안에 목표가 있으면 Attack으로 전환
    *********************************************************************************************/
    void Move()
    {
        // 타겟이 나의 공격 거리 안에 있음
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            state = DroneState.Attack;  // 상태를 공격으로 전환
            currentTime = 0f;   // 이동 중에는 경과시간이 누적되지 않음
        }
    }

    /*********************************************************************************************
   함수: Attack
   기능: 
    1. 이동을 멈추고 다른 적이 피해가도록 Obstacle 활성
    2. 타겟을 바라봄
    3. 공격
   *********************************************************************************************/
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
            } 

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

    /*********************************************************************************************
    함수: AttackProcess
    기능: 
    1. 목표 방향을 구함
    2. 구한 방향으로 Ray 발사
    3. Ray가 목표에 닿은 곳에 폭발 이펙트 생성
    4. Target의 체력 감소
    *********************************************************************************************/
    protected virtual void AttackProcess()
    {
        Vector3 bulletDirection = (target.position - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, bulletDirection, out RaycastHit hitInfo, attackRange, targetLayerMask))
        {
            if (attackEffect)
            {
                GameObject spawnedEffect = Instantiate(attackEffect, hitInfo.point, Quaternion.identity);
                if (spawnedEffect)
                {
                    spawnedEffect.GetComponent<ParticleSystem>().Play();
                    Destroy(spawnedEffect, 3f);
                }
            }

            Debug.DrawRay(bulletSpawnPoint.position, bulletDirection * attackRange, Color.red, 1f);
            if (hitInfo.transform.gameObject.TryGetComponent<MainTarget>(out MainTarget mainTarget))
            {
                mainTarget.OnTargetDamaged(attackPower);
            }
        }
    }

    /*********************************************************************************************
    함수: OnDamageProcess
    기능: 
    1. 체력 감소
    2. 체력바 UI 갱신
    3. 체력이 0일시 Die함수 호출
    *********************************************************************************************/
    public virtual void OnDamageProcess(float amount)
    {
        if (state == DroneState.Die) return;

        hp -= amount; // 적의 체력 감소
        healthSlider.value = (hp / startHP) * 100;  // 체력 UI 갱신        

        if (hp <= 0)
        {
            Die();
        }
    }

    /*********************************************************************************************
    함수: Die
    기능: 
    1. 드론의 상태를 Die로 변경
    2. 플레이어에게 Coin을 줌
    3. 자신의 위치에서 폭발 이펙트 생성
    *********************************************************************************************/
    protected virtual void Die()
    {
        state = DroneState.Die;

        GameManager.Instance.AddPlayerCoin(enemyCoin);
        GameManager.Instance.SpawnExplosionParticle(transform);

        Destroy(gameObject);
    }
}
