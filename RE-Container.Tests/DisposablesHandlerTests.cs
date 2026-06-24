using NSubstitute;
using REContainer.LifecycleHooks;

namespace Tests;

public class DisposablesHandlerTests
{
	[Test]
	public void DisposeAll_DisposesAllDisposablesOnce()
	{
		var disposable0 = Substitute.For<IDisposable>();
		var disposable1 = Substitute.For<IDisposable>();
		
		var disposablesHandler = new DisposablesHandler([disposable0, disposable1]);
		
		disposablesHandler.Dispose();
		
		disposable0.Received(1).Dispose();
		disposable1.Received(1).Dispose();
	}
}