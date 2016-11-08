using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;
using RtmpSharp.Net;
using StackExchange.Redis;

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
		Dictionary<string, RtmpCtx> m_dict = new Dictionary<string, RtmpCtx>();

		private object theLockObj;
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
		private RtmpCtx GetCtx(string value)
		{
			if (m_dict.ContainsKey(value))
			{
				return m_dict[value];
			}
			else
			{
				var ctx = new RtmpCtx(value);
				m_dict[value] = ctx;
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
					if (ctx.count == 0)
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
					if (ctx.count == 0)
					{//start
						ctx.Stop();
					}
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
		}

		public void Stop()
		{
			//throw new NotImplementedException();

			redis.Close();
		}

	}
}
