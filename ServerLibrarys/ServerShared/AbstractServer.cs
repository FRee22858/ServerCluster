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
    public abstract class AbstractServer
    {
        private Mode startMode;
        public Mode StartMode
        {
            get { return startMode; }
            set { startMode = value; }
        }
        private string serverName;
        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }
        public abstract void Init(string[] args);
        public abstract  void Exit();
        public abstract void Run();
        public abstract void ProcessInput();
    }
}
