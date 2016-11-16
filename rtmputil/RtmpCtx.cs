using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;
using RtmpSharp.Net;
using StackExchange.Redis;

namespace rtmputil
{
	public class RtmpCtx
	{
		EventLog m_logger;

		public RtmpCtx(int Id,string url)
		{
			count = 0;
			aliveAt = DateTime.Now;
			theUrl = url;
			theId = Id;
		}
		public bool IsRuning()
		{
			//Console.WriteLine("IsRuning? {0} {1}", process, (process ==null)? 0: process.Id);
			return process != null
				&& !process.HasExited
				;
		}
		public int count { get; private set; }
		public int theId { get; private set; }
		private DateTime aliveAt;//最后活动时间
								 //private RtmpClient client;
		private Process process;
		private int thePorcossId =0;
		private string theUrl;
		public int Inc()
		{
			count++;
			this.aliveAt = DateTime.Now;
			return count;
		}

		private void LogMsg(string msg)
		{
			if (this.m_logger != null)
			{
				m_logger.WriteEntry("RtmpCtx: " + theId + " " + msg);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine(msg);
			}
		}

		internal void CheckAlive()
		{
			if (this.IsRuning())
			{
				var ss = (DateTime.Now - this.aliveAt).TotalSeconds;
				if (ss >= 60)
				{
					LogMsg("Idle STOP " + ss);
					this.Stop();
				}
			}
		}

		public int Dec()
		{
			this.aliveAt = DateTime.Now;
			if (count >= 1)
			{
				count--;
			}
			else
			{
				
			}
			return count;
		}

		internal bool Start()
		{
			// 			client = new RtmpClient(new Uri(theUrl)
			// 				,new SerializationContext()
			// 				,ObjectEncoding.Amf3
			// 				);
			// 			await client.ConnectAsync();
			// 			var kk = await client.SubscribeAsync()
			process = new Process();
			string arg = String.Format("-r {0} -o nul",theUrl);
			process.StartInfo.FileName = "rtmpdump";
			process.StartInfo.Arguments = arg;

			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;

			//process.ErrorDataReceived += Process_ErrorDataReceived;
			//process.OutputDataReceived += Process_OutputDataReceived;
			process.Exited += Process_Exited;
			//process.StartInfo.
			//process.StartInfo.RedirectStandardError = true;
			//process.StartInfo.RedirectStandardOutput = true;
			if (process.Start())
			{
				process.EnableRaisingEvents = true;
				LogMsg("Proc start ok:" + process.Id);
				thePorcossId = process.Id;
				return true;
			}
			else
			{
				LogMsg("Proc start failed");
				process.Close();
				process = null;
				return false;
			}
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			LogMsg("Process_Exited " + this.theUrl);
			process.Close();
			process = null;
		}

		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			LogMsg("Process_OutputDataReceived " + this.theUrl + " "+ e.Data);
			//throw new NotImplementedException();
		}

		internal void UpdateLog(EventLog lg)
		{
			this.m_logger = lg;
		}

		internal void UpdateUrl(string urlKey)
		{
			this.theUrl = urlKey;
		}

		private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			LogMsg("Process_ErrorDataReceived " + this.theUrl + " " + e.Data);
		}

		internal void Stop()
		{
			// 			client.Close();
			// 			client = null;
			if (process != null)
			{
				process.Kill();
				process.Close();
				process = null;
			}
		}

		internal void Alive()
		{
			if (!IsRuning())
			{
				this.Start();
			}
			this.aliveAt = DateTime.Now;
		}
	}
}
