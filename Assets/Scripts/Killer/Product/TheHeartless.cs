using System.Collections;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheHeartless : MonoBehaviour, IKiller
{
    //킬러 공통 보유 속성
    public int Id { get; set; }
    public string EnglishName { get; set; }
    public string KoreanName { get; set; }
    public string EnglishDescription { get; set; }
    public string KoreanDescription { get; set; }
    public string EnglishAbilityName { get; set; }
    public string KoreanAbilityName { get; set; }
    public string EnglishAbilityDescription { get; set; }
    public string KoreanAbilityDescription { get; set; }
    public float SkillCoolTimeSeconds { get; set; } //스킬 쿨타임 초
    public bool CanUseSkill { get; set; } //스킬 사용 가능 여부
    
    //이 킬러만의 속성
    public float HeartlessSeconds { get; set; } //심장소리 안들리게 할 시간
    public float _currentCoolTime; //현재 스킬쿨 값

    
    public void Setting()
    {
        //킬러 매니저로부터 킬러 데이터를 받아와서 설정
        TheHeartless theHeartlessData = Managers.Killer._killers[0] as TheHeartless;
        
        Id = theHeartlessData.Id;
        EnglishName = theHeartlessData.EnglishName;
        KoreanName = theHeartlessData.KoreanName;
        EnglishDescription = theHeartlessData.EnglishDescription;
        KoreanDescription = theHeartlessData.KoreanDescription;
        EnglishAbilityName = theHeartlessData.EnglishAbilityName;
        KoreanAbilityName = theHeartlessData.KoreanAbilityName;
        EnglishAbilityDescription = theHeartlessData.EnglishAbilityDescription;
        KoreanAbilityDescription = theHeartlessData.KoreanAbilityDescription;
        SkillCoolTimeSeconds = theHeartlessData.SkillCoolTimeSeconds;
        
        HeartlessSeconds = theHeartlessData.HeartlessSeconds;
        
        //유니티 자체 설정
        CanUseSkill = true;
    }

    public void Use(int killerPlayerId)
    {
        if (Managers.Player._myDediPlayerId == killerPlayerId && CanUseSkill)
        {
            //스킬 사용 처리
            CanUseSkill = false;
            Managers.Sound.Play(EnglishName, Define.Sound.Bgm);
            //패킷 보내기
            CDS_UseHeartlessSkill usePacket = new CDS_UseHeartlessSkill();
            usePacket.MyDediplayerId = Managers.Player._myDediPlayerId;
            usePacket.KillerId = Id;
            Managers.Network._dedicatedServerSession.Send(usePacket);
            
            Managers.Game._myKillerSkill.UsedSkill(SkillCoolTimeSeconds,HeartlessSeconds);
            StartCoroutine(MyHeartlessSkill());
        }
        else if (Managers.Player._myDediPlayerId != killerPlayerId)
        {
            StartCoroutine(HeartlessSkill());
        }
    }
    
    public void BaseAttack()
    {
        transform.GetComponent<PlayerAnimController>().KillerBaseAttack();
    }
    
    IEnumerator HeartlessSkill()
    {
        Debug.Log("Heartless skill used");
        Managers.Game._playKillerSound._heartlessSkillUsed = true;
        yield return new WaitForSeconds(HeartlessSeconds);
        Managers.Game._playKillerSound._heartlessSkillUsed = false;
    }

    IEnumerator MyHeartlessSkill()
    {
        Debug.Log("Heartless skill used");
        yield return new WaitForSeconds(HeartlessSeconds);
        Managers.Sound.Play("tense-horror-background", Define.Sound.Bgm);
        yield return new WaitForSeconds(SkillCoolTimeSeconds);
        CanUseSkill = true;
        Managers.Sound.Play("SkillReady");
    }
    
    public void UnAssign()
    {
        CanUseSkill = false;
        Managers.Sound.Play("tense-horror-background", Define.Sound.Bgm); 
    }
}