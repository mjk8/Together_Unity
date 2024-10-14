using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

/// <summary>
/// Shop UI의 슬롯을 관리하는 클래스
/// </summary>
public class ShopSlot : MonoBehaviour
{
    private int itemID;
    private int price;
    
    /// <summary>
    /// 슬롯 초기 설정
    /// </summary>
    /// <param name="itemId"></param>
    public void Init(int itemId)
    {
        transform.Find($"ItemName").GetComponent<TMP_Text>().fontSize = (Screen.height/ 1080) * 36;
        transform.Find($"ItemDescription").GetComponent<TMP_Text>().fontSize = (Screen.height/ 1080) * 24;
        itemID = itemId;
        price = Managers.Item.GetItemPrice(itemID);
        //아이템 아이콘 설정
        transform.Find($"Icon/Sprite").GetComponent<Image>().sprite = Managers.Resource.GetIcon(itemID.ToString());
        //아이템 가격 설정
        transform.Find($"Price/Amount").GetComponent<TMP_Text>().text = price.ToString();
        if (Util.CheckLocale(Define.SupportedLanguages.Korean))
        {
            //아이템 이름 설정
            transform.Find($"ItemName").GetComponent<TMP_Text>().text = Managers.Item.GetItemKoreanName(itemID);
            //아이템 설명 설정
            transform.Find($"ItemDescription").GetComponent<TMP_Text>().text = Managers.Item.GetItemKoreanDescription(itemID);
        }
        else
        {
            //아이템 이름 설정
            transform.Find($"ItemName").GetComponent<TMP_Text>().text = Managers.Item.GetItemEnglishName(itemID);
            //아이템 설명 설정
            transform.Find($"ItemDescription").GetComponent<TMP_Text>().text = Managers.Item.GetItemEnglishDescription(itemID);
        }
    }

    /// <summary>
    /// 해당 아이템을 구매하는 버튼 클릭 시 호출
    /// </summary>
    public void BuyButtonClicked()
    {
        //Managers.Inventory.TryBuyItem(itemID);
        //테스트 용 바로 아이템 구매로 넘어가기
        Debug.Log("Buy " +itemID);
        Managers.Inventory.TryBuyItem(itemID);
    }
}
