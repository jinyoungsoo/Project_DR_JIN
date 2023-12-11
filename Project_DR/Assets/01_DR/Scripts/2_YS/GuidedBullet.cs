using UnityEngine;

public class GuidedBullet : MonoBehaviour
{
    public float speed = 15.0f;
    public float turnSpeed = 15.0f;
    public Transform target;
    private Vector3 initialDirection;
    

    void Start()
    {
        // 플레이어를 찾아서 타겟으로 설정
        target = GameObject.FindWithTag("Player")?.GetComponent<PlayerPosition>()?.playerPos;

        // 타겟이 없을 경우 리턴
        if (target == null)
        {
            return;
        }

        // 초기 방향 벡터 계산
        initialDirection = target.position - transform.position;
    }

    void Update()
    {
        
        Shoot();
    }

    void Shoot()
    {
        // 초기 방향 벡터의 크기가 0인 경우 리턴
        if (initialDirection.magnitude == 0f)
            return;

        float t = 0.015f; // 원하는 시간 비율로 조정할 것
        transform.position = Vector3.LerpUnclamped(transform.position, target.position, t);

        // Lerp를 이용하여 위치 이동
        transform.position = Vector3.LerpUnclamped(transform.position, target.position, t);

        // 타겟 방향으로 회전
        Vector3 directionVec = target.position - transform.position;
        Quaternion qua = Quaternion.LookRotation(directionVec);
        transform.rotation = Quaternion.Slerp(transform.rotation, qua, Time.deltaTime * turnSpeed); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Player") || collision.collider.CompareTag("Wall"))
        {
            Destroy(this.gameObject);
            Debug.Log($"파괴:{this.gameObject}");
        }
    }
}
