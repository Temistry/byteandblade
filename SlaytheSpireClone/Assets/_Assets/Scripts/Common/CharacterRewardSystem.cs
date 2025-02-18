using UnityEngine;
using CCGKit;
using System;
using System.Collections.Generic;
public class CharacterRewardSystem : MonoBehaviour
{
    // 캐릭터 조각 보상 시스템
    public Canvas Canvas;

    public UI_CharacterRewardView _CharacterRewardUI;

    [SerializeField]
    private int getMinCount;
    [SerializeField]
    private int getMaxCount;
    
    public void OnPlayerRewardCharacterPiece()
    {
        Canvas.gameObject.SetActive(true);

        int randomIndex = UnityEngine.Random.Range(0, (int)SaveCharacterIndex.Max);
        SaveCharacterIndex characterIndex = (SaveCharacterIndex)randomIndex;
        int count = UnityEngine.Random.Range(getMinCount, getMaxCount);

        GetPlayerRewardCharacterPiece(characterIndex, count);
    }

    // 캐릭터 조각 보상 시스템
    public void GetPlayerRewardCharacterPiece(SaveCharacterIndex characterIndex, int count)
    {
        Canvas.gameObject.SetActive(true);
        var data = new CharacterPieceData(characterIndex, count);
        _CharacterRewardUI.SetInfo(data);
        GameManager.GetInstance().AddCharacterPiece(data);
    }
}