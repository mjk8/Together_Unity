using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//<summary>
//인벤토리 관련 입력 처리
//</summary>
public class InventoryInput : MonoBehaviour
{
    /// <summary>
    /// 인벤토리 여닫는 버튼
    /// </summary>
    /// <param name="value"></param>
    void OnInventory(InputValue value)
    {
        InGameUI inGameUI = Managers.UI.GetComponentInSceneUI<InGameUI>();
        if (inGameUI._isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            //다른 하던 일 멈추기
            transform.GetComponent<ObjectInput>().QuitCleansing();
            Managers.Input.EnableCursor();
            inGameUI.OpenInventory();
        }
    }
    
    void OnSkill(InputValue value)
    {
        Debug.Log("Skill Try");
        if (Managers.Player.IsMyDediPlayerKiller())
        {
            //Managers.Game._myKillerSkill.TryUseSkill();
            Managers.Killer.UseSkill(Managers.Player._myDediPlayerId, Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._killerType);
        }
    }
    
    void OnUseItem(InputValue value)
    {
        int itemID = Managers.Inventory._hotbar.CurrentSelectedItemID();
        Debug.Log("Current selected item ID = " + itemID);
        if(itemID != -1)
        {
            Debug.Log("Try use item");
            Managers.Item.UseItem(Managers.Player._myDediPlayerId , itemID);
        }
    }
    
    void OnHotbar0(InputValue value)
    {
        Managers.Inventory._hotbar.ChangeSelected(0);
    }
    
    void OnHotbar1(InputValue value)
    {
        Managers.Inventory._hotbar.ChangeSelected(1);
    }
    
    void OnHotbar2(InputValue value)
    {
        Managers.Inventory._hotbar.ChangeSelected(2);
    }
    
    void OnHotbar3(InputValue value)
    {
        Managers.Inventory._hotbar.ChangeSelected(3);
    }
    
    void OnHotbar4(InputValue value)
    {
        Managers.Inventory._hotbar.ChangeSelected(4);
    }

    public void OnCancelUI(InputValue value)
    {
        CloseInventory();
    }

    private void CloseInventory()
    {
        Managers.Input.DisableCursor();
        Managers.UI.GetComponentInSceneUI<InGameUI>().CloseInventory();
    }
}
