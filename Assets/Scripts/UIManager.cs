using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameStartUI;
    public GameObject cctvPanelUI;
    public GameObject gameOverUI;

    public Text waveText;
    public Text enemyCountText;
    public Text towerHPText;
    public Text coinText;

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
}