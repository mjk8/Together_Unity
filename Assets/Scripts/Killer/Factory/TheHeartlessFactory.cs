using UnityEngine;

public class TheHeartlessFactory : KillerFactory
{
    protected override GameObject CreateProduct(bool isMyPlayer)
    {
        if (isMyPlayer)
        {
            GameObject theHeartlessObj = GameObject.Instantiate(Managers.Killer._myKillerPrefabs[0]);
            theHeartlessObj.AddComponent<TheHeartless>();
            return theHeartlessObj;
        }
        else
        {
            GameObject theHeartlessObj = GameObject.Instantiate(Managers.Killer._otherPlayerKillerPrefabs[0]);
            theHeartlessObj.AddComponent<TheHeartless>();
            return theHeartlessObj;
        }
    }
}