using System;
using System.Diagnostics;

namespace EldenRingBackup // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            string applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string eldenRingPath = Path.Combine(applicationData, "EldenRing");

            if (Directory.Exists(eldenRingPath))
            {
                var task = StartBackup(eldenRingPath);
                task.Start();

                var taskWithProcess = StartProcessBackup(eldenRingPath);
                taskWithProcess.Start();
            }
            else
            {
                Console.WriteLine("未找到EldenRing备份目录");
            }

            Console.ReadKey();
        }


        private static async Task<bool> StartProcessBackup(string path)
        {
            Console.WriteLine($"开始检测游戏关闭...");
            while (true)
            {
                var processArray = Process.GetProcessesByName("eldenring.exe");
                if (processArray.Length > 0)
                {
                    Console.WriteLine($"检测到游戏进程[eldenring.exe]，等待结束...");
                    var process = processArray.First();
                    process.WaitForExit();
                    Console.WriteLine($"游戏已结束，进行备份...");
                    BackupDir(path);
                    UpdateWrite(path);
                }
                Thread.Sleep(10 * 1000);
            }
            return true;
        }


        private static async Task<bool> StartBackup(string path)
        {
            Console.WriteLine($"开始检测文件夹更改 {path}");
            while (true)
            {
                if (CheckWrite(path))
                {
                    Console.WriteLine("文件夹已更改，开始备份");
                    BackupDir(path);
                    UpdateWrite(path);
                }
                Thread.Sleep(3 * 60 * 1000);
            }
            return true;
        }

        /// <summary>
        /// 检查MD5是否需要备份
        /// </summary>
        /// <param name="path"></param>
        private static bool CheckWrite(string path)
        {
            var hashName = EncryptionHelper.GetMD5(path) + "-" + Path.GetFileName(path);
            if (File.Exists(hashName))
            {
                //读入
                var fileInfo = new DirectoryInfo(path);
                var time = fileInfo.LastWriteTime.ToString("yyyy-MM-dd-HH-mm-ss");
                //Console.WriteLine(time);

                if (File.ReadAllText(hashName) != time)
                {
                    return true;
                }

            }
            else
            {
                //生成
                return true;
            }
            return false;
        }

        /// <summary>
        /// 更新备份时间
        /// </summary>
        /// <param name="path"></param>
        private static void UpdateWrite(string path)
        {
            var hashName = EncryptionHelper.GetMD5(path) + "-" + Path.GetFileName(path);
            var fileInfo = new DirectoryInfo(path);
            var time = fileInfo.LastWriteTime.ToString("yyyy-MM-dd-HH-mm-ss");
            Console.WriteLine("最后备份时间" + time);
            File.WriteAllText(hashName, time);
        }

        private static void BackupDir(string path)
        {
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "save") ;

            string timeStr = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            string newPath = Path.Combine(savePath, Path.GetFileName(path) + "-" + timeStr);


            Console.WriteLine("备份到:" + newPath);


            DirectoryCopy(path, newPath, true);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

    }

}


