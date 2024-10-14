using System.Collections;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Firework : MonoBehaviour,IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }


    //이 아이템만의 속성
    public float FlightHeight { get; set; }

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float flightHeight)
    {
        Init(itemId,playerId, englishName);
        FlightHeight = flightHeight;
    }

    public bool Use(IMessage recvPacket = null)
    {
        Managers.Player.GetAnimator(PlayerID).SetTriggerByString(EnglishName);
        Debug.Log("Item Firework Use");

        //폭죽 추진력(Constant Force의 Y값을 FlightHeight*1.5으로 설정)
        ConstantForce constantForce = gameObject.GetComponent<ConstantForce>();
        if(constantForce == null)
            Debug.Log("constantforce가 null입니다");
        constantForce.force = new Vector3(0, FlightHeight  * 1.5f, 0);

        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            //폭죽 아이템 사용 패킷 서버로 보내기
            CDS_UseFireworkItem cdsUseFireworkItem = new CDS_UseFireworkItem()
            {
                MyDediplayerId = PlayerID,
                ItemId = ItemID,
                FireworkStartingTransform = new TransformInfo()
                {
                    Position = new PositionInfo()
                    {
                        PosX = transform.position.x,
                        PosY = transform.position.y,
                        PosZ = transform.position.z
                    },
                    Rotation = new RotationInfo()
                    {
                        RotX = transform.rotation.x,
                        RotY = transform.rotation.y,
                        RotZ = transform.rotation.z,
                        RotW = transform.rotation.w
                    }
                }
            };
            Managers.Network._dedicatedServerSession.Send(cdsUseFireworkItem);
        }
        else
        {
            if (recvPacket != null)
            {
                DSC_UseFireworkItem dscUseFireworkItem = recvPacket as DSC_UseFireworkItem;
                //다른 플레이어가 사용한 폭죽 시작 위치로 폭죽을 이동시킴
                transform.position = new Vector3(dscUseFireworkItem.FireworkStartingTransform.Position.PosX,
                    dscUseFireworkItem.FireworkStartingTransform.Position.PosY,
                    dscUseFireworkItem.FireworkStartingTransform.Position.PosZ);
                transform.rotation = new Quaternion(dscUseFireworkItem.FireworkStartingTransform.Rotation.RotX,
                    dscUseFireworkItem.FireworkStartingTransform.Rotation.RotY,
                    dscUseFireworkItem.FireworkStartingTransform.Rotation.RotZ,
                    dscUseFireworkItem.FireworkStartingTransform.Rotation.RotW);
            }
        }

        //5초후 폭죽 오브젝트 삭제
        StartCoroutine(DestroyAfterSeconds(5f));
        return true;
    }

    IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    public void OnHold()
    {

    }


    public void OnHit()
    {
        
    }
}