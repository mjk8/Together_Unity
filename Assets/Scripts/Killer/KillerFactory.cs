using UnityEngine;

public abstract class KillerFactory
{
    public GameObject CreateKiller(bool isMyPlayer)
    {
        GameObject killerObj = CreateProduct(isMyPlayer);
        
        //killer오브젝트에서 IKiller을 상속받은 컴포넌트를 찾는다
        IKiller killerComponent = killerObj.GetComponent<IKiller>();
        
        //초기 세팅
        killerComponent.Setting();
        
        return killerObj;
    }
    
    protected abstract GameObject CreateProduct(bool isMyPlayer); //상속한 팩토리에서 구현
}