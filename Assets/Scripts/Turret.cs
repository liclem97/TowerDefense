using UnityEngine;
using UnityEngine.UI;

// ���� �ڵ� �����ϴ� �ͷ� Ŭ����
public class Turret : MonoBehaviour
{
    public enum TurretState { Deactivate, Searching, Attack }

    [Header("Turret Stats")]
    [SerializeField] private float gunDamage = 3f;          // ��ž�� ������ �ִ� ���ط�
    [SerializeField] private float gunDelay = 2f;           // ���� ����(�� ����)
    [SerializeField] private float gunRadius = 10f;         // ��ž�� ���� ���� (�ݰ�)
    [SerializeField] private bool isBombAttack = false;     // ��ź �������� �Ǻ��ϴ� ��
    public int turretLevel = 0;                             // �ͷ��� ����
    public int upgradeCost = 100;                           // �ͷ� ���׷��̵� �ڽ�Ʈ

    [Header("Bomb")]
    [SerializeField] private GameObject bombPrefab;         // ���ݿ� ��ź ������ 
    [SerializeField] private Transform bombSpawnPoint;      // ��ź ���� ��ġ (�ͷ� �ѱ� ��)
    [SerializeField] private float bombThrowForce = 20f;    // ��ź ������ ��

    [Header("Effect & Sound")]
    [SerializeField] private Transform bulletImpact;        // Ÿ�� ����Ʈ ��ġ (Ÿ�� ��ġ�� �̵����� ���)
    ParticleSystem bulletEffect;                            // �Ѿ� �߻� ����Ʈ(�̻��)
    AudioSource bulletAudio;                                // �Ѿ� �߻� ����(�̻��)

    [Header("Target Settings")]
    [SerializeField] private LayerMask targetLayer;         // Ÿ�� Ž�� �� ����� ���̾� ����ũ ("Drone" ���̾ ����)
    [SerializeField] private GameObject towerLauncher;      // ȸ����ų ��ó �Ž�

    [Header("Turret Anim")]
    [SerializeField] private float scanSpeed = 30f;         // ȸ�� �ӵ� (��/��)
    private float currentAngle = 0f;
    private bool rotatingRight = true;

    [Header("Turret UI")]
    public Text preLevelText;
    public Text nextLevelText;
    [SerializeField] private Text coinText;

    public TurretState turretState = TurretState.Deactivate;// �ͷ� ���� ���� ����
    private float delayTimer = 0f;                          // ���� ��� �ð� ������ Ÿ�̸�
    private Transform mainTarget;                           // ������ ���� ��ġ 

    private Quaternion initialLauncherRotation;             // ��ó�� �⺻ ����
    private bool isReturningToOrigin = false;               // �⺻ �������� ���ư��� ������ ����

    /*********************************************************************************************
    �Լ�: UpgradeTurret
    ���: �ͷ��� ���׷��̵��ϰ� UI�� ������
    *********************************************************************************************/
    public void UpgradeTurret()
    {
        if (GameManager.Instance.playerCoin < upgradeCost) return; // �÷��̾� ������ �����ϸ� ����

        GameManager.Instance.playerCoin -= upgradeCost; // ���׷��̵� �ڽ�Ʈ ����
        UIManager.Instance.coinText.text = $"COIN: {GameManager.Instance.playerCoin}$"; // ���� �г� ���� ������Ʈ

        UpgradeDamage(gunDamage + 1);   
        UpgradeDelay(gunDelay - 0.01f);
        UpgradeRadius(gunRadius + 0.5f);
        turretLevel += 1;
        upgradeCost = turretLevel * 100;                     

        preLevelText.text = "LV." + turretLevel.ToString();
        nextLevelText.text = "LV." + (turretLevel + 1).ToString();
        coinText.text = upgradeCost.ToString();
    }

    void Start()
    {
        if (towerLauncher != null)
        {
            initialLauncherRotation = towerLauncher.transform.rotation; // �ʱ� Ÿ����ó�� ȸ�� �� ����
        }
        preLevelText.text = turretLevel.ToString();
        nextLevelText.text = (turretLevel + 1).ToString();
        coinText.text = upgradeCost.ToString();                         // UI ����
    }

    void Update()
    {
        if (turretState == TurretState.Deactivate) return;

        mainTarget = FindNearestTarget();   // Ÿ�� Ž��

        switch (turretState)
        {
            case TurretState.Searching:
                if (isReturningToOrigin)
                {
                    ReturnToInitialRotation();  // Ÿ���� ������ ó�� ��ġ�� ���ư��� ȸ��
                }
                else
                {
                    TurretSearchingMotion();    // Ÿ���� -30 ~ 30���� ȸ����Ŵ
                }

                if (mainTarget) // Ÿ�� Ž�� ���� �� Attack���� ��ȯ
                {
                    turretState = TurretState.Attack;
                }
                break;

            case TurretState.Attack:
                if (mainTarget)
                {
                    LootAtMainTarget(mainTarget);   // ���� Ÿ���� �ٶ�

                    delayTimer += Time.deltaTime;   // ���� ������
                    if (delayTimer >= gunDelay)
                    {
                        Attack(mainTarget);   // ���� �ð����� �߻�
                        delayTimer = 0f;
                    }
                }
                else
                {
                    turretState = TurretState.Searching;    // Ÿ���� ���� �� Searching���� ��ȯ
                    isReturningToOrigin = true; // �ʱ� �������� ȸ�� ����
                }
                break;
        }
    }

    /*********************************************************************************************
    �Լ�: ReturnToInitialRotation
    ���: Attack -> Searching���� ��ó �Ž��� ȸ�� ���� �ʱ�� �ǵ���
          ���� �� ������ ���� ��ġ�� �������� ȸ���ϱ� ����
    *********************************************************************************************/
    void ReturnToInitialRotation()
    {
        if (towerLauncher == null) return;

        float angleDiff = Quaternion.Angle(towerLauncher.transform.rotation, initialLauncherRotation);
        float step = scanSpeed * Time.deltaTime;

        // ���� ȸ������ �ʱ� ȸ������ ������ ȸ��
        towerLauncher.transform.rotation = Quaternion.RotateTowards(
            towerLauncher.transform.rotation,
            initialLauncherRotation,
            step
        );

        // ���� �����ϸ� ���� �Ϸ� ó��
        if (angleDiff <= 0.5f)
        {
            isReturningToOrigin = false;
            currentAngle = 0f;
            rotatingRight = true;
        }
    }

    /*********************************************************************************************
    �Լ�: TurretSearchingMotion
    ���: Searching ���¿��� Ÿ�� ��ó �Ž��� �¿� -30 ~ 30 ��ŭ ȸ����Ŵ
    *********************************************************************************************/
    void TurretSearchingMotion()
    {
        if (towerLauncher == null) return;

        Transform launcherTransform = towerLauncher.transform;
        float deltaAngle = scanSpeed * Time.deltaTime;

        if (rotatingRight)
        {
            launcherTransform.Rotate(0f, deltaAngle, 0f, Space.Self);
            currentAngle += deltaAngle;

            if (currentAngle >= 30f)
            {
                currentAngle = 30f;
                rotatingRight = false;
            }
        }
        else
        {
            launcherTransform.Rotate(0f, -deltaAngle, 0f, Space.Self);
            currentAngle -= deltaAngle;

            if (currentAngle <= -30f)
            {
                currentAngle = -30f;
                rotatingRight = true;
            }
        }
    }

    /*********************************************************************************************
    �Լ�: FindNearestTarget
    ���: ���� ����� ���� ��ġ�� ��ȯ��
    ��ȯ��:
        - transform: ���� ��ġ
    *********************************************************************************************/
    Transform FindNearestTarget()
    {
        // ������ ���̾� ����ũ�� ������� ���� �ݰ� �� �ݶ��̴� Ž��
        Collider[] hits = Physics.OverlapSphere(transform.position, gunRadius, targetLayer);
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Collider hit in hits)
        {
            // ���� ��ġ���� �Ÿ� ����
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.transform;
            }
        }

        return closest; // ���� ����� ��� ��ȯ
    }

    /*********************************************************************************************
    �Լ�: LootAtMainTarget
    ���: Attack ���¿��� Ÿ�� ��ó �Ž��� ���� ������ �ٶ󺸵��� ��
    �Ű�����:
        - target: ���� ��ġ
    *********************************************************************************************/
    void LootAtMainTarget(Transform target)
    {
        if (towerLauncher == null || target == null) return;

        Vector3 direction = target.position - towerLauncher.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            // �⺻ �ٶ󺸴� ���� ȸ��
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // �������� 90�� ȸ�� (Y�� ����)
            Quaternion leftOffset = Quaternion.Euler(0f, -90f, 0f);

            // ���� ȸ�� ����
            Quaternion adjustedRotation = lookRotation * leftOffset;

            // ȸ�� ���� (�ε巴�� ȸ��)
            towerLauncher.transform.rotation = Quaternion.RotateTowards(
                towerLauncher.transform.rotation,
                adjustedRotation,
                scanSpeed * Time.deltaTime * 2f
            );
        }
    }

    /*********************************************************************************************
    �Լ�: Attack
    ���: ��ź�� �����ϰ� ���� �ִ� �������� �߻�
    �Ű�����:
        - target: ���� ��ġ
    *********************************************************************************************/
    void Attack(Transform target)
    {
        if (bombPrefab != null && bombSpawnPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation);

            // Ÿ�� ���� ���
            Vector3 throwDirection = (target.position - bombSpawnPoint.position).normalized;

            // ��ź�� forward ������ throwDirection�� �°� ȸ��
            bomb.transform.rotation = Quaternion.LookRotation(throwDirection);

            // Rigidbody�� �ִٸ� ������
            Rigidbody bombRb = bomb.GetComponent<Rigidbody>();
            if (bombRb != null)
            {
                bombRb.AddForce(throwDirection * bombThrowForce, ForceMode.Impulse);
            }
            Bomb spawnedBomb = bomb.GetComponent<Bomb>();
            if (spawnedBomb)
            {
                spawnedBomb.maxDamage = gunDamage;
            }
        }
    }

    /*********************************************************************************************
    �Լ�: UpgradeDamage
    ���: �ͷ� ���ݷ� ���׷��̵�
    �Ű�����:
        - newDamage: ���׷��̵� �� ���ݷ�
    *********************************************************************************************/
    public void UpgradeDamage(float newDamage)
    {
        gunDamage = newDamage;
    }

    /*********************************************************************************************
    �Լ�: UpgradeDelay
    ���: �ͷ� ���� ������ ���׷��̵�
    �Ű�����:
        - newDelay: ���׷��̵� �� ���� ������
    *********************************************************************************************/
    public void UpgradeDelay(float newDelay)
    {
        gunDelay = newDelay;
    }

    /*********************************************************************************************
    �Լ�: UpgradeRadius
    ���: �ͷ� ���� �ݰ� ���׷��̵�
    �Ű�����:
        - newRadius: ���׷��̵� �� ���� �ݰ�
    *********************************************************************************************/
    public void UpgradeRadius(float newRadius)
    {
        gunRadius = newRadius;
    }

    // �����Ϳ��� ���� �ݰ��� �ð�ȭ�ϱ� ���� �����
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gunRadius); // ���� ���� ǥ��

        Gizmos.color = Color.green;
        // ������: ��ó ��ġ
        Vector3 start = towerLauncher.transform.position;

        // ����: ��ó ��ġ + ���� ���� * ����
        Vector3 end = start + towerLauncher.transform.forward * 2f; // 2f�� ���� ���� (���ϴ� ��� ����)

        Gizmos.DrawLine(start, end);
    }
}
