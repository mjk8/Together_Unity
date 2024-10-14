using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFactory : ItemFactory
{
    public float TrapDuration { get; set; }
    public float TrapRadius { get; set; }
    public float StunDuration { get; set; }

    public int _trapId = 0;
    
    public TrapFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float trapDuration, float trapRadius, float stunDuration)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        TrapDuration = trapDuration;
        TrapRadius = trapRadius;
        StunDuration = stunDuration;
    }
    
    public override GameObject CreateItem(int playerId)
    {
        //'내'가 사용할 경우 트랩은 설치가능여부를 파악하기 위해 반드시 온홀드 아이템이 먼저 생성되어 있을때만 사용가능하기 때문에
        //OnHold게임오브젝트가 없다면 null리턴,
        //있다면 해당 OnHold게임오브젝트를 리턴해줌
        if (playerId == Managers.Player._myDediPlayerId)
        {
            GameObject trapOnHoldGameObject = null;
            // "@OnHoldItem" 오브젝트 찾기
            Transform onHoldItemTransform = Managers.Item._root.transform.Find("@OnHoldItem");
            if (onHoldItemTransform != null)
            {
                // "OnHoldTrap" 오브젝트 찾기
                Transform onHoldTrapTransform = onHoldItemTransform.Find("OnHoldTrap");

                if (onHoldTrapTransform != null)
                {
                    // 모든 과정이 성공적이면 gameObject 할당
                    trapOnHoldGameObject = onHoldTrapTransform.gameObject;
                }
            }
            if (trapOnHoldGameObject == null)
            {
                return null;
            }
            else
            {
                trapOnHoldGameObject.name = "Trap";
                return trapOnHoldGameObject;
            }
        }
        else
        {
            GameObject trapGameObject = Managers.Resource.Instantiate("Items/Trap/Trap");
            trapGameObject.name = "Trap";
            Trap trap = trapGameObject.AddComponent<Trap>();
            trap.Init(FactoryId, playerId, FactoryEnglishName, TrapDuration, TrapRadius, StunDuration);
            return trapGameObject;
        }
    }

    public override GameObject CreateOnHoldItem(int playerId)
    {
        //내가 아닌경우 onHoldItem을 생성하지 않음
        if (playerId != Managers.Player._myDediPlayerId)
        {
            return null;
        }
        else
        {
            Debug.Log("trapfactory의 if문 건너뜀");
            GameObject trapGameObject = Managers.Resource.Instantiate("Items/Trap/Trap");
            trapGameObject.name = "OnHoldTrap";
            Trap trap = trapGameObject.AddComponent<Trap>();
            trap.Init(FactoryId, playerId, FactoryEnglishName, TrapDuration, TrapRadius, StunDuration);

            //trapGameObject의 Mesh Renderer와 Sphere Collider를 비활성화
            trapGameObject.GetComponent<MeshRenderer>().enabled = false;
            trapGameObject.GetComponent<SphereCollider>().enabled = false;

            //trapGameObject의 자식인 TrapGreen의 Mesh Renderer를 비활성화
            trapGameObject.transform.Find("TrapGreen").GetComponent<MeshRenderer>().enabled = false;

            //trapGameObject의 자식인 TrapRed의 Mesh Renderer를 활성화
            trapGameObject.transform.Find("TrapRed").GetComponent<MeshRenderer>().enabled = true;

            return trapGameObject;
        }
    }
}
