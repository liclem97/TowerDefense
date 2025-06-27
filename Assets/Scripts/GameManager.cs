using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    enum WaveState { BeforeWave, Process, EndWave }             // ���̺� ���� ������

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefab_Drone;      // ������ ���(���Ÿ�) �� ������
    [SerializeField] private GameObject enemyPrefab_Melee;      // ������ �ٰŸ� �� ������

    [Header("Enemy SpawnPoint")]
    [SerializeField] private Transform meleeEnemySpawnPoint;    // �ٰŸ� ���� ���� ��ġ
    [SerializeField] private Transform droneEnemySpawnPoint;    // ���Ÿ� ���� ���� ��ġ

    [Header("Target")]
    public Transform mainTarget;                                // ���� Ÿ��(Tower)

    //���� ����
    [Header("Game Over")]
    [SerializeField] private GameObject player;                 // �÷��̾� ������Ʈ
    [SerializeField] private Transform gameOverPosition;        // ���� ���� �� �÷��̾ �̵��� ��ġ
    [SerializeField] private Canvas redScreenEffect;            // ���� ���� �� ȭ���� �Ӱ� �ϴ� ����Ʈ

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffect;        // ���� ����Ʈ

    [Header("Sounds")]
    [SerializeField] private AudioClip backgroundSound;         // �����

    public bool isGameStarted = false;                          // ���� ���� �Ǻ� Boolean
    private WaveState waveState = WaveState.BeforeWave;         // ���̺� ���� ����
    private int waveCount;                                      // ���� ���� ���̺��� ��
    private int meleeEnemyToSpawn;                              // �����ؾ� �ϴ� �ٰŸ� ���� ��
    private int droneEnemyToSpawn;                              // �����ؾ� �ϴ� ���Ÿ� ���� ��
    private float currentTime = 0f;                             // ��� �ð� ����
    private float waitingTime = 3f;                             // ���� ���̺���� ��ٸ��� �ð�
    private float spawnDelayTime = 2f;                          // �� ���� �� ���� ���� �����ϴ� ������ �ð�

    public int playerCoin;                                      // �÷��̾ ������ ����

    private readonly WaitForSeconds enemyCheckInterval = new WaitForSeconds(0.5f);  // ���� ���� ���� �Ǻ��ϴ� ����
    private Coroutine enemyCheckRoutine;       // �ڷ�ƾ �ڵ� ������

    private void Awake()
    {
        // �̹� �ν��Ͻ��� �����ϸ� ���� ������ ��ü �ı�
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // ���� �ν��Ͻ� �ʱ�ȭ
        Instance = this;
        //DontDestroyOnLoad(gameObject);   // �� ��ȯ �ÿ��� ����
        // Ȱ�� �� ReStart�� ���װ� �Ͼ
    }

    /*********************************************************************************************
    �Լ�: SpawnExplosionParticle
    ���: ������ ��ġ���� ���� ����Ʈ�� ������
    �Ű�����:
        - transform: ���� ����Ʈ�� ������ ��ġ ��
    *********************************************************************************************/
    public void SpawnExplosionParticle(Transform transform)
    {
        if (explosionEffect)
        {
            GameObject spawnedEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            if (spawnedEffect)
            {
                spawnedEffect.GetComponent<ParticleSystem>().Play();
                Destroy(spawnedEffect, 2f);
            }
        }
    }

    /*********************************************************************************************
    �Լ�: GameStart
    ���: ������ ���۽�Ű�� CCTV �г��� UI�� ������
    *********************************************************************************************/
    public void GameStart()
    {
        isGameStarted = true;
        waveCount = 0;
        playerCoin = 0;

        // UI ����
        UIManager.Instance.gameStartUI.SetActive(false);
        UIManager.Instance.cctvPanelUI.SetActive(true);
        UIManager.Instance.waveText.text = $"Wave : {waveCount}";
        UIManager.Instance.coinText.text = $"COIN: {playerCoin}$";
    }

    private void Update()
    {
        if (!isGameStarted) return;

        switch (waveState)
        {
            case WaveState.BeforeWave: BeforeWave(); break;
            case WaveState.Process: break;
            case WaveState.EndWave: break;
        }
    }

    /*********************************************************************************************
    �Լ�: BeforeWave
    ���: ���̺� ���� �� �Լ�, ��� �ð��� ������ Process�� ���¸� �����ϰ� �� ��ȯ �ڷ�ƾ ����
    *********************************************************************************************/
    private void BeforeWave()
    {
        currentTime += Time.time;
        if (currentTime > waitingTime)
        {
            currentTime = 0f;
            waveState = WaveState.Process;
            StartCoroutine(nameof(EnemySpwanProcess));
        }
    }

    /*********************************************************************************************
    �Լ�: EnemySpwanProcess
    ���: ���� ������ ��ġ���� �����ϴ� �Լ�
    *********************************************************************************************/
    private IEnumerator EnemySpwanProcess()
    {
        // 1. ���� �� ����
        for (int i = 0; i < meleeEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Melee, meleeEnemySpawnPoint.position, meleeEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // 2. ��� �� ����
        for (int i = 0; i < droneEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Drone, droneEnemySpawnPoint.position, droneEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // ���� ��� ������ �� üũ �÷��� Ȱ��ȭ
        enemyCheckRoutine = StartCoroutine(CheckEnemiesRoutine());
    }

    /*********************************************************************************************
    �Լ�: CheckEnemiesRoutine
    ���: ���� �ִ� ���� ���� �Ǻ��ϰ�, ���� �� ������ EndWave�� �Ѿ
    *********************************************************************************************/
    private IEnumerator CheckEnemiesRoutine()
    {
        while (true)
        {
            // �±� ��� �˻� - �ʿ��ϸ� Ǯ�� �Ŵ�����ī���ͷ� ��ü ����
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                // ���̺� ���� ó��
                waveState = WaveState.EndWave;
                EndWave();
                yield break;                 // ���� ����
            }

            yield return enemyCheckInterval; // 0.5 �� ���
            UIManager.Instance.enemyCountText.text = $"ENEMY : {GameObject.FindGameObjectsWithTag("Enemy").Length}";
        }
    }

    /*********************************************************************************************
    �Լ�: EndWave
    ���: 
        1. ���̺� ī��Ʈ ����
        2. ���Ÿ� ���� ���̺� ���� 1������, �ٰŸ� ���� 3���̺긶�� 1������ ����
        3. ����ð��� �ʱ�ȭ�ϰ� ���¸� BeforWave�� �ѱ�
    *********************************************************************************************/
    private void EndWave()
    {
        waveCount++;
        droneEnemyToSpawn = waveCount;
        meleeEnemyToSpawn = waveCount / 3;
        currentTime = 0f;
        waveState = WaveState.BeforeWave;

        // UI ������Ʈ
        UIManager.Instance.waveText.text = $"Wave : {waveCount}";

        // �˻� �ڷ�ƾ ����(������ġ)
        if (enemyCheckRoutine != null)
        {
            StopCoroutine(enemyCheckRoutine);
            enemyCheckRoutine = null;
        }
    }

    /*********************************************************************************************
    �Լ�: GameOver
    ���: 
        1. ���� ���� ���·� �ٲ�
        2. �÷��̾ gameOverPosition���� �̵� �� ȸ��
        3. ���� ���� UI�� Ȱ��ȭ�ϰ� �÷��̾��� ȭ���� ������ ��
    *********************************************************************************************/
    public void GameOver()
    {
        isGameStarted = false; // ���� ���� ���·� ���� (���Ͻô� ���·� ����)
        // ���� ���� (���� ���� ī�޶� Ȱ��ȭ)
        player.transform.position = gameOverPosition.position;
        player.transform.rotation = gameOverPosition.rotation;

        // Game Over UI ǥ��
        UIManager.Instance.gameOverUI.SetActive(true);

        // �������� ����Ʈ 
        redScreenEffect.gameObject.SetActive(true);
    }

    /*********************************************************************************************
    �Լ�: AddPlayerCoin
    ���: �÷��̾��� ������ ������Ű�� UI�� ������
    �Ű�����
        - coin: �÷��̾�� �߰��� ������ ��
*********************************************************************************************/
    public void AddPlayerCoin(int coin)
    {
        playerCoin += coin;
        UIManager.Instance.coinText.text = $"COIN: {playerCoin}$";
    }
}