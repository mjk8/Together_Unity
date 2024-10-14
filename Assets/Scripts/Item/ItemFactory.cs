using UnityEngine;

public abstract class ItemFactory
{
    //공통 속성
    public int FactoryId { get; set; }
    public int FactoryPrice { get; set; }
    public string FactoryEnglishName { get; set; }
    public string FactoryKoreanName { get; set; }
    public string FactoryEnglishDescription { get; set; }
    public string FactoryKoreanDescription { get; set; }

    //필수 설정되어야 하는 것들 설정
    public virtual void FactoryInit(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription)
    {
        FactoryId = id;
        FactoryPrice = price;
        FactoryEnglishName = englishName;
        FactoryKoreanName = koreanName;
        FactoryEnglishDescription = englishDescription;
        FactoryKoreanDescription = koreanDescription;
    }

    /// <summary>
    /// 실제 사용할 아이템 생성
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public abstract GameObject CreateItem(int playerId);

    /// <summary>
    /// 아이템을 들고있을때 생성할 아이템
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns>들고있을때 효과가 없다면 null 리턴</returns>
    public abstract GameObject CreateOnHoldItem(int playerId);
}