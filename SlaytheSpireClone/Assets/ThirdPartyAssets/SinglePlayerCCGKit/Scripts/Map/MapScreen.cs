// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

using Random = System.Random;

namespace CCGKit
{
    /// <summary>
    /// This component brings together the map generator, map view and map tracker in a concerted
    /// effort to provide the map functionality to the current Unity scene.
    /// </summary>
    public class MapScreen : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private MapGenerator mapGenerator;
        [SerializeField]
        private MapView mapView;
        [SerializeField]
        private MapTracker mapTracker;
#pragma warning restore 649

        private Random rng;
        private GameManager gameManager;

        //private readonly string mapPrefKey = "map";
        private readonly string saveDataPrefKey = "playerData";

        private void Awake()
        {
            SceneManager.sceneUnloaded += scene => {
                DOTween.KillAll();
            };
            gameManager = GameManager.GetInstance();
        }

        private void Start()
        {
            rng = new Random();

            // 맵 로드 또는 생성
            var map = gameManager.LoadOrGenerateMap(rng, mapGenerator);
            mapView.ShowMap(map);
            mapTracker.TrackMap(map, rng);
            
            gameManager.SaveCurrentMap();

            var gameInfo = FindFirstObjectByType<GameInfo>();
            if (gameInfo != null)
            {
                if (gameInfo.PlayerWonEncounter)
                {
                    var mapNode = map.GetNode(gameInfo.PlayerCoordinate);
                    map.Path.Add(mapNode.Coordinate);

                    mapView.SetReachableNodes();
                    mapView.SetLineColors();
                    var mapNodeView = mapView.GetNode(gameInfo.PlayerCoordinate);
                    mapNodeView.ShowSwirl();
                    
                    // 맵 저장
                    gameManager.SaveCurrentMap();

                    var camPos = Camera.main.transform.position;
                    camPos.y = mapNodeView.transform.position.y;
                    Camera.main.transform.position = camPos;

                    SavePlayerData(gameInfo.SaveData);
                }
            }
            else
            {
                LoadPlayerData();

                foreach (var coordinate in map.Path)
                {
                    var mapNode = mapView.GetNode(coordinate);
                    mapNode.ShowSwirl();

                    var camPos = Camera.main.transform.position;
                    camPos.y = mapNode.transform.position.y;
                    Camera.main.transform.position = camPos;
                }
            }
        }

        private void LoadPlayerData()
        {
            if (PlayerPrefs.HasKey(saveDataPrefKey))
            {
                var json = PlayerPrefs.GetString(saveDataPrefKey);
                var saveData = JsonUtility.FromJson<SaveData>(json);
                var gameInfo = FindFirstObjectByType<GameInfo>();
                if (gameInfo != null)
                {
                    gameInfo.SaveData = saveData;
                }
            }
        }

        private void SavePlayerData(SaveData data)
        {
            gameManager.Save();
        }
    }
}