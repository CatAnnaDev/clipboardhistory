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
        }

        private async void timer1_Tick(object sender, EventArgs e)
        { 
            await JsonUpdate("topmost", checkBox1);
            await JsonUpdate("toast", checkBox2);
            SettingsInit.Config.topmost = checkBox1.Checked;
            SettingsInit.Config.toast = checkBox2.Checked;

            TopMost = SettingsInit.Config.topmost;

            if (checkExist(GetClipboardData().Replace("\0", ""))) { 
                listBox1.Items.Add(GetClipboardData().Replace("\0", ""));
                if (SettingsInit.Config.toast)
                    new ToastContentBuilder()
                        .AddText(GetClipboardData().Replace("\0", ""))
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

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalSize(IntPtr handle);
        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool CloseClipboard();
        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        enum TextEncode : uint
        {
            CF_TEXT = 1,
            CF_UNICODETEXT = 13, 
            CF_OEMTEXT = 7,
        }

        public string GetClipboardData()
        {
            OpenClipboard(IntPtr.Zero);
            IntPtr ClipboardDataPointer = GetClipboardData((uint)TextEncode.CF_TEXT); // fix é è à etc
            IntPtr Length = GlobalSize(ClipboardDataPointer);
            label1.Text = $"Length: {(int)Length}";
            IntPtr gLock = GlobalLock(ClipboardDataPointer);
            label2.Text = $"Location: {string.Format("{0:X8}", gLock)}";
            byte[] Buffer = new byte[(int)Length];
            Marshal.Copy(gLock, Buffer, 0, (int)Length);
            CloseClipboard();
            //ReadMemory(gLock); // TEST
            return Encoding.Default.GetString(Buffer);

        }

        public void SetClipboardData(string data)
        {
            OpenClipboard(IntPtr.Zero);
            var ptr = Marshal.StringToHGlobalUni(data);
            SetClipboardData((uint)TextEncode.CF_TEXT, ptr);
            CloseClipboard();
            Marshal.FreeHGlobal(ptr);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && listBox1.SelectedItem != null)
                Clipboard.SetText((string)listBox1.SelectedItem, TextDataFormat.UnicodeText);
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
                File.AppendAllText(DateTime.Now.ToString("MM_dd_yyyy") + "_Save.txt", line+ listBoxItem.ToString()+ line + "\n");
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label2.Text.Replace("Location: ", "" ));
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