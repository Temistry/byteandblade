using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SelectCharPiece : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nameText;       // 캐릭터 이름
    [SerializeField] TextMeshProUGUI _countText;      // 조각 개수
    [SerializeField] Image _iconImage;                // 버튼 아이콘
    [SerializeField] Button _GetCharacterButton;      // 캐릭터 획득 버튼
    [SerializeField] GameObject _ButtonCharacterIllust;   // 이 버튼과 연결된 캐릭터 일러스트
    [SerializeField] GameObject[] _allCharacterIllust; // 하이어라키창에 있는 모든 캐릭터 일러스트
    [SerializeField] SaveCharacterIndex _characterIndex;      // 이 버튼과 연결된 캐릭터 정보 설정

    void Start()
    {
        _nameText.text = _characterIndex.ToString();
        string spritePath = $"Character/{_characterIndex.ToString()}";
        Debug.Log($"Loading sprite from path: {spritePath}");
        _iconImage.sprite = Resources.Load<Sprite>(spritePath);

         _GetCharacterButton.interactable = false;
    }

    void OnEnable()
    {
        GetPieceCount();
    }

    // 획득 가능 여부 확인
    private int GetPieceCount()
    {
        // 이미 획득한 캐릭터라면 획득 버튼 비활성화, 이미지 밝게
        if (GameManager.GetInstance().GetSavedCharacterList().Contains(_characterIndex))
        {
            _GetCharacterButton.interactable = false;
            if(_ButtonCharacterIllust != null)
            {
                _ButtonCharacterIllust.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
            return 0;
        }

        var data = GameManager.GetInstance().GetCharacterPieceData(_characterIndex);
        
        // 1개도 채우지 못했다면 이미지 어둡게
        if (data.count < 1)
        {
            if(_ButtonCharacterIllust != null)
            {
                ToolFunctions.FindChild<Image>
                (_ButtonCharacterIllust, "Placeholder Model", true).color = new Color(0.1f, 0.1f, 0.1f, 1f);
            }
        }
        else
        {
            if(_ButtonCharacterIllust != null)
            {
                ToolFunctions.FindChild<Image>
                (_ButtonCharacterIllust, "Placeholder Model", true).color = new Color(1, 1, 1, 1);
            }
        }

        _countText.text = data.count.ToString() + " / 10";
        // 얼마나 조각을 갖고 있는지 리턴
        return data.count;
    }


    public void OnClickSelectButton()
    {
        // 모든 캐릭터 일러스트 비활성화
        foreach (var item in _allCharacterIllust)
        {
            item.SetActive(false);
        }
        // 버튼과 연결된 캐릭터 일러스트 활성화
        _ButtonCharacterIllust.SetActive(true);

        // 버튼 이벤트 해제
        _GetCharacterButton.onClick.RemoveAllListeners();

        // 버튼 이벤트 재설정
        _GetCharacterButton.onClick.AddListener(OnClickGetCharacterButton);

        // 이미 획득한 캐릭터라면 비활성화
        if (GameManager.GetInstance().GetSavedCharacterList().Contains(_characterIndex))
        {
            _GetCharacterButton.interactable = false;
            return;
        }

        // 10개를 채웠다면 버튼 활성화
        if (GetPieceCount() >= 10)
        {
            _GetCharacterButton.interactable = true;
        }
        else
        {
            _GetCharacterButton.interactable = false;
        }
    }

    public void OnClickGetCharacterButton()
    {
        // 조각 개수 10개 줄이기
        GameManager.GetInstance().AddCharacterPieceData(_characterIndex, -10);

        // GameManager를 통해 캐릭터 저장
        GameManager.GetInstance().SetCharacterData(_characterIndex);
        
        // 저장하기
        GameManager.GetInstance().Save();

        // 캐릭터 획득 버튼 비활성화
        _GetCharacterButton.interactable = false;
    }
}
