// SimpleMovementTest.cs 새로 생성
using UnityEngine;

public class SimpleMovementTest : MonoBehaviour
{
    void Update()
    {
        // 매우 간단한 이동 테스트
        transform.position += new Vector3(1, 0, 0) * Time.deltaTime;
        
        // 매 프레임 위치 출력
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Test Position: {transform.position}");
            Debug.Log($"Time.deltaTime: {Time.deltaTime}");
            Debug.Log($"Time.timeScale: {Time.timeScale}");
        }
    }
}