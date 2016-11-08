using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using rtmputil;

namespace rtmpService
{
	public partial class Service1 : ServiceBase
	{
		static RtmpMgr theMgr = new RtmpMgr();
		public Service1()
		{
			InitializeComponent();
			//Setup Service
			//this.ServiceName = "MyService2";
			//this.CanStop = true;
			//this.CanPauseAndContinue = true;

			//Setup logging
			this.AutoLog = false;

			((ISupportInitialize)this.EventLog).BeginInit();
			if (!EventLog.SourceExists(this.ServiceName))
			{
				EventLog.CreateEventSource(this.ServiceName, "Application");
			}
			((ISupportInitialize)this.EventLog).EndInit();

			this.EventLog.Source = this.ServiceName;
			this.EventLog.Log = "Application";

		}

		protected override void OnStart(string[] args)
		{
			theMgr.Start();
		}

		protected override void OnStop()
		{
			theMgr.Stop();
		}
	}
}
