using BNG;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;


public class Monster : MonoBehaviour
{
    public UnityEngine.UI.Image smashImage;

    public UnityEngine.UI.Slider monsterHpSlider;

    //스턴 추가 - hit상태
    public enum State
    {
        IDLE,
        TRACE,
        ATTACK,
        STUN,
        DIE
    }

    public State state = State.IDLE;

    //public enum MonsterGroup
    //{
    //    NORMAL,
    //    ELITE
    //}

    public enum Type
    {
        HUMAN_ROBOT,
        HUMAN_GOLEM,
        BEAST_SPIDER,
        BEAST_STING,
        SIMPLE_FUNGI,
        SIMPLE_SPOOK,

        HUMAN_ROBOTRED,
        HUMAN_GOLEMFIRE,
        BEAST_SCORPION,
        BEAST_QUEENWORM,
        SIMPLE_TOADSTOOL,
        SIMPLE_PHANTOM
    }

    //[System.Serializable]
    //public class MonsterData
    //{
    //    public Type MonsterType { get; set; }
    //    public MonsterGroup MonsterGroup { get; set; }
    //}

    //public MonsterData monsterData = new MonsterData();

    public int monsterId;

    public Type monsterType = Type.HUMAN_ROBOT;

    [Header("분쇄")]
    public int count = 0;      //깍아냄
    public int maxCount = 10;  //깍아냄 횟수 충족

    [Header("몬스터 원거리 관련")]
    public Transform bulletPortLeft;
    public Transform bulletPortRight;
    public Transform bulletPort;
    public GameObject monsterBullet;

    [Header("몬스터 테이블")]
    public float hp = default;       //체력이랑 damageble 보내준다
    public float attack = default;
    public float attDelay = default;   //몬스터 공격간격 
    public float exp = default;
    public float speed = default;      //몬스터 이동속도
    public float recRange = 30.0f;   //pc 인식범위
    public float attRange = 2.0f;   //pc 공격범위
    public float stunDelay = 1f;
    public float stunCount = default;  //경직 횟수, 일반 몬스터는 필요 없음
    public float stopDistance = default;
    //몬스터 이름도 추가될 예정

    [Header("트랜스폼")]
    public Transform monsterTr;
    public Transform playerTr;

    [Header("몬스터 컴포넌트")]
    public Animator anim;
    public Rigidbody rigid;
    public NavMeshAgent nav;
   

    public DamageCollider[] damageCollider;

    public Damageable damageable;

    public readonly int hashRun = Animator.StringToHash("isRun");

    public readonly int hashWalkingAttack = Animator.StringToHash("isWalkingAttack");
    
    public readonly int hashAttack = Animator.StringToHash("isAttack");
    public readonly int hashAttack2 = Animator.StringToHash("isAttack2");
    public readonly int hashAttack3 = Animator.StringToHash("isAttack3");
    public readonly int hashAttack4 = Animator.StringToHash("isAttack4");

    public readonly int hashAttackRuning = Animator.StringToHash("isAttackRuning");
    public readonly int hashAttackRuning2 = Animator.StringToHash("isAttackRuning2");
    public readonly int hashAttackRuning3 = Animator.StringToHash("isAttackRuning3");

    public readonly int hashHit = Animator.StringToHash("isDamage");

    public readonly int hashDie = Animator.StringToHash("isDie");

    public readonly int hashidle = Animator.StringToHash("isIdle");

    public readonly int hashStun = Animator.StringToHash("isStun");

    public bool isDie = false;
    public bool isStun = false;

    public IEnumerator stunRoutine; // 스턴 루틴

    [Header("Debug")]
    public float distanceDebug;


    // Start is called before the first frame update
    void Start()
    {
        GetData(monsterId);

        monsterTr = GetComponent<Transform>();
        playerTr = GameObject.FindWithTag("Player").GetComponent<PlayerPosition>().playerPos;

        damageable = GetComponent<Damageable>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();

        damageable.Health = hp;

        foreach (DamageCollider damageCollider in damageCollider)
        {
            damageCollider.Damage = attack;
            //attack = damageCollider.Damage; // 지환 : attack은 시트에서 가져온 데이터 값
        }


        nav.speed = speed;

        nav.stoppingDistance = stopDistance;
        //nav.stoppingDistance = attRange - 0.5f;

        SetMaxHealth(hp);

        InitMonster();
    }

    public void InitMonster()
    {
        isDie = false;
        nav.isStopped = false;
        StartCoroutine(MonsterState());
        StartCoroutine(MonsterAction());
    }


