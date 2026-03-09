using Microsoft.Win32;
using System;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LinCop
{
    public partial class startSetting : Form
    {
        public startSetting()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            PopulateListBox();
        }

        private void PopulateListBox()
        {
            listBox1.Items.Clear();
            using (var hkcr = Registry.ClassesRoot)
            {
                foreach (var name in hkcr.GetSubKeyNames())
                {
                    if (name.StartsWith("InDesign.Application.", StringComparison.OrdinalIgnoreCase))
                    {
                        var ver = name.Substring("InDesign.Application.".Length);
                        if (!listBox1.Items.Contains(ver)) listBox1.Items.Add(ver);
                    }
                }
            }
            // Sorted は false にしておく
            listBox1.Sorted = false;

            // 例：ListBox に入っている "2021","2022",... を数値として降順に並べ替える
            var sorted = listBox1.Items
                .Cast<string>()
                .OrderByDescending(s => {
                    int v;
                    return int.TryParse(s, out v) ? v : int.MinValue;
                })
                .ToArray();
            listBox1.Items.Clear();
            listBox1.Items.AddRange(sorted);
            listBox1.SelectedIndex = Properties.Settings.Default.ver;
        }

        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) LetsIndesign();
        }

        private void listBox1_Click(object sender, EventArgs e) => LetsIndesign();
        private void button1_Click(object sender, EventArgs e) => LetsIndesign();

        private void LetsIndesign()
        {
            if (listBox1.SelectedItem == null) { MessageBox.Show("バージョンを選択してください。"); return; }
            var ver = listBox1.SelectedItem.ToString();
            try
            {
                var progId = "InDesign.Application." + ver;
                var t = Type.GetTypeFromProgID(progId, false);
                if (t == null) { MessageBox.Show(progId + " は登録されていません。"); return; }
                Form1.tID = t;
                Form1.inddApp = Activator.CreateInstance(t, true);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("InDesign " + ver + " を起動できませんでした。\n" + ex.Message);
            }
            Properties.Settings.Default.ver = listBox1.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x112;
            const long SC_CLOSE = 0xF060L;
            if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt64() & 0xFFF0L) == SC_CLOSE)
            {
                Application.Exit();
            }
            base.WndProc(ref m);
        }
    }
}