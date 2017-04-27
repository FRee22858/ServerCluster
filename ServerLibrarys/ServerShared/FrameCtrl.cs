using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerShared
{
    public class FrameCtrl
    {
        /// <summary>
        /// FPS
        /// </summary>
        double fps = 40;
        /// <summary>
        /// 1秒 = 1000ms
        /// </summary>
        const double ONESECOND = 1000.0;
        /// <summary>
        /// 每帧耗时 单位ms
        /// </summary>
        double _milliSecondPerFrame = 0;
        /// <summary>
        /// 当前时间
        /// </summary>
        DateTime _now;
        /// <summary>
        /// 帧起点时间戳
        /// </summary>
        DateTime _frameBeginTimeStamp;
        /// <summary>
        /// 帧结束时间戳
        /// </summary>
        DateTime _frameEndTimeStamp;
        /// <summary>
        /// 前一帧结束时间戳
        /// </summary>
        DateTime _lastFrameEndStamp;
        /// <summary>
        /// 下一帧开始时间戳
        /// </summary>
        DateTime _nextFrameBeginStamp;
        /// <summary>
        /// 统计帧数
        /// </summary>
        double _statFrames = 0;
        /// <summary>
        /// 统计睡眠时间  单位ms
        /// </summary>
        double _statSleepTimes = 0;
        /// <summary>
        /// 用来标记 1秒钟开始
        /// </summary>
        DateTime _timeStartStamp;
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            _milliSecondPerFrame = ONESECOND / fps;
        }
        /// <summary>
        /// 设置帧开始点
        /// </summary>
        public void SetFrameBegin()
        {
            DateTime now = DateTime.Now;
            while (now<_nextFrameBeginStamp)
            {
                Thread.Sleep(1);
            }
            _frameBeginTimeStamp = now;
            _statSleepTimes += (now - _lastFrameEndStamp).TotalMilliseconds; 
            _statFrames++;
        }
        /// <summary>
        /// 设置帧结束点
        /// </summary>
        public void SetFrameEnd()
        {
            DateTime now = DateTime.Now;
            _frameEndTimeStamp = now;
            _lastFrameEndStamp = now;
            _nextFrameBeginStamp = _frameBeginTimeStamp.AddMilliseconds(_milliSecondPerFrame - (_frameEndTimeStamp - _frameBeginTimeStamp).TotalMilliseconds);
            if ((_frameEndTimeStamp-_timeStartStamp).TotalSeconds > 1)
            {
                LOG.Info("fps:{0},sleep time{1}", _statFrames, _statSleepTimes);
                _statFrames = 0;
                _statSleepTimes = 0;
            }
        }
        /// <summary>
        /// 设置FPS
        /// </summary>
        public void SetFPS(double fpsValue)
        {
            fps = fpsValue;
            _milliSecondPerFrame = ONESECOND / fps;
        }
    }
}
