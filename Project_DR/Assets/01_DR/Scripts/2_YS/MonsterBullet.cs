using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBullet : MonoBehaviour
{
    [Header("테이블 관련")]
    [SerializeField]
    private float speed = default;

    private Rigidbody rigid;

    public float hp = default;

    [Header("테이블 아이디")]
    public int tableId;

    void Awake()
    {
        GetData(tableId);
    }

    public void GetData(int tableId)
    {
        hp = (float)DataManager.instance.GetData(tableId, "Hp", typeof(float));
        speed = (float)DataManager.instance.GetData(tableId, "Speed", typeof(float));


    }

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();

        rigid.velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(hp >= 0)
        {
            Destroy(this.gameObject);
        }
    }


    public virtual void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Player") || collision.collider.CompareTag("Wall"))
        {
            Destroy(this.gameObject);
        }
    }

    
}
