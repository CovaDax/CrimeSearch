using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlServerCe;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Crime_Search
{
    public partial class CrimeSearch : Form
    {
        private Boolean isNight = false;
        private List<Statute> list = new List<Statute>();

        public CrimeSearch()
        {
            InitializeComponent();
            makeStatutes();
            list = sort();
            // To report progress from the background worker we need to set this property
            backgroundWorker1.WorkerReportsProgress = true;
            // This event will be raised on the worker thread when the worker starts
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            // This event will be raised when we call ReportProgress
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;
        }

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 100; i++)
            {
                // Report progress to 'UI' thread
                backgroundWorker1.ReportProgress(i);
            }
        }

        // Back on the 'UI' thread so we can update the progress bar
        void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // The progress percentage is a property of e
            progressBar1.Value = e.ProgressPercentage;
        }

        //When Program is initialized Automatically fills list with default order.
        private void CrimeSearch_Shown(object sender, EventArgs e)
        {
            // Start the background worker
            backgroundWorker1.RunWorkerAsync();
            updateList();
            list = sort();
            updateList();
            updateColor();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Make the program close when pressed
            Application.Exit();
        }

        private void savedFiles()
        {
            string[] fileEntries = Directory.GetFiles(Application.StartupPath + "\\Data\\");
            if (fileEntries.Length > 0)
            {
                foreach (string name in fileEntries)
                {
                    //Console.WriteLine(name);
                    Statute stat = ReadXML(name);
                    list.Add(stat);
                }
            }
        } 

        public void newFiles()
        {
            List<Statute> list = new List<Statute>();
            string[] fileEntries = null;
            fileEntries = Directory.GetFiles(Application.StartupPath + "\\Data\\");
            if (fileEntries.Length > 0)
            {
                foreach (string name in fileEntries)
                    list.Add(ReadXML(name));
            }

            fileEntries = Directory.GetFiles(Application.StartupPath + "\\Statutes\\");
            foreach (string fileName in fileEntries)
            {
                if (!fileName.EndsWith(".html"))
                {
                    continue;
                }
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                var document = new HtmlAgilityPack.HtmlDocument();
                doc.Load(new FileStream(fileName, FileMode.Open));
                string title = doc.DocumentNode.SelectSingleNode(@"//div[@class=""ChapterName""]").InnerText;
                string chapter = doc.DocumentNode.SelectSingleNode(@"//div[@class=""ChapterNumber""]").InnerText;
                chapter = chapter.Replace("CHAPTER ", "");
                int temp;
                Int32.TryParse(chapter, out temp);

                doc.LoadHtml(fileName);
                Statute statute = new Statute(fileName);
                statute.setChapter(temp);
                statute.setTitle(title);
                list.Add(statute);
            }
            Console.Write("Normal List");
            this.list = list;
        }

        //Create objects of statutes made from given html files stored on the disk
        public void makeStatutes()
        {
            string[] files = System.IO.Directory.GetFiles(Application.StartupPath + "\\Data\\");
            if (files.Length > 0)
                savedFiles();
            else
                newFiles();
        }

        private void updateList()
        {
            listView1.Items.Clear();
            foreach (Statute stat in list)
            {
                ListViewItem item = new ListViewItem((stat.getChapter().ToString()));
                item.SubItems.Add(stat.getTitle());
                if (stat.isFavorite())
                {
                    item.Checked = true;
                    if (isNight != true)
                    {
                        item.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        item.BackColor = Color.LimeGreen;
                        item.ForeColor = Color.Black;
                    }
                }
                listView1.Items.Add(item);
                listView1.Font = new Font("Times New Roman", 10.0f);
                listView1.Alignment = ListViewAlignment.Left;
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            list = sort();
            updateList();
        }

        private void updateStatutes()
        {

            HtmlWeb web = new HtmlWeb();
            int count = 1;
            while (true)
            {
                if (count > 1013)
                {
                    break;
                }
                HtmlAgilityPack.HtmlDocument doc;
                try
                {
                    string c = count.ToString().PadLeft(4, '0');

                    int toNearest = 100;
                    int value = count;
                    int rest = value % toNearest;
                    string high = (value - rest).ToString().PadLeft(4, '0');
                    string low = (count + (toNearest - rest - 1)).ToString().PadLeft(4, '0');
                    doc = web.Load("http://www.leg.state.fl.us/Statutes/index.cfm?App_mode=Display_Statute&URL=" + high + "-" + low + "/" + c + "/" + c + ".html");

                }
                catch (Exception e2)
                {
                    count++;
                    continue;
                }
                string path = Application.StartupPath + "\\Statutes\\ch" + count + ".html";
                string dashboard;
                try
                {
                    dashboard = doc.DocumentNode.SelectSingleNode("//div[@id='statutes']").InnerHtml;

                }
                catch (Exception e1)
                {
                    count++;
                    continue;
                }
                doc.LoadHtml(dashboard);
                doc.Save(path);
                count++;
            }
        }

        private List<Statute> sort()
        {
            List<Statute> SortedList;
            if (SearchBar.Text != "")
            {
                foreach (Statute stat in list)
                {
                    stat.setWordCount(SearchBar.Text.ToLower());
                }
                SortedList = list.OrderBy(i => !i.isFavorite()).ThenByDescending(x => x.getWordCount()).ThenBy(z => z.getChapter()).ToList();
            }
            else
            {
                SortedList = list.OrderBy(i => !i.isFavorite()).ThenBy(x => x.getChapter()).ToList();
            }
            return SortedList;
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            string title = null;
            Statute stat = null;
            if (listView1.SelectedItems.Count > 0)
            {
                title = listView1.SelectedItems[0].SubItems[1].Text;
                stat = list.Where(i => i.getTitle() == title).FirstOrDefault();
                stat.setFavorite(listView1.SelectedItems[0].Checked);
                list = sort();
                updateList();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = null;
            string title = null;
            Statute stat = null;
            if (listView1.SelectedItems.Count > 0)
            {
                title = listView1.SelectedItems[0].SubItems[1].Text;
                stat = list.Where(i => i.getTitle() == title).FirstOrDefault();
                string s = System.IO.File.ReadAllText(stat.getStatute());
                webBrowser1.DocumentText = System.IO.File.ReadAllText(stat.getStatute());
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            updateColor();
            highlightText();
        }

        private void night_Click(object sender, EventArgs e)
        {
            if (isNight)
                isNight = false;
            else
                isNight = true;

            updateColor();
            highlightText();

        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("You are about to update this programs statute files. This may take some time. Are you sure you want to continue?", "Update Statues", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                this.timer1.Start();
                updateStatutes();
                this.timer1.Stop();
            }
        }

        private void updateColor()
        {
            if (isNight)
            {
                this.BackColor = System.Drawing.Color.Black;
                listView1.BackColor = System.Drawing.ColorTranslator.FromHtml("#413E3E");
                listView1.ForeColor = System.Drawing.Color.LimeGreen;
                menuStrip3.BackColor = System.Drawing.ColorTranslator.FromHtml("#413E3E");
                menuStrip3.ForeColor = System.Drawing.Color.LimeGreen;
                if (webBrowser1.Document != null)
                {
                    webBrowser1.Document.BackColor = System.Drawing.ColorTranslator.FromHtml("#413E3E");
                    webBrowser1.Document.ForeColor = System.Drawing.Color.LimeGreen;
                }

                ListView.CheckedListViewItemCollection faves = listView1.CheckedItems;
                foreach (ListViewItem item in faves)
                {
                    item.ForeColor = Color.Black;
                    item.BackColor = Color.LimeGreen;
                }
            }
            else
            {
                this.BackColor = System.Drawing.Color.AliceBlue;
                listView1.BackColor = System.Drawing.Color.GhostWhite;
                listView1.ForeColor = System.Drawing.Color.Black;
                SearchButton.ForeColor = Color.Black;
                menuStrip3.BackColor = System.Drawing.Color.AliceBlue;
                menuStrip3.ForeColor = System.Drawing.Color.Black;
                if (webBrowser1.Document != null)
                {
                    webBrowser1.Document.BackColor = System.Drawing.Color.GhostWhite;
                    webBrowser1.Document.ForeColor = System.Drawing.Color.Blue;
                }
                ListView.CheckedListViewItemCollection faves = listView1.CheckedItems;
                foreach (ListViewItem item in faves)
                {
                    item.ForeColor = Color.Black;
                    item.BackColor = Color.LightBlue;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.progressBar1.Increment(1);
        }

        private void highlightText()
        {
            if (SearchBar.Text != "")
            {
                StringBuilder html = new StringBuilder(webBrowser1.Document.Body.OuterHtml);
                String substitution = null;
                if(!isNight)
                    substitution = "<span style='background-color: rgb(255, 255, 0);'>" + SearchBar.Text + "</span>";
                else
                    substitution = "<span style='background-color: rgb(153, 0, 0);'>" + SearchBar.Text + "</span>";
                html.Replace(SearchBar.Text, substitution);
                webBrowser1.Document.Body.InnerHtml = html.ToString();
            }
        }

        public static void WriteXML(Statute stat)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Statute));

            System.IO.StreamWriter file = new System.IO.StreamWriter(@Application.StartupPath + "\\Data\\ch"+stat.getChapter()+".xml");
            writer.Serialize(file, stat);
            file.Close();
        }

        public Statute ReadXML(string name)
        {
            Statute myObject;
            // Construct an instance of the XmlSerializer with the type
            // of object that is being deserialized.
            XmlSerializer mySerializer = 
            new XmlSerializer(typeof(Statute));
            // To read the file, create a FileStream.
            FileStream myFileStream = 
            new FileStream(name, FileMode.Open);
            // Call the Deserialize method and cast to the object type.
            myObject = (Statute)
            mySerializer.Deserialize(myFileStream);
            return myObject;
        }

        private void CrimeSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            SearchBar.Text = "";
            foreach (Statute stat in list)
            {
                WriteXML(stat);
            }
        }
   }
}

