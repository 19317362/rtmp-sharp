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

		public RtmpCtx(string Id)
		{
			count = 0;
			aliveAt = DateTime.Now;
			theUrl = Id;
		}
		public int count { get; private set}
		private DateTime aliveAt;//最后活动时间
								 //private RtmpClient client;
		private Process process;
		private string theUrl;
		public int Inc()
		{
			count++;
			return count;
		}

		private void LogMsg(string msg)
		{
			System.Diagnostics.Debug.WriteLine(msg);
		}
		public int Dec()
		{
			if (count >= 1)
			{
				count--;
			}
			else
			{
				
			}
			return count;
		}

		internal async void Start()
		{
			// 			client = new RtmpClient(new Uri(theUrl)
			// 				,new SerializationContext()
			// 				,ObjectEncoding.Amf3
			// 				);
			// 			await client.ConnectAsync();
			// 			var kk = await client.SubscribeAsync()
			process = new Process();
			string arg = "-r \"rtmp://192.168.1.117/live/1\" -o null";
			process.StartInfo.FileName = "rtmpdump";
			process.StartInfo.Arguments = arg;

			process.StartInfo.CreateNoWindow = false;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;

			process.ErrorDataReceived += Process_ErrorDataReceived;
			process.OutputDataReceived += Process_OutputDataReceived;
			process.Exited += Process_Exited;
			//process.StartInfo.RedirectStandardError = true;
			//process.StartInfo.RedirectStandardOutput = true;
			if (process.Start())
			{

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
			throw new NotImplementedException();
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
			}
		}

		internal void Alive()
		{
			this.aliveAt = DateTime.Now;
		}
	}
}
