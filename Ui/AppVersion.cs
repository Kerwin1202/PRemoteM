using Shawn.Utils;

namespace PRM
{
    public static class AppVersion
    {
        public const uint Major = 0;
        public const uint Minor = 7;
        public const uint Patch = 0;
        public const uint Build = 0;
        public const string PreRelease = "alpha.2"; // e.g. "alpha" "beta.2"

        public static readonly VersionHelper.Version VersionData = new VersionHelper.Version(Major, Minor, Patch, Build, PreRelease);
        public static string Version => VersionData.ToString();


        public static readonly string[] UpdateUrls =
        {
            "https://github.com/VShawn/PRemoteM",
            "https://gitee.com/vshawn/PRemoteM"
        };
    }
}