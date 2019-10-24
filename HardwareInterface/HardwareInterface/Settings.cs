using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace HardwareInterface
{
    public class Settings
    {
        public string ComPort { set; get; }
        public int Version { set; get; }
        public int DelayBetweenTwoPacket { set; get; }
        public int TryToSendPacket { set; get; }
        public int MaxSendListCapacity { set; get; }
       
    }
    public class SettingManager
    {
        #region PRIVATE
        private static SettingManager _instance;
        readonly int LastVersion = 1;
        private string setFile;
        #endregion

        public Settings Settings { get; private set; }

        private SettingManager()
        {
            
        }

        public static SettingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingManager();
                    _instance.Initial();
                }

                return _instance;
            }
        }

        private void Initial()
        {
            setFile = AppDomain.CurrentDomain.BaseDirectory + "\\settings.json";
            if (File.Exists(setFile))
            {
                try
                {
                    Settings = LoadSettings(setFile);
                    if (Settings == null)
                    {
                        Logger.Instance.AddError(this, MethodBase.GetCurrentMethod().Name, "LoadSettings returns null");
                        //TODO Logger add Error
                        MessageBox.Show("ERR01: Error while parsing the settings file", "Settings file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SetBasicSettings();
                        SaveSettings(setFile);
                    }

                    //TODO: Upgrade settings
                    if(Settings.Version != LastVersion)
                    {

                    }

                    Logger.Instance.AddInfo(this, MethodBase.GetCurrentMethod().Name, $"LoadSettings: {JsonConvert.SerializeObject(Settings, Formatting.None)}");
                }
                catch (Exception e)
                {
                    Logger.Instance.AddError(this, MethodBase.GetCurrentMethod().Name, $"Catch on LoadSettings:" + e.Message);
                    MessageBox.Show("ERR02: Error while parsing the settings file", "Settings file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetBasicSettings();
                    SaveSettings(setFile);
                }

                return;
            }

            Logger.Instance.AddError(this, MethodBase.GetCurrentMethod().Name, $"Load Basic settings");
            MessageBox.Show("ERR03: Setting file not exist, Making default settings file", "Settings file error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            SetBasicSettings();
            SaveSettings(setFile);
        }

        private Settings LoadSettings(string setFile)
        {
            StreamReader sr = new StreamReader(setFile);
            string sc = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();

            return JsonConvert.DeserializeObject<Settings>(sc);
        }

        private void SetBasicSettings()
        {
            Settings = new Settings()
            {
                Version = 1,
                DelayBetweenTwoPacket = 5,
                TryToSendPacket = 5,
                ComPort = "COM1",
                MaxSendListCapacity = 1000,
            };
        }

        private void SaveSettings(string setFile)
        {
            string settingContent = JsonConvert.SerializeObject(Settings, Formatting.Indented);

            StreamWriter sw = new StreamWriter(setFile);
            sw.Write(settingContent);
            sw.Flush();
            sw.Close();
        }

        public void Save()
        {
            SaveSettings(setFile);
        }
    }
}
