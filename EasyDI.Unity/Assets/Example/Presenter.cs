using System;
using System.Threading;
using EasyDI.LifecycleHooks.Games;
using EasyDI.Unity.LifecycleHooks;

namespace EasyDI.Unity.Example
{
	public class Presenter : IInitialisable, IStartable, ITickable, IDisposable
	{
		private readonly GameModel _gameModel;
		private readonly ApplicationService _applicationService;
		private readonly ISceneView _sceneView;

		private CancellationTokenSource _cts = new();

		public Presenter(GameModel gameModel,
			ApplicationService applicationService,
			ISceneView sceneView)
		{
			_gameModel = gameModel;
			_applicationService = applicationService;
			_sceneView = sceneView;
		}

		public void Initialise()
		{
			_gameModel.Updated += OnUpdated;
			_sceneView.ButtonPressed += _gameModel.Update;
		}

		public void Start()
		{
			_applicationService.SetStartTime();
			_sceneView.SetSecondsElapsed(0);
		}

		public void Tick()
		{
			_sceneView.SetSecondsElapsed((float)_applicationService.Elapsed.TotalSeconds);
		}

		public void Dispose()
		{
			_cts.Cancel();
			_cts.Dispose();

			_gameModel.Updated -= OnUpdated;
		}

		private void OnUpdated()
		{
			_cts.Cancel();
			_cts.Dispose();

			_cts = new CancellationTokenSource();

			_sceneView.Jump(_cts.Token);
		}
	}
}