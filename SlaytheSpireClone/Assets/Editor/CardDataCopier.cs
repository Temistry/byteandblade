#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CardDataCopier
{
    [MenuItem("Tools/Copy Card Data to Resources")]
    public static void CopyCardDataToResources()
    {
        // 소스 경로와 대상 경로 설정
        string sourcePath = "Assets/_Assets/GameData/Cards";
        string targetPath = "Assets/Resources/GameData/Cards";
        
        // 대상 디렉토리가 없으면 생성
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }
        
        // 모든 카드 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:CardTemplate", new[] { sourcePath });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(assetPath);
            string targetAssetPath = Path.Combine(targetPath, fileName);
            
            // 에셋 복사
            AssetDatabase.CopyAsset(assetPath, targetAssetPath);
            Debug.Log($"카드 에셋 복사: {assetPath} -> {targetAssetPath}");
        }
        
        AssetDatabase.Refresh();
        Debug.Log("모든 카드 데이터가 Resources 폴더로 복사되었습니다.");
    }
}
#endif 