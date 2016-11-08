using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;
using RtmpSharp.Net;
using StackExchange.Redis;
using System.Threading;

namespace rtmputil
{
	public class RtmpMgr
	{
		ConnectionMultiplexer redis ;
		private string connStr;
		private string m_ip;
		private int m_port;
		private int m_dbNum;
		ISubscriber subscriber;
		Dictionary<int, RtmpCtx> m_dict = new Dictionary<int, RtmpCtx>();

		System.Threading.Timer _timer;

		public RtmpMgr()
		{
			_timer = new Timer(OnTimer,0, Timeout.Infinite, Timeout.Infinite);
		}
		private object theLockObj = new object();

		private void OnTimer(Object state)
		{
			LogMsg("OnTimer");
			lock (theLockObj)
			{
				_timer.Change(Timeout.Infinite, Timeout.Infinite);//stop timer temp
				foreach (var v in m_dict)
				{
					v.Value.CheckAlive();
				}
				_timer.Change(10000, Timeout.Infinite);//stop timer temp
			}
		}
		public void LoadConfig()
		{
			var cfg = new Settings1();
			cfg.Reload();
			//connStr = new ConfigurationOptions();
			var ip = cfg.ip;// System.Configuration.ConfigurationManager.AppSettings["redis.server"];
			var port = cfg.port;
			var auth = cfg.auth;

			var db = cfg.db;
			m_ip = ip;
			m_port = Convert.ToInt32(port);
			m_dbNum = Convert.ToInt32(db);
			connStr = String.Format("{0}:{1},password={2},defaultDatabase={3}", ip, port, auth, db);
		}
		protected void LogMsg(string msg)
		{
			var tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
			Console.WriteLine("TID: "+tid.ToString() + " " + msg);
		}
// 		private string GetIpcM3u8Key(long Id)
// 		{
// 			return String.Format("Ipc:Camera:{0}:m3u8", Id);
// 		}
		private string GetIpcPullKey(long Id)
		{
			return String.Format("Ipc:Camera:{0}:pull", Id);
		}
		private RtmpCtx GetCtx(string value)
		{
			int intId = Convert.ToInt32(value);
			if (m_dict.ContainsKey(intId))
			{
				return m_dict[intId];
			}
			else
			{
				var urlKey = GetIpcPullKey(intId);
				var db = redis.GetDatabase();
				var url = db.StringGet(urlKey);
				var ctx = new RtmpCtx(intId,url);
				m_dict[intId] = ctx;
				return ctx;
			}
		}
		public void Start()
		{
			LoadConfig();
			redis = ConnectionMultiplexer.Connect(connStr);
			subscriber = redis.GetSubscriber();

			subscriber.Subscribe("rtmp_start", (channel, value) =>
			{
				lock (this.theLockObj)
				{
					var ctx = GetCtx(value);
					if (!ctx.IsRuning() )
					{//start
						ctx.Start();
					}
					ctx.Inc();
					LogMsg(channel + " " + value + " Cnt:" + ctx.count);
				}
			}
			);

			subscriber.Subscribe("rtmp_stop", (channel, value) =>
			{
				lock (this.theLockObj)
				{
					var ctx = GetCtx(value);
					ctx.Dec();
// 					if (ctx.IsRuning()) //不做停止操作 -- DELAY以后再停
// 					{//start
// 						ctx.Stop();
// 					}
					LogMsg(channel + " " + value + " Cnt:" + ctx.count);
				}
			}
			);
			subscriber.Subscribe("rtmp_alive", (channel, value) =>
			{
				lock (this.theLockObj)
				{
					var ctx = GetCtx(value);
					ctx.Alive();
					LogMsg(channel + " " + value + " Cnt:" + ctx.count + " " + DateTime.Now);
				}
			}
			);
			_timer.Change(10000, Timeout.Infinite);//stop timer temp
		}

		public void Stop()
		{
			//throw new NotImplementedException();
			redis.Close();

			foreach (var v in m_dict)
			{
				v.Value.Stop();
			}
		}

	}
}
