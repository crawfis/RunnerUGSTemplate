using Unity.Services.RemoteConfig.Authoring.Editor.Assets;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Blocks.Achievements.Editor.UI
{
    [ScriptedImporter(1, "ach")]
    class AchievementsAssetImporter : RemoteConfigImporter<AchievementsAsset, AchievementsFile>
    {
    }
}
