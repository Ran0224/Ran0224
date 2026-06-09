using System;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private List<string> deviceList = new List<string>();
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice originalSource;
        private AsyncVideoSource transSource;
        private int frameRate = 20;//默认帧率
        private VideoFileWriter videoWriter = null;
        private bool createNewFile = true;
        private string videoFileName;
        private string videoPath = AppDomain.CurrentDomain.BaseDirectory + "Capture\\";


        public Form1()
        {
            InitializeComponent();
        }
        private void CameraConn()
        {

            originalSource = new VideoCaptureDevice(videoDevices[this.deviceListBox.SelectedIndex].MonikerString);
            this.videoSourcePlayer1.VideoSource = originalSource;
            originalSource.NewFrame += new NewFrameEventHandler(transform);
            transSource = new AsyncVideoSource(originalSource);
            this.videoSourcePlayer2.VideoSource = transSource;
            transSource.Start();
            videoSourcePlayer2.Start();
        }
        private void transform(object sender, NewFrameEventArgs eventArgs)
        {

        }

        private bool stopREC = true; // 控制录像状态

        private void captureVedio(object sender, NewFrameEventArgs eventArgs)
        {
            if (stopREC) return; // 停止录像时直接返回

            Bitmap image = eventArgs.Frame;

            
            using (Graphics g = Graphics.FromImage(image))
            using (SolidBrush drawBrush = new SolidBrush(Color.Yellow))
            using (Font drawFont = new Font("Arial", 6, FontStyle.Bold, GraphicsUnit.Millimeter))
            {
                int xPos = 15; // 简化计算
                int yPos = 10;
                string drawDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                g.DrawString(drawDate, drawFont, drawBrush, xPos, yPos);
            }

            // 2. 创建保存目录
            if (!Directory.Exists(videoPath))
                Directory.CreateDirectory(videoPath);

            // 3. 录像写入
            if (createNewFile) // 第一帧
            {
                string videoFileName = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".avi";
                string videoFileFullPath = Path.Combine(videoPath, videoFileName); // 用Path.Combine更安全
                createNewFile = false;

                // 释放之前的写入器
                if (videoWriter != null)
                {
                    videoWriter.Close();
                    videoWriter.Dispose();
                }

                videoWriter = new VideoFileWriter();
                videoWriter.Open(videoFileFullPath, image.Width, image.Height, frameRate, VideoCodec.MPEG4);
                videoWriter.WriteVideoFrame(image);
            }
            else // 后续帧
            {
                videoWriter.WriteVideoFrame(image);
            }
        }

        private bool isRecording = false;         // 是否正在录制
        private bool isCaptureVedioEventBind = false; // 保证事件不会被多次绑定

        private void button4_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                // 开始录制
                stopREC = false;
                createNewFile = true; // 新的录像文件
                button4.Text = "停止录像";

                // 只绑定一次事件，防止多绑异常
                if (originalSource != null && !isCaptureVedioEventBind)
                {
                    originalSource.NewFrame += captureVedio;
                    isCaptureVedioEventBind = true;
                }
                isRecording = true;
            }
            else
            {
                // 停止录制
                stopREC = true;
                button4.Text = "录制视频";

                // 解绑事件，避���持续写入
                if (originalSource != null && isCaptureVedioEventBind)
                {
                    originalSource.NewFrame -= captureVedio;
                    isCaptureVedioEventBind = false;
                }

                // 关闭写入器
                if (videoWriter != null)
                {
                    videoWriter.Close();
                    videoWriter.Dispose();
                    videoWriter = null;
                }
                isRecording = false;
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                // 开始录制
                stopREC = false;
                createNewFile = true; // 新的录像文件
                button4.Text = "停止录像";

                // 只绑定一次事件，防止多绑异常
                if (originalSource != null && !isCaptureVedioEventBind)
                {
                    originalSource.NewFrame += captureVedio;
                    isCaptureVedioEventBind = true;
                }
                isRecording = true;
            }
            else
            {
                // 停止录制
                stopREC = true;
                button4.Text = "录制视频";

                // 解绑事件，避���持续写入
                if (originalSource != null && isCaptureVedioEventBind)
                {
                    originalSource.NewFrame -= captureVedio;
                    isCaptureVedioEventBind = false;
                }

                // 关闭写入器
                if (videoWriter != null)
                {
                    videoWriter.Close();
                    videoWriter.Dispose();
                    videoWriter = null;
                }
                isRecording = false;
            }
        }

        // 截图
        private void capturePicture(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                //抓到图保存到指定路径
                string drawDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                if (!Directory.Exists(videoPath))
                    Directory.CreateDirectory(videoPath);

                string fileImageName = drawDate + ".bmp";
                Bitmap bmp = eventArgs.Frame;

                if (bmp == null)
                {
                    MessageBox.Show("捕获图像失败！", "提示");
                    return;
                }

                bmp.Save(videoPath + fileImageName, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                MessageBox.Show("捕获图像失败！" + ex.Message, "提示");
            }
            finally
            {
                transSource.NewFrame -= new NewFrameEventHandler(capturePicture); //结束截图
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 枚举所有视频输入设备
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo device in videoDevices)
                {
                    deviceList.Add(device.Name);
                }

                BindingSource bs = new BindingSource();
                bs.DataSource = this.deviceList;

                //deviceListBox.DataSource = bs;
                this.comboBox1.DataSource = bs;

                deviceListBox.SelectedIndex = 0;
                comboBox1.SelectedIndex = 0;

                CameraConn();
            }
            catch (ApplicationException)
            {
                deviceList.Add("No local capture devices");
                videoDevices = null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.videoSourcePlayer1.SignalToStop();
            this.originalSource.SignalToStop();

            this.videoSourcePlayer2.SignalToStop();
            this.transSource.SignalToStop();

            this.Dispose();
            System.Environment.Exit(0);
        }

       

        private void button3_Click(object sender, EventArgs e)
        {
            transSource.NewFrame += new NewFrameEventHandler(capturePicture); // 截图
        }

        // 捕获视频：启动摄像头
        

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (transSource != null)
                transSource.NewFrame += new NewFrameEventHandler(capturePicture);
        }
        private bool isPaused = false; // 标识当前是否已暂停
        private void button2_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                // 1. 停止预览
                if (videoSourcePlayer1.VideoSource != null)
                {
                    videoSourcePlayer1.SignalToStop();
                    videoSourcePlayer1.WaitForStop();
                    videoSourcePlayer1.VideoSource = null;
                }
                if (videoSourcePlayer2.VideoSource != null)
                {
                    videoSourcePlayer2.SignalToStop();
                    videoSourcePlayer2.WaitForStop();
                    videoSourcePlayer2.VideoSource = null;
                }
                // 2. 停止并释放流
                if (originalSource != null)
                {
                    originalSource.NewFrame -= new NewFrameEventHandler(transform);
                    originalSource.SignalToStop();
                    originalSource.WaitForStop();
                    originalSource = null;
                }
                if (transSource != null)
                {
                    transSource.SignalToStop();
                    transSource.WaitForStop();
                    transSource = null;
                }

                isPaused = true;
                button2.Text = "继续捕获";
            }
            else
            {
                // 3. 新建原始源
                originalSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
                originalSource.NewFrame += new NewFrameEventHandler(transform);
                // 4. 新建变换源
                transSource = new AsyncVideoSource(originalSource);

                // 5. 分配给两个播放器
                videoSourcePlayer1.VideoSource = originalSource;
                videoSourcePlayer2.VideoSource = transSource;

                // 6. 启动
                videoSourcePlayer1.Start();
                videoSourcePlayer2.Start();

                isPaused = false;
                button2.Text = "暂停捕获";
            }
        }

        private void deviceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
       
      
       
    }
}