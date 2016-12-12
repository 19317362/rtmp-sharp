using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;
using RtmpSharp.Net;
using StackExchange.Redis;
using System.Threading;
using System.Diagnostics;

namespace rtmputil
{
	public class RtmpMgr
	{
		ConnectionMultiplexer redis ;
		EventLog m_logger;
		private string connStr;
		private string m_ip;
		private int m_port;
		private int m_dbNum;
		//ISubscriber subscriber;
		MQUtil.Listener mqRx;

		Dictionary<int, RtmpCtx> m_dict = new Dictionary<int, RtmpCtx>();

		System.Threading.Timer _timer;

		public RtmpMgr()
		{
			_timer = new Timer(OnTimer,0, Timeout.Infinite, Timeout.Infinite);
		}
		public void SetLogger(EventLog lg)
		{
			m_logger = lg;
		}
		private object theLockObj = new object();

		private void OnTimer(Object state)
		{
			//LogMsg("OnTimer");
			lock (theLockObj)
			{
				_timer.Change(Timeout.Infinite, Timeout.Infinite);//stop timer temp
				foreach (var v in m_dict)
				{
					v.Value.CheckAlive();
				}
				_timer.Change(30000, Timeout.Infinite);//stop timer temp
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
			Console.WriteLine("REDIS:{0}:{1},password={2},defaultDatabase={3}", ip, port, auth, db);
		}
		protected void LogMsg(string msg)
		{
			var tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
// 			if (this.m_logger != null)
// 			{
// 				m_logger.WriteEntry("TID: " + tid.ToString() + " " + msg);
// 			}
// 			else
			{
				Console.WriteLine("TID: " + tid.ToString() + " " + msg);
			}
		}
		// 		private string GetIpcM3u8Key(long Id)
		// 		{
		// 			return String.Format("Ipc:Camera:{0}:m3u8", Id);
		// 		}
		private string GetIpcCameraKey(long Id)
		{
			return String.Format("Ipc:Camera:{0}:url", Id);
		}
		private string GetPullUrl(string ffUrl)
		{//ffmpeg -i rtsp://192.168.1.17:554/user=admin&password=&channel=1&stream=1.sdp?real_stream -f flv -vcodec copy -an rtmp://192.168.1.117/live/1
			if (string.IsNullOrEmpty(ffUrl))
			{
				Console.WriteLine("ERROR: URL NULL");
				return "";
			}

			var kk = ffUrl.ToLower().Split(' ');
			var uu = kk.First(L => L.StartsWith("rtmp://"));
			return uu;
		}

		// 		private string GetIpcPullKey(long Id)
		// 		{
		// 			return String.Format("Ipc:Camera:{0}:pull", Id);
		// 		}
		private RtmpCtx GetCtx(string value)
		{
			int intId = Convert.ToInt32(value);
			if (m_dict.ContainsKey(intId))
			{
				return m_dict[intId];
			}
			else
			{
				var urlKey = GetIpcCameraKey(intId);
				var db = redis.GetDatabase();
				var url = GetPullUrl( db.StringGet(urlKey) );
				var ctx = new RtmpCtx(intId,url);
				ctx.UpdateLog(this.m_logger);
				m_dict[intId] = ctx;
				return ctx;
			}
		}
		public void OnMQMessage(string msg)
		{
			var tt = msg.Split(' ');
			if (tt.Length != 2)
			{
				LogMsg("ERROR MQ:" + msg);
				return;
			}
			var channel = tt[0];
			var value = tt[1];
			//subscriber.Subscribe("rtmp_start", (channel, value) =>
			if (channel == "rtmp_start")
			{
				lock (this.theLockObj)
				{
					var ctx = GetCtx(value);
					if (!ctx.IsRuning())
					{//start
						var urlKey = GetIpcCameraKey(ctx.theId);
						var db = redis.GetDatabase();
						var theVal = db.StringGet(urlKey);
						if (theVal.IsNullOrEmpty)
						{
							Console.WriteLine("URL of " + value + " NOT FOUND");
						}
						else
						{
							var url = GetPullUrl(theVal);
							ctx.UpdateUrl(url);
							ctx.Start();
						}

					}
					ctx.Inc();
					LogMsg(channel + " " + value + " Cnt:" + ctx.count);
				}
			}
			//subscriber.Subscribe("rtmp_stop", (channel, value) =>
			else if(channel == "rtmp_stop")
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
			//subscriber.Subscribe("rtmp_alive", (channel, value) =>
			else if (channel == "rtmp_alive")
			{
				lock (this.theLockObj)
				{
					var ctx = GetCtx(value);
					ctx.Alive();
					LogMsg(channel + " " + value + " Cnt:" + ctx.count + " " + DateTime.Now);
				}
			}
		}
		public void Start()
		{
			LoadConfig();
			redis = ConnectionMultiplexer.Connect(connStr);
			mqRx = new MQUtil.Listener(this);
			mqRx.Initialize();

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
