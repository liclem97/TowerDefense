using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Bullet FX")]
    //[SerializeField] private GameObject bulletImpact;  // 총알 파편 효과
    [SerializeField] private Transform bulletImpact;
    ParticleSystem bulletEffect;
    AudioSource bulletAudio;

    [Header("Combat")]
    [SerializeField] private float gunDamage = 1f;    // Gun 대미지
    [SerializeField] private float autoReloadTime = 3f; // 탄알이 5 이하일 시 정해진 시간마다 1씩 참

    [Header("Player Move")]
    [SerializeField] private PlayerMove playerMove;

    private void Start()
    {
        bulletEffect = bulletImpact.GetComponent<ParticleSystem>();
        bulletAudio = GetComponent<AudioSource>();
        Cursor.visible = false; // 커서 숨김
    }

    private void Update()
    {

        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger)) // 오른손 핸드 트리거, pc 마우스 우클릭
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
                    // 표면 방향을 바라보도록 회전값 생성
                    Quaternion impactRotation = Quaternion.LookRotation(hitInfo.normal);
                    bulletEffect.Play();

                    bulletImpact.forward = hitInfo.normal;
                    bulletImpact.position = hitInfo.point;

                    if (hitInfo.transform.gameObject.TryGetComponent<DroneAI>(out DroneAI enemy))
                    {
                        enemy.OnDamageProcess(gunDamage);
                    }
                }
                else if (hitInfo.transform.gameObject.CompareTag("CCTV") &&
                        hitInfo.transform.gameObject.TryGetComponent<CCTV>(out CCTV cctv))
                {
                    transform.position = cctv.cctvCamera.transform.position;
                    playerMove.currentCameraIndex = cctv.cameraIndex;

                }
                else if (hitInfo.transform.name == "GameStartButton")
                {
                    GameManager.Instance.GameStart();
                }
                //Debug.DrawRay(ray.origin, ray.direction * 200, Color.red, 1f); // 레이캐스트 시각화
            }
        }
    }
}