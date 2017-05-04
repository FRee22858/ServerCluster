﻿using Logger;
using System;
using System.Collections.Generic;
namespace ServerShared
{
    public enum Mode
    {
        /// <summary>
        /// 手动
        /// </summary>
        Manual =0,
        /// <summary>
        /// 自动
        /// </summary>
        Auto =1,
    }
    public enum ServerState
    {
        DisConnect =0,
        Connect = 1,
        Starting,
        Started,
        Stopping,
        Stopped,
    }
    public abstract class AbstractServer
    {
        /// <summary>
        /// 开启方式
        /// </summary>
        private Mode _startMode;
        public Mode StartMode
        {
            get { return _startMode; }
            set { _startMode = value; }
        }
        /// <summary>
        /// 预存当前时间
        /// </summary>
        private static DateTime _now;

        public static DateTime Now
        {
            get { return AbstractServer._now; }
            set { AbstractServer._now = value; }
        }
        /// <summary>
        /// 服务器名称
        /// </summary>
        private string _serverName;
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        /// <summary>
        /// 运行标示
        /// </summary>
        bool _isRuning;

        public bool IsRuning
        {
            get { return _isRuning; }
            set { _isRuning = value; }
        }

        /// <summary>
        /// 帧数
        /// </summary>
        FrameCtrl _fps;

        public FrameCtrl Fps
        {
            get { return _fps; }
            set { _fps = value; }
        }
        /// <summary>
        /// 当前状态
        /// </summary>
        ServerState _state;

        public ServerState State
        {
            get { return _state; }
            set { _state = value; }
        }
        /// <summary>
        /// 服务器关闭时间
        /// </summary>
        DateTime _stoppingTime;

        public DateTime StoppingTime
        {
            get { return _stoppingTime; }
            set { _stoppingTime = value; }
        }
        public abstract void InitModule(string[] args);
        public abstract  void Exit();
        public abstract void Update();
        /// <summary>
        /// 处理GM命令 没有后台 临时用控制台输入替代
        /// </summary>
        public Queue<string> cmdList = new Queue<string>();
        public void ProcessInput()
        {
            try
            {
                string cmd = Console.ReadLine().ToLower().Trim();
                lock (cmdList)
                {
                    cmdList.Enqueue(cmd);
                }
            }
            catch (Exception e)
            {

                LOG.Error(e.ToString());
            }
        }
        public void InitLogger()
        {
           var logger = new ServerLogger.ServerLogger("c:/log/");
           logger.Init(ServerName, true, true, true, true);
           LOG.InitLogger(logger);
        }
        public void Init(string[] args)
        {
            InitModule(args);
        }
        public void Run()
        { 
            IsRuning = true;
            Fps = new FrameCtrl();
            Fps.Init(); 
            while (IsRuning)
            {
                Fps.SetFrameBegin();
                Now = DateTime.Now;
                Update();
                if (State ==ServerState.Stopping)
                {
                    if (StoppingTime < Now)
                    {
                        State = ServerState.Stopped;
                        Console.WriteLine("{0} stopped!", ServerName);
                        //LOG.Error("{0} stopped!", ServerName);
                    }
                }
                Fps.SetFrameEnd();
            }
        }

    }
}
