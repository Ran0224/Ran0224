using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    /// <summary>
    /// 视频录制和捕获设置配置类
    /// </summary>
    public class VideoSettings
    {
        private int frameRate;
        private int videoWidth;
        private int videoHeight;
        private bool enableTimestampWatermark;
        private string captureDirectoryPath;
        private VideoEffectType defaultEffectType;

        /// <summary>
        /// 获取或设置视频帧率(fps)
        /// </summary>
        public int FrameRate
        {
            get { return frameRate; }
            set { frameRate = value; }
        }

        /// <summary>
        /// 获取或设置视频分辨率宽度
        /// </summary>
        public int VideoWidth
        {
            get { return videoWidth; }
            set { videoWidth = value; }
        }

        /// <summary>
        /// 获取或设置视频分辨率高度
        /// </summary>
        public int VideoHeight
        {
            get { return videoHeight; }
            set { videoHeight = value; }
        }

        /// <summary>
        /// 获取或设置是否启用时间戳水印
        /// </summary>
        public bool EnableTimestampWatermark
        {
            get { return enableTimestampWatermark; }
            set { enableTimestampWatermark = value; }
        }

        /// <summary>
        /// 获取或设置捕获目录路径
        /// </summary>
        public string CaptureDirectoryPath
        {
            get { return captureDirectoryPath; }
            set { captureDirectoryPath = value; }
        }

        /// <summary>
        /// 获取或设置默认视频效果类型
        /// </summary>
        public VideoEffectType DefaultEffectType
        {
            get { return defaultEffectType; }
            set { defaultEffectType = value; }
        }

        /// <summary>
        /// 构造函数 - 使用默认值初始化
        /// </summary>
        public VideoSettings()
        {
            frameRate = 20;
            videoWidth = 640;
            videoHeight = 480;
            enableTimestampWatermark = true;
            captureDirectoryPath = "Capture";
            defaultEffectType = VideoEffectType.None;
        }

        /// <summary>
        /// 验证设置是否有效
        /// </summary>
        public bool Validate()
        {
            if (frameRate < 1 || frameRate > 60)
                return false;

            if (videoWidth < 320 || videoWidth > 1920)
                return false;

            if (videoHeight < 240 || videoHeight > 1080)
                return false;

            if (string.IsNullOrEmpty(captureDirectoryPath))
                return false;

            return true;
        }

        /// <summary>
        /// 创建默认设置
        /// </summary>
        public static VideoSettings CreateDefault()
        {
            return new VideoSettings();
        }
    }
}
