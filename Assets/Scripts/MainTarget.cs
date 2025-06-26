using UnityEngine;

public class MainTarget : MonoBehaviour
{
    public float targetHP;

    private PlayerMove playerMove;

    private void Start()
    {
        UIManager.Instance.towerHPText.text = $"TOWER HP\r\n{targetHP}/100";
    }

    public void OnTargetDamaged(float damage)
    {
        targetHP -= damage;
        if (targetHP <= 0)
        {
            targetHP = 0;
            GameManager.Instance.GameOver();
        }
        UIManager.Instance.towerHPText.text = $"TOWER HP\r\n{targetHP}/100";    // UI 업데이트
        // todo: 화면 피격 효과
    }
}
