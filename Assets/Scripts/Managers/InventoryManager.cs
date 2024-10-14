using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    InGameUI _inGameUI;
    public Hotbar _hotbar;
    public PlayerInventory _inventory;
    public Shop _shop;
    
    private int _totalPoint = 0; //상자로 얻은 총 포인트(낮마다 초기화)
    
    public Dictionary<int,int> _ownedItems = new Dictionary<int, int>(); //key: 아이템Id, value: 아이템 개수
    public Dictionary<int,InventorySlot> _address = new Dictionary<int, InventorySlot>();

    private List<int> _exceptionHoldHotbarItemId = new List<int>{ 0, 3, 4 }; //holdHotbarItem 예외 아이템Id 리스트 (아이템 자체적으로 holdHotbarItem을 호출하는 아이템들의 id)

    public void Init()
    {
        _totalPoint = 0;
        _inGameUI = Managers.UI.GetComponentInSceneUI<InGameUI>();
        _hotbar = _inGameUI._hotbar.GetComponent<Hotbar>();
        _inventory = _inGameUI._inventory.GetComponent<PlayerInventory>();
        _shop = _inGameUI._shop.GetComponent<Shop>();
        _shop.Init();
    }
    
    /// <summary>
    /// 포인트 및 인벤토리 모든 아이템 초기화
    /// </summary>
    public void Clear()
    {
        _totalPoint = 0;
        foreach (var key in _address.Keys)
        {
            _address[key].ClearSlot();
        }
        _ownedItems.Clear();
        _address.Clear();
    }
    
    
    #region 포인트 관련 함수
    
    /// <summary>
    /// 포인트 설정
    /// </summary>
    /// <param name="item">설정할 포인트 int</param>
    public void SetTotalPoint(int point)
    {
        _totalPoint = point;
    }
    #endregion

    #region 아이템 서버 요청 함수

    /// <summary>
    /// 아이템 구매 시도
    /// </summary>
    /// <param name = "itemID">구매하려는 아이템id</param>
    public void TryBuyItem(int itemID)
    {
        //내 플레이어가 죽었으면 불가능
        if (Managers.Player.IsPlayerDead(Managers.Player._myDediPlayerId))
        {
            return;
        }

        if ((_totalPoint < Managers.Item.GetItemPrice(itemID)||!(Managers.Game._isDay)))
        {
            Managers.Sound.Play("Error", Define.Sound.Effects,null,1.3f);
        }
        else
        {
            Debug.Log("TryBuyItem Packet Send");
            CDS_ItemBuyRequest buyItemRequest = new CDS_ItemBuyRequest()
            {
                MyDediplayerId = Managers.Player._myDediPlayerId,
                ItemId = itemID
            };
            Managers.Network._dedicatedServerSession.Send(buyItemRequest);
        }
    }
    

    #endregion

    #region 서버 답변 처리 함수

    /// <summary>
    /// 아이템을 인벤토리에서 1개 추가함
    /// </summary>
    /// <param name="itemID">구매 가능한 아이템id</param>
    public void BuyItemSuccess(int itemID, int itemTotalCount, bool isBuySuccess, int remainPoint)
    {
        if(isBuySuccess)
        {
            Managers.Sound.Play("PurchaseSuccess");
            _totalPoint = remainPoint;
            _inGameUI.SetCurrentCoin(_totalPoint);
            _inGameUI.AddGetCoin(Managers.Item.GetItemPrice(itemID),false);
            if(_ownedItems.ContainsKey(itemID))
            {
                _ownedItems[itemID] = itemTotalCount;
                _address[itemID].UpdateAmount();
            }
            else
            {
                _ownedItems.Add(itemID, itemTotalCount);
                _inventory.AddNewItem(itemID);
            }
        }
        else
        {
            Managers.Sound.Play("Error", Define.Sound.Effects,null,1.3f);
        }
    }
    #endregion

    
    /// <summary>
    /// 아이템을 인벤토리에서 1개 제거함
    /// </summary>
    /// <param name="itemID"></param>
    public void RemoveItemOnce(int itemID)
    {
        if(_ownedItems.ContainsKey(itemID))
        {
            _ownedItems[itemID]--;
            if(_ownedItems[itemID] == 0)
            {
                _address[itemID].ClearSlot(); //인벤토리 슬롯 비우기
                _address.Remove(itemID); // 슬롯 주소 삭제
                _ownedItems.Remove(itemID); //아이템 삭제

                //_exceptionHoldHotbarItemId에 포함되어 있지 않은 아이템이라면 holdHotbarItem 호출
                if (!_exceptionHoldHotbarItemId.Contains(itemID))
                {
                    _hotbar.HoldHotbarItem(); //손에 들고있는 프리팹 바꾸고 정보 뿌리기.
                }
            }
            else
            {
                _address[itemID].UpdateAmount();
            }
        }
    }
}
