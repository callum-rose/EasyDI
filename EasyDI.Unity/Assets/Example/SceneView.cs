using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyDI.Unity.Example
{
	public interface ISceneView
	{
		event Action ButtonPressed;

		void Jump(CancellationToken cancellationToken);
		void SetSecondsElapsed(float seconds);
	}

	internal class SceneView : MonoBehaviour, ISceneView
	{
		[SerializeField] private TextMeshProUGUI secondsElapsedText = null!;
		[SerializeField] private Transform jumper = null!;
		[SerializeField] private Button button = null!;
		
		public event Action ButtonPressed;

		private void Awake()
		{
			button.onClick.AddListener(OnButtonPressed);
		}

		public void Jump(CancellationToken cancellationToken)
		{
			StartCoroutine(Routine());
			
			IEnumerator Routine()
			{
				for (float time = 0; time < 1; time += Time.deltaTime)
				{
					float y = 4 * (time - time * time);
					jumper.position = Vector3.up * y;
					
					if (cancellationToken.IsCancellationRequested)
					{
						yield break;
					}
					
					yield return null;
				}
			}
		}

		public void SetSecondsElapsed(float seconds)
		{
			secondsElapsedText.text = $"{seconds:0.00} seconds elapsed";
		}

		private void OnButtonPressed()
		{
			ButtonPressed?.Invoke();
		}
	}
}