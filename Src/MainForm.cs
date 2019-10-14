using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PineSaveEditor
{
    public partial class MainForm : Form
    {
        string currentFilePath = string.Empty;
        JObject currentFileData = null;
        bool saved = true;


        public MainForm()
        {
            InitializeComponent();

            UpdateTitle();

            openFileDialog.InitialDirectory = GetSavesDirectory();
        }


        private void UpdateTitle()
        {
            string title = "Pain Save Editor";

            if (!string.IsNullOrEmpty(currentFilePath))
            {
                title += " [" + currentFilePath + ']';
                if (!saved)
                    title += '*';
            }

            Text = title;
        }

        
        private string GetSavesDirectory()
        {
            // %UserProfile%\AppData\LocalLow
            // https://docs.microsoft.com/ru-ru/windows/win32/shell/knownfolderid
            return GetKnownFolderPath(new Guid("{A520A1A4-1780-4FF6-BD18-167343C5AF16}"))
                + @"\Twirlbound\Pine\saves";
        }

        
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = openFileDialog.FileName;
                ParseSaveFile();
                UpdateTitle();
            }
        }


        void ParseSaveFile()
        {
            byte[] bytes = File.ReadAllBytes(currentFilePath);

            // Сейв-файл состоит из двух строк или трех строк
            string[] contents = Encoding.UTF8.GetString(bytes)
                .Replace("\r\n", "\n")
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (contents.Length != 2 && contents.Length != 3)
                throw new Exception("Inorrect file");

            // Первая строка всегда "5"
            if (contents[0] != "5")
                throw new Exception("Inorrect file");

            // Вторая строка - превьюшка в меню загрузки игры (название текущего квеста и т.п.)
            // Информация в ней не важна, да и вообще эта строка может отсутствовать

            // В последней строке находятся все данные
            string data = contents[contents.Length - 1];

            // Парсим данные
            // Установка Newtonsoft.Json:
            //   https://www.youtube.com/watch?v=XssLaKDRV4Y
            //   https://www.softwaretestinghelp.com/create-json-objects-using-c/
            currentFileData = JObject.Parse(data);
        }

        // https://stackoverflow.com/questions/4494290/detect-the-location-of-appdata-locallow
        // https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/Interop/Windows/Shell32/Interop.SHGetKnownFolderPath.cs
        // https://msdn.microsoft.com/en-us/library/windows/desktop/bb762188.aspx
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = false, BestFitMapping = false, ExactSpelling = true)]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string ppszPath);

        
        // https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Environment.Win32.cs
        private static string GetKnownFolderPath(Guid folderGuid)
        {
            int hr = SHGetKnownFolderPath(folderGuid, 0, IntPtr.Zero, out string path);
            if (hr != 0) // Not S_OK
                return string.Empty;
            return path;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFileData == null)
                return;

            string contents = "5\n" + currentFileData.ToString(Formatting.None);
            File.WriteAllText(currentFilePath, contents);
            
            saved = true;
            UpdateTitle();
        }

        // Сферы Ихтионов = Amphiscus Orbs = Emblems (в инвентаре может быть несколько стаков)
        private void addEmblemsButton_Click(object sender, EventArgs e)
        {
            if (currentFileData == null)
                return;

            JArray inventory = (JArray)currentFileData["playerData"]["inventory"];
            // ...,{"id":{"value":296},"amount":70},...
            JObject newItem = new JObject();
            JObject id = new JObject();
            id["value"] = 296;
            newItem["id"] = id;
            newItem["amount"] = 200;
            inventory.Add(newItem);

            saved = false;
            UpdateTitle();
        }
    }
}
