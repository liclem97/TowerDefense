using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public float minTime = 1;
    public float maxTime = 5;
    float createTime;
    float currentTime;
    public Transform[] spawnPoints;
    public GameObject droneFactory;
    
    void Start()
    {
        createTime = Random.Range(minTime, maxTime);
    }
    
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > createTime)   // 1~5초 사이 랜덤하게 드론 생성
        {
            GameObject drone = Instantiate(droneFactory);
            int index = Random.Range(0, spawnPoints.Length);
            drone.transform.position = spawnPoints[index].position; // 스폰한 드론을 4개의 스폰 포인트 중 하나의 위치 지정
            currentTime = 0f;   // 경과 시간 초기화
            createTime = Random.Range(minTime, maxTime);    // 생성시간 재할당
        }
    }
}
