#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EnvironmentColorEditorTools
{
    // =====================================================
    // Mark as Gray
    // =====================================================

    [MenuItem("GameObject/Environment Color/Mark As Gray", false, 0)]
    private static void MarkAsGray()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "EnvironmentColorManager was not found in the scene."
            );
            return;
        }

        GameObject[] selectedObjects =
            Selection.gameObjects;

        if (selectedObjects == null ||
            selectedObjects.Length == 0)
        {
            Debug.LogWarning(
                "Please select an object in the Hierarchy first."
            );
            return;
        }

        Undo.RecordObject(
            manager,
            "Mark Environment As Gray"
        );

        int addedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null)
                continue;

            // Include this object and all child Renderers
            Renderer[] renderers =
                obj.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                // Remove from Dark list first
                manager.darkRenderers.Remove(r);

                // Add to Gray list
                if (!manager.grayRenderers.Contains(r))
                {
                    manager.grayRenderers.Add(r);
                    addedCount++;
                }
            }
        }

        EditorUtility.SetDirty(manager);

        Debug.Log(
            "Marked " +
            addedCount +
            " Renderer(s) as Gray."
        );
    }


    // =====================================================
    // Mark as Dark
    // =====================================================

    [MenuItem("GameObject/Environment Color/Mark As Dark", false, 1)]
    private static void MarkAsDark()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "EnvironmentColorManager was not found in the scene."
            );
            return;
        }

        GameObject[] selectedObjects =
            Selection.gameObjects;

        if (selectedObjects == null ||
            selectedObjects.Length == 0)
        {
            Debug.LogWarning(
                "Please select an object in the Hierarchy first."
            );
            return;
        }

        Undo.RecordObject(
            manager,
            "Mark Environment As Dark"
        );

        int addedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null)
                continue;

            Renderer[] renderers =
                obj.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                // Remove from Gray list first
                manager.grayRenderers.Remove(r);

                // Add to Dark list
                if (!manager.darkRenderers.Contains(r))
                {
                    manager.darkRenderers.Add(r);
                    addedCount++;
                }
            }
        }

        EditorUtility.SetDirty(manager);

        Debug.Log(
            "Marked " +
            addedCount +
            " Renderer(s) as Dark."
        );
    }


    // =====================================================
    // Restore to Default White
    // Removes Renderer from both lists
    // =====================================================

    [MenuItem("GameObject/Environment Color/Restore Default White", false, 2)]
    private static void MarkAsWhite()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "EnvironmentColorManager was not found in the scene."
            );
            return;
        }

        GameObject[] selectedObjects =
            Selection.gameObjects;

        if (selectedObjects == null ||
            selectedObjects.Length == 0)
        {
            Debug.LogWarning(
                "Please select an object in the Hierarchy first."
            );
            return;
        }

        Undo.RecordObject(
            manager,
            "Mark Environment As White"
        );

        int removedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null)
                continue;

            Renderer[] renderers =
                obj.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                if (manager.grayRenderers.Remove(r))
                    removedCount++;

                if (manager.darkRenderers.Remove(r))
                    removedCount++;
            }
        }

        EditorUtility.SetDirty(manager);

        Debug.Log(
            "Removed " +
            removedCount +
            " color marker(s). They will use Default White at runtime."
        );
    }


    // =====================================================
    // Clean Null References
    // =====================================================

    [MenuItem("GameObject/Environment Color/Clean Null References", false, 20)]
    private static void CleanNullReferences()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "EnvironmentColorManager was not found in the scene."
            );
            return;
        }

        Undo.RecordObject(
            manager,
            "Clean Environment Color Lists"
        );

        manager.grayRenderers.RemoveAll(
            r => r == null
        );

        manager.darkRenderers.RemoveAll(
            r => r == null
        );

        EditorUtility.SetDirty(manager);

        Debug.Log(
            "Null references in Gray / Dark lists have been cleaned."
        );
    }
}

#endif