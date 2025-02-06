using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_MessageBox : MonoBehaviour
{
    [Header("Message")]
    [SerializeField] private TextMeshProUGUI _messageText;

    [Header("Buttons")]
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _cancelButton;

    public Action onClickOkButton;
    public Action onClickCancelButton;

    // 메시지박스 생성 함수
    public static UI_MessageBox CreateMessageBox(string message, Action onClickOkButton = null, Action onClickCancelButton = null)
    {
        UI_MessageBox messageBox = Instantiate(Resources.Load<UI_MessageBox>("UI/UI_MessageBox"));
        messageBox.Create(message, onClickOkButton, onClickCancelButton);
        return messageBox;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 버튼 클릭 이벤트 등록
        _okButton.onClick.AddListener(OnClickOkButton);
        _cancelButton.onClick.AddListener(OnClickCancelButton);


        // 메시지박스 생성
        //CreateMessageBox("test", () => Debug.Log("OK test"), () => Debug.Log("Cancel test"));
    }

    public void Create(string message, Action onClickOkButton = null, Action onClickCancelButton = null)
    {
        SetMessage(message);

        // 확인 버튼 활성 여부 설정
        if(onClickOkButton == null)
            _okButton.gameObject.SetActive(false);
        else
            SetOnClickOkButton(onClickOkButton);

        // 취소 버튼 활성 여부 설정 
        if(onClickCancelButton == null)
            _cancelButton.gameObject.SetActive(false);
        else
            SetOnClickCancelButton(onClickCancelButton);
    }

    public void SetMessage(string message)
    {
        _messageText.text = message;
    }

    // ok 버튼 클릭 시 발동시킬 이벤트 등록 함수
    public void SetOnClickOkButton(Action onclickOkButton)
    {
        _okButton.onClick.AddListener(() => onclickOkButton?.Invoke());
    }

    // cancel 버튼 클릭 시 발동시킬 이벤트 등록 함수
    public void SetOnClickCancelButton(Action onclickCancelButton)
    {
        _cancelButton.onClick.AddListener(() => onclickCancelButton?.Invoke());
    }

    // ok, cancel 버튼 클릭 시 반드시 호출되는 함수
    void OnClickOkButton()
    {
        onClickOkButton?.Invoke();
        gameObject.SetActive(false);
    }

    void OnClickCancelButton()
    {
        onClickCancelButton?.Invoke();
        gameObject.SetActive(false);
    }
}
