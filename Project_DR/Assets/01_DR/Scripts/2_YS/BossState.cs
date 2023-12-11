using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossState : MonoBehaviour
{
    public enum Type
    {
        GOLEM_ICE,
        BAT_LORD,
        SHADOW,
        WRAITH,
        DEATH_MAGE
    }

    public Type bossType = Type.GOLEM_ICE;

    public Animator anim;
    public Transform target;

    void Start()
    {
        anim = GetComponent<Animator>();
        target = GameObject.FindWithTag("Player").GetComponent<PlayerPosition>().playerPos;
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
        //transform.LookAt(playerTr.position);

    }

    public void Die()
    {
        Debug.Log("애니메이션 작동");
        anim.SetTrigger("isDie");

    }

    public void CastSpell()
    {
        anim.SetTrigger("isCast");
    }

    public void Attack()
    {
        switch(bossType)
        {
            case Type.DEATH_MAGE:
            int deathMage = UnityEngine.Random.Range(0, 5);
                switch (deathMage)
                {
                    case 0:
                        anim.SetTrigger("isAttack");
                        break;
                    case 1:
                        anim.SetTrigger("isAttack2");
                        break;
                    case 2:
                        anim.SetTrigger("isAttack3");
                        break;
                    case 3:
                        anim.SetTrigger("isAttack4");
                        break;
                    case 4:
                        anim.SetTrigger("isAttack5");
                        break;
                }
                break;

            case Type.BAT_LORD:
            int batLord = UnityEngine.Random.Range(0, 4);
                switch(batLord)
                {
                    case 0:
                        anim.SetTrigger("isAttack");
                        break;
                        case 1:
                        anim.SetTrigger("isAttack2");
                        break;
                        case 2:
                        anim.SetTrigger("isAttack3");
                        break;
                        case 3:
                        anim.SetTrigger("isAttack4");
                        break;
                }
                break;

            case Type.GOLEM_ICE:
                int golemIce = UnityEngine.Random.Range(0, 5);
                switch(golemIce)
                {
                    case 0:
                        anim.SetTrigger("isAttack");
                        break;
                    case 1:
                        anim.SetTrigger("isAttack2");
                        break;
                    case 2:
                        anim.SetTrigger("isAttack3");
                        break;
                    case 3:
                        anim.SetTrigger("isAttack4");
                        break;
                    case 4:
                        anim.SetTrigger("isAttack5");
                        break;
                }
                break;

            case Type.SHADOW:
                int shadow = UnityEngine.Random.Range(0, 3);
                switch(shadow)
                {
                    case 0:
                        anim.SetTrigger("isAttack");
                        break;
                    case 1:
                        anim.SetTrigger("isAttack2");
                        break;
                    case 2:
                        anim.SetTrigger("isAttack3");
                        break;
                }
                break;

            case Type.WRAITH:
                int wraith = UnityEngine.Random.Range(0, 4);
                switch(wraith)
                {
                    case 0:
                        anim.SetTrigger("isAttack");
                        break;
                    case 1:
                        anim.SetTrigger("isAttack2");
                        break;
                    case 2:
                        anim.SetTrigger("isAttack3");
                        break;
                    case 3:
                        anim.SetTrigger("isAttack4");
                        break;
                }
                break;
        }
    }

       
    public void Explosion(int index)
    {
        switch (index)
        {
            case 0:
                Destroy(this.gameObject, 1.0f);
                break;
        }

    }

}
