/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/RandomStrangers)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using RandomStrangers.Network;
using RandomStrangers.Tasks;

namespace RandomStrangers {
    /// <summary> Checks for and applies software updates. </summary>
    public static class Updater {
        
        public static string SourceURL = "https://github.com/RandomStrangers/RandomStrangers/";
        public const string BaseURL    = "https://github.com/RandomStrangers/RandomStrangers/blob/master/";
        public const string UploadsURL = "https://github.com/RandomStrangers/RandomStrangers/tree/master/Uploads";
        public const string UpdatesURL = "https://github.com/RandomStrangers/RandomStrangers/raw/master/Uploads/";
        public static string WikiURL = "https://github.com/UnknownShadow200/MCGalaxy";


        const string CurrentVersionURL = BaseURL + "Uploads/current_version.txt";
#if DEV_BUILD_RS
        const string dllURL = UpdatesURL + "RandomStrangers_Core.dll";
        const string guiURL = UpdatesURL + "RandomStrangers_CoreGUI.exe";
        // const string changelogURL = BaseURL + "Changelog.txt";
        // pointless since I don't really update the changelog...
        const string cliURL = UpdatesURL + "RandomStrangersCLI_Core.exe";
#else
        const string dllURL = UpdatesURL + "RandomStrangers_.dll";
        const string guiURL = UpdatesURL + "RandomStrangers.exe";
       // const string changelogURL = BaseURL + "Changelog.txt";
      // pointless since I don't really update the changelog...
        const string cliURL = UpdatesURL + "RandomStrangersCLI.exe";
#endif


        public static event EventHandler NewerVersionDetected;
        
        public static void UpdaterTask(SchedulerTask task) {
            UpdateCheck();
            task.Delay = TimeSpan.FromHours(2);
        }

        static void UpdateCheck() {
            if (!Server.Config.CheckForUpdates) return;
            WebClient client = HttpUtil.CreateWebClient();

            try {
                string latest = client.DownloadString(CurrentVersionURL);
                
                if (new Version(Server.Version) >= new Version(latest)) {
                    Logger.Log(LogType.SystemActivity, "No update found!");
                } else if (NewerVersionDetected != null) {
                    NewerVersionDetected(null, EventArgs.Empty);
                }
            } catch (Exception ex) {
                Logger.LogError("Error checking for updates", ex);
            }
            
            client.Dispose();
        }

        public static void PerformUpdate() {
            try {
                try {
                    DeleteFiles("Changelog.txt", "RandomStrangers_.update", "RandomStrangers.update", "RandomStrangersCLI.update",
#if DEV_BUILD_RS
                                                        "prev_RandomStrangers_Core.dll", "prev_RandomStrangers_CoreGUI.exe", "prev_RandomStrangersCLI_Core.exe");
#else
                    "prev_RandomStrangers_.dll", "prev_RandomStrangers.exe", "prev_RandomStrangersCLI.exe");
#endif
                } catch {
                }
                
                WebClient client = HttpUtil.CreateWebClient();
                client.DownloadFile(dllURL, "RandomStrangers_.update");
                client.DownloadFile(guiURL, "RandomStrangers.update");
                client.DownloadFile(cliURL, "RandomStrangersCLI.update");
                // client.DownloadFile(changelogURL, "Changelog.txt");
                // pointless since I don't really update the changelog...

                Level[] levels = LevelInfo.Loaded.Items;
                foreach (Level lvl in levels) {
                    if (!lvl.SaveChanges) continue;
                    lvl.Save();
                    lvl.SaveBlockDBChanges();
                }

                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players) pl.SaveStats();

                // Move current files to previous files (by moving instead of copying, 
                //  can overwrite original the files without breaking the server)
#if DEV_BUILD_RS
                AtomicIO.TryMove("RandomStrangers_.dll", "prev_RandomStrangers_Core.dll");
                AtomicIO.TryMove("RandomStrangers.exe", "prev_RandomStrangers_CoreGUI.exe");
                AtomicIO.TryMove("RandomStrangersCLI.exe", "prev_RandomStrangersCLI_Core.exe");
#else
                AtomicIO.TryMove("RandomStrangers_.dll", "prev_RandomStrangers_.dll");
                AtomicIO.TryMove("RandomStrangers.exe", "prev_RandomStrangers.exe");
                AtomicIO.TryMove("RandomStrangersCLI.exe", "prev_RandomStrangersCLI.exe");
#endif

                // Move update files to current files
                File.Move("RandomStrangers_.update", "RandomStrangers_.dll");
                File.Move("RandomStrangers.update",    "RandomStrangers.exe");
                File.Move("RandomStrangersCLI.update", "RandomStrangersCLI.exe");
                Server.Update(true, "Updating server.");
            } catch (Exception ex) {
                Logger.LogError("Error performing update", ex);
            }
        }

        static void DeleteFiles(params string[] paths) {
            foreach (string path in paths) { AtomicIO.TryDelete(path); }
        }
    }
}
