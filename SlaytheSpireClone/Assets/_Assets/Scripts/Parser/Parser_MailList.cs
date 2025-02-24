using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class Parser_MailList : Singleton<Parser_MailList>
{
    [SerializeField] private GameObject _mailPrefab;
    [SerializeField] private Transform _mailParent;
    [SerializeField] MailData[] _mailDataList;

    public List<GameObject> _currentmailDataList = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitMailList();
    }

    public void InitMailList()
    {
        // GameManager로부터 저장된 메일 데이터 가져오기
        var savedMailList = GameManager.GetInstance().GetMailDataList();
        
        // 메일 프리팹 생성
        int i = 0;
        foreach (var mail in _mailDataList)
        {
            // 저장된 메일이 있다면 읽음 상태 동기화
            if(savedMailList.Count > i && savedMailList[i].title == mail.title)
            {
                _mailDataList[i].isRead = savedMailList[i].isRead;  
            }
            
            var mailObj = Instantiate(_mailPrefab, _mailParent);
            mailObj.GetComponent<UI_Mail>().SetMail(mail.title, mail.isRead, mail.content, mail.giftGold);
            _currentmailDataList.Add(mailObj);
            i++;
        }
    }

    public void ResetMailList()
    {
        foreach (var mail in _currentmailDataList)
        {
            Destroy(mail);
        }
        _currentmailDataList.Clear();
        
        InitMailList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
