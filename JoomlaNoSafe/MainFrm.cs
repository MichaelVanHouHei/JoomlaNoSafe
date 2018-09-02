#region

using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using HttpCodeLib;
using MetroFramework.Forms;

#endregion

namespace JoomlaNoSafe
{
    public partial class MainFrm : MetroForm
    {
        public joomlaFucker fucker = new joomlaFucker();

        public MainFrm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var size = Size;
            Resize += (fuck, shit) => { Size = size; };
            ThreadPool.SetMinThreads(20, 20);
            // CheckForIllegalCrossThreadCalls = false;
            ServicePointManager.DefaultConnectionLimit = 1024;
            fucker.loadExps();
            SettingGrid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.Fill);
            SettingPage.Text = "Setting(" + fucker.exps.Count + ")";
            SettingGrid.DataSource = fucker.exps;
        }

        private void metroTextButton1_Click(object sender, EventArgs e)
        {
            StatusBox.Clear();
            var url = singeUrlBox.Text;
            //   var s= new XJHTTP().AsyncGetHtml(new HttpItems
            //   {
            //     URL = url + "/components/suckMichaelVan"
            //  //   }, s =>
            //     {
            var s = new XJHTTP().GetHtml(url + "/components/suckMichaelVan");
            if (!string.IsNullOrEmpty(s.Html))
            {
                foreach (var exp in fucker.exps)
                {
                    fucker.Scan(url, exp.comName, s.Html,
                        b =>
                        {
                            BeginInvoke(new Action(() =>
                            {
                                StatusBox.AppendText("Scaning:" + exp.comName + Environment.NewLine);
                                if (b)
                                {
                                    //   this.BeginInvoke(new Action(() => {
                                    resultGrid.Rows.Add(exp.comName, exp.info,
                                        exp.link);
                                    StatusBox.AppendText("Found" + exp.comName +
                                                         "-" + exp.info +
                                                         Environment.NewLine);
                                    //    }));
                                }
                                else
                                {
                                    StatusBox.AppendText("Not Found" + exp.comName + Environment.NewLine);
                                }
                            }));
                        });
                }
            }
            else
            {
                StatusBox.AppendText("shit,cannot connect");
            }
            //   });
        }

        private void metroTextButton2_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                BeginInvoke(new Action(() => { metroTextButton2.Text = "正在更新"; }));

                fucker.updateExps();
                BeginInvoke(new Action(() => { metroTextButton2.Text = "更新exps"; }));
                ;
            });
        }
    }
}