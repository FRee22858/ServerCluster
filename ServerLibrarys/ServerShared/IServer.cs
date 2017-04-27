namespace SocketShared
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
    public interface IServer
    {
        Mode StartMode { get;}
    }
}
