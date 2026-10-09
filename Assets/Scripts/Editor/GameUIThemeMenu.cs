using KnockBallsCase.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KnockBallsCase.Editor
{
    public static class GameUIThemeMenu
    {
        [MenuItem("Tools/Knock Balls/UI/Themed")]
        private static void Themed() { Apply(GameUIThemeController.Appearance.Themed); }

        [MenuItem("Tools/Knock Balls/UI/Legacy")]
        private static void Legacy() { Apply(GameUIThemeController.Appearance.Legacy); }

        private static void Apply(GameUIThemeController.Appearance appearance)
        {
            var controllers = Object.FindObjectsByType<GameUIThemeController>(FindObjectsInactive.Include);
            foreach (var controller in controllers)
            {
                Undo.RegisterFullObjectHierarchyUndo(controller.gameObject, "Change UI appearance");
                controller.SetAppearance(appearance);
                EditorUtility.SetDirty(controller);
                if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }
            if (controllers.Length == 0)
                Debug.LogWarning("Open the PlayGround scene to change its UI appearance.");
        }
    }
}
