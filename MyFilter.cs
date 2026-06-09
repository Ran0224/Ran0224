using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    /// <summary>
    /// 自定义视频滤镜实现
    /// </summary>
    public static class VideoFilterFactory
    {
        /// <summary>
        /// 根据类型创建视频效果滤镜
        /// </summary>
        public static IVideoEffect CreateFilter(VideoEffectType effectType)
        {
            switch (effectType)
            {
                case VideoEffectType.GhostEffect:
                    return new GhostEffectFilter();
                case VideoEffectType.CenterConcave:
                    return new CenterConcaveFilter();
                case VideoEffectType.CenterConvex:
                    return new CenterConvexFilter();
                case VideoEffectType.Emboss:
                    return new EmbossEffectFilter();
                case VideoEffectType.BeautyEffect:
                    return new BeautyEffectFilter();
                default:
                    return new NoEffectFilter();
            }
        }
    }

    /// <summary>
    /// 视频效果类型枚举
    /// </summary>
    public enum VideoEffectType
    {
        None = -1,
        GhostEffect = 0,
        CenterConcave = 1,
        CenterConvex = 2,
        Emboss = 3,
        BeautyEffect = 4
    }

    /// <summary>
    /// 视频效果基类接口
    /// </summary>
    public interface IVideoEffect
    {
        Bitmap Apply(Bitmap source);
    }

    /// <summary>
    /// 鬼影特效 - 运动模糊效果
    /// </summary>
    public class GhostEffectFilter : IVideoEffect
    {
        private const int BlurAmount = 10;
        private const int Angle = 45;

        public Bitmap Apply(Bitmap source)
        {
            try
            {
                Bitmap result = new Bitmap(source);
                MotionBlur filter = new MotionBlur(BlurAmount, Angle);
                result = filter.Apply(result);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("鬼影特效应用失败: " + ex.Message);
                return (Bitmap)source.Clone();
            }
        }
    }

    /// <summary>
    /// 中心内凹特效 - Wiener平滑滤镜
    /// </summary>
    public class CenterConcaveFilter : IVideoEffect
    {
        public Bitmap Apply(Bitmap source)
        {
            try
            {
                Bitmap result = new Bitmap(source);
                WienerFilter filter = new WienerFilter(3);
                result = filter.Apply(result);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("中心内凹特效应用失败: " + ex.Message);
                return (Bitmap)source.Clone();
            }
        }
    }

    /// <summary>
    /// 中心外凹特效 - 锐化滤镜
    /// </summary>
    public class CenterConvexFilter : IVideoEffect
    {
        public Bitmap Apply(Bitmap source)
        {
            try
            {
                Bitmap result = new Bitmap(source);
                SharpenFilter filter = new SharpenFilter();
                result = filter.Apply(result);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("中心外凹特效应用失败: " + ex.Message);
                return (Bitmap)source.Clone();
            }
        }
    }

    /// <summary>
    /// 浮雕特效 - 3D浮雕效果
    /// </summary>
    public class EmbossEffectFilter : IVideoEffect
    {
        public Bitmap Apply(Bitmap source)
        {
            try
            {
                Bitmap result = new Bitmap(source);
                EmbossFilter filter = new EmbossFilter();
                result = filter.Apply(result);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("浮雕特效应用失败: " + ex.Message);
                return (Bitmap)source.Clone();
            }
        }
    }

    /// <summary>
    /// 美艳特效 - 高斯模糊+亮度增强组合
    /// </summary>
    public class BeautyEffectFilter : IVideoEffect
    {
        private const double BlurRadius = 1.5;
        private const int BrightnessBoost = 20;

        public Bitmap Apply(Bitmap source)
        {
            try
            {
                Bitmap result = new Bitmap(source);
                
                // 应用高斯模糊
                GaussianBlur blurFilter = new GaussianBlur(BlurRadius);
                result = blurFilter.Apply(result);

                // 增强亮度
                BrightnessCorrection brightnessFilter = new BrightnessCorrection(BrightnessBoost);
                result = brightnessFilter.Apply(result);

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("美艳特效应用失败: " + ex.Message);
                return (Bitmap)source.Clone();
            }
        }
    }

    /// <summary>
    /// 无效果 - 返回原始图像
    /// </summary>
    public class NoEffectFilter : IVideoEffect
    {
        public Bitmap Apply(Bitmap source)
        {
            return (Bitmap)source.Clone();
        }
    }
}
