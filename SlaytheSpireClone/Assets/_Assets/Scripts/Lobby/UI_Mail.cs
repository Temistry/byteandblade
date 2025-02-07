using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Mail : MonoBehaviour
{
    [Header("읽음/읽지않음 상태 변화")]
    [SerializeField] TextMeshProUGUI txt_MailTitle;
    [SerializeField] TextMeshProUGUI txt_MailState;
    [SerializeField] Button btn_Mail;

    UI_MessageBox ui_MessageBox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 버튼을 클릭하면 텍스트의 색이 어둡게 변경, 읽음 상태로 변경
        btn_Mail.onClick.AddListener(() => {
            SetMailState(true);
            OpenMail();
        });
    }

    public void SetMail(string title, bool isRead, string content)
    {
        txt_MailTitle.text = title;
        SetMailState(isRead);

        ui_MessageBox = UI_MessageBox.CreateMessageBox(title, OnOKButtonClick, null);
        ui_MessageBox.SetMessage(content);
        ui_MessageBox.gameObject.SetActive(false);
    }

    void SetMailState(bool isRead)
    {
        txt_MailState.text = isRead ? "읽음" : "읽지않음";
        txt_MailState.color = isRead ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
    }

    void OnOKButtonClick()
    {
        SaveSystem.GetInstance().SetSaveMailData(new MailData(txt_MailTitle.text, "", true));
    }

    public void OpenMail()
    {
        // 메일 오픈 로직 추가

        // 메시지박스를 연다.
        ui_MessageBox.gameObject.SetActive(true);
    }

}
