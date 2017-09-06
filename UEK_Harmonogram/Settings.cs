using System;

namespace UEK_Harmonogram
{
    [Flags]
    enum Setting : byte
    {
        None = 0x0,
        ShowAfterLaunch = 0x1,
        ShowPrevious = 0x2,
        ShowLanguageCourses = 0x4,
        ActivateLiveTile = 0x8,
        ActivateAutoSync = 0x10
    }

    static class Settings
    {
        public static Setting FlagsValue = KeysStorage.GetKey<Setting>("setting", (Setting)4);

        public static bool IsSetFlag(Setting setting)
        {
            return (FlagsValue & setting) == setting;
        }

        public static void SetFlagValue(bool isGiveFlag, Setting setting)
        {
            Settings.FlagsValue = isGiveFlag ? Settings.FlagsValue | setting : Settings.FlagsValue & ~setting;
        }
    }
}
