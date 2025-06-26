using UnityEngine;

public class ItemFloat : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatAmplitude = 0.5f; // 바운스 높이
    [SerializeField] private float floatFrequency = 1f;   // 바운스 속도

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // y축으로 사인 곡선을 따라 상하 진동
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}