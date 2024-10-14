using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hotbar UI를 관리하는 클래스.
/// ***Hotbar는 5개의 슬롯만 있다는 가정하에 구현***
/// </summary>
public class Hotbar : MonoBehaviour
{
    public Transform _currentSlot; //현재 하이라이트 된 슬롯. Item이 아닌 슬롯을 가리킴!
    private Color _selectedColor = Color.white;
    private Color _unselectColor= Color.black;
    
    private void Start()
    {
        _currentSlot = GetSlot(0);
        _currentSlot.Find("Paint").GetComponent<Image>().color = _selectedColor;
    }

    /// <summary>
    /// 슬롯에 아이템 추가
    /// </summary>
    /// <param name="slot"> 슬롯 index</param>
    /// <param name="itemID"></param>
    public void AddToSlot(int slot, int itemID)
    {
        if (BadIndexCheck(slot))
        {
            Debug.Log("Hotbar Slot Index Out of Range");
            return;
        }
        GetSlot(slot).GetComponentInChildren<InventorySlot>().Init(slot);
    }

    /// <summary>
    /// 선택된 슬롯 변경
    /// </summary>
    /// <param name="index">슬롯 index</param>
    public void ChangeSelected(int index)
    {
        //인덱스 체크
        if (BadIndexCheck(index))
        {
            Debug.Log("Hotbar Slot Index Out of Range");
            return;
        }

        //이전 슬롯과 현재 슬롯 색상 변경
        _currentSlot.Find("Paint").GetComponent<Image>().color = _unselectColor;
        _currentSlot = GetSlot(index);
        _currentSlot.Find("Paint").GetComponent<Image>().color = _selectedColor;
        Debug.Log("Current Slot : " + index);


        //아이템 들고 있는 처리
        HoldHotbarItem();
    }
    
    public void HoldHotbarItem()
    {
        int itemId = CurrentSelectedItemID();
        int myDediPlayerId = Managers.Player._myDediPlayerId;
        CDS_OnHoldItem onHoldItem = new CDS_OnHoldItem();
        onHoldItem.ItemId = itemId;
        onHoldItem.MyDediplayerId = myDediPlayerId;
        Managers.Network._dedicatedServerSession.Send(onHoldItem);
        Managers.Item.HoldItem(itemId, myDediPlayerId);
    }

    /// <summary>
    /// 현재 선택된 슬롯의 아이템 ID 반환
    /// </summary>
    /// <returns>현재 선택된 슬롯의 아이템 ID</returns>
    public int CurrentSelectedItemID()
    {
        return _currentSlot.GetComponentInChildren<InventorySlot>()._itemID;
    }
    
    
    /// <summary>
    /// 잘못된 인덱스 체크 (Out of Range 방지)
    /// </summary>
    /// <param name="index"> 슬롯 index</param>
    /// <returns>문제가 있을 시 true. 괜찮으면 false</returns>
    private bool BadIndexCheck(int index)
    {
        return index >= transform.childCount || index < 0;
    }
    
    /// <summary>
    /// 슬롯 반환
    /// </summary>
    private Transform GetSlot(int index)
    {
        return transform.GetChild(index);
    }
}
