using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightFactory : ItemFactory
{
    public float BlindDuration {get; set;}
    public float FlashlightDistance {get; set;}
    public float FlashlightAngle {get; set;}
    public float FlashlightAvailableTime {get; set;}
    public float FlashlightTimeRequired {get; set;}
    
    public FlashlightFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float blindDuration, float flashlightDistance, float flashlightAngle, float flashlightAvailableTime, float flashlightTimeRequired)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        BlindDuration = blindDuration;
        FlashlightDistance = flashlightDistance;
        FlashlightAngle = flashlightAngle;
        FlashlightAvailableTime = flashlightAvailableTime;
        FlashlightTimeRequired = flashlightTimeRequired;
    }
    
    public override GameObject CreateItem(int playerId)
    {
        if (playerId == Managers.Player._myDediPlayerId)
        {
            GameObject existedFlashLightGameObject = null;
            Transform existedFlashLightTransform = Managers.Item._root.transform.Find($"FlashLight{playerId}");
            if (existedFlashLightTransform != null)
            {
                existedFlashLightGameObject = existedFlashLightTransform.gameObject;
                return existedFlashLightGameObject;
            }
            else
            {
                GameObject flashlightGameObject = new GameObject($"FlashLight{playerId}");
                Flashlight flashlight = flashlightGameObject.AddComponent<Flashlight>();
                flashlight.Init(FactoryId, playerId, FactoryEnglishName, BlindDuration, FlashlightDistance, FlashlightAngle, FlashlightAvailableTime, FlashlightTimeRequired);
                return flashlightGameObject;
            }
        }
        else
        {
            GameObject existedFlashLightGameObject = null;
            Transform existedFlashLightTransform = Managers.Item._root.transform.Find($"FlashLight{playerId}");
            if (existedFlashLightTransform != null)
            {
                existedFlashLightGameObject = existedFlashLightTransform.gameObject;
                return existedFlashLightGameObject;
            }
            else
            {
                GameObject flashlightGameObject = new GameObject($"FlashLight{playerId}");
                Flashlight flashlight = flashlightGameObject.AddComponent<Flashlight>();
                flashlight.Init(FactoryId, playerId, FactoryEnglishName, BlindDuration, FlashlightDistance, FlashlightAngle, FlashlightAvailableTime, FlashlightTimeRequired);
                return flashlightGameObject;
            }
        }

        return null;
    }

    public override GameObject CreateOnHoldItem(int playerId)
    {
        return null;
    }
}
