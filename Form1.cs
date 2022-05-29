using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace clipboardhistory
{
    public partial class Form1 : Form
    {
        private static SettingsInit _globalData;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 2000;
            timer1.Start();

            ext.AddClipboardFormatListener(this.Handle);

            _globalData = new SettingsInit();
            await _globalData.InitializeAsync();

            checkBox1.Checked = SettingsInit.Config.topmost;
            checkBox2.Checked = SettingsInit.Config.toast;

            button1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            button2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);

            checkBox1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
            checkBox2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);

            listBox1.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                JsonUpdate("topmost", checkBox1);
                JsonUpdate("toast", checkBox2);
            }
            base.OnFormClosing(e);

            ext.RemoveClipboardFormatListener(this.Handle);
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            await JsonUpdate("topmost", checkBox1);
            await JsonUpdate("toast", checkBox2);
            SettingsInit.Config.topmost = checkBox1.Checked;
            SettingsInit.Config.toast = checkBox2.Checked;

            TopMost = SettingsInit.Config.topmost;

            if (checkExist(GetClipboardDatatype()))
            {
                listBox1.Items.Add(GetClipboardDatatype());
                if (SettingsInit.Config.toast)
                    new ToastContentBuilder()
                        .AddText(GetClipboardDatatype())
                        .Show();
            }
        }

        public bool checkExist(string v)
        {
            foreach (var listBoxItem in listBox1.Items)
                if (string.Equals((string)listBoxItem, v, StringComparison.Ordinal)) return false;
            return true;
        }

        private Task JsonUpdate(string value, CheckBox cb)
        {
            string jsonString = File.ReadAllText("Config.json");
            JObject jObject = JsonConvert.DeserializeObject(jsonString) as JObject;
            JToken jToken = jObject.SelectToken(value);
            jToken.Replace(cb.Checked);
            string updatedJsonString = jObject.ToString();
            File.WriteAllText("Config.json", updatedJsonString);
            return Task.CompletedTask;
        }

        // get clipboard data type
        private string GetClipboardDatatype()
        {
            var data = "";
            if (Clipboard.ContainsText())
            {
                data = Clipboard.GetText();
            }
            else if (Clipboard.ContainsImage())
            {
                data = "Image";
            }
            else if (Clipboard.ContainsFileDropList())
            {
                data = Clipboard.GetFileDropList()[0].Replace(@"\", "\\");
            }
            else if (Clipboard.ContainsAudio())
            {
                data = "Audio";
            }
            else if (Clipboard.ContainsData(DataFormats.Rtf))
            {
                data = "RTF";
            }
            else if (Clipboard.ContainsData(DataFormats.CommaSeparatedValue))
            {
                data = "CSV";
            }
            else if (Clipboard.ContainsData(DataFormats.Html))
            {
                data = "HTML";
            }
            else if (Clipboard.ContainsData(DataFormats.Dib))
            {
                data = "DIB";
            }
            else if (Clipboard.ContainsData(DataFormats.Dif))
            {
                data = "DIF";
            }
            else if (Clipboard.ContainsData(DataFormats.EnhancedMetafile))
            {
                data = "EMF";
            }
            else if (Clipboard.ContainsData(DataFormats.FileDrop))
            {
                data = Clipboard.GetFileDropList()[0].Replace(@"\", @"\\");
            }
            else if (Clipboard.ContainsData(DataFormats.Locale))
            {
                data = "Loc";
            }
            return data;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && listBox1.SelectedItem != null)
                Clipboard.SetText((string)listBox1.SelectedItem);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var line = "\n------------------------------------------------------------------------------------------------------------\n";
            foreach (var listBoxItem in listBox1.Items)
                File.AppendAllText(DateTime.Now.ToString("MM_dd_yyyy") + "_Save.txt", line + listBoxItem.ToString() + line + "\n");
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label2.Text.Replace("Location: ", ""));
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label1.Text.Replace("Length: ", ""));
        }

        // TEST ----------------------------------------------

        public void ReadMemory(IntPtr loca)
        {
            string ProcessName = "clipboardhistory";
            Process proc = Process.GetProcessesByName(ProcessName)[0];
            var hProc = mem.OpenProcess(mem.ProcessAccessFlags.All, false, proc.Id);
            var read = mem.GetModuleBaseAddress(proc.Id, $"{ProcessName}.exe");
            var blap = mem.FindDMAAddy(hProc, (IntPtr)(read + (int)loca), new int[] { 1337 });
            Debug.WriteLine("Last Error: " + Marshal.GetLastWin32Error());
            Debug.WriteLine("Something address " + "0x" + blap.ToString("X"));
        }
    }
}