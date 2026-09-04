public static class HelpRecord
{
    // 掉文件的人
    public static bool HelpedMansuit = false;

    // 推车司机
    public static bool HelpedDriver = false;

    // 办公室老人
    public static bool HelpedOldman = false;


    // =====================================================
    // 新游戏开始时重置
    // =====================================================

    public static void ResetAll()
    {
        HelpedMansuit = false;

        HelpedDriver = false;

        HelpedOldman = false;
    }
}