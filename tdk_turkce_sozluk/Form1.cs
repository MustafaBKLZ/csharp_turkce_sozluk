using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using HtmlAgilityPack;

namespace tdk_turkce_sozluk
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var appName = Process.GetCurrentProcess().ProcessName + ".exe";
            SetIE8KeyforWebBrowserControl(appName);
            webBrowser1.ScriptErrorsSuppressed = true;
            string url = "https://sozluk.gov.tr/";
            this.webBrowser1.Navigate(url);
            await PageLoad(1);
        }

        private async void btn_ara_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.GetElementById("tdk-srch-input").SetAttribute("value", txt_ara.Text);
            webBrowser1.Document.GetElementById("tdk-search-btn").InvokeMember("Click");
            await PageLoad(2); // aramanın yüklenmesini bekliyoruz.
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(webBrowser1.DocumentText);
            HtmlElement html_maddeler = webBrowser1.Document.GetElementById("maddeler0");
            try
            {
                textBox1.Text = (html_maddeler.InnerText).Replace(@"GÜNCEL TÜRKÇE SÖZLÜK", "").Replace("Birleşik Kelimeler", "");
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("TDK'da böyle bir kelime yer almamaktadır.", "Kelime Yok!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private async Task PageLoad(int TimeOut) // sayfayı bekletme
        {
            TaskCompletionSource<bool> PageLoaded = null;
            PageLoaded = new TaskCompletionSource<bool>();
            int TimeElapsed = 0;
            webBrowser1.DocumentCompleted += (s, e) =>
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                if (PageLoaded.Task.IsCompleted) return; PageLoaded.SetResult(true);
            };
            while (PageLoaded.Task.Status != TaskStatus.RanToCompletion)
            {
                await Task.Delay(10);
                TimeElapsed++;
                if (TimeElapsed >= TimeOut * 100) PageLoaded.TrySetResult(true);
            }
        }


        private void SetIE8KeyforWebBrowserControl(string appName) //web browser sürümü yükseltiyoruz.
        {
            RegistryKey Regkey = null;
            try
            {
                if (Environment.Is64BitOperatingSystem)
                    Regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
                else
                    Regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
                if (Regkey == null)
                {
                    return;
                }
                string FindAppkey = Convert.ToString(Regkey.GetValue(appName));
                if (FindAppkey == "8000")
                {
                    Regkey.Close();
                    return;
                }
                if (string.IsNullOrEmpty(FindAppkey))
                    Regkey.SetValue(appName, unchecked((int)0x1F40), RegistryValueKind.DWord);
                FindAppkey = Convert.ToString(Regkey.GetValue(appName));
            }
            catch (Exception ex) { }
            finally
            {
                if (Regkey != null) Regkey.Close();
            }
        }

    }
}
