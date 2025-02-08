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
    int giftGold;

    bool isRead;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btn_Mail.onClick.AddListener(() =>
        {
            OpenMail();
        });
    }

    public void SetMail(string title, bool isRead, string content, int giftGold)
    {
        txt_MailTitle.text = title;

        this.isRead = isRead;
        SetMailState(isRead);

        ui_MessageBox = UI_MessageBox.CreateMessageBox(title, OnOKButtonClick, null);
        ui_MessageBox.SetMessage(content + "\n\n" + giftGold.ToString() + " Gold 지급");
        ui_MessageBox.gameObject.SetActive(false);
        this.giftGold = giftGold;
    }

    void SetMailState(bool isRead)
    {
        txt_MailState.text = isRead ? "읽음" : "읽지않음";
        txt_MailState.color = isRead ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
        this.isRead = isRead;
    }

    void OnOKButtonClick()
    {
        if (isRead)
        {
            return;
        }

        SaveSystem.GetInstance().SetSaveMailData(new MailData(txt_MailTitle.text, "", true, giftGold));
        GameManager.GetInstance().AddGold(giftGold);

        // 읽음 상태로 변경
        SetMailState(true);
    }

    public void OpenMail()
    {
        // 메일 오픈 로직 추가

        // 메시지박스를 연다.
        ui_MessageBox.gameObject.SetActive(true);
    }

}
