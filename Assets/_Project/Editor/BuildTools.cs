using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Animas.EditorTools
{
    // One click for the Friday demo build (issue #64): Ánimas → Build → Windows. It builds every scene in
    // Build Settings into Builds/Windows/AnimasMexicanas_<date>/ and zips that folder next to it, ready to
    // copy to another computer. Builds/ is ignored by git: builds are never committed.
    public static class BuildTools
    {
        [MenuItem("Ánimas/Build/Windows (demo)")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows", "AnimasMexicanas.exe");

        [MenuItem("Ánimas/Build/Linux")]
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Linux", "AnimasMexicanas.x86_64");

        static void Build(BuildTarget target, string platform, string executable)
        {
            string folder = Path.Combine("Builds", platform, $"AnimasMexicanas_{DateTime.Now:yyyy-MM-dd}");
            if (Directory.Exists(folder)) Directory.Delete(folder, true);

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(folder, executable),
                target = target,
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[Build] {platform}: {report.summary.result}, {report.summary.totalErrors} errores.");
                return;
            }

            // The folder Unity adds for crash debugging must not travel with the game.
            foreach (string dir in Directory.GetDirectories(folder, "*_DoNotShip")) Directory.Delete(dir, true);
            string zip = folder + ".zip";
            if (File.Exists(zip)) File.Delete(zip);
            ZipFile.CreateFromDirectory(folder, zip);

            Debug.Log($"[Build] {platform} listo: {zip} ({new FileInfo(zip).Length / 1048576} MB, " +
                      $"{report.summary.totalTime.TotalSeconds:0} s, {report.summary.totalWarnings} avisos).");
        }
    }
}
