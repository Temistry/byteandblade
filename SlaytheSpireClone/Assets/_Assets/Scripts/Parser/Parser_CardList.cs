using System.Collections.Generic;
using CCGKit;
using UnityEngine;

public class Parser_CardList : Singleton<Parser_CardList>
{
    public List<CardTemplate> cardList = new List<CardTemplate>();

    public CardTemplate GetCardTemplate(int id)
    {
        return cardList.Find(card => card.Id == id);
    }
}
