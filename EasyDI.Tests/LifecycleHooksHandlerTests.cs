using NSubstitute;
using EasyDI.LifecycleHooks;

namespace Tests;

public class LifecycleHooksHandlerTests
{
	public interface IEventHandler
	{
		void Invoke();
	}

	public sealed class TestLifecycleHooksHandler : LifecycleHooksHandler<IEventHandler>
	{
		public TestLifecycleHooksHandler(IReadOnlyList<IEventHandler> items) : base(e => e.Invoke(), items) { }
	}

	[Test]
	public void InvokeAll_InvokesAllEventHandlersOnce()
	{
		var eventHandler0 = Substitute.For<IEventHandler>();
		var eventHandler1 = Substitute.For<IEventHandler>();
		var lifeCycleEventHandler = new TestLifecycleHooksHandler([eventHandler0, eventHandler1]);
		
		lifeCycleEventHandler.InvokeAll();
		
		eventHandler0.Received(1).Invoke();
		eventHandler1.Received(1).Invoke();
	}
}