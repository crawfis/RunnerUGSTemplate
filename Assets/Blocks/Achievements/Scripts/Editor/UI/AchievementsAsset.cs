using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Services.RemoteConfig.Authoring.Editor.Assets;
using UnityEditor;
using IoPath = System.IO.Path;

namespace Blocks.Achievements.Editor.UI
{
    class AchievementsAsset : RemoteConfigAsset
    {
        const string k_TemplatePath = "Packages/com.unity.kits-manager/Assets/Blocks/Achievements/Scripts/Editor/Template/Achievements.ach.txt";
        const string k_DefaultFileName = "Achievements.ach";

        public AchievementDefinitions AchievementDefinitions
        {
            get;
            private set;
        }

        public AchievementsAsset()
        {
            m_Model = new AchievementsFile();
        }

        [MenuItem("Assets/Create/Blocks/Achievements Configuration")]
        static void CreateAchievementAsset()
        {
            var templatePath = GetTemplateScriptPath();
            if (string.IsNullOrEmpty(templatePath))
            {
                Debug.LogError($"Could not locate template at path: {templatePath}");
                return;
            }

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, k_DefaultFileName);
        }

        static string GetTemplateScriptPath()
        {
            var templatePath = string.Empty;
            var thisScriptGuid = AssetDatabase.FindAssets($"t:Script {nameof(AchievementsAssetInspector)}");
            if (thisScriptGuid.Length > 0)
            {
                var thisScriptPath = AssetDatabase.GUIDToAssetPath(thisScriptGuid[0]);
                var containerFolderPath = thisScriptPath.Substring(0, thisScriptPath.LastIndexOf(IoPath.AltDirectorySeparatorChar));
                templatePath = IoPath.Combine(containerFolderPath, "Templates", k_DefaultFileName + ".txt");
            }

            return templatePath;
        }

        public void LoadAchievements()
        {
            try
            {
                if (File.Exists(Path))
                {
                    var contents = File.ReadAllText(Path);
                    var remoteConfig = JsonConvert.DeserializeObject<RemoteConfigFileContent>(contents);
                    var settings = remoteConfig.AchievementSettings ?? new AchievementSettings();
                    AchievementDefinitions = CreateInstance<AchievementDefinitions>();
                    foreach (var achievementSetting in settings)
                    {
                        AchievementDefinitions.Achievements.Add(achievementSetting);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (AchievementDefinitions == null)
            {
                AchievementDefinitions = CreateInstance<AchievementDefinitions>();
            }
        }

        public bool SaveToDisk()
        {
            try
            {
                var achievements = new AchievementSettings();
                achievements.AddRange(AchievementDefinitions.Achievements);
                var repeatedKeys = AchievementDefinitions.Achievements.GroupBy(a => a.Id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (repeatedKeys.Count > 0)
                {
                    var repeatedKeysStr = string.Join(", ", repeatedKeys);
                    Debug.LogError($"The following achievements have conflicting IDs, please fix before saving: {repeatedKeysStr}");
                    return false;
                }

                var content = JsonConvert.SerializeObject(new RemoteConfigFileContent(achievements), Formatting.Indented);
                File.WriteAllText(Path, content);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }
    }
}

