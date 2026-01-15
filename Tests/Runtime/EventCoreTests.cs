using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace XFG
{
	
	class EventCoreTests 
	{
		const string _testEvent = "TestEvent";

		[Test]
		public void TestSubscribeGameEvent() 
		{
			Core.SubscribeEvent(_testEvent, OnTestEvent);
			Assert.True(Core.HasEvent(_testEvent));
            Assert.True(Core.GetEventHandlerCount(_testEvent) == 1);
            Core.BroadcastEvent(_testEvent, this, "walaw");
			Core.UnsubscribeEvent(_testEvent, OnTestEvent);
            Assert.False(Core.HasEvent(_testEvent));
        }
		void OnTestEvent(string eventName, object sender, object[] args)
		{
			Assert.AreEqual(_testEvent, eventName);
			Assert.True(true, "Event received successfully");
        }
	}
}