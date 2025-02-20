using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SelectCharPiece : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nameText;       // 캐릭터 이름
    [SerializeField] TextMeshProUGUI _countText;      // 조각 개수
    [SerializeField] Image _iconImage;                // 버튼 아이콘
    [SerializeField] GameObject _ButtonCharacterIllust;   // 이 버튼과 연결된 캐릭터 일러스트
    [SerializeField] GameObject[] _allCharacterIllust; // 하이어라키창에 있는 모든 캐릭터 일러스트
    [SerializeField] SaveCharacterIndex _characterIndex;      // 이 버튼과 연결된 캐릭터 정보 설정

    void Start()
    {
        _nameText.text = _characterIndex.ToString();
        _iconImage.sprite = Resources.Load<Sprite>($"Character/{_characterIndex.ToString()}");
    }

    void OnEnable()
    {
        GetPieceCount();
    }

    // 획득 가능 여부 확인
    private int GetPieceCount()
    {
        var data = GameManager.GetInstance().GetCharacterPieceData(_characterIndex);
        if (data != null)
        {

            _countText.text = GetPieceCount().ToString() + " / 10";

            // 얼마나 조각을 갖고 있는지 리턴
            return data.count;
        }
        else
        {
            _countText.text = GetPieceCount().ToString() + " / 10";

        }
        return 0;
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
    }
}
