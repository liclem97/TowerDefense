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
    [SerializeField] protected int enemyCoin; // ���� ������ �÷��̾ ȹ���ϴ� ����

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
        //target = GameObject.Find("Target").transform; // ������ Ÿ���� ã��
        target = GameManager.Instance.mainTarget.transform;
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();

        agent.enabled = false;      // ������̼��� �Ҵ� �ް� �ٷ� ��Ȱ��ȭ
        agent.speed = moveSpeed;    // moveSpeed�� �̵��ӵ� ����
        obstacle.enabled = false;   // ��ֹ� ���� ��

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
        currentTime += Time.deltaTime;   // ��� �ð� ����
        if (currentTime > idleDelayTime) // ��� �ð��� ��� �ð��� �ʰ���
        {
            agent.enabled = true;
            state = DroneState.Move;    //�̵� ���·� ��ȯ
            if (target)
            {
                agent.SetDestination(target.position);   // ������Ʈ ������ ����
            }
        }
    }

    void Move()
    {
        // Ÿ���� ���� ���� �Ÿ� �ȿ� ����
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            state = DroneState.Attack;  // ���¸� �������� ��ȯ
            currentTime = 0f;   // �̵� �߿��� ����ð��� �������� ����
        }
    }

    protected void Attack()
    {
        currentTime += Time.deltaTime;      // ��� �ð� ����
        if (Vector3.Distance(transform.position, target.position) <= attackRange) // Ÿ���� ���� ���� �Ÿ� �ȿ� �ִ� ���
        {
            agent.enabled = false;      // ���� �� �̵� ����
            obstacle.enabled = true;    // �ٸ� ������ ���ϵ��� obstacle Ȱ��ȭ

            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0f;  // ���� ���� ���� �� x�� ȸ�� ����

            if (lookPos != Vector3.zero)  // 0 ���� ����
            {
                Quaternion rot = Quaternion.LookRotation(lookPos);
                transform.rotation = rot;
            }    // Ÿ���� �ٶ�

            if (currentTime > attackDelayTime)  // 2�ʿ� �ѹ� �� ���� ����
            {
                AttackProcess();
                currentTime = 0f;       // ��� �ð� �ʱ�ȭ
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
            // TODO: �Ҹ� ���� ����Ʈ���� �Ѿ� �߻� ����Ʈ ���
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

        hp -= amount; // ���� ü�� ����
        healthSlider.value = (hp / startHP) * 100;  // ü�� UI ����        

        if (hp > 0) // ���� �ʾ����� Damage ���·� ����
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
        agent.enabled = false;                                          // �� ã�� ����
        obstacle.enabled = true;                                        // �ٸ� ����� ���ذ����� obstacle Ȱ��ȭ

        yield return new WaitForSeconds(damageFreeze);  // damageFreeze �ð����� ����

        obstacle.enabled = false;   // obstacle ����
        state = DroneState.Idle;    // Idle ���·� ����
        currentTime = 0f;           // ��� �ð� �ʱ�ȭ
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
