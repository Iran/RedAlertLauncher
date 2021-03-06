﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using Microsoft.Win32;
using System.Threading;
using Nyerguds.Ini;

namespace RedAlertLauncher
{
    public partial class Form1 : Form
    {
        public static IniFile RedAlertINI;
        public static String Path_;
        public static char seperator = System.IO.Path.DirectorySeparatorChar;
        public Form1()
        {
            InitializeComponent();
        }

        public static void Start_Application(string Filename, string Arguments)
        {
            string FilePath = Path_ + seperator + Filename;
            if (File.Exists(FilePath))
            {
                System.Diagnostics.Process.Start(FilePath, Arguments);
            }
            else
            {
                String ErrorMessage = String.Format("Can't launch as executable file '{0}' wasn't found in the game directory.", Filename);
                MessageBox.Show(ErrorMessage, "RedAlertLauncher Error");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Path_ = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Thread oThread = new Thread(new ThreadStart(Update_CnCNet_Player_Count));
            oThread.Start();

            but_LaunchNewMissions.Select();
            //            EnableCompatMode(); // Doesn't work for everyone

/*            if (File.Exists(Path_ + seperator + "edwin.exe") == false)
            {
                this.but_EDWINMapEditor.Visible = false;
                this.panel17.Visible = false;
                this.panel18.Visible = false;
            } */

//            if (File.Exists(Path_ + seperator + "raed.exe") == false)
//            {
                this.but_RAEDMapEditor.Visible = false;
                this.panel20.Visible = false;
                this.panel19.Visible = false;
//            }
        }

        private bool CheckForInternetConnection()
        {
            try
            {
                var client = new WebClient();
                client.Proxy = null;
                using (var stream = client.OpenRead("http://cncnet.org/live-status"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void Update_CnCNet_Player_Count()
        {
            if (!CheckForInternetConnection()) return;
            WebClient client = new WebClient();
            client.Proxy = null;

            Stream data = client.OpenRead("http://cncnet.org/live-status");
            StreamReader reader = new StreamReader(data);
            string xml = reader.ReadToEnd();

            data.Close();
            reader.Close();

            string[] words = xml.Split('"');

            int Index = -1;

            for (int i = 0; i < words.GetLength(0)-1; i++)
            {
                if (words[i] == "cncnet5")
                {
                    Index = i;
                    break;
                }
            }
            if (Index == -1) return;

//            MessageBox.Show(words[17]);
            String PlayerCount = words[Index + 1];
            PlayerCount = PlayerCount.Trim(new char[] { ':', ',' });

            String NewButtonText = String.Format("Online ({0} players)", PlayerCount);
            but_PlayOnline.Text = NewButtonText;
        }

        private void but_Configuration_Click(object sender, EventArgs e)
        {
            Start_Application("RedAlertConfig.exe", "");
        }

        private void But_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void but_PlayRedAlert_Click(object sender, EventArgs e)
        {
            Start_Application("ra95.exe", "");
            Application.Exit();
        }

        private void but_PlayOnline_Click(object sender, EventArgs e)
        {
            Start_Application("cncnet5.exe", "");
            Application.Exit();
        }

        private void but_OpenSkirmish_Click(object sender, EventArgs e)
        {
            Start_Application("cncnet5.exe", "-skirmish -noupdate");
            Application.Exit();
        }

        private void but_OpenLAN_Click(object sender, EventArgs e)
        {
            Start_Application("ra95.exe", "-LAN");
            Application.Exit();
        }

        private void but_LaunchAntMissions_Click(object sender, EventArgs e)
        {
            Start_Application("ra95.exe", "-ANTMISSIONS");
            Application.Exit();           
        }

        // Not used
        private void but_LaunchNewMissions_Click(object sender, EventArgs e)
        {
            Start_Application("ra95.exe", "-NEWMISSIONS");
            Application.Exit();
        }

        private void but_EDWINMapEditor_Click(object sender, EventArgs e)
        {
            MapEditorsForm form = new MapEditorsForm();
            form.ShowDialog();
            //Start_Application("edwin.exe", "");
            //Application.Exit();
        }

        private void but_RAEDMapEditor_Click(object sender, EventArgs e)
        {
            Start_Application("raed.exe", "");
            Application.Exit();
        }

        public enum WindowsVersion
        {
            UNKNOWN,
            WIN95,
            WIN98,
            WIN98SE,
            WINME,
            WINNT351,
            WINNT40,
            WIN2000,
            WINXP,
            WINVISTA,
            WIN7,
            WIN8
        }

        public static WindowsVersion getOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            WindowsVersion operatingSystem = WindowsVersion.UNKNOWN;

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        return WindowsVersion.WIN95;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            return WindowsVersion.WIN98SE;
                        else
                            return WindowsVersion.WIN98;
                    case 90:
                        return WindowsVersion.WINME;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        return WindowsVersion.WINNT351;
                    case 4:
                        return WindowsVersion.WINNT40;
                    case 5:
                        if (vs.Minor == 0)
                            return WindowsVersion.WIN2000;
                        else
                            return WindowsVersion.WINXP;
                    case 6:
                        switch (vs.Minor)
                        {
                            case 0:
                                return WindowsVersion.WINVISTA;
                            case 1:
                                return WindowsVersion.WIN7;
                            //case 2:
                            default:
                                return WindowsVersion.WIN8;
                        }
                    default:
                        break;
                }
            }
            return operatingSystem;
        }

        public const String COMPAT_REGISTRY = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        public const String COMPAT_XP = "WIN95";
        public const String COMPAT_VISTA = "WIN98 DISABLEDWM";
        public static String GAMEPATH { get { return Path.GetDirectoryName(Application.ExecutablePath) + @"\"; } }
        public const String GAME_EXE = "RA95.exe";

        public static void EnableCompatMode()
        {
            try
            {

                RegistryKey regKeyCompat = Registry.CurrentUser.CreateSubKey(COMPAT_REGISTRY);
                WindowsVersion winver = getOSInfo();
                String compat = "";
                switch (winver)
                {
                    case WindowsVersion.WINME:
                    case WindowsVersion.WINXP:
                        compat = COMPAT_XP;
                        break;
                    case WindowsVersion.WINVISTA:
                    case WindowsVersion.WIN7:
                    case WindowsVersion.WIN8:
                        compat = COMPAT_VISTA;
                        break;
                }
                regKeyCompat.SetValue(GAMEPATH + GAME_EXE, compat);
            }
            catch { } // ignore
        }
    }
}
