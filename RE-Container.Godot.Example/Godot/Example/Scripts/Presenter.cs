using System;
using System.Threading;
using Godot;
using REContainer.Godot.EntryPoints;
using REContainer.LifecycleHooks.Games;

namespace REContainer.Godot.Example.Scripts
{
	public class Presenter : IInitialisable, IReadyable, ITickable, IDisposable
	{
		private readonly GameModel _gameModel;
		private readonly ApplicationService _applicationService;
		private readonly ISceneView _sceneView;

		private CancellationTokenSource _cts = new();

		public Presenter(GameModel gameModel,
			ApplicationService applicationService,
			ISceneView sceneView
			)
		{
			_gameModel = gameModel;
			_applicationService = applicationService;
			_sceneView = sceneView;
		}

		public void Initialise()
		{
			GD.Print("Presenter Initialised");
			_gameModel.Updated += OnUpdated;
			_sceneView.ButtonPressed += _gameModel.Update;
		}

		public void Ready()
		{
			GD.Print("Presenter Readied");
			_applicationService.SetStartTime();
			_sceneView.SetSecondsElapsed(0);
		}

		private bool _firstTick = true;
		public void Tick()
		{
			if (_firstTick)
			{
				GD.Print("Presenter Tick");
				_firstTick = false;
				return;
			}
			
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