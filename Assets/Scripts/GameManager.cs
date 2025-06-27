using Meta.WitAi;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    enum WaveState { BeforeWave, Process, EndWave }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefab_Drone;
    [SerializeField] private GameObject enemyPrefab_Melee;

    [Header("Enemy SpawnPoint")]
    [SerializeField] private Transform meleeEnemySpawnPoint;
    [SerializeField] private Transform droneEnemySpawnPoint;

    [Header("Target")]
    public Transform mainTarget;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffect;

    public bool isGameStarted = false;
    private WaveState waveState = WaveState.BeforeWave;
    private int waveCount;
    private int meleeEnemyToSpawn;
    private int droneEnemyToSpawn;
    private float currentTime = 0f;
    private float waitingTime = 5f;
    private float spawnDelayTime = 1.5f;
    private bool shouldCheckEnemy = false;

    public int playerCoin;

    private readonly WaitForSeconds enemyCheckInterval = new WaitForSeconds(0.5f);
    private Coroutine enemyCheckRoutine;       // 코루틴 핸들 보관용

    private void Awake()
    {
        // 이미 인스턴스가 존재하면 새로 생성된 객체 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 최초 인스턴스 초기화
        Instance = this;
        DontDestroyOnLoad(gameObject);   // 씬 전환 시에도 유지
    }

    public void SpawnExplosionParticle(Transform transform)
    {
        if (explosionEffect)
        {
            GameObject spawnedEffect = Instantiate(explosionEffect, transform);
            if (spawnedEffect)
            {
                spawnedEffect.GetComponent<ParticleSystem>().Play();
                Debug.Log("effect spawned");
            }
            //spawnedEffect.GetComponent<ParticleSystem>().Play();
        }
    }
    public void GameStart()
    {
        isGameStarted = true;
        waveCount = 0;
        playerCoin = 0;

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

    private IEnumerator EnemySpwanProcess()
    {
        // 1. 근접 적 스폰
        for (int i = 0; i < meleeEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Melee, meleeEnemySpawnPoint.position, meleeEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // 2. 드론 적 스폰
        for (int i = 0; i < droneEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Drone, droneEnemySpawnPoint.position, droneEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // 적을 모두 스폰한 후 체크 플래그 활성화
        enemyCheckRoutine = StartCoroutine(CheckEnemiesRoutine());
    }

    private IEnumerator CheckEnemiesRoutine()
    {
        while (true)
        {
            // 태그 기반 검색 - 필요하면 풀링 매니저·카운터로 교체 가능
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                // 웨이브 종료 처리
                waveState = WaveState.EndWave;
                EndWave();
                yield break;                 // 루프 종료
            }

            yield return enemyCheckInterval; // 0.5 초 대기
            UIManager.Instance.enemyCountText.text = $"ENEMY : {GameObject.FindGameObjectsWithTag("Enemy").Length}";
        }
    }

    private void EndWave()
    {
        waveCount++;
        droneEnemyToSpawn = waveCount;
        meleeEnemyToSpawn = waveCount /*/3*/;
        currentTime = 0f;
        waveState = WaveState.BeforeWave;

        // UI 업데이트
        UIManager.Instance.waveText.text = $"Wave : {waveCount}";

        // 검사 코루틴 정지(안전장치)
        if (enemyCheckRoutine != null)
        {
            StopCoroutine(enemyCheckRoutine);
            enemyCheckRoutine = null;
        }
    }

    public void GameOver()
    {
        // 게임 오버 시 맵 전체의 적을 찾음
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy); // 적을 찾아서 모두 제거
        }

        isGameStarted = false; // 게임 종료 상태로 설정 (원하시는 상태로 조정)
        Debug.Log("GameOVer");
    }

    public void AddPlayerCoin(int coin)
    {
        playerCoin += coin;
        UIManager.Instance.coinText.text = $"COIN: {playerCoin}$";
    }
}