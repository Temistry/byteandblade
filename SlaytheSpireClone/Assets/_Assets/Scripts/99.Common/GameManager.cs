using System.Collections.Generic;
using CCGKit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WanzyeeStudio;

public class GameManager : BaseSingleton<GameManager>
{   
    private readonly string saveDataPrefKey = "save";
    private List<CardTemplate> playerDeck = new List<CardTemplate>();
    public AssetReference characterTemplate;
    public bool IsGetStartRelic 
    { 
        get;
        private set;
    }

    protected override void Awake()
    {
        base.Awake();
        var handle = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplate);
        handle.Completed += op =>
        {
            var template = op.Result;
            if (PlayerPrefs.HasKey(saveDataPrefKey))
            {
                var json = PlayerPrefs.GetString(saveDataPrefKey);
                var saveData = JsonUtility.FromJson<SaveData>(json);
                playerDeck.Clear();
                foreach (var id in saveData.Deck)
                {
                    var card = template.StartingDeck.Entries.Find(x => x.Card.Id == id);
                    if (card == null)
                    {
                        card = template.RewardDeck.Entries.Find(x => x.Card.Id == id);
                    }
                    if (card != null)
                    {
                        playerDeck.Add(card.Card);
                    }
                }
            }
            else
            {
                foreach (var entry in template.StartingDeck.Entries)
                {
                    for (var i = 0; i < entry.NumCopies; i++)
                    {
                        playerDeck.Add(entry.Card);
                    }
                }
            }

        }; 
    }

    public List<CardTemplate> GetCardList()
    {
        return playerDeck;
    }

    public void SetIsGetStartRelic(bool value)
    {
        IsGetStartRelic = value;
    }
}
