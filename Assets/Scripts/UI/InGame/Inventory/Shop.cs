using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 UI를 관리하는 클래스
/// </summary>
public class Shop : MonoBehaviour
{
    string _viewContentPath = "Scroll View/Viewport/Content"; //상점 슬롯이 생성될 부모 오브젝트 경로
    string _slotPath = "UI/Inventory/InventoryParts/ShopSlot"; //상점 슬롯 프리팹 경로
    //private Dictionary<int, ShopSlot> _shopSlots;
    
    /// <summary>
    /// 존재하는 아이템 마다 슬롯을 생성하고 관련 정보 입력
    /// </summary>
    public void Init()
    {
        foreach (var key in Managers.Item._itemFactories.Keys)
        {
            ShopSlot temp = Managers.Resource.Instantiate(_slotPath,transform.Find(_viewContentPath)).GetComponent<ShopSlot>();
            temp.Init(key);
        }
    }
}
