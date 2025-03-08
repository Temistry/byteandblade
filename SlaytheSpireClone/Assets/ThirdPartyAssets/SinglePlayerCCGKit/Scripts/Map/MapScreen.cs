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
            SceneManager.sceneUnloaded += scene =>
            {
                DOTween.KillAll();
            };
            gameManager = GameManager.GetInstance();
        }
        // 게임 오브젝트의 전체 경로를 반환하는 헬퍼 메서드
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
        private void Start()
        {
            var mapScreens = FindObjectsByType<MapScreen>(FindObjectsSortMode.None);
            Debug.Log($"씬에 존재하는 MapScreen 컴포넌트 수: {mapScreens.Length}");

            foreach (var screen in mapScreens)
            {
                Debug.Log($"MapScreen 인스턴스: {screen.gameObject.name}, 경로: {GetGameObjectPath(screen.gameObject)}");
            }

            rng = new Random();

            // 맵 로드 또는 생성
            var map = gameManager.LoadOrGenerateMap(rng, mapGenerator);
            mapView.ShowMap(map);
            mapTracker.TrackMap(map, rng);

            // 현재 노드 설정 (맵의 마지막 경로 지점을 현재 노드로 설정)
            if (map.Path != null && map.Path.Count > 0)
            {
                var currentCoordinate = map.Path[map.Path.Count - 1];
                var currentNode = map.GetNode(currentCoordinate);
                gameManager.SetCurrentNode(currentNode);
                Debug.Log($"현재 노드 설정: {currentNode.Type} at {currentCoordinate}");
            }

            gameManager.SaveCurrentMap();

            var gameInfo = FindFirstObjectByType<GameInfo>();
            if (gameInfo != null)
            {
                if (gameInfo.PlayerWonEncounter)
                {
                    var mapNode = map.GetNode(gameInfo.PlayerCoordinate);
                    map.Path.Add(mapNode.Coordinate);

                    // 보스 노드를 클리어했는지 확인
                    if (mapNode.Type == NodeType.Boss)
                    {
                        // 보스 클리어 처리
                        gameManager.SetBossCleared(true);
                        Debug.Log("보스를 클리어했습니다! 다음 맵을 준비합니다.");
                    }

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