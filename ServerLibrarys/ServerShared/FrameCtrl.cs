﻿using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// 10秒计时器
        /// </summary>
        int _statTime = 0;
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
        DateTime _oneSecStartStamp;
        /// <summary>
        /// 用来标记 1秒钟帧数
        /// </summary>
        private double _oneSecFrames;
        /// <summary>
        /// 用来标记 1秒钟睡眠数
        /// </summary>
        private double _oneSecSleepTimes;
        /// <summary>
        /// 平均每秒帧数
        /// </summary>
        private double _averageFramesPerSecond=0;
        /// <summary>
        /// 平均每秒睡眠数
        /// </summary>
        private double _averageSleepTimesPerSecond=0;
        /// <summary>
        /// 内存使用情况
        /// </summary>
        private long _memoryUse=0;
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            _now = DateTime.Now;
            _milliSecondPerFrame = 0;
            _frameBeginTimeStamp = _now;
            _frameEndTimeStamp = _now;
            _lastFrameEndStamp = _now;
            _nextFrameBeginStamp = _now;
            _statTime = 0;
            _statFrames = 0;
            _statSleepTimes = 0;
            _oneSecStartStamp = _now;
            _oneSecFrames = 0;
            _oneSecSleepTimes = 0;
            _averageFramesPerSecond = 0;
            _averageSleepTimesPerSecond = 0;
            _memoryUse = 0;
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
                now = DateTime.Now;
                Thread.Sleep(1);
            }
            _frameBeginTimeStamp = now;
            _oneSecSleepTimes += (now - _lastFrameEndStamp).TotalMilliseconds;
            _oneSecFrames++;
        }
        /// <summary>
        /// 设置帧结束点
        /// </summary>
        public void SetFrameEnd()
        {
            DateTime now = DateTime.Now;
            _frameEndTimeStamp = now;
            _lastFrameEndStamp = now;
            _nextFrameBeginStamp = _frameEndTimeStamp.AddMilliseconds(_milliSecondPerFrame - (_frameEndTimeStamp - _frameBeginTimeStamp).TotalMilliseconds);
            if ((_frameEndTimeStamp-_oneSecStartStamp).TotalSeconds > 1)
            {
                RecordFrameStatPerSec((int)_oneSecFrames, (int)_oneSecSleepTimes);
                _oneSecFrames = 0;
                _oneSecSleepTimes = 0;
                _oneSecStartStamp = now;
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
        /// <summary>
        /// 获取当前平均帧数
        /// </summary>
        public double GetFPS()
        {
            return _averageFramesPerSecond; 
        }
        /// <summary>
        /// 获取当前平均睡眠时间
        /// </summary>
        public double GetSleepTime()
        {
            return _averageSleepTimesPerSecond;
        }
        /// <summary>
        /// 获取当前内存使用
        /// </summary>
        public double GetMemorySize()
        {
            return _memoryUse;
        }
        /// <summary>
        /// 统计最近10秒内的FPS和CPU睡眠时间等进程状态
        /// 这个函数1秒调用一次
        /// </summary>
        /// <param name="frameCount">1秒内的帧数</param>
        /// <param name="sleepTime">1秒内睡眠时间数</param>
        private void RecordFrameStatPerSec(int frameCount,int sleepTime)
        {
            if (_statTime<9)
            {
                _statFrames += frameCount;
                _statSleepTimes += sleepTime;
                _statTime++;
            }
            else
            {
                //从0 开始这里是到9了，正好10秒
                _averageFramesPerSecond = _statFrames / _statTime;
                _averageSleepTimesPerSecond = _statSleepTimes / _statTime;
                Process proc = Process.GetCurrentProcess();
                _memoryUse = (long)(proc.PrivateMemorySize64 / (1024 * 1024));
                LOG.Info("fps:{0},sleepTime:{1},memorySize:{2}", _averageFramesPerSecond, _averageSleepTimesPerSecond, _memoryUse);
                _statTime = 0;
                _statFrames = 0;
                _statSleepTimes = 0;
            }
        }
        /// <summary>
        /// 帧状态数据初始化
        /// </summary>
        public void ResetAverageFrameStat()
        {
            _averageFramesPerSecond = 0;
            _averageSleepTimesPerSecond = 0;
            _memoryUse = 0;
        }
    }
}
