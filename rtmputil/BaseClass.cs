using Apache.NMS;
using Apache.NMS.ActiveMQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQUtil
{
	public class BaseClass
	{
		//
		public string URI = "activemq:tcp://219.141.127.214:61616";
		//public const string URI = "activemq:tcp://120.24.208.122:61616";
		public IConnectionFactory connectionFactory;
		public IConnection _connection;
		public ISession _session;

		public BaseClass()
		{
			connectionFactory = new ConnectionFactory(URI);
			if (_connection == null)
			{
				_connection = connectionFactory.CreateConnection();
				_connection.Start();
				_session = _connection.CreateSession();
			}
		}
		public void Log(string msg)
		{
			Console.WriteLine(msg);
			//Debug.WriteLine(msg);
		}
	}
}
