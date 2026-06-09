using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        #region Fields

        private List<string> deviceList = new List<string>();
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice originalSource;
        private AsyncVideoSource transSource;
        
        private const int DEFAULT_FRAME_RATE = 20;
        private const string CAPTURE_DIRECTORY = "Capture";
        
        private VideoFileWriter videoWriter;
        private string videoPath;
        
        private bool isRecording;
        private bool isPaused;
        private bool isCaptureBound;
        private bool shouldStopRecording = true;
        private bool createNewVideoFile = true;

        #endregion

        #region Constructor & Initialization

        public Form1()
        {
            InitializeComponent();
            videoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CAPTURE_DIRECTORY);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                InitializeVideoDevices();
                CameraConnect();
            }
            catch (ApplicationException ex)
            {
                deviceList.Add("No local capture devices");
                MessageBox.Show("未找到视频捕获设备！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeVideoDevices()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
                throw new ApplicationException("No video devices found");

            foreach (FilterInfo device in videoDevices)
            {
                deviceList.Add(device.Name);
            }

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = deviceList;
            comboBox1.DataSource = bindingSource;
            deviceListBox.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
        }

        #endregion

        #region Camera Connection & Control

        private void CameraConnect()
        {
            if (videoDevices == null || videoDevices.Count == 0)
                return;

            originalSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
            originalSource.NewFrame += OnOriginalFrameReceived;

            videoSourcePlayer1.VideoSource = originalSource;
            videoSourcePlayer1.Start();

            transSource = new AsyncVideoSource(originalSource);
            videoSourcePlayer2.VideoSource = transSource;
            transSource.Start();
            videoSourcePlayer2.Start();
        }

        private void OnOriginalFrameReceived(object sender, NewFrameEventArgs eventArgs)
        {
            // Apply video effects to the frame
            Bitmap processedFrame = ApplyVideoEffect(eventArgs.Frame);
            if (processedFrame != null && processedFrame != eventArgs.Frame)
            {
                eventArgs.Frame.Dispose();
                eventArgs.Frame = processedFrame;
            }
        }

        #endregion

        #region Video Effects

        private Bitmap ApplyVideoEffect(Bitmap frame)
        {
            if (frame == null)
                return null;

            int selectedIndex = deviceListBox.SelectedIndex;
            
            try
            {
                switch (selectedIndex)
                {
                    case 0: // Ghost effect (鬼影特效)
                        return ApplyGhostEffect(frame);
                    case 1: // Center concave effect (中心内凹特效)
                        return ApplyCenterConcaveEffect(frame);
                    case 2: // Center convex effect (中心外凹特效)
                        return ApplyCenterConvexEffect(frame);
                    case 3: // Emboss effect (浮雕特效)
                        return ApplyEmbossEffect(frame);
                    case 4: // Beauty effect (美艳特效)
                        return ApplyBeautyEffect(frame);
                    default:
                        return frame;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying effect: " + ex.Message);
                return frame;
            }
        }

        private Bitmap ApplyGhostEffect(Bitmap frame)
        {
            Bitmap result = new Bitmap(frame);
            try
            {
                MotionBlur filter = new MotionBlur(10, 45);
                result = filter.Apply(result);
            }
            catch
            {
                // 如果效果应用失败，返回克隆的原始图像
            }
            return result;
        }

        private Bitmap ApplyCenterConcaveEffect(Bitmap frame)
        {
            Bitmap result = new Bitmap(frame);
            try
            {
                WienerFilter filter = new WienerFilter(3);
                result = filter.Apply(result);
            }
            catch
            {
                // 如果效果应用失败，返回克隆的原始图像
            }
            return result;
        }

        private Bitmap ApplyCenterConvexEffect(Bitmap frame)
        {
            Bitmap result = new Bitmap(frame);
            try
            {
                SharpenFilter filter = new SharpenFilter();
                result = filter.Apply(result);
            }
            catch
            {
                // 如果效果应用失败，返回克隆的原始图像
            }
            return result;
        }

        private Bitmap ApplyEmbossEffect(Bitmap frame)
        {
            Bitmap result = new Bitmap(frame);
            try
            {
                EmbossFilter filter = new EmbossFilter();
                result = filter.Apply(result);
            }
            catch
            {
                // 如果效果应用失败，返回克隆的原始图像
            }
            return result;
        }

        private Bitmap ApplyBeautyEffect(Bitmap frame)
        {
            Bitmap result = new Bitmap(frame);
            try
            {
                GaussianBlur blur = new GaussianBlur(1.5);
                result = blur.Apply(result);

                BrightnessCorrection brightness = new BrightnessCorrection(20);
                result = brightness.Apply(result);
            }
            catch
            {
                // 如果效果应用失败，返回克隆的原始图像
            }
            return result;
        }

        #endregion

        #region Video Recording

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!isRecording)
                StartVideoRecording();
            else
                StopVideoRecording();
        }

        private void StartVideoRecording()
        {
            shouldStopRecording = false;
            createNewVideoFile = true;
            button4.Text = "停止录像";

            if (originalSource != null && !isCaptureBound)
            {
                originalSource.NewFrame += OnVideoFrameCapture;
                isCaptureBound = true;
            }

            isRecording = true;
        }

        private void StopVideoRecording()
        {
            shouldStopRecording = true;
            button4.Text = "录制视频";

            if (originalSource != null && isCaptureBound)
            {
                originalSource.NewFrame -= OnVideoFrameCapture;
                isCaptureBound = false;
            }

            CloseVideoWriter();
            isRecording = false;
        }

        private void OnVideoFrameCapture(object sender, NewFrameEventArgs eventArgs)
        {
            if (shouldStopRecording)
                return;

            Bitmap image = null;
            try
            {
                image = (Bitmap)eventArgs.Frame.Clone();

                // Add timestamp watermark
                DrawTimestampWatermark(image);

                // Create save directory if not exists
                if (!Directory.Exists(videoPath))
                    Directory.CreateDirectory(videoPath);

                // Write video frame
                if (createNewVideoFile)
                {
                    CreateNewVideoFile(image);
                    createNewVideoFile = false;
                }
                else
                {
                    if (videoWriter != null)
                        videoWriter.WriteVideoFrame(image);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error capturing video frame: " + ex.Message);
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        private void DrawTimestampWatermark(Bitmap image)
        {
            Graphics g = null;
            SolidBrush brush = null;
            Font font = null;
            
            try
            {
                g = Graphics.FromImage(image);
                brush = new SolidBrush(Color.Yellow);
                font = new Font("Arial", 6, FontStyle.Bold, GraphicsUnit.Millimeter);
                
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                g.DrawString(timestamp, font, brush, 15, 10);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error drawing watermark: " + ex.Message);
            }
            finally
            {
                if (font != null)
                    font.Dispose();
                if (brush != null)
                    brush.Dispose();
                if (g != null)
                    g.Dispose();
            }
        }

        private void CreateNewVideoFile(Bitmap firstFrame)
        {
            CloseVideoWriter();

            string fileName = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".avi";
            string filePath = Path.Combine(videoPath, fileName);

            videoWriter = new VideoFileWriter();
            videoWriter.Open(filePath, firstFrame.Width, firstFrame.Height, DEFAULT_FRAME_RATE, VideoCodec.MPEG4);
            videoWriter.WriteVideoFrame(firstFrame);
        }

        private void CloseVideoWriter()
        {
            if (videoWriter != null)
            {
                try
                {
                    videoWriter.Close();
                    videoWriter.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error closing video writer: " + ex.Message);
                }
                finally
                {
                    videoWriter = null;
                }
            }
        }

        #endregion

        #region Screenshot

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (transSource != null)
                transSource.NewFrame += OnScreenCapture;
        }

        private void OnScreenCapture(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                if (!Directory.Exists(videoPath))
                    Directory.CreateDirectory(videoPath);

                Bitmap frame = eventArgs.Frame;
                if (frame == null)
                {
                    MessageBox.Show("捕获图像失败！", "提示");
                    return;
                }

                string fileName = timestamp + ".bmp";
                string filePath = Path.Combine(videoPath, fileName);
                frame.Save(filePath, ImageFormat.Bmp);

                MessageBox.Show("截图已保存: " + fileName, "成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("捕获图像失败！" + ex.Message, "错误");
            }
            finally
            {
                if (transSource != null)
                    transSource.NewFrame -= OnScreenCapture;
            }
        }

        #endregion

        #region Pause/Resume

        private void button2_Click(object sender, EventArgs e)
        {
            if (!isPaused)
                PauseCapture();
            else
                ResumeCapture();
        }

        private void PauseCapture()
        {
            StopVideoSources();
            isPaused = true;
            button2.Text = "继续捕获";
        }

        private void ResumeCapture()
        {
            StartVideoSources();
            isPaused = false;
            button2.Text = "暂停捕获";
        }

        private void StopVideoSources()
        {
            try
            {
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

                if (originalSource != null)
                {
                    originalSource.NewFrame -= OnOriginalFrameReceived;
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error stopping video sources: " + ex.Message);
            }
        }

        private void StartVideoSources()
        {
            if (videoDevices == null || videoDevices.Count == 0)
                return;

            try
            {
                originalSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
                originalSource.NewFrame += OnOriginalFrameReceived;

                transSource = new AsyncVideoSource(originalSource);

                videoSourcePlayer1.VideoSource = originalSource;
                videoSourcePlayer2.VideoSource = transSource;

                videoSourcePlayer1.Start();
                videoSourcePlayer2.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("恢复捕获失败: " + ex.Message, "错误");
            }
        }

        #endregion

        #region Cleanup

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cleanup();
        }

        private void Cleanup()
        {
            try
            {
                StopVideoSources();
                CloseVideoWriter();

                if (videoDevices != null)
                    videoDevices.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error during cleanup: " + ex.Message);
            }
        }

        #endregion

        #region Event Handlers

        private void deviceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Video effect will be applied in real-time when index changes
        }

        #endregion
    }
}
