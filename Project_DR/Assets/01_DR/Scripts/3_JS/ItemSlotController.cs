using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rito.InventorySystem;

public class ItemSlotController : MonoBehaviour
{
    /*************************************************
     *                 Private Fields
     *************************************************/
    #region [+]
    [SerializeField] private GameObject itemSlot;
    private BoxCollider boxCollider;

    #endregion
    /*************************************************
     *                 Unity Events
     *************************************************/
    #region [+]
    void Start()
    {
        itemSlot = GetParentGameObject(transform);
        boxCollider = gameObject.GetComponent<BoxCollider>();
        SetBoxColliderSize(GetSizeVector2(itemSlot), boxCollider);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Enter");
    }

    #endregion
    /*************************************************
     *                Private Methods
     *************************************************/
    #region [+]
    // 부모의 게임 오브젝트를 가져온다
    private GameObject GetParentGameObject(Transform child)
    {
        Transform parent = child.parent;

        return parent.gameObject;
    }

    // 오브젝트의 Rect Transform Vector2 사이즈를 가져온다
    private Vector2 GetSizeVector2(GameObject gameObject)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        return rect.sizeDelta;
    }

    // 박스 콜라이더의 사이즈(Vector2)를 변경한다.
    private void SetBoxColliderSize(Vector2 sizeDelta, BoxCollider boxCollider, float z = 0f)
    {
        Vector3 size = new Vector3(sizeDelta.x, sizeDelta.y, z);
        boxCollider.size = size;
    }

    #endregion
}