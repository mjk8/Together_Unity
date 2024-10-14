using UnityEngine;

public sealed class FireworkFactory : ItemFactory
{
    //이 아이템만의 속성
    public float FlightHeight { get; set; }

    public FireworkFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float flightHeight)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        FlightHeight = flightHeight;
    }
    public override GameObject CreateItem(int playerId)
    {
        //숫자 1~4중 랜덤으로 선택
        int random = Random.Range(1, 5);
        GameObject fireworkGameObject = Managers.Resource.Instantiate($"Items/Firework/Rocket{random}");
        fireworkGameObject.name = "Firework";
        Firework firework = fireworkGameObject.AddComponent<Firework>();
        firework.Init(FactoryId, playerId, FactoryEnglishName, FlightHeight);

        //위치는 현재 플레이어 위치에서 살짝 앞에 생성
        fireworkGameObject.transform.position = Managers.Player.GetPlayerObject(playerId).transform.position +
                                                Managers.Player.GetPlayerObject(playerId).transform.forward * 2;
        return fireworkGameObject;
    }

    public override GameObject CreateOnHoldItem(int playerId)
    {
        return null;
    }
}