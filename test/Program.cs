using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
	class Program
	{
		static void Main(string[] args)
		{
			var utl = new rtmputil.RtmpMgr();
			utl.Start();
			string cmd;
			Console.WriteLine("press q to quit");
			do 
			{
				cmd = Console.ReadLine();
			} while (cmd != "q");
			
			utl.Stop();
		}
	}
}
