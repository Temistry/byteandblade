using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_MakeNickName : MonoBehaviour
{
    // 닉네임 입력 필드
    public TMP_InputField NickNameInputField;

    // 확인버튼
    public Button ConfirmButton;

    UI_MessageBox msgbox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ConfirmButton.onClick.AddListener(OnConfirm);

        GameManager.GetInstance().OnRegiserNickName -= OnRegiserNickName;
        GameManager.GetInstance().OnRegiserNickName += OnRegiserNickName;
    }

    // Update is called once per frame
    void Update()
    {
        // 메시지박스가 활성화 되어 있으면 입력 방지
        if(null != msgbox)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Return))
        {
            OnConfirm();
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            NickNameInputField.text = "";
        }
    }

    private void OnRegiserNickName(string nickName)
    {
        NickNameInputField.text = nickName;
    }

    private void OnConfirm()
    {
        // 닉네임 입력 필드 텍스트 가져오기
        string nickName = NickNameInputField.text;


        // 정말로 닉네임을 설정하시겠습니까?
        if(nickName == "")
        {
            return;
        }

        Action onConfirm = () =>
        {
            GameManager.GetInstance().ResetPlayerData();
            GameManager.GetInstance().UpdateUserData();

            GameManager.GetInstance().NickName = nickName;

            gameObject.SetActive(false);
            Destroy(msgbox.gameObject);
        };

        Action onCancel = () =>
        {
            NickNameInputField.text = "";
            Destroy(msgbox.gameObject);
        };

        // 메시지박스
        msgbox = UI_MessageBox.CreateMessageBox($"{nickName} 으로 확정 하시겠습니까? 되돌릴 수 없습니다.", onConfirm, onCancel);
        

    }
}
