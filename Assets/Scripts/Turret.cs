using UnityEngine;
using UnityEngine.UI;

// 적을 자동 공격하는 터렛 클래스
public class Turret : MonoBehaviour
{
    public enum TurretState { Deactivate, Searching, Attack }

    [Header("Turret Stats")]
    [SerializeField] private float gunDamage = 3f;          // 포탑이 적에게 주는 피해량
    [SerializeField] private float gunDelay = 2f;           // 공격 간격(초 단위)
    [SerializeField] private float gunRadius = 10f;         // 포탑의 공격 범위 (반경)
    [SerializeField] private bool isBombAttack = false;     // 폭탄 공격인지 판별하는 값
    public int turretLevel = 0;                             // 터렛의 레벨
    public int upgradeCost = 100;                           // 터렛 업그레이드 코스트

    [Header("Bomb")]
    [SerializeField] private GameObject bombPrefab;         // 공격용 폭탄 프리팹 
    [SerializeField] private Transform bombSpawnPoint;      // 폭탄 스폰 위치 (터렛 총구 등)
    [SerializeField] private float bombThrowForce = 20f;    // 폭탄 던지는 힘

    [Header("Effect & Sound")]
    [SerializeField] private Transform bulletImpact;        // 타격 이펙트 위치 (타겟 위치로 이동시켜 사용)
    ParticleSystem bulletEffect;                            // 총알 발사 이펙트(미사용)
    AudioSource bulletAudio;                                // 총알 발사 사운드(미사용)

    [Header("Target Settings")]
    [SerializeField] private LayerMask targetLayer;         // 타겟 탐지 시 사용할 레이어 마스크 ("Drone" 레이어만 포함)
    [SerializeField] private GameObject towerLauncher;      // 회전시킬 런처 매쉬

    [Header("Turret Anim")]
    [SerializeField] private float scanSpeed = 30f;         // 회전 속도 (도/초)
    private float currentAngle = 0f;
    private bool rotatingRight = true;

    [Header("Turret UI")]
    public Text preLevelText;
    public Text nextLevelText;
    [SerializeField] private Text coinText;

    public TurretState turretState = TurretState.Deactivate;// 터렛 상태 저장 변수
    private float delayTimer = 0f;                          // 공격 대기 시간 누적용 타이머
    private Transform mainTarget;                           // 공격할 적의 위치 

    private Quaternion initialLauncherRotation;             // 런처의 기본 방향
    private bool isReturningToOrigin = false;               // 기본 방향으로 돌아가는 중인지 여부

    /*********************************************************************************************
    함수: UpgradeTurret
    기능: 터렛을 업그레이드하고 UI를 갱신함
    *********************************************************************************************/
    public void UpgradeTurret()
    {
        if (GameManager.Instance.playerCoin < upgradeCost) return; // 플레이어 코인이 부족하면 리턴

        GameManager.Instance.playerCoin -= upgradeCost; // 업그레이드 코스트 차감
        UIManager.Instance.coinText.text = $"COIN: {GameManager.Instance.playerCoin}$"; // 메인 패널 코인 업데이트

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
            initialLauncherRotation = towerLauncher.transform.rotation; // 초기 타워런처의 회전 값 저장
        }
        preLevelText.text = turretLevel.ToString();
        nextLevelText.text = (turretLevel + 1).ToString();
        coinText.text = upgradeCost.ToString();                         // UI 갱신
    }

    void Update()
    {
        if (turretState == TurretState.Deactivate) return;

        mainTarget = FindNearestTarget();   // 타겟 탐색

        switch (turretState)
        {
            case TurretState.Searching:
                if (isReturningToOrigin)
                {
                    ReturnToInitialRotation();  // 타겟이 없으면 처음 위치로 돌아가서 회전
                }
                else
                {
                    TurretSearchingMotion();    // 타겟을 -30 ~ 30도로 회전시킴
                }

                if (mainTarget) // 타겟 탐색 성공 시 Attack으로 전환
                {
                    turretState = TurretState.Attack;
                }
                break;

            case TurretState.Attack:
                if (mainTarget)
                {
                    LootAtMainTarget(mainTarget);   // 메인 타겟을 바라봄

                    delayTimer += Time.deltaTime;   // 공격 딜레이
                    if (delayTimer >= gunDelay)
                    {
                        Attack(mainTarget);   // 일정 시간마다 발사
                        delayTimer = 0f;
                    }
                }
                else
                {
                    turretState = TurretState.Searching;    // 타겟이 없을 시 Searching으로 전환
                    isReturningToOrigin = true; // 초기 방향으로 회전 시작
                }
                break;
        }
    }

    /*********************************************************************************************
    함수: ReturnToInitialRotation
    기능: Attack -> Searching에서 런처 매쉬의 회전 값을 초기로 되돌림
          없을 시 마지막 적의 위치를 기준으로 회전하기 때문
    *********************************************************************************************/
    void ReturnToInitialRotation()
    {
        if (towerLauncher == null) return;

        float angleDiff = Quaternion.Angle(towerLauncher.transform.rotation, initialLauncherRotation);
        float step = scanSpeed * Time.deltaTime;

        // 현재 회전에서 초기 회전으로 서서히 회전
        towerLauncher.transform.rotation = Quaternion.RotateTowards(
            towerLauncher.transform.rotation,
            initialLauncherRotation,
            step
        );

        // 거의 도달하면 복귀 완료 처리
        if (angleDiff <= 0.5f)
        {
            isReturningToOrigin = false;
            currentAngle = 0f;
            rotatingRight = true;
        }
    }

    /*********************************************************************************************
    함수: TurretSearchingMotion
    기능: Searching 상태에서 타워 런처 매쉬를 좌우 -30 ~ 30 만큼 회전시킴
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
    함수: FindNearestTarget
    기능: 가장 가까운 적의 위치를 반환함
    반환값:
        - transform: 적의 위치
    *********************************************************************************************/
    Transform FindNearestTarget()
    {
        // 설정된 레이어 마스크를 기반으로 일정 반경 내 콜라이더 탐색
        Collider[] hits = Physics.OverlapSphere(transform.position, gunRadius, targetLayer);
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Collider hit in hits)
        {
            // 현재 위치와의 거리 측정
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.transform;
            }
        }

        return closest; // 가장 가까운 대상 반환
    }

    /*********************************************************************************************
    함수: LootAtMainTarget
    기능: Attack 상태에서 타워 런처 매쉬가 적의 방향을 바라보도록 함
    매개변수:
        - target: 적의 위치
    *********************************************************************************************/
    void LootAtMainTarget(Transform target)
    {
        if (towerLauncher == null || target == null) return;

        Vector3 direction = target.position - towerLauncher.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            // 기본 바라보는 방향 회전
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // 왼쪽으로 90도 회전 (Y축 기준)
            Quaternion leftOffset = Quaternion.Euler(0f, -90f, 0f);

            // 보정 회전 적용
            Quaternion adjustedRotation = lookRotation * leftOffset;

            // 회전 적용 (부드럽게 회전)
            towerLauncher.transform.rotation = Quaternion.RotateTowards(
                towerLauncher.transform.rotation,
                adjustedRotation,
                scanSpeed * Time.deltaTime * 2f
            );
        }
    }

    /*********************************************************************************************
    함수: Attack
    기능: 폭탄을 스폰하고 적이 있는 방향으로 발사
    매개변수:
        - target: 적의 위치
    *********************************************************************************************/
    void Attack(Transform target)
    {
        if (bombPrefab != null && bombSpawnPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation);

            // 타겟 방향 계산
            Vector3 throwDirection = (target.position - bombSpawnPoint.position).normalized;

            // 폭탄의 forward 방향을 throwDirection에 맞게 회전
            bomb.transform.rotation = Quaternion.LookRotation(throwDirection);

            // Rigidbody가 있다면 던지기
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
    함수: UpgradeDamage
    기능: 터렛 공격력 업그레이드
    매개변수:
        - newDamage: 업그레이드 된 공격력
    *********************************************************************************************/
    public void UpgradeDamage(float newDamage)
    {
        gunDamage = newDamage;
    }

    /*********************************************************************************************
    함수: UpgradeDelay
    기능: 터렛 공격 딜레이 업그레이드
    매개변수:
        - newDelay: 업그레이드 된 공격 딜레이
    *********************************************************************************************/
    public void UpgradeDelay(float newDelay)
    {
        gunDelay = newDelay;
    }

    /*********************************************************************************************
    함수: UpgradeRadius
    기능: 터렛 공격 반경 업그레이드
    매개변수:
        - newRadius: 업그레이드 된 공격 반경
    *********************************************************************************************/
    public void UpgradeRadius(float newRadius)
    {
        gunRadius = newRadius;
    }

    // 에디터에서 공격 반경을 시각화하기 위한 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gunRadius); // 공격 범위 표시

        Gizmos.color = Color.green;
        // 시작점: 런처 위치
        Vector3 start = towerLauncher.transform.position;

        // 끝점: 런처 위치 + 전방 방향 * 길이
        Vector3 end = start + towerLauncher.transform.forward * 2f; // 2f는 선의 길이 (원하는 대로 조절)

        Gizmos.DrawLine(start, end);
    }
}
