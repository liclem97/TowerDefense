using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Bullet FX")]
    //[SerializeField] private GameObject bulletImpact;  // �Ѿ� ���� ȿ��
    [SerializeField] private Transform bulletImpact;
    ParticleSystem bulletEffect;
    AudioSource bulletAudio;

    [Header("Combat")]
    [SerializeField] private float gunDamage = 1f;    // Gun �����
    [SerializeField] private float autoReloadTime = 3f; // ź���� 5 ������ �� ������ �ð����� 1�� ��

    [Header("Player Move")]
    [SerializeField] private PlayerMove playerMove;

    private void Start()
    {
        bulletEffect = bulletImpact.GetComponent<ParticleSystem>();
        bulletAudio = GetComponent<AudioSource>();
        Cursor.visible = false; // Ŀ�� ����
    }

    private void Update()
    {

        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger)) // ������ �ڵ� Ʈ����, pc ���콺 ��Ŭ��
        {
            ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch); // ��Ʈ�ѷ� ���� ���

            Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
            RaycastHit hitInfo;
            int playerLayer = 1 << LayerMask.NameToLayer("Player");     // 1 << 9, 0001 0000 0000
            int towerLayer = 1 << LayerMask.NameToLayer("Tower");       // 1 << 10, 0010 0000 0000
            int layerMask = playerLayer | towerLayer;                   // 0001 0000 0000 | 0010 0000 0000 = 0011 0000 0000
            if (Physics.Raycast(ray, out hitInfo, 200, ~layerMask))     // ���̾� ����ũ�� �������Ѽ� �÷��̾�� Ÿ���� ������ ��� ���̾ ���� ����ĳ��Ʈ
            {                                                           // 1100 1111 1111
                if (hitInfo.transform.gameObject.CompareTag("Enemy"))   // ray�� �ε��� ���� ������Ʈ�� enemy �±׸� ���� ����
                {
                    // ǥ�� ������ �ٶ󺸵��� ȸ���� ����
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
                //Debug.DrawRay(ray.origin, ray.direction * 200, Color.red, 1f); // ����ĳ��Ʈ �ð�ȭ
            }
        }
    }
}