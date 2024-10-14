using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleFactory : ItemFactory
{
    public float InvisibleSeconds { get; set; }
    
    public InvisibleFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float invisibleSeconds)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        InvisibleSeconds = invisibleSeconds;
    }
    
    public override GameObject CreateItem(int playerId)
    {
        if (playerId == Managers.Player._myDediPlayerId)
        {
            GameObject existedInvisibleGameObject = null;
            Transform existedInvisibleTransform = Managers.Item._root.transform.Find($"Invisible{playerId}");
            if (existedInvisibleTransform != null)
            {
                existedInvisibleGameObject = existedInvisibleTransform.gameObject;
                return existedInvisibleGameObject;
            }
            else
            {
                GameObject invisibleGameObject = new GameObject($"Invisible{playerId}");
                Invisible invisible = invisibleGameObject.AddComponent<Invisible>();
                invisible.Init(FactoryId, playerId, FactoryEnglishName, InvisibleSeconds);
                return invisibleGameObject;
            }
        }
        else
        {
            GameObject existedInvisibleGameObject = null;
            Transform existedInvisibleTransform = Managers.Item._root.transform.Find($"Invisible{playerId}");
            if (existedInvisibleTransform != null)
            {
                existedInvisibleGameObject = existedInvisibleTransform.gameObject;
                return existedInvisibleGameObject;
            }
            else
            {
                GameObject invisibleGameObject = new GameObject($"Invisible{playerId}");
                Invisible invisible = invisibleGameObject.AddComponent<Invisible>();
                invisible.Init(FactoryId, playerId, FactoryEnglishName, InvisibleSeconds);
                return invisibleGameObject;
            }
        }
    }

    public override GameObject CreateOnHoldItem(int playerId)
    {
        return null;
    }
}
