using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TaisiyaClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        }

        private static BackgroundWorker worker = new BackgroundWorker();
        private String fileName = "";
        private Socket mainSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        private FileStream fileStream;

        private void createSocket()
        {
            mainSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            mainSocket.ReceiveTimeout = 5000;
            mainSocket.ReceiveTimeout = 5000;
        }

        //opening file
        private void button1_Click(object sender, EventArgs e)
        {
            progressStatus.Value = 0;
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileInfo file = new FileInfo(openFileDialog1.FileName);
                fileName = file.FullName;
                fileBox.Text = file.Name;
                progressStatus.Maximum = (int)file.Length;
                bytesSended.Text = "0/"+progressStatus.Maximum+" bytes";
                fileStream = file.OpenRead();
            }
        }

        //sending
        private void button2_Click(object sender, EventArgs e)
        {
            worker.RunWorkerAsync();
            progressStatus.Value = 0;
            test.Enabled = false;
        }

        private void test_Click(object sender, EventArgs e)
        {
            try
            {
                createSocket();
                mainSocket.Connect(serverAddr.Text, Int16.Parse(serverPort.Text));
                mainSocket.Send(Encoding.UTF8.GetBytes("test"));
                byte[] data = new byte[1024];
                int readed = mainSocket.Receive(data);
                if(Encoding.UTF8.GetString(data,0,readed) == "OK")
                {
                    MessageBox.Show("Server ready.");
                }
                mainSocket.Disconnect(false);
                status.Text = "Server ready.";
            }
            catch(SocketException)
            {
                MessageBox.Show("Server unaviable now.");
            }
            catch(Exception exception)
            {
                MessageBox.Show("Error:{0}",exception.Message);
            }
        }

        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker b = (BackgroundWorker)sender;
            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                byte[] data = new byte[1024];
                createSocket();
                mainSocket.Connect(serverAddr.Text, Int16.Parse(serverPort.Text));
                String header = fileBox.Text + "\n" + fileStream.Length.ToString();
                mainSocket.Send(Encoding.UTF8.GetBytes(header));
                int readed = mainSocket.Receive(data);
                if (Encoding.UTF8.GetString(data, 0, readed) == "OK")
                {
                    while ((readed = fileStream.Read(data, 0, 1024)) > 0)
                    {
                        mainSocket.Send(data, readed, SocketFlags.None);
                        b.ReportProgress((int)(readed));
                        //progressStatus.PerformStep();
                        //bytesSended.Text = progressStatus.Value + "/" + progressStatus.Maximum + " bytes";
                    }
                }
                readed = mainSocket.Receive(data);
                if (Encoding.UTF8.GetString(data, 0, readed) == "OK")
                {
                    MessageBox.Show("sended");
                }
                mainSocket.Close(4);
            }
            catch (SocketException exception)
            {
                MessageBox.Show(exception.Message, "Network error!");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Unexpected error!");
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressStatus.PerformStep();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            status.Text = "Finished!";
            test.Enabled = true;
        }
    }
}
