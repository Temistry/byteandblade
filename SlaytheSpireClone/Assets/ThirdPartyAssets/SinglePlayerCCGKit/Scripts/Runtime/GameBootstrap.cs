// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using System;

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

        private int numAssetsToLoad;
        private int numAssetsLoaded;

        private void Start()
        {
            var character = GameManager.GetInstance().GetCurrentCharacterTemplate();

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

                // background.sprite를 2배로, 하단부를 화면 하단에 맞추기
                background.transform.localScale = new Vector3(1.1f, 1.1f, 1);
                background.transform.position = new Vector3(0, 5, 0);
            };
        }

        private void CreatePlayer(HeroTemplate template)
        {
            player = Instantiate(template.Prefab, playerPivot);
            Assert.IsNotNull(player);

            // 플레이어 설정 초기화
            var health = playerConfig.Hp;
            health.Value = template.Health;

            var mana = playerConfig.Mana;
            mana.Value = template.Mana;

            manaResetSystem.SetDefaultMana(mana.Value);

            // GameManager를 통해 저장된 데이터 로드
            var gameManager = GameManager.GetInstance();
            if (gameManager != null && gameManager.IsContinueGame())
            {
                try
                {
                    // 덱 설정
                    playerDeck.Clear();
                    var savedDeck = gameManager.GetCardList();
                    if (savedDeck != null && savedDeck.Count > 0)
                    {
                        foreach (var card in savedDeck)
                        {
                            if (card != null)
                            {
                                playerDeck.Add(card);
                            }
                        }
                    }
                    else
                    {
                        LoadDefaultDeck(template);
                        GameManager.GetInstance().Save();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"GameManager에서 데이터 로드 중 오류: {e.Message}");
                    LoadDefaultDeck(template);
                    GameManager.GetInstance().Save();
                }
            }
            else
            {

                // 캐릭터 설정에 따른 체력 설정
                health.Value = template.Health;

                // 게임매니저에 저장
                gameManager.Health = health.Value;
                gameManager.MaxHealth = template.MaxHealth;
                
                LoadDefaultDeck(template);
            }

            // GameInfo 업데이트
            var gameInfo = FindFirstObjectByType<GameInfo>();
            if (gameInfo != null && gameInfo.SaveData != null)
            {
                if (gameInfo.SaveData.stats == null)
                    gameInfo.SaveData.stats = new SavePlayerStats();

                gameInfo.SaveData.stats.MaxHp = health.Value;
                gameInfo.SaveData.stats.Shield = 0;

                if (gameInfo.SaveData.deckData == null)
                    gameInfo.SaveData.deckData = new DeckData();

                gameInfo.SaveData.deckData.Deck.Clear();
                foreach (var card in playerDeck)
                {
                    gameInfo.SaveData.deckData.Deck.Add(card.Id);
                }
            }


            CreateHpWidget(playerConfig.HpWidget, player, playerConfig.Hp, template.MaxHealth, playerConfig.Shield);
            CreateStatusWidget(playerConfig.StatusWidget, player);

            manaWidget.Initialize(playerConfig.Mana);

            var obj = player.GetComponent<CharacterObject>();
            obj.Template = template;
            obj.Character = new RuntimeCharacter
            {
                Hp = playerConfig.Hp,
                Shield = playerConfig.Shield,
                Mana = playerConfig.Mana,
                Status = playerConfig.Status,
                MaxHp = playerConfig.Hp.Value,
                CurrentUsedCard = null
            };
            obj.Character.Status.Value.Clear();

            numAssetsLoaded++;
            InitializeGame();


            // UI_PlayPannel 찾아서 텍스트 업데이트
            var playPannel = FindFirstObjectByType<UI_PlayPannel>();
            if (playPannel != null)
            {
                // 강제로 UI 업데이트 호출
                playPannel.UpdateUI();
            }
            else
            {
                Debug.LogWarning("UI_PlayPannel을 찾을 수 없습니다.");
            }
        }

        private void LoadDefaultDeck(HeroTemplate template)
        {
            foreach (var entry in template.StartingDeck.Entries)
            {
                for (var i = 0; i < entry.NumCopies; i++)
                {
                    playerDeck.Add(entry.Card);
                    GameManager.GetInstance().AddCard(entry.Card);
                }
            }
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

                // 적이 보스라면 브금 변경
                if (template.IsBoss)
                {
                    BGMManager.Instance.PlayBossBGM();
                }
                // 적이 엘리트라면 브금 변경
                else if (template.IsElite)
                {
                    BGMManager.Instance.PlayEliteBGM();
                }

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
