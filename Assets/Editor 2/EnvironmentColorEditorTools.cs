using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EnvironmentColorEditorTools
{
    // =====================================================
    // 标记为灰色
    // =====================================================

    [MenuItem("GameObject/环境颜色/标记为灰色", false, 0)]
    private static void MarkAsGray()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "场景里没有找到 EnvironmentColorManager。"
            );
            return;
        }

        GameObject[] selectedObjects =
            Selection.gameObjects;

        if (selectedObjects == null ||
            selectedObjects.Length == 0)
        {
            Debug.LogWarning(
                "请先在 Hierarchy 里选择物体。"
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

            // 自动包含这个物体以及所有子物体 Renderer
            Renderer[] renderers =
                obj.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                // 如果之前在深灰列表，先移除
                manager.darkRenderers.Remove(r);

                // 加到灰色列表
                if (!manager.grayRenderers.Contains(r))
                {
                    manager.grayRenderers.Add(r);
                    addedCount++;
                }
            }
        }

        EditorUtility.SetDirty(manager);

        Debug.Log(
            "已将 " +
            addedCount +
            " 个 Renderer 标记为灰色。"
        );
    }


    // =====================================================
    // 标记为深灰 / 黑色
    // =====================================================

    [MenuItem("GameObject/环境颜色/标记为深灰", false, 1)]
    private static void MarkAsDark()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "场景里没有找到 EnvironmentColorManager。"
            );
            return;
        }

        GameObject[] selectedObjects =
            Selection.gameObjects;

        if (selectedObjects == null ||
            selectedObjects.Length == 0)
        {
            Debug.LogWarning(
                "请先在 Hierarchy 里选择物体。"
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

                // 如果之前在灰色列表，先移除
                manager.grayRenderers.Remove(r);

                // 加到深灰列表
                if (!manager.darkRenderers.Contains(r))
                {
                    manager.darkRenderers.Add(r);
                    addedCount++;
                }
            }
        }

        EditorUtility.SetDirty(manager);

        Debug.Log(
            "已将 " +
            addedCount +
            " 个 Renderer 标记为深灰。"
        );
    }


    // =====================================================
    // 恢复为默认白色
    // 其实就是从两个列表里移除
    // =====================================================

    [MenuItem("GameObject/环境颜色/恢复为默认白色", false, 2)]
    private static void MarkAsWhite()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "场景里没有找到 EnvironmentColorManager。"
            );
            return;
        }

        GameObject[] selectedObjects =
            Selection.gameObjects;

        if (selectedObjects == null ||
            selectedObjects.Length == 0)
        {
            Debug.LogWarning(
                "请先在 Hierarchy 里选择物体。"
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
            "已清除 " +
            removedCount +
            " 个颜色标记，运行时会使用默认白色。"
        );
    }


    // =====================================================
    // 清理列表中的空引用
    // =====================================================

    [MenuItem("GameObject/环境颜色/清理空引用", false, 20)]
    private static void CleanNullReferences()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "场景里没有找到 EnvironmentColorManager。"
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
            "Gray / Dark 列表中的空引用已清理。"
        );
    }
}