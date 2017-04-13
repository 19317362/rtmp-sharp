using Apache.NMS;
using Apache.NMS.ActiveMQ;
using rtmputil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQUtil
{
	public class Listener : BaseClass
	{
		public const string DESTINATION = "queue://to_rtmp_puller";
		//MainWindow mainWindow;
		public Listener(RtmpMgr mgr)
		{
			_mgr = mgr;
			this.URI = mgr.m_mqurl;
		}
		IMessageConsumer consumer;
		IDestination dest;
		RtmpMgr _mgr;


		public void Initialize()
		{
			try
			{
				dest = _session.GetDestination(DESTINATION);
				consumer = _session.CreateConsumer(dest);
				consumer.Listener += Consumer_Listener;
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
			}
		}

		private void Consumer_Listener(IMessage message)
		{
			ITextMessage textMessage = message as ITextMessage;
			if (!string.IsNullOrEmpty(textMessage.Text))
			{
				//Log("RX " + textMessage.Text);
				_mgr.OnMQMessage(textMessage.Text);
				//Chat pMesg = JsonConvert.DeserializeObject<Chat>(textMessage.Text);
				//mainWindow.UpdateCollection(pMesg);
			}
			else
			{
				Log("RX EMPTY");
			}
		}

	}
}
