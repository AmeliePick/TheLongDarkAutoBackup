using System;
using System.IO;
using System.Diagnostics;
using TheLongDarkAutoBackup.Utils;

namespace TheLongDarkAutoBackup
{
    class Saver
    {
        private string _backupFolder;
        private IniParser _iniParser;

        public Saver()
        {
            this._iniParser = new IniParser("Settings.ini");

            string backupPath = _iniParser.GetValue("Settings", "BackupsPath");
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);
        }

        private void FileCopy(string pathToSourceFile, string pathToDestinationFile)
        {
            FileStream fileStream = File.Open(pathToDestinationFile, FileMode.Create);

            byte[] array = File.ReadAllBytes(pathToSourceFile);
            fileStream.Write(array, 0, array.Length);
            fileStream.Close();
        }

        private string GetBackupFilePathBy(string sourceFilePath)
        {
            string backupFileName = "";
            for (int i = sourceFilePath.Length - 1; ; --i)
            {
                backupFileName += sourceFilePath[i];

                if (i - 1 >= 0 && sourceFilePath[i - 1] == '\\')
                    break;
            }

            char[] charArray = backupFileName.ToCharArray();
            Array.Reverse(charArray);
            backupFileName = _backupFolder + @"/" + new string(charArray);

            return backupFileName;
        }

        private void UpdateBackup()
        {

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine('\n' + DateTime.Now.ToString(), Console.ForegroundColor);

            _backupFolder = _iniParser.GetValue("Settings", "BackupsPath");
            bool backupFileNames = Convert.ToBoolean(Directory.GetFiles(_backupFolder).Length);
            string[] savesFiles = Directory.GetFiles(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Hinterland\TheLongDark");
            foreach (string fileName in savesFiles)
            {

                // Make a backup file name/path.
                string backupFileName = GetBackupFilePathBy(fileName);




                // Do backup when there is no backup files.
                if (backupFileNames == false && savesFiles.Length > 0)
                {
                    foreach (string copyingFile in savesFiles)
                    {
                        FileCopy(copyingFile, GetBackupFilePathBy(copyingFile));
                    }


                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("DONE: Files has been copied", Console.ForegroundColor);

                    backupFileNames = true;
                    break;
                }
                else // Do backup file if there is a new save file.
                {
                    FileInfo file = new FileInfo(fileName);
                    FileInfo backupFile = new FileInfo(backupFileName);

                    if (!backupFile.Exists)
                    {
                        FileCopy(fileName, backupFileName);

                        Console.WriteLine("DONE: " + backupFileName + " has been added to backup folder", Console.ForegroundColor);
                    }
                    else if (file.LastWriteTime.CompareTo(backupFile.LastWriteTime) > 0) // If the save file is newer than the backup.
                    {
                        FileCopy(fileName, backupFileName);

                        Console.WriteLine("DONE: " + backupFileName + " has been updated", Console.ForegroundColor);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("DONE: No changes found for " + backupFileName, Console.ForegroundColor);
                    }
                }
            }
        }

        private bool IsEPGSGame()
        {
            bool returnValue = false;
            string IsEPGS = _iniParser.GetValue("Settings", "IsEPGSOfficialGame");
            while(true)
            {
                try
                {
                    returnValue = bool.Parse(IsEPGS);
                    break;
                }
                catch
                {
                    try
                    {
                        returnValue = Convert.ToBoolean(int.Parse(IsEPGS));
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Is game from EpicGamesShitStore? Type [true/false/1/0]");
                        IsEPGS = Console.ReadLine();
                        continue;
                    }
                }
            }

            _iniParser.SetValue("Settings", "IsEPGSOfficialGame", IsEPGS);

            return returnValue;
        }


        public void Main()
        {
            Process gameProcess = new Process();
            ConsoleColor bgColor = Console.BackgroundColor;


            if (IsEPGSGame())
            {
                _iniParser.SetValue("Settings", "GamePath", Directory.GetCurrentDirectory() + @"\The Long Dark.url");

                Process.Start(new ProcessStartInfo(Directory.GetCurrentDirectory() + @"\The Long Dark.url"));
                while (true)
                {
                    try
                    {
                        gameProcess = Process.GetProcessesByName("tld")[0];
                        break;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(2000);
                    }
                }
            }     
            else if (!IsEPGSGame() && _iniParser.GetValue("Settings", "GamePath").Length <= 3)
            {
                Console.WriteLine("Specify the path to the game: ");
                string gamePath = Console.ReadLine();

                while (true)
                {
                    try
                    {
                        _iniParser.SetValue("Settings", "GamePath", gamePath);

                        Process.Start(new ProcessStartInfo(_iniParser.GetValue("Settings", "GamePath")));
                        while (true)
                        {
                            try
                            {
                                gameProcess = Process.GetProcessesByName("tld")[0];
                                break;
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(2000);
                            }
                        }

                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Wrong path to the game, try again: ");
                        gamePath = Console.ReadLine();
                    }
                } 
            }
            else
            {
                Process.Start(new ProcessStartInfo(_iniParser.GetValue("Settings", "GamePath")));

                while(true)
                {
                    try
                    {
                        gameProcess = Process.GetProcessesByName("tld")[0];
                        break;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(2000);
                    }
                } 
            }


            int AutoBackupTimer = 0;
            if (int.TryParse(_iniParser.GetValue("Settings", "AutoBackupTimer"), out AutoBackupTimer) == false)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[!]Incorrect value of AutoBackupTimer parameter[!]", Console.ForegroundColor);

                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[!]The game is working without auto backups. Fix the value and restart it.[!]", Console.ForegroundColor);
            }

            gameProcess.Exited += (object sender, EventArgs e) =>
            {
                UpdateBackup();
            };


            while (!gameProcess.HasExited)
            {
                UpdateBackup();

                int wakeUpTime = (DateTime.Now.Minute + AutoBackupTimer) % 60;
                while (DateTime.Now.Minute < wakeUpTime && !gameProcess.HasExited)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
