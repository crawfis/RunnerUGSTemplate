using System;
using System.Collections.Generic;
using System.IO;
using Unity.Services.DeploymentApi.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements.Editor.UI
{
    [CustomEditor(typeof(AchievementsAsset))]
    public class AchievementsAssetInspector : UnityEditor.Editor
    {
        AchievementsAsset m_TargetAsset;
        AchievementDefinitions m_AchievementDefinitions;
        InspectorElement m_AssetInspectorElement;
        SerializedObject m_SerializedObjectOriginal;
        SerializedObject m_SerializedObjectCurrent;

        VisualElement m_RootElement;
        Button m_ApplyButton;
        Button m_RevertButton;
        Button m_ViewInDeploymentButton;

        public override VisualElement CreateInspectorGUI()
        {
            m_TargetAsset = target as AchievementsAsset;

            m_RootElement = new VisualElement();

#if !UNITY_2022_1_OR_NEWER
            //2021 does not support nested inspector elements properly
            var applyRevert = m_RootElement.Q<VisualElement>("ContainerApplyRevert");
            m_RootElement.Remove(applyRevert);
            var txtEle = new TextField() { multiline = true, isReadOnly = true };
            m_RootElement.Insert(0, txtEle);
            txtEle.value = ReadResourceBody(targets[0]);
#else
            if (CloneUxml())
            {
                CreateInspectorElement();
                BuildAchievementDefinitions();
                InitializeSerializedObjects();
                InitializeApplyRevertButtons();
                InitializeViewInDeployment();
            }
#endif

            return m_RootElement;
        }

#if !UNITY_2022_1_OR_NEWER
        static string ReadResourceBody(UnityEngine.Object resource)
        {
            var path = AssetDatabase.GetAssetPath(resource);
            var lines = File.ReadLines(path).Take(75).ToList();
            if (lines.Count == 75)
            {
                lines.Add("...");
            }
            return string.Join(Environment.NewLine, lines);
        }
#endif

        bool CloneUxml()
        {
            var uxmlPath = string.Empty;
            VisualTreeAsset uiAsset = null;
            var thisScriptGuid = AssetDatabase.FindAssets($"t:Script {nameof(AchievementsAssetInspector)}");
            if (thisScriptGuid.Length > 0)
            {
                var thisScriptPath = AssetDatabase.GUIDToAssetPath(thisScriptGuid[0]);
                var containerFolderPath = thisScriptPath.Substring(0, thisScriptPath.LastIndexOf(Path.AltDirectorySeparatorChar));
                uxmlPath = Path.Combine(containerFolderPath, "Templates", "AchievementsInspector.uxml");
                uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            }

            if (!uiAsset)
            {
                Debug.LogError($"Could not load UXML at path '{uxmlPath}'");
                return false;
            }

            uiAsset.CloneTree(m_RootElement);
            return true;
        }

        void CreateInspectorElement()
        {
            var container = m_RootElement.Q("ContainerConfig");
            m_AssetInspectorElement = new InspectorElement();
            container.Add(m_AssetInspectorElement);
        }

        void BuildAchievementDefinitions()
        {
            m_TargetAsset.LoadAchievements();
            m_AchievementDefinitions = CreateInstance<AchievementDefinitions>();
            m_AchievementDefinitions.CopyFrom(m_TargetAsset.AchievementDefinitions);
        }

        void InitializeSerializedObjects()
        {
            m_SerializedObjectOriginal = new SerializedObject(m_AchievementDefinitions);
            m_SerializedObjectCurrent = new SerializedObject(m_AchievementDefinitions);

            m_AssetInspectorElement.Unbind();
            m_AssetInspectorElement.Bind(m_SerializedObjectCurrent);
            m_AssetInspectorElement.TrackSerializedObjectValue(m_SerializedObjectCurrent, SerializedObjectValueChanged);
        }

        void SerializedObjectValueChanged(SerializedObject obj)
        {
            var areObjectsEqual = AreSerializedObjectsEqual(m_SerializedObjectOriginal, m_SerializedObjectCurrent);
            UpdateApplyRevertButtons(!areObjectsEqual);
            hasUnsavedChanges = !areObjectsEqual;
        }

        void InitializeApplyRevertButtons()
        {
            m_ApplyButton = m_RootElement.Q<Button>("ButtonApply");
            m_ApplyButton.clicked += SaveChanges;
            m_RevertButton = m_RootElement.Q<Button>("ButtonRevert");
            m_RevertButton.clicked += DiscardChanges;

            UpdateApplyRevertButtons(false);
        }

        void UpdateApplyRevertButtons(bool toggle)
        {
            m_RevertButton.SetEnabled(toggle);
            m_ApplyButton.SetEnabled(toggle);
        }

        void SaveAssetChanges()
        {
            m_TargetAsset.AchievementDefinitions.CopyFrom((AchievementDefinitions)m_SerializedObjectCurrent.targetObject);
            if (m_TargetAsset.SaveToDisk())
            {
                InitializeSerializedObjects();
            }
        }

        void RevertAssetChanges()
        {
            BuildAchievementDefinitions();
            InitializeSerializedObjects();
        }

        public override void SaveChanges()
        {
            SaveAssetChanges();
            base.SaveChanges();
            UpdateApplyRevertButtons(false);
        }

        public override void DiscardChanges()
        {
            RevertAssetChanges();
            base.DiscardChanges();
            UpdateApplyRevertButtons(false);
        }

        static bool AreSerializedObjectsEqual(SerializedObject obj1, SerializedObject obj2)
        {
            if (obj1 == null || obj2 == null)
                return false;

            var iterator1 = obj1.GetIterator();
            var iterator2 = obj2.GetIterator();

            while (iterator1.NextVisible(true) && iterator2.NextVisible(true))
            {
                if (iterator1.propertyType != iterator2.propertyType || iterator1.name != iterator2.name)
                    return false;

                if (!SerializedProperty.DataEquals(iterator1, iterator2))
                    return false;
            }

            return true;
        }

        void InitializeViewInDeployment()
        {
            m_ViewInDeploymentButton = m_RootElement.Q<Button>("ViewInDeployment");
            m_ViewInDeploymentButton.clickable.clicked += ViewDeployment;
        }

        void ViewDeployment()
        {
            Deployments.Instance.DeploymentWindow.OpenWindow();

            var filePath = Path.GetFullPath(m_TargetAsset.Path);
            if (File.Exists(filePath))
            {
                Deployments.Instance.DeploymentWindow.ClearSelection();
                var deploymentItems = Deployments.Instance.DeploymentWindow.GetFromFiles(new List<string> { filePath });
                Deployments.Instance.DeploymentWindow.Select(deploymentItems);
            }
        }
    }
}
