using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

/// <summary>
/// 방 생성 팝업
/// </summary>
public class CreateRoomPopup : UI_popup
{
    private UI_InputField _roomName;
    private UI_Toggle _passwordToggle;
    private UI_InputField _password;
    void Start()
    {
        _roomName = transform.GetChild(0).GetChild(1).GetComponent<UI_InputField>();
        _passwordToggle = transform.GetChild(1).GetChild(1).GetComponent<UI_Toggle>();
        _password = transform.GetChild(2).GetChild(1).GetComponent<UI_InputField>();
        transform.GetChild(0).GetChild(0).GetComponent<UI_Text>().SetString("RoomName");
        transform.GetChild(1).GetChild(0).GetComponent<UI_Text>().SetString("UsePassword");
        transform.GetChild(2).GetChild(0).GetComponent<UI_Text>().SetString("Password");
        _passwordToggle.SetOnClick(UsePassword);
        transform.GetChild(3).GetComponent<UI_Button>().GetComponentInChildren<UI_Text>().SetString("CreateRoom");
        transform.GetChild(3).GetComponent<UI_Button>().SetOnClick(SubmitCreateRoom);
        transform.GetChild(4).GetComponent<UI_Button>().GetComponentInChildren<UI_Text>().SetString("Close");
        transform.GetChild(4).GetComponent<UI_Button>().SetOnClick(ClosePopup);
    }

    void UsePassword()
    {
        _password.SetInteractable(_passwordToggle.GetToggleState());
    }

    void SubmitCreateRoom()
    {
        if (_roomName.GetInputText().Length < 1)
        {
            Managers.UI.LoadPopupPanel<InputRoomNamePopup>();
        }
        
        else if (_password.GetInputText().Length < 1 && _passwordToggle.GetToggleState())
        {
            Managers.UI.LoadPopupPanel<InputPasswordPopup>();
        }
        else
        {
            UIPacketHandler.MakeNewRoomSendPacket(_roomName.GetInputText(),_passwordToggle.GetToggleState(),_password.GetInputText());
            ClosePopup();
        }
    }
}