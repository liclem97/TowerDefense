using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DroneAI : MonoBehaviour
{
    protected enum DroneState { Idle, Move, Attack, Damage, Die }   // ��� ���� ������

    [Header("State Timers")]
    [SerializeField] protected float idleDelayTime = 2f;            // Idle�� �ִ� �ð�
    [SerializeField] protected float attackDelayTime = 2f;          // ���� ������ Ÿ��
    [SerializeField] protected float damageFreeze = 0.05f;          // ���� ���� �� ���� �ð�

    [Header("Movement / Combat")]
    [SerializeField] protected float moveSpeed = 1f;                // �̵��ӵ�
    [SerializeField] protected float attackRange = 3f;              // ���ݰŸ�
    [SerializeField] protected float attackPower = 2f;              // ���� �����
    [SerializeField] protected Transform bulletSpawnPoint;          // �Ѿ��� ������ ��ġ
    [SerializeField] protected LayerMask targetLayerMask;           // ��ǥ�� ���̾� ����ũ
    [SerializeField] private GameObject attackEffect;               // ���� ����Ʈ

    [Header("Stats")]
    [SerializeField] protected float hp;                            // ü��
    [SerializeField] protected float startHP = 10f;                 // ���� ü��
    [SerializeField] protected int enemyCoin;                       // ���� �� �÷��̾ ȹ���ϴ� ����

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;                   // ü�¹� UI

    [Header("Effect References")]
    [SerializeField] protected Transform explosion;                 // ���� ����Ʈ
    [SerializeField] protected ParticleSystem expEffect;
    [SerializeField] protected AudioSource expAudio;
    [SerializeField] private Transform meshTransform;               // ���߿� ���ִ� �Ž�("��� ����")

    protected DroneState state = DroneState.Idle;                   // ���� ���� ����
    protected float currentTime;                                    // ��� �ð� ���� ����
    protected Transform target;                                     // Ÿ���� ��ġ
    protected NavMeshAgent agent;                                   // AI �̵�
    protected NavMeshObstacle obstacle;                             // AI ��ֹ�

    void Start()
    {
        InitEnemy();
    }

    /*********************************************************************************************
    �Լ�: InitEnemy
    ���: �� ���� �� �ʿ��� ��Ҹ� �ʱ�ȭ ��
    *********************************************************************************************/
    protected void InitEnemy()
    {
        target = GameManager.Instance.mainTarget.transform;
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();

        agent.enabled = false;      // ������̼��� �Ҵ� �ް� �ٷ� ��Ȱ��ȭ
        agent.speed = moveSpeed;    // moveSpeed�� �̵��ӵ� ����
        obstacle.enabled = false;   // ��ֹ� ���� ��

        hp = startHP;
        healthSlider.value = (hp / startHP) * 100;
    }

    void Update()
    {   
        // DroneState�� ���� �Լ� ȣ��
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
    �Լ�: Idle
    ���: ���� �ð��� ������ ������Ʈ Ȱ��ȭ, ���¸� Move�� ��ȯ�ϰ� �������� ����
    *********************************************************************************************/
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

    /*********************************************************************************************
    �Լ�: Move
    ���: ������������ �̵�, ���� �Ÿ� �ȿ� ��ǥ�� ������ Attack���� ��ȯ
    *********************************************************************************************/
    void Move()
    {
        // Ÿ���� ���� ���� �Ÿ� �ȿ� ����
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            state = DroneState.Attack;  // ���¸� �������� ��ȯ
            currentTime = 0f;   // �̵� �߿��� ����ð��� �������� ����
        }
    }

    /*********************************************************************************************
   �Լ�: Attack
   ���: 
    1. �̵��� ���߰� �ٸ� ���� ���ذ����� Obstacle Ȱ��
    2. Ÿ���� �ٶ�
    3. ����
   *********************************************************************************************/
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
            } 

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

    /*********************************************************************************************
    �Լ�: AttackProcess
    ���: 
    1. ��ǥ ������ ����
    2. ���� �������� Ray �߻�
    3. Ray�� ��ǥ�� ���� ���� ���� ����Ʈ ����
    4. Target�� ü�� ����
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
    �Լ�: OnDamageProcess
    ���: 
    1. ü�� ����
    2. ü�¹� UI ����
    3. ü���� 0�Ͻ� Die�Լ� ȣ��
    *********************************************************************************************/
    public virtual void OnDamageProcess(float amount)
    {
        if (state == DroneState.Die) return;

        hp -= amount; // ���� ü�� ����
        healthSlider.value = (hp / startHP) * 100;  // ü�� UI ����        

        if (hp <= 0)
        {
            Die();
        }
    }

    /*********************************************************************************************
    �Լ�: Die
    ���: 
    1. ����� ���¸� Die�� ����
    2. �÷��̾�� Coin�� ��
    3. �ڽ��� ��ġ���� ���� ����Ʈ ����
    *********************************************************************************************/
    protected virtual void Die()
    {
        state = DroneState.Die;

        GameManager.Instance.AddPlayerCoin(enemyCoin);
        GameManager.Instance.SpawnExplosionParticle(transform);

        Destroy(gameObject);
    }
}
