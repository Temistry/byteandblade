using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class Parser_MailList : Singleton<Parser_MailList>
{
    [SerializeField] private GameObject _mailPrefab;
    [SerializeField] private Transform _mailParent;
    [SerializeField] MailData[] _mailDataList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 메일 데이터 리스트 초기화
        var saveMailDataList = SaveSystem.GetInstance().LoadGameData();
        
        // 메일 프리팹 생성
        int i = 0;
        foreach (var mail in _mailDataList)
        {
            if(0 < saveMailDataList.mailDataList.Count && saveMailDataList.mailDataList[i].title == mail.title)
            {
                _mailDataList[i].isRead = saveMailDataList.mailDataList[i].isRead;  
            }
            
            i++;
            var mailObj = Instantiate(_mailPrefab, _mailParent);
            mailObj.GetComponent<UI_Mail>().SetMail(mail.title, mail.isRead, mail.content);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
