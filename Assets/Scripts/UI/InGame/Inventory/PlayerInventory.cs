using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 UI를 관리하는 클래스
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    int _initialLines = 3; //초기 라인 개수
    string _viewContentPath = "Scroll View/Viewport/Content"; //인벤토리 슬롯이 생성될 부모 오브젝트 경로
    string _linePath = "UI/Inventory/InventoryParts/InventoryLine"; //인벤토리 라인 프리팹 경로
    
    /// <summary>
    /// 인벤토리 초기 라인 생성 및 슬롯 사이 간격 설정
    /// </summary>
    void Start()
    {
        transform.Find(_viewContentPath).GetComponent<VerticalLayoutGroup>().spacing = Screen.width / 300;
        for(int i =0;i<_initialLines;i++)
        {
            MakeNewLine();
        }
    }

    /// <summary>
    /// 새로운 인벤토리 라인 생성
    /// </summary>
    void MakeNewLine()
    {
        GameObject cur = Managers.Resource.Instantiate(_linePath, transform.Find(_viewContentPath));
        cur.GetComponent<HorizontalLayoutGroup>().spacing = Screen.width / 300;
    }

    
    /// <summary>
    /// 아이템 추가
    /// </summary>
    /// <param name="itemId">추가할 itemID</param>
    public void AddNewItem(int itemId)
    {
        InventorySlot slot;
        //비어있는 슬롯이 있을 경우
        Transform _lineTransform = transform.Find(_viewContentPath);
        foreach(Transform child in _lineTransform)
        {
            for(int i=0;i<child.childCount;i++)
            {
                slot = child.GetChild(i).GetComponentInChildren<InventorySlot>();
                if (slot._itemID == -1)
                {
                    slot.Init(itemId);
                    Managers.Inventory._address.Add(itemId,slot);
                    return; //비어있는 슬롯 존재 시, 아이템 추가 후 함수 종료
                }
            }
        }
        //모든 슬롯이 차있을 경우, 새로운 라인 생성 후 아이템 추가
        MakeNewLine();
        slot = _lineTransform.GetChild(_lineTransform.childCount-1).GetChild(0).GetComponentInChildren<InventorySlot>();
        slot.Init(itemId);
        Managers.Inventory._address.Add(itemId,slot);
    }
}
