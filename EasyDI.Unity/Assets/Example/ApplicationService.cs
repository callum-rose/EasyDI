using System;

namespace EasyDI.Unity.Example
{
	public class ApplicationService
	{
		public TimeSpan Elapsed => DateTime.Now - _startTime;

		private DateTime _startTime;
		
		public void SetStartTime()
		{
			_startTime = DateTime.Now;
		}
	}
}