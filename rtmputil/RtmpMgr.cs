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
			Console.WriteLine(msg);
		}
		public void Start()
		{
			LoadConfig();
			redis = ConnectionMultiplexer.Connect(connStr);
			subscriber = redis.GetSubscriber();

			subscriber.Subscribe("rtmp_start", (channel, value) =>
			{
				LogMsg(channel + " " + value);
			}
			);

			subscriber.Subscribe("rtmp_stop", (channel, value) =>
			{
				LogMsg(channel + " " + value);
			}
			);
			subscriber.Subscribe("rtmp_alive", (channel, value) =>
			{
				LogMsg(channel + " " + value);
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
