using MvM.Socket.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using SysSocket = System.Net.Sockets;

namespace MvM.Socket.Server
{
    public class ClientCollection : IEnumerable<UserData>
    {
        private static List<UserData> UserDatas;
        static ClientCollection()
        {
            UserDatas = new List<UserData>();
        }
        public UserData this[object userid] 
        {
            set 
            {
                if (userid == null) throw new InvalidOperationException("userid can not be null");
                var data = UserDatas.FirstOrDefault(m => m.UserId == userid);
                lock (UserDatas)
                {
                    if (data != null)
                        UserDatas.Remove(data);
                    UserDatas.Add(value);
                }
            }
            get
            {
                return UserDatas.FirstOrDefault(m => m.UserId.ToString() == userid.ToString());
            }
        }
        public void Remove(object userid)
        {
            lock (UserDatas)
            {
                UserDatas.Remove(this[userid]);
            }
        }
        public IEnumerator<UserData> GetEnumerator()
        {
            return UserDatas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return UserDatas.GetEnumerator();
        }
    }
    public class UserData
    {
        private static PropertyInfo[] Properties;
        static UserData() 
        {
            Properties = typeof(UserData).GetProperties();
        }
        public UserData() 
        {
            User = new ClientUser() { UserId = Guid.NewGuid() };
        }
        public object UserId { get { return User == null ? null : User.UserId; } }
        public SocketAsyncEventArgs ReceiveArgs { set; get; }
        /// <summary>  
        /// 客户端IP地址  
        /// </summary>  
        public EndPoint RemoteEndPoint { get; set; }
        /// <summary>  
        /// 通信SOKET  
        /// </summary>  
        public SysSocket.Socket Socket { get; set; }
        /// <summary>  
        /// 连接时间  
        /// </summary>  
        public DateTime? ConnectTime { get; set; }
        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTime? LastHeartbeatTime { set; get; }
        /// <summary>
        /// 数据缓冲区
        /// </summary>
        public List<byte> DataBuffer { set; get; } = new List<byte>();
        /// <summary>
        /// 用户信息
        /// </summary>
        public ClientUser User { set; get; }
        /// <summary>
        /// 重置用户数据
        /// </summary>
        public void Reset()
        {
            foreach (var property in Properties)
            {
                if(!property.PropertyType.IsValueType)
                    property.SetValue(this, null);
            }
        }
    }
}
