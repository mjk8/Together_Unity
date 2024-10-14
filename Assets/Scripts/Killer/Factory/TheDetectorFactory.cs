using UnityEngine;

public class TheDetectorFactory : KillerFactory
{
    protected override GameObject CreateProduct(bool isMyPlayer)
    {
        if (isMyPlayer)
        {
            GameObject theDetectorObj = GameObject.Instantiate(Managers.Killer._myKillerPrefabs[1]);
            theDetectorObj.AddComponent<TheDetector>();
            return theDetectorObj;
        }
        else
        {
            GameObject theDetectorObj = GameObject.Instantiate(Managers.Killer._otherPlayerKillerPrefabs[1]);
            theDetectorObj.AddComponent<TheDetector>();
            return theDetectorObj;
        }
    }
}
