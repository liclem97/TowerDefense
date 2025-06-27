using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
{
    public enum TurretState { Deactivate, Searching, Attack }

    [Header("Turret Stats")]
    [SerializeField] private float gunDamage = 3f;     // ��ž�� ������ �ִ� ���ط�
    [SerializeField] private float gunDelay = 2f;      // ���� ����(�� ����)
    [SerializeField] private float gunRadius = 10f;     // ��ž�� ���� ���� (�ݰ�)
    [SerializeField] private bool isBombAttack = false;
    [SerializeField] private int turretLevel = 0;
    [SerializeField] private int upgradeCost = 100;

    [Header("Bomb")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombSpawnPoint;     // ��ź ���� ��ġ (�ͷ� �ѱ� ��)
    [SerializeField] private float bombThrowForce = 20f;   // ��ź ������ ��

    [Header("Effect & Sound")]
    [SerializeField] private Transform bulletImpact;   // Ÿ�� ����Ʈ ��ġ (Ÿ�� ��ġ�� �̵����� ���)
    ParticleSystem bulletEffect;                       // �Ѿ� �߻� ����Ʈ
    AudioSource bulletAudio;                           // �Ѿ� �߻� ����

    [Header("Target Settings")]
    [SerializeField] private LayerMask targetLayer;    // Ÿ�� Ž�� �� ����� ���̾� ����ũ ("Drone" ���̾ ����)
    [SerializeField] private GameObject towerLauncher; // ��ó �Ž�

    [Header("Turret Anim")]
    [SerializeField] private float scanSpeed = 30f; // ȸ�� �ӵ� (��/��)
    private float currentAngle = 0f;
    private bool rotatingRight = true;

    [Header("Turret UI")]
    [SerializeField] private Text preLevelText;
    [SerializeField] private Text nextLevelText;
    [SerializeField] private Text coinText;

    public TurretState turretState = TurretState.Deactivate;
    private float delayTimer = 0f;                          // ���� ��� �ð� ������ Ÿ�̸�
    private Transform mainTarget;

    private Quaternion initialLauncherRotation;   // ��ó�� �⺻ ����
    private bool isReturningToOrigin = false;     // �⺻ �������� ���ư��� ������ ����

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
            initialLauncherRotation = towerLauncher.transform.rotation;
        }
        preLevelText.text = turretLevel.ToString();
        nextLevelText.text = (turretLevel + 1).ToString();
        coinText.text = upgradeCost.ToString();
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

    // �ͷ� ��ó�� ȸ�� ��Ű�� �Լ�
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

    // ���� ����� Ÿ���� Ž���Ͽ� ��ȯ
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

    // Ÿ�ٿ��� ���ظ� ������ ����Ʈ �� ���� ���
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

    // ������ ���׷��̵�
    public void UpgradeDamage(float newDamage)
    {
        gunDamage = newDamage;
    }

    // ���� ������ ���׷��̵� (������ ����)
    public void UpgradeDelay(float newDelay)
    {
        gunDelay = newDelay;
    }

    // ���� �ݰ� ���׷��̵�
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
