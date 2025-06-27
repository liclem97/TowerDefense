using UnityEngine;
using UnityEngine.SceneManagement;

public class Gun : MonoBehaviour
{
    [Header("Bullet FX")]
    [SerializeField] private Transform bulletImpact;    // �Ѿ� ���� ����Ʈ
    ParticleSystem bulletEffect;

    [Header("Combat")]
    [SerializeField] private float gunDamage = 1f;      // Gun �����
    [SerializeField] private float autoReloadTime = 3f; // ź���� 5 ������ �� ������ �ð����� 1�� ��

    [Header("Player Move")]
    [SerializeField] private PlayerMove playerMove;     // PlayerMove ����

    [Header("Sounds")]
    [SerializeField] AudioClip bulletSound;             // �Ѿ� ����
    [SerializeField] AudioClip uiPositiveSound;         // UI ���� ����
    [SerializeField] AudioClip uiNegativeSound;         // UI ���� ����

    private AudioSource audioSource;                    // ������ҽ� ������Ʈ

    private void Start()
    {
        // AudioSource ������Ʈ �ʱ�ȭ
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        bulletEffect = bulletImpact.GetComponent<ParticleSystem>();
        Cursor.visible = false; // Ŀ�� ����
    }

    private void Update()
    {
        // vr�� handTrigger -> IndexTrigger
        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.RTouch)) // ������ �ڵ� Ʈ����, pc ���콺 ��Ŭ��
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
                    Quaternion impactRotation = Quaternion.LookRotation(hitInfo.normal);            // ǥ�� ������ �ٶ󺸵��� ȸ���� ����
                    bulletEffect.Play();                                                            // �Ѿ� ����Ʈ ���
                    audioSource.PlayOneShot(bulletSound);                                           // �Ѿ� ���� ���

                    bulletImpact.forward = hitInfo.normal;                                          // �Ѿ� ����Ʈ�� ���� ����
                    bulletImpact.position = hitInfo.point;                                          // �Ѿ� ����Ʈ�� ��ġ ����

                    if (hitInfo.transform.gameObject.TryGetComponent<DroneAI>(out DroneAI enemy))   // ��Ʈ�� ���� ������Ʈ�� �����ͼ� ����� ó��
                    {   
                        enemy.OnDamageProcess(gunDamage);
                    }
                }
                else if (hitInfo.transform.gameObject.CompareTag("CCTV") &&                         // ���̰� ��Ʈ�� ���� CCTV �г� ȭ���� ���
                        hitInfo.transform.gameObject.TryGetComponent<CCTV>(out CCTV cctv))          // CCTV ������Ʈ�� ������
                {
                    transform.position = cctv.cctvCamera.transform.position;                        // �÷��̾ CCTV�� ����� ī�޶��� ��ġ�� �̵�
                    playerMove.currentCameraIndex = cctv.cameraIndex;                               // ī�޶� �ε����� CCTVī�޶� �ε����� ����

                    audioSource.PlayOneShot(uiPositiveSound);                                       // UI ���� ���� ���
                }
                else if (hitInfo.transform.name == "GameStartButton")                               // ���� ��ŸƮ ��ư�� ����
                {
                    audioSource.PlayOneShot(uiPositiveSound);                                       // UI ���� ���� ���
                    GameManager.Instance.GameStart();                                               // ���ӸŴ������� ���� ��ŸƮ ��û
                }
                else if (hitInfo.transform.name == "GameRestartButton")                             // ���� ����ŸƮ ��ư�� ����
                {
                    audioSource.PlayOneShot(uiPositiveSound);
                    SceneManager.LoadScene("MainMap"); // ���� �����                                // MainMap �ٽ� �ε�
                }
                else if (hitInfo.transform.name == "GameExitButton")                                // ������ ��ư�� ����
                {
                    audioSource.PlayOneShot(uiPositiveSound);
                    Application.Quit();                                                             // ���� ����
                }
                else if (hitInfo.transform.name == "UpgradeBtn")                                    // �ͷ��� ���׷��̵� ��ư�� ����
                {   
                    Turret turret = hitInfo.transform.gameObject.GetComponentInParent<Turret>();    // �ͷ��� ������Ʈ�� ������
                    if (turret && GameManager.Instance.playerCoin > turret.upgradeCost)             // �ͷ� ������Ʈ�� �ְ�, ������ Upgrade Cost���� ����
                    {   
                        audioSource.PlayOneShot(uiPositiveSound);
                        if (turret.turretState == Turret.TurretState.Deactivate)                    // �ͷ� ���°� ��Ȱ��ȭ�� ���
                        {
                            GameManager.Instance.playerCoin -= 100;                                 // ���� ����
                            turret.turretLevel += 1;                                                // �ͷ� ���� ����
                            UIManager.Instance.coinText.text = $"COIN: {GameManager.Instance.playerCoin}$";
                            turret.preLevelText.text = "LV." + turret.turretLevel.ToString();
                            turret.nextLevelText.text = "LV." + (turret.turretLevel + 1).ToString();// �� UI ����
                            turret.turretState = Turret.TurretState.Searching;                      // Ȱ��ȭ
                        }
                        else
                        {
                            turret.UpgradeTurret();                                                 // �ͷ��� Upgrade �Լ� ȣ��
                        }
                    }
                    else
                    {
                        audioSource.PlayOneShot(uiNegativeSound);                                   // ���� ������ ��� UI ���� ���� ���
                    }
                }               
                //Debug.DrawRay(ray.origin, ray.direction * 200, Color.red, 1f); // ����ĳ��Ʈ �ð�ȭ
            }
        }
    }
}