using MediaToolkit;
using MediaToolkit.Model;
using VideoLibrary;
using System.Runtime.InteropServices;

namespace YouTubeDownloader
{
    public partial class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        string folderLocation;
        string youTubeLink;
        bool locationSet = false;

        public Form1()
        {
            InitializeComponent();
            label2.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) 
            { 
                folderLocation = folderBrowserDialog1.SelectedPath;
                textBox2.Text = folderLocation;
                check_contents();

            }
            if (folderLocation != null)
            {
                locationSet = true;
            }
        }

        void xmpp_OnPresence()
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                if (listBox1.Items.Count >= 0)
                {
                    listBox1.Items.Clear();
                }
                }));
        }

        void addToList(string str)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.Items.Add(str);
                
            }));
        }

        void ScrollToBottom()
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.TopIndex = listBox1.Items.Count - 1;
                listBox1.SelectedIndex = listBox1.Items.Count - 1;

            }));
        }
        void ProgressBarToZero()
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                progressBar1.Value = 0;
            }));
        }
        void ProgressAppear()
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                progressBar1.Visible = true;
            }));
        }

        private void check_contents()
        {
            xmpp_OnPresence();

            DirectoryInfo dir = new DirectoryInfo(folderLocation);
            DirectoryInfo dir2 = new DirectoryInfo(folderLocation);

            FileInfo[] files;
            FileInfo[] files2;

            files = dir.GetFiles("*.mp3").OrderBy(f => f.LastWriteTime).ToArray();
            files2 = dir2.GetFiles("*.mp4").OrderBy(f => f.LastWriteTime).ToArray();

            FileInfo[] files3 = files.Concat(files2).ToArray().OrderBy(f => f.LastWriteTime).ToArray();

            foreach (FileInfo file in files3)
            {
                addToList(file.Name);
            }

            ScrollToBottom();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread thr1 = new Thread(ExecuteProcesses);
            thr1.Start();
        }

        private void ExecuteProcesses()
        {
            if (locationSet == true && youTubeLink != null)
            {
                // Download MP4
                YouTube youtube = YouTube.Default;
                Video vid = youtube.GetVideo(youTubeLink);
                VideoClient videoClient = new VideoClient();

                string vidFullName = vid.FullName.Substring(0, vid.FullName.Length - 4);

                if (File.Exists(folderLocation + "\\" + vidFullName + ".mp3") || File.Exists(folderLocation + "\\" + vidFullName + ".mp4"))
                {
                    MessageBox.Show(vidFullName + ", already exists");
                }
                else
                {
                    SetText("Downloading..");

                    File.WriteAllBytes(folderLocation + "\\" + vid.FullName, vid.GetBytes());

                    ProgressAppear();
                    ProgressBarToZero();
                    ProgressPlus();

                    if (File.Exists(folderLocation + "\\" + vid.FullName))
                    {
                        SetText("Downloaded");
                    }

                    // Convert to MP3
                    if (checkBox1.Checked)
                    {

                        string fileName = folderLocation + "\\" + vidFullName;

                        var inputFile = new MediaFile { Filename = folderLocation + "\\" + vid.FullName };
                        var outputFile = new MediaFile { Filename = $"{folderLocation + "\\" + vidFullName}.mp3" };

                        Thread.Sleep(1000);

                        SetText("Converting file...");

                        using (var engine = new Engine())
                        {
                            engine.GetMetadata(inputFile);
                            engine.Convert(inputFile, outputFile);
                        }

                        if (File.Exists(folderLocation + "\\" + vid.FullName))
                        {
                            File.Delete($"{folderLocation + "\\" + vid.FullName}");
                            SetText("File conversion successful!");
                        }

                        ProgressPlus();
                        if (progressBar1.Value == 100)
                        {
                            check_contents();
                        }
                    }
                    else if (checkBox2.Checked)
                    {
                        ProgressPlus();
                        check_contents();
                    }
                }
            }
        }

        private void ProgressPlus()
        {
            if (progressBar1.InvokeRequired)
                progressBar1.Invoke(new Action(ProgressPlus));
            else
                progressBar1.Value += 50;
        }

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label2.Text = text;
                check_contents();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            youTubeLink = textBox1.Text;
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                checkBox2.Checked = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                checkBox1.Checked = false;
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folderLocation = folderBrowserDialog1.SelectedPath;
                textBox2.Text = folderLocation;
            }
            if (folderLocation != null)
            {
                locationSet = true;
            }
            check_contents();
        }
    }
}