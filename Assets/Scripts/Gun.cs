using UnityEngine;
using UnityEngine.SceneManagement;

public class Gun : MonoBehaviour
{
    [Header("Bullet FX")]
    [SerializeField] private Transform bulletImpact;    // 총알 파편 이펙트
    ParticleSystem bulletEffect;

    [Header("Combat")]
    [SerializeField] private float gunDamage = 1f;      // Gun 대미지
    [SerializeField] private float autoReloadTime = 3f; // 탄알이 5 이하일 시 정해진 시간마다 1씩 참

    [Header("Player Move")]
    [SerializeField] private PlayerMove playerMove;     // PlayerMove 저장

    [Header("Sounds")]
    [SerializeField] AudioClip bulletSound;             // 총알 사운드
    [SerializeField] AudioClip uiPositiveSound;         // UI 긍정 사운드
    [SerializeField] AudioClip uiNegativeSound;         // UI 부정 사운드

    private AudioSource audioSource;                    // 오디오소스 컴포넌트

    private void Start()
    {
        // AudioSource 컴포넌트 초기화
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        bulletEffect = bulletImpact.GetComponent<ParticleSystem>();
        Cursor.visible = false; // 커서 숨김
    }

    private void Update()
    {
        // vr시 handTrigger -> IndexTrigger
        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.RTouch)) // 오른손 핸드 트리거, pc 마우스 우클릭
        {
            ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch); // 컨트롤러 진동 재생

            Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
            RaycastHit hitInfo;
            int playerLayer = 1 << LayerMask.NameToLayer("Player");     // 1 << 9, 0001 0000 0000
            int towerLayer = 1 << LayerMask.NameToLayer("Tower");       // 1 << 10, 0010 0000 0000
            int layerMask = playerLayer | towerLayer;                   // 0001 0000 0000 | 0010 0000 0000 = 0011 0000 0000
            if (Physics.Raycast(ray, out hitInfo, 200, ~layerMask))     // 레이어 마스크를 반전시켜서 플레이어와 타워를 제외한 모든 레이어에 대해 레이캐스트
            {                                                           // 1100 1111 1111
                if (hitInfo.transform.gameObject.CompareTag("Enemy"))   // ray와 부딪힌 게임 오브젝트가 enemy 태그를 갖고 있음
                {
                    Quaternion impactRotation = Quaternion.LookRotation(hitInfo.normal);            // 표면 방향을 바라보도록 회전값 생성
                    bulletEffect.Play();                                                            // 총알 이펙트 재생
                    audioSource.PlayOneShot(bulletSound);                                           // 총알 사운드 재생

                    bulletImpact.forward = hitInfo.normal;                                          // 총알 이펙트의 방향 설정
                    bulletImpact.position = hitInfo.point;                                          // 총알 이펙트의 위치 설정

                    if (hitInfo.transform.gameObject.TryGetComponent<DroneAI>(out DroneAI enemy))   // 히트한 적의 컴포넌트를 가져와서 대미지 처리
                    {   
                        enemy.OnDamageProcess(gunDamage);
                    }
                }
                else if (hitInfo.transform.gameObject.CompareTag("CCTV") &&                         // 레이가 히트한 것이 CCTV 패널 화면인 경우
                        hitInfo.transform.gameObject.TryGetComponent<CCTV>(out CCTV cctv))          // CCTV 컴포넌트를 가져옴
                {
                    transform.position = cctv.cctvCamera.transform.position;                        // 플레이어를 CCTV에 저장딘 카메라의 위치로 이동
                    playerMove.currentCameraIndex = cctv.cameraIndex;                               // 카메라 인덱스를 CCTV카메라 인덱스로 저장

                    audioSource.PlayOneShot(uiPositiveSound);                                       // UI 긍정 사운드 재생
                }
                else if (hitInfo.transform.name == "GameStartButton")                               // 게임 스타트 버튼을 누름
                {
                    audioSource.PlayOneShot(uiPositiveSound);                                       // UI 긍정 사운드 재생
                    GameManager.Instance.GameStart();                                               // 게임매니저에게 게임 스타트 요청
                }
                else if (hitInfo.transform.name == "GameRestartButton")                             // 게임 리스타트 버튼을 누름
                {
                    audioSource.PlayOneShot(uiPositiveSound);
                    SceneManager.LoadScene("MainMap"); // 게임 재시작                                // MainMap 다시 로드
                }
                else if (hitInfo.transform.name == "GameExitButton")                                // 나가기 버튼을 누름
                {
                    audioSource.PlayOneShot(uiPositiveSound);
                    Application.Quit();                                                             // 게임 종료
                }
                else if (hitInfo.transform.name == "UpgradeBtn")                                    // 터렛의 업그레이드 버튼을 누름
                {   
                    Turret turret = hitInfo.transform.gameObject.GetComponentInParent<Turret>();    // 터렛의 컴포넌트를 가져옴
                    if (turret && GameManager.Instance.playerCoin > turret.upgradeCost)             // 터렛 컴포넌트가 있고, 코인이 Upgrade Cost보다 많음
                    {   
                        audioSource.PlayOneShot(uiPositiveSound);
                        if (turret.turretState == Turret.TurretState.Deactivate)                    // 터렛 상태가 비활성화인 경우
                        {
                            GameManager.Instance.playerCoin -= 100;                                 // 코인 차감
                            turret.turretLevel += 1;                                                // 터렛 레벨 증가
                            UIManager.Instance.coinText.text = $"COIN: {GameManager.Instance.playerCoin}$";
                            turret.preLevelText.text = "LV." + turret.turretLevel.ToString();
                            turret.nextLevelText.text = "LV." + (turret.turretLevel + 1).ToString();// 각 UI 갱신
                            turret.turretState = Turret.TurretState.Searching;                      // 활성화
                        }
                        else
                        {
                            turret.UpgradeTurret();                                                 // 터렛의 Upgrade 함수 호출
                        }
                    }
                    else
                    {
                        audioSource.PlayOneShot(uiNegativeSound);                                   // 돈이 부족한 경우 UI 부정 사운드 재생
                    }
                }               
                //Debug.DrawRay(ray.origin, ray.direction * 200, Color.red, 1f); // 레이캐스트 시각화
            }
        }
    }
}