// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

using Random = UnityEngine.Random;

namespace CCGKit
{
    /// <summary>
    /// This component is responsible for bootstrapping the game/battle scene. This process
    /// mainly consists on:
    ///     - Creating the player character and the associated UI widgets.
    ///     - Creating the enemy character/s and the associated UI widgets.
    ///     - Starting the turn sequence.
    ///(한국어번역)이 컴포넌트는 게임/전투 장면을 부트스트랩하는 역할을 합니다. 이 과정은 주로 다음을 포함합니다:
    ///     - 플레이어 캐릭터와 관련 UI 위젯 생성.
    ///     - 적 캐릭터/적 캐릭터들과 관련 UI 위젯 생성.
    ///     - 턴 시퀀스 시작.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
#pragma warning disable 649
        [Header("Configuration")]
        [SerializeField]
        private PlayableCharacterConfiguration playerConfig;
        [SerializeField]
        private List<NonPlayableCharacterConfiguration> enemyConfig;

        [Header("Systems")]
        [SerializeField]
        private DeckDrawingSystem deckDrawingSystem;
        [SerializeField]
        private HandPresentationSystem handPresentationSystem;
        [SerializeField]
        private ManaResetSystem manaResetSystem;
        [SerializeField]
        private TurnManagementSystem turnManagementSystem;
        [SerializeField]
        private CardWithArrowSelectionSystem cardWithArrowSelectionSystem;
        [SerializeField]
        private EnemyBrainSystem enemyBrainSystem;
        [SerializeField]
        private EffectResolutionSystem effectResolutionSystem;
        [SerializeField]
        private PoisonResolutionSystem poisonResolutionSystem;
        [SerializeField]
        private CharacterDeathSystem characterDeathSystem;

        [Header("Character pivots")]
        [SerializeField]
        public Transform playerPivot;
        [SerializeField]
        public List<Transform> enemyPivots1;
        [SerializeField]
        public List<Transform> enemyPivots2;

        [Header("UI")]
        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private ManaWidget manaWidget;
        [SerializeField]
        private DeckWidget deckWidget;
        [SerializeField]
        private DiscardPileWidget discardPileWidget;
        [SerializeField]
        private EndTurnButton endTurnButton;
        [SerializeField]
        private SpriteRenderer background;

        [Header("Pools")]
        [SerializeField]
        private ObjectPool cardPool;
#pragma warning restore 649

        private Camera mainCamera;

        private List<CardTemplate> playerDeck = new List<CardTemplate>();

        private GameObject player;
        private List<GameObject> enemies = new List<GameObject>();

        private GameInfo gameInfo;

        private readonly string saveDataPrefKey = "save";

        private int numAssetsToLoad;
        private int numAssetsLoaded;

        private void Start()
        {
            var character = GameManager.GetInstance().GetCurrentCharacter();

            mainCamera = Camera.main;

            cardPool.Initialize();

            Addressables.InitializeAsync().Completed += op =>
            {
                CreatePlayer(character);
                numAssetsToLoad++;

                var gameInfo = FindFirstObjectByType<GameInfo>();
                var idx = 0;
                foreach (var template in gameInfo.Encounter.Enemies)
                {
                    CreateEnemy(template, idx++);
                    numAssetsToLoad++;
                }

                background.sprite = gameInfo.Encounter.Background;
            };
        }

        private void CreatePlayer(AssetReference templateRef)
        {
            var handle = Addressables.LoadAssetAsync<HeroTemplate>(templateRef);
            handle.Completed += op =>
            {
                var template = op.Result;
                player = Instantiate(template.Prefab, playerPivot);
                Assert.IsNotNull(player);

                var hp = playerConfig.Hp;
                var mana = playerConfig.Mana;
                var shield = playerConfig.Shield;
                hp.Value = template.Hp;
                mana.Value = template.Mana;
                shield.Value = 0;

                manaResetSystem.SetDefaultMana(template.Mana);

                if (PlayerPrefs.HasKey(saveDataPrefKey))
                {
                    var json = PlayerPrefs.GetString(saveDataPrefKey);
                    var saveData = JsonUtility.FromJson<SaveData>(json);
                    hp.Value = saveData.Hp;
                    shield.Value = saveData.Shield;

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

                var gameInfo = FindFirstObjectByType<GameInfo>();
                if (gameInfo != null)
                {
                    gameInfo.SaveData.Hp = hp.Value;
                    gameInfo.SaveData.Shield = shield.Value;
                    gameInfo.SaveData.Deck.Clear();
                    foreach (var card in playerDeck)
                    {
                        gameInfo.SaveData.Deck.Add(card.Id);
                    }
                }

                CreateHpWidget(playerConfig.HpWidget, player, hp, template.Hp, shield);
                CreateStatusWidget(playerConfig.StatusWidget, player);

                manaWidget.Initialize(mana);

                var obj = player.GetComponent<CharacterObject>();
                obj.Template = template;
                obj.Character = new RuntimeCharacter
                { 
                    Hp = hp, 
                    Shield = shield,
                    Mana = mana, 
                    Status = playerConfig.Status,
                    MaxHp = template.Hp,
                    CurrentUsedCard = null
                };
                obj.Character.Status.Value.Clear();

                numAssetsLoaded++;
                InitializeGame();
            };
        }

        private void CreateEnemy(AssetReference templateRef, int index)
        {
            var handle = Addressables.LoadAssetAsync<EnemyTemplate>(templateRef);
            handle.Completed += op =>
            {
                var pivots = enemyPivots1;
                var gameInfo = FindFirstObjectByType<GameInfo>();
                if (gameInfo.Encounter.Enemies.Count == 2)
                {
                    pivots = enemyPivots2;
                }

                var template = op.Result;
                var enemy = Instantiate(template.Prefab, pivots[index]);
                Assert.IsNotNull(enemy);

                var initialHp = Random.Range(template.HpLow, template.HpHigh + 1);
                var hp = enemyConfig[index].Hp;
                var shield = enemyConfig[index].Shield;
                hp.Value = initialHp;
                shield.Value = 0;

                CreateHpWidget(enemyConfig[index].HpWidget, enemy, hp, initialHp, shield);
                CreateStatusWidget(enemyConfig[index].StatusWidget, enemy);
                CreateIntentWidget(enemyConfig[index].IntentWidget, enemy);

                var obj = enemy.GetComponent<CharacterObject>();
                obj.Template = template;
                obj.Character = new RuntimeCharacter 
                { 
                    Hp = hp, 
                    Shield = shield,
                    Status = enemyConfig[index].Status,
                    MaxHp = enemyConfig[index].Hp.Value 
                };
                obj.Character.Status.Value.Clear();

                enemies.Add(enemy);

                numAssetsLoaded++;
                InitializeGame();
            };
        }

        private void InitializeGame()
        {
            if (numAssetsLoaded != numAssetsToLoad)
            {
                return;
            }

            deckDrawingSystem.Initialize(deckWidget, discardPileWidget);
            var deckSize = deckDrawingSystem.LoadDeck(playerDeck);
            deckDrawingSystem.ShuffleDeck();

            handPresentationSystem.Initialize(cardPool, deckWidget, discardPileWidget);

            var playerCharacter = player.GetComponent<CharacterObject>();
            var enemyCharacters = new List<CharacterObject>(enemies.Count);
            foreach (var enemy in enemies)
            {
                enemyCharacters.Add(enemy.GetComponent<CharacterObject>());
            }
            cardWithArrowSelectionSystem.Initialize(playerCharacter, enemyCharacters);
            enemyBrainSystem.Initialize(playerCharacter, enemyCharacters);
            effectResolutionSystem.Initialize(playerCharacter, enemyCharacters);
            poisonResolutionSystem.Initialize(playerCharacter, enemyCharacters);
            characterDeathSystem.Initialize(playerCharacter, enemyCharacters);

            turnManagementSystem.BeginGame();
        }

        private void CreateHpWidget(GameObject prefab, GameObject character, IntVariable hp, int maxHp, IntVariable shield)
        {
            if (canvas == null) return;
            
            var hpWidget = Instantiate(prefab, canvas.transform, false);
            var pivot = character.transform;
            var canvasPos = mainCamera.WorldToViewportPoint(pivot.position + new Vector3(0.0f, -0.5f, 0.0f));
            hpWidget.GetComponent<RectTransform>().anchorMin = canvasPos;
            hpWidget.GetComponent<RectTransform>().anchorMax = canvasPos;
            hpWidget.GetComponent<HpWidget>().Initialize(hp, maxHp, shield);
        }

        private void CreateStatusWidget(GameObject prefab, GameObject character)
        {
            var widget = Instantiate(prefab, canvas.transform, false);
            var pivot = character.transform;
            var canvasPos = mainCamera.WorldToViewportPoint(pivot.position + new Vector3(0.0f, -1.0f, 0.0f));
            widget.GetComponent<RectTransform>().anchorMin = canvasPos;
            widget.GetComponent<RectTransform>().anchorMax = canvasPos;
        }

        private void CreateIntentWidget(GameObject prefab, GameObject character)
        {
            var widget = Instantiate(prefab, canvas.transform, false);
            var pivot = character.transform;
            var size = character.GetComponent<BoxCollider2D>().bounds.size;
            var canvasPos = mainCamera.WorldToViewportPoint(
                pivot.position + new Vector3(-0.5f, size.y + 0.7f, 0.0f));
            widget.GetComponent<RectTransform>().anchorMin = canvasPos;
            widget.GetComponent<RectTransform>().anchorMax = canvasPos;
        }

        public List<CardTemplate> GetPlayerDeck()
        {
            return playerDeck;
        }
    }
}
