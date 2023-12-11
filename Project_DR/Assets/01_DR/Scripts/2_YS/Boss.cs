using BNG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    //public static Action boss;

    public UnityEvent unityEvent;

    public UnityEngine.UI.Slider bossHPSlider;

    public GameObject bossState;

    public Rigidbody rigid;
    public Damageable damageable;
    GameObject instantLazer;
    public int bossId;

    public float power = 5.0f;

    [Header("넉백 관련")]
    private bool isMoving = false;
    private float moveDuration = 1.0f;
    private float moveTimer = 0.0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    [Header("레이저 관련")]
    public Transform lazerPort;
    public GameObject lazer;

    [Header("원거리 공격")]
    public int bulletCount = 6;
    public GameObject smallBulletPrefab;

    [Header("애드벌룬 투사체 생성")]
    public Transform bouncePort;
    public Transform bouncePortLeft;
    public Transform bouncePortRight;
    public GameObject bounce;

    [Header("타겟")]
    public Transform target;

    [Header("테이블")]
    public float hp = default;
    public float maxHp = default;

    [Header("조건")]
    public bool isLazer = false;
    public bool isPattern = false;
    public bool isDie = false;
    public bool isPatternExecuting = false;
    public bool isShoot = false;
    public bool isPushPlayer = false;

    [Header("패턴 간격")]
    public float patternInterval = 2.0f;

    [Header("포물선 터지는 오브젝트")]
    public GameObject explosionPrefab;
    public Transform explosionPort;
    public Transform explosionPortLeft;
    public Transform explosionPortRight;

    [Header("포물선 안 터지는 오브젝트")]
    public Transform bigBrickPort;
    public Transform bigBrickPortLeft;
    public Transform bigBrickPortRight;
    public GameObject bigBrick;

    [Header("유도 미사일 테스트")]
    public Transform testPort;
    public GameObject testBullet;



    void Awake()
    {
        GetData(bossId);
    }

    void Start()
    {
        InitializeBoss();
    }
    void InitializeBoss()
    {
        Debug.Log($"게임시작");
        bossState = GameObject.FindWithTag("Boss");

        rigid = GetComponent<Rigidbody>();
        target = GameObject.FindWithTag("Player").GetComponent<PlayerPosition>().playerPos;
        damageable.Health = maxHp;

        SetMaxHealth(maxHp);

        if(bossState)
        {
            bossState.GetComponent<BossState>().CastSpell();
        }


    }

    void FixedUpdate()
    {
        if (!target)
        {
            return;
        }

        // Look At Y 각도로만 기울어지게 하기
        Vector3 targetPostition =
            new Vector3(target.position.x, this.transform.position.y, target.position.z);
        this.transform.LookAt(targetPostition);
        


    }



    public void GetData(int id)
    {
        maxHp = (float)DataManager.instance.GetData(id, "BossHP", typeof(float));


    }

    public void SetMaxHealth(float newHealth)
    {
        bossHPSlider.maxValue = newHealth;
        bossHPSlider.value = newHealth;
    }
    public void SetHealth(float newHealth)
    {
        bossHPSlider.value = newHealth;
    }

  

    IEnumerator ExecutePattern()
    {
        //Debug.Log("코루틴이 한번만 실행이 되는지");

        while (!isDie && !isPatternExecuting)
        {
            //Debug.Log($"hp{hp}");
            yield return new WaitForSeconds(0.1f);

            //체력에 따라 랜덤으로 패턴 선택
            if (damageable.Health <= maxHp * 1.0f && damageable.Health > maxHp * 0.75f)
            {
                Debug.Log("작동");
                RandomPattern();
                if (bossState)
                {
                    bossState.GetComponent<BossState>().Attack();
                }

                ////T0DO:넉백시
                //if(bossState)
                //{
                //    bossState.GetComponent<BossState>().CastSpell();
                //}

                isPatternExecuting = true;
                yield return new WaitForSeconds(patternInterval);
                isPatternExecuting = false;

            }
            else if (damageable.Health <= maxHp * 0.75f && damageable.Health > maxHp * 0.5f)
            {
                Debug.Log("작동2");

                RandomPattern();
                if (bossState)
                {
                    bossState.GetComponent<BossState>().Attack();
                }

                ////T0DO:넉백시
                //if(bossState)
                //{
                //    bossState.GetComponent<BossState>().CastSpell();
                //}

                isPatternExecuting = true;
                yield return new WaitForSeconds(patternInterval);
                isPatternExecuting = false;
                
            }
            else if (damageable.Health <= maxHp * 0.5f && damageable.Health > maxHp * 0.25f)
            {
                Debug.Log("작동3");


                RandomPattern();
                if (bossState)
                {
                    bossState.GetComponent<BossState>().Attack();
                }

                ////T0DO:넉백시
                //if(bossState)
                //{
                //    bossState.GetComponent<BossState>().CastSpell();
                //}


                isPatternExecuting = true;
                yield return new WaitForSeconds(patternInterval);
                isPatternExecuting = false;
               

            }
            else if (damageable.Health < maxHp * 0.25f)
            {
                Debug.Log("작동4");

                RandomPattern();
                if (bossState)
                {
                    bossState.GetComponent<BossState>().Attack();
                }

                ////T0DO:넉백시
                //if(bossState)
                //{
                //    bossState.GetComponent<BossState>().CastSpell();
                //}

                isPatternExecuting = true;
                yield return new WaitForSeconds(patternInterval);
                isPatternExecuting = false;
               
            }



        }


    }

    void RandomPattern()
    {
        int pattern = UnityEngine.Random.Range(0, 4);

        switch (pattern)
        {
            case 0:
                TestShoot();   //유도미사일 테스트
                break;
            case 1:
                //ExplosionShoot();
                break;
            case 2:
                //BigBrickShoot();
                break;
            case 3:
                //ExplosionShoot();
                break;
        }
    }

    IEnumerator PlayShoot()
    {
        if (!isShoot)
        {
            isShoot = true;

            for (int i = 0; i < bulletCount; i++)
            {
                // 위치 조절
                Vector3 offset = Vector3.zero;

                if (i % 2 == 0)
                {
                    offset = new Vector3(2.0f, 0, 0); 
                }
                else
                {
                    if (i % 4 == 1)
                    {
                        offset = new Vector3(0, 2.0f, 0); 
                    }
                    else
                    {
                        offset = new Vector3(-2.0f, 0, 0); 
                    }
                }


                GameObject instantBullet = Instantiate(smallBulletPrefab, transform.position + offset, Quaternion.identity);


                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();

                // 총알 속도 설정
                rigidBullet.velocity = offset.normalized * 10.0f;

                instantBullet.transform.LookAt(target);

                yield return new WaitForSeconds(0.4f);

                Destroy(instantBullet, 6.0f);

            }

            isShoot = false;
        }
    }



    void LazerShoot()
    {
        instantLazer = Instantiate(lazer, lazerPort.position, lazerPort.rotation);
        instantLazer.transform.LookAt(target.position);
        lazerPort.transform.LookAt(target.position);

        Invoke("LazerDestroy", 1.5f);
        //Debug.Log($"작동:{Invoke}");
    }

    void LazerDestroy()
    {
        Destroy(instantLazer);
    }



    public Vector3 BigBrick(Vector3 portPosition)
    {
        int randomBrick = UnityEngine.Random.Range(0, 10);
        // 해당 위치에 따라 초기 속도 설정
        Vector3 initialVelocity = transform.forward * randomBrick;  //10.0f;

        Vector3 target = transform.forward * randomBrick;   //5.0f;

        //int randomBrick = UnityEngine.Random.Range(0, 3);
        // 각 포트 위치에 따라 Y축 오프셋 값 조절
        if (portPosition == bigBrickPort.position)
        {
            target.y += randomBrick;
        }
        else if (portPosition == bigBrickPortLeft.position)
        {   
            target.y += randomBrick;
        }
        else if (portPosition == bigBrickPortRight.position)
        {
            target.y += randomBrick;
        }

        // 초기 속도와 목표 위치를 결합
        Vector3 combinedVector = initialVelocity + target;

        return combinedVector;
    }

    void BigBrickShoot()
    {
        // 가운데 위치에 대해 오브젝트를 생성하고 힘을 적용
        GameObject instantBrick = Instantiate(bigBrick, bigBrickPort.position, Quaternion.identity);
        instantBrick.GetComponent<Rigidbody>().AddForce(BigBrick(bigBrickPort.position), ForceMode.Impulse);

        // 왼쪽 위치에 대해 오브젝트를 생성하고 힘을 적용
        GameObject instantBrickLeft = Instantiate(bigBrick, bigBrickPortLeft.position, Quaternion.identity);
        instantBrickLeft.GetComponent<Rigidbody>().AddForce(BigBrick(bigBrickPortLeft.position), ForceMode.Impulse);

        // 오른쪽 위치에 대해 오브젝트를 생성하고 힘을 적용
        GameObject instantBrickRight = Instantiate(bigBrick, bigBrickPortRight.position, Quaternion.identity);
        instantBrickRight.GetComponent<Rigidbody>().AddForce(BigBrick(bigBrickPortRight.position), ForceMode.Impulse);

        Destroy(instantBrick, 6.0f);
        Destroy(instantBrickLeft, 6.0f);
        Destroy(instantBrickRight, 6.0f);
    }

    public Vector3 ExplosionBox(Vector3 portPosition)
    {
        int randomBox = UnityEngine.Random.Range(0, 10);
        // 해당 위치에 따라 초기 속도 설정
        Vector3 initialVelocity = transform.forward * randomBox;  //10.0f;

        Vector3 target = transform.forward * randomBox;   //5.0f;

        // 각 포트 위치에 따라 Y축 오프셋 값 조절
        if (portPosition == explosionPort.position)
        {
            target.y += randomBox;
        }
        else if (portPosition == explosionPortLeft.position)
        {
            target.y += randomBox;
        }
        else if (portPosition == explosionPortRight.position)
        {
            target.y += randomBox;
        }

        // 초기 속도와 목표 위치를 결합
        Vector3 combinedVector = initialVelocity + target;

        return combinedVector;
    }


    void ExplosionShoot()
    {
        GameObject instantExplosion = Instantiate(explosionPrefab, explosionPort.position, Quaternion.identity);
        instantExplosion.GetComponent<Rigidbody>().AddForce(ExplosionBox(explosionPort.position), ForceMode.Impulse);

        GameObject instantExplosionLeft = Instantiate(explosionPrefab, explosionPortLeft.position, Quaternion.identity);
        instantExplosionLeft.GetComponent<Rigidbody>().AddForce(ExplosionBox(explosionPortLeft.position), ForceMode.Impulse);

        GameObject instantExplosionRight = Instantiate(explosionPrefab, explosionPortRight.position, Quaternion.identity);
        instantExplosionRight.GetComponent<Rigidbody>().AddForce(ExplosionBox(explosionPortRight.position), ForceMode.Impulse);
    }

    void BounceShoot()
    {
        GameObject instantBounce = Instantiate(bounce, bouncePort.position, bouncePort.rotation);

        GameObject bounceLeft = Instantiate(bounce, bouncePortLeft.position, bouncePortLeft.rotation);

        GameObject bounceRight = Instantiate(bounce, bouncePortRight.position, bouncePortRight.rotation);

        Destroy(instantBounce, 8.0f);
        Destroy(bounceLeft, 8.0f);
        Destroy(bounceRight, 8.0f);
    }

    //유도미사일 테스트
    void TestShoot()
    {
        GameObject instanceTestBullet = Instantiate(testBullet, testPort.position, testPort.rotation);
    }


    public void OnDeal()
    {
        if (!isDie)
        {
            if (damageable.Health >= 0)
            {
                SetHealth(damageable.Health);
                Debug.Log($"Current HP: {damageable.Health}");
            }

            if (damageable.Health <= 0)
            {
                SetHealth(0);
                isDie = true;
                Debug.Log($"isDie:{isDie}");
                // 이벤트 호출
                unityEvent?.Invoke();

                if (bossState)
                {
                    bossState.GetComponent<BossState>().Attack();
                }
                StopAllCoroutines();
                //Debug.Log("코루틴 멈춤");
            }
        }

    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("인식되냐");
            StartCoroutine(ExecutePattern());
        }
    }

}