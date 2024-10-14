using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 UI의 슬롯을 관리하는 클래스
/// </summary>
public class InventorySlot : MonoBehaviour
{
    public int _itemID;
    Image _icon;
    TMP_Text _amount;
    private Sprite _baseSprite; //기본 투명 이미지의 아이콘

    private void Start()
    {
        _itemID = -1;
        _icon = transform.Find($"Icon").GetComponent<Image>();
        _baseSprite = _icon.sprite;
        _amount = transform.Find($"Amount").GetComponent<TMP_Text>();
    }

    /// <summary>
    /// 슬롯에 정보 입력
    /// </summary>
    /// <param name="itemId"></param>
    public void Init(int itemId)
    {
        _itemID = itemId;
        //아이템 아이콘 설정
        _icon.sprite = Managers.Resource.GetIcon(_itemID.ToString());
        //아이템 개수 설정
        _amount.text = Managers.Inventory._ownedItems[_itemID].ToString();
    }
    
    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void ClearSlot()
    {
        _icon.sprite = _baseSprite;
        _amount.text = string.Empty;
        _itemID = -1;
    }

    /// <summary>
    /// 아이템 개수 갱신
    /// </summary>
    public void UpdateAmount()
    {
        _amount.text = Managers.Inventory._ownedItems[_itemID].ToString();
    }
}