    void FixedUpdate()
    {
        if (state != State.DIE)
        {
            // Look At Y 각도로만 기울어지게 하기
            Vector3 targetPostition = 
                new Vector3(playerTr.position.x,this.transform.position.y, playerTr.position.z);
            this.transform.LookAt(targetPostition);
            //transform.LookAt(playerTr.position);
        }
    }

    public void GetData(int id)
    {
        hp = (float)DataManager.instance.GetData(id, "MonHP", typeof(float));  
        exp = (float)DataManager.instance.GetData(id, "MonExp", typeof(float));
        attack = (float)DataManager.instance.GetData(id, "MonAtt", typeof(float));
        attDelay = (float)DataManager.instance.GetData(id, "MonDel", typeof(float));
        speed = (float)DataManager.instance.GetData(id, "MonSpd", typeof(float));
        attRange = (float)DataManager.instance.GetData(id, "MonAtr", typeof(float));
        recRange = (float)DataManager.instance.GetData(id, "MonRer", typeof(float));
        stunDelay = (float)DataManager.instance.GetData(id, "MonSTFDel", typeof(float));

        stopDistance = (float)DataManager.instance.GetData(id, "MonStd", typeof(float));
    }

    public void SetMaxHealth(float newHealth)
    {
        monsterHpSlider.maxValue = newHealth;
        monsterHpSlider.value = newHealth;
    }

    public void SetHealth(float newHealth)
    {
        monsterHpSlider.value = newHealth;
    }


    // 스테이트를 관리하는 코루틴
    // 역할 : 스테이트를 변환만 해준다. 다른건 없음.
    IEnumerator MonsterState()
    {
        while (true)
        {
            // 체력이 0 이하면 죽은 상태로 전이
            if (damageable.Health <= 0)
            {
                SetHealth(0);
                state = State.DIE;
            }
           

            // 스턴을 맞을 경우 스턴 상태로 전이
            else if(isStun)
                state = State.STUN;

            else
            {
                float distance = Vector3.Distance(playerTr.position, monsterTr.position);
                distanceDebug = distance;
                if (distance <= attRange)
                {
                    state = State.ATTACK;
                }
                else if (distance <= recRange)
                {
                    state = State.TRACE;
                }
                else
                {
                    state = State.IDLE;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // 몬스터의 상태에 따라 전이되는 액션
    public virtual IEnumerator MonsterAction()
    {
        while(true)
        { 
        switch (state)
        {
            // IDLE 상태 =======================================================
            case State.IDLE:
                //Debug.Log("IDLE state");
                nav.isStopped = true;
                anim.SetBool(hashRun, false);
                anim.SetBool(hashidle, true);
                //anim.SetBool(hashWalkingAttack, false);
                anim.SetBool(hashAttack, false);
                anim.SetBool(hashAttack2, false);
                anim.SetBool(hashAttack3, false);
                anim.SetBool(hashAttack4, false);
                break;

            // TRACE 상태 =======================================================
            case State.TRACE:
                //Debug.Log("TRACE state");
                nav.isStopped = false;
                nav.SetDestination(playerTr.position);
                anim.SetBool(hashRun, true);
                anim.SetBool(hashWalkingAttack, false);
                anim.SetBool(hashAttackRuning, true);
                anim.SetBool(hashAttackRuning2, true);
                anim.SetBool(hashAttackRuning3, true);
                break;

            // ATTACK 상태 =======================================================
            case State.ATTACK:
                    
                //Debug.Log("ATTACK state");

                switch (monsterType)
                {
                    case Type.HUMAN_ROBOT:

                        anim.SetBool(hashWalkingAttack, true);
                        anim.SetBool(hashAttack, true);
                        yield return new WaitForSeconds(0.5f);
                        anim.SetBool(hashidle, true);
                        anim.SetBool(hashAttack, false);
                        anim.SetBool(hashRun, false);
                        yield return new WaitForSeconds(0.3f);
                        break;

                    case Type.HUMAN_GOLEM:

                        int humanGolem = Random.Range(0, 3);

                        switch (humanGolem)
                        {
                            case 0:
                                anim.SetBool(hashWalkingAttack, true);
                                anim.SetBool(hashAttack, true);
                                yield return new WaitForSeconds(1.3f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack, false);
                                anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 1:
                                //anim.SetBool(hashWalkingAttack, true);
                                anim.SetBool(hashAttack2, true);
                                    
                                yield return new WaitForSeconds(1.3f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack2, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 2:
                                //anim.SetBool(hashWalkingAttack, true);
                                anim.SetBool(hashAttack3, true);
                                yield return new WaitForSeconds(1.2f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack3, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                        }
                        break;

                    case Type.BEAST_SPIDER:

                        int spider = Random.Range(0, 3);

                        switch (spider)
                        {
                            case 0:
                                anim.SetBool(hashWalkingAttack, true);
                                anim.SetBool(hashAttack, true);
                                yield return new WaitForSeconds(0.8f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 1:
                                anim.SetBool(hashAttack2, true);
                                yield return new WaitForSeconds(0.8f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack2, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 2:
                                anim.SetBool(hashAttack3, true);
                                yield return new WaitForSeconds(0.8f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack3, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                        }
                        break;

                    case Type.BEAST_STING:

                        int sting = Random.Range(0, 3);

                        switch (sting)
                        {
                            case 0:
                                anim.SetBool(hashWalkingAttack, true);
                                anim.SetBool(hashAttack, true);
                                yield return new WaitForSeconds(0.8f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 1:
                                anim.SetBool(hashAttack2, true);
                                yield return new WaitForSeconds(0.8f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack2, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 2:
                                anim.SetBool(hashAttack3, true);
                                yield return new WaitForSeconds(0.8f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack3, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;
                            //case 3:
                            //    anim.SetBool(hashAttack4, true);
                            //    yield return new WaitForSeconds(0.8f);
                            //    anim.SetBool(hashidle, true);
                            //    anim.SetBool(hashAttack4, false);
                            //    //anim.SetBool(hashWalkingAttack, false);
                            //    anim.SetBool(hashRun, false);
                            //    yield return new WaitForSeconds(0.3f);
                            //    break;
                        }
                        break;

                    case Type.SIMPLE_FUNGI:

                        int fungi = Random.Range(0, 2);

                        switch (fungi)
                        {
                            case 0:
                                anim.SetBool(hashWalkingAttack, true);
                                anim.SetBool(hashAttack, true);
                                yield return new WaitForSeconds(0.2f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 1:
                                anim.SetBool(hashAttack2, true);
                                yield return new WaitForSeconds(0.2f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack2, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;
                        }
                        break;

                    case Type.SIMPLE_SPOOK:

                        int spook = Random.Range(0, 3);

                        switch (spook)
                        {
                            case 0:
                                anim.SetBool(hashWalkingAttack, true);
                                anim.SetBool(hashAttack, true);
                                yield return new WaitForSeconds(0.7f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 1:
                                    
                                anim.SetBool(hashAttack2, true);
                                yield return new WaitForSeconds(0.7f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack2, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;

                            case 2:
                                anim.SetBool(hashAttack3, true);
                                yield return new WaitForSeconds(0.7f);
                                anim.SetBool(hashidle, true);
                                anim.SetBool(hashAttack3, false);
                                //anim.SetBool(hashWalkingAttack, false);
                                anim.SetBool(hashRun, false);
                                yield return new WaitForSeconds(0.3f);
                                break;
                        }
                        break;
                }
                break;

                    
            case State.DIE:
                isDie = true;
                nav.isStopped = true;
                //Debug.Log("nav.isStopped: " + nav.isStopped);
                anim.SetTrigger(hashDie);

                //Destroy(this.gameObject, 1.3f); //damageable 쪽에서 처리
                yield break;
            }

        yield return new WaitForSeconds(0.1f);
        }
        

    }

    public virtual void OnDeal()
    {
        if (isStun)
            return;
        if (state != State.DIE && state != State.STUN)
        {
            if (damageable.Health >= 0)
            {
                SetHealth(damageable.Health);
                // 만약에 스턴루틴에 이미 다른 코루틴이 실행중인 경우
                if (stunRoutine != null)
                {
                    StopCoroutine(stunRoutine);
                    stunRoutine = null;
                }

                stunRoutine = StunDelay();
                StartCoroutine(stunRoutine);

                //Debug.Log($"state:{state}");
            }
        }
        //Debug.Log($"hp:{damageable.Health}");
    }
    // 스턴 딜레이
    public virtual IEnumerator StunDelay()
    {
        //분쇄 카운터
        if (count >= maxCount)
        {
            //TODO:데미지 받아서 카운트 충족시 디버프


            //smashImage.enabled = true;

        }

        isStun = true;
        anim.SetTrigger(hashHit);
        damageable.stun = true;
        yield return new WaitForSeconds(stunDelay);
        isStun = false;
        damageable.stun = false;
        yield break;
    }

    

    
   
    void OnDrawGizmos()
    {
        if (state == State.TRACE)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, recRange);
        }

        if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attRange);
        }

    }

    public virtual void Explosion(int index)
    {
        switch (index)
        {
            case 0:
                Destroy(this.gameObject);
                break;
        }

    }



}



