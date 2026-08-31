#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class EnvironmentColorSelectionTools
{
    // =====================================================
    // 找到场景里的 EnvironmentColorManager
    // =====================================================

    private static EnvironmentColorManager FindManager()
    {
        EnvironmentColorManager manager =
            Object.FindObjectOfType<EnvironmentColorManager>();

        if (manager == null)
        {
            Debug.LogError(
                "场景里没有找到 EnvironmentColorManager。"
            );
        }

        return manager;
    }


    // =====================================================
    // 获取当前选中物体的所有 Renderer
    // =====================================================

    private static List<Renderer> GetSelectedRenderers()
    {
        List<Renderer> result =
            new List<Renderer>();


        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj == null)
                continue;


            Renderer[] renderers =
                obj.GetComponentsInChildren<Renderer>(true);


            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;


                if (!result.Contains(r))
                {
                    result.Add(r);
                }
            }
        }


        return result;
    }


    // =====================================================
    // 标记为灰色
    // =====================================================

    [MenuItem("GameObject/环境颜色/设为灰色", false, 20)]
    private static void SetGray()
    {
        EnvironmentColorManager manager =
            FindManager();

        if (manager == null)
            return;


        List<Renderer> selected =
            GetSelectedRenderers();


        Undo.RecordObject(
            manager,
            "Set Environment Gray"
        );


        int count = 0;


        foreach (Renderer r in selected)
        {
            // 如果原来在黑色列表里
            // 先删除
            manager.darkRenderers.Remove(r);


            // 加入灰色列表
            if (!manager.grayRenderers.Contains(r))
            {
                manager.grayRenderers.Add(r);

                count++;
            }
        }


        EditorUtility.SetDirty(manager);


        Debug.Log(
            "已把 " +
            count +
            " 个环境 Renderer 标记为灰色。"
        );
    }


    // =====================================================
    // 标记为黑色 / 深灰
    // =====================================================

    [MenuItem("GameObject/环境颜色/设为黑色", false, 21)]
    private static void SetDark()
    {
        EnvironmentColorManager manager =
            FindManager();

        if (manager == null)
            return;


        List<Renderer> selected =
            GetSelectedRenderers();


        Undo.RecordObject(
            manager,
            "Set Environment Dark"
        );


        int count = 0;


        foreach (Renderer r in selected)
        {
            // 如果原来在灰色列表
            // 先删除
            manager.grayRenderers.Remove(r);


            // 加入黑色列表
            if (!manager.darkRenderers.Contains(r))
            {
                manager.darkRenderers.Add(r);

                count++;
            }
        }


        EditorUtility.SetDirty(manager);


        Debug.Log(
            "已把 " +
            count +
            " 个环境 Renderer 标记为黑色。"
        );
    }


    // =====================================================
    // 恢复成默认白色
    // =====================================================

    [MenuItem("GameObject/环境颜色/设为默认白色", false, 22)]
    private static void SetWhite()
    {
        EnvironmentColorManager manager =
            FindManager();

        if (manager == null)
            return;


        List<Renderer> selected =
            GetSelectedRenderers();


        Undo.RecordObject(
            manager,
            "Set Environment White"
        );


        int count = 0;


        foreach (Renderer r in selected)
        {
            bool changed = false;


            if (manager.grayRenderers.Remove(r))
                changed = true;


            if (manager.darkRenderers.Remove(r))
                changed = true;


            if (changed)
                count++;
        }


        EditorUtility.SetDirty(manager);


        Debug.Log(
            "已把 " +
            count +
            " 个 Renderer 改回默认白色。"
        );
    }
}

#endif