using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 아이템 드래그 앤 드롭을 관리하는 클래스. !슬롯!의 !아이템!에만 붙어있어야 한다.
/// </summary>
public class ItemDragDrop : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    private Transform _originaSlot;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    public void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>(); //드래그 중에는 레이캐스트를 막아야함
    }

    /// <summary>
    /// 드래그 시작 시 호출되는 함수
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        _originaSlot = transform.parent; //드래그 시작 시 부모 저장
        transform.SetParent(Managers.UI.SceneUI.transform.Find($"DragPanel"));//드래그 중 옮길 가라 패널
        _canvasGroup.blocksRaycasts = false; //드래그 중 레이캐스트를 막음
        _canvasGroup.alpha = 0.6f; //드래그 중 투명도 조절
    }

    /// <summary>
    /// 드래그 중 호출되는 함수
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.position = Input.mousePosition; //드래그 중 마우스 위치로 이동
    }

    /// <summary>
    /// 드래그 종료 시 호출되는 함수
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        
        GameObject droppedLocation = eventData.pointerCurrentRaycast.gameObject;
        //입력되는 게임 오브젝트가 Item이 아닌 child인 Icon이기에 부모를 찾아야함
        if (droppedLocation != null)
        {
            droppedLocation = droppedLocation.transform.parent.gameObject;
        }
        //드롭 위치가 InventorySlot일 경우 슬롯끼리 교체
        if (droppedLocation != null && droppedLocation.GetComponent<InventorySlot>() != null)
        {
            InventorySlot current = transform.GetComponent<InventorySlot>();
            InventorySlot target = droppedLocation.GetComponent<InventorySlot>();
            //기존 슬롯의 아이템을 신규 슬롯에 보내기
            transform.SetParent(droppedLocation.transform.parent);
            transform.localPosition = Vector3.zero;

            //신규 슬롯의 아이템을 기존 슬롯에 보내기
            droppedLocation.transform.SetParent(_originaSlot);
            droppedLocation.transform.localPosition = Vector3.zero;

            //신규 위치와 기존 위치의 크기 맞추기
            RectTransform droppedLocationRect = droppedLocation.GetComponent<RectTransform>();
            RectTransform transformRect = transform.GetComponent<RectTransform>();
            droppedLocationRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transformRect.rect.width);
            droppedLocationRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transformRect.rect.height);
            
            //HoldHotbarItem 호출
            Managers.Inventory._hotbar.HoldHotbarItem();
            
            Debug.Log("Swap Complete");
        }
        //드롭 위치가 InventorySlot이 아닐 경우 원래 위치로 복귀
        else
        {
            transform.SetParent(_originaSlot);
            transform.localPosition = Vector3.zero;
        }
        _canvasGroup.blocksRaycasts = true; //드래그 종료 시 레이캐스트 허용
        _canvasGroup.alpha = 1f; //드래그 종료 시 투명도 원상복귀
    }
}