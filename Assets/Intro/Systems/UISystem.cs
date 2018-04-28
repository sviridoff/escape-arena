using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using Kino;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Intro.Systems
{
    public class UISystem : IManualSystem
    {
        public IGroup TargetGroup
        {
            get
            {
                return new EmptyGroup();
            }
        }

        private AsyncOperation _scene;
        private Slider _sceneBarSlider;
        private GameObject _startButton;
        private GameObject _sceneBar;
        private GameObject _loadingSceneText;
        private Button _button;
        private List<IDisposable> _subscriptions;
        private GameObject _logo;
        private Camera _camera;
        private IDisposable _subscription;

        public UISystem(IEventSystem eventSystem, IPoolManager poolManager)
        {
            _subscriptions = new List<IDisposable>();
            _loadingSceneText = GameObject.Find("Canvas/LoadingSceneText");
            _startButton = GameObject.Find("Canvas/StartButton");
            _button = _startButton.GetComponent<Button>();
            _sceneBar = GameObject.Find("Canvas/SceneBar");
            _logo = GameObject.Find("Logo");
            _sceneBarSlider = _sceneBar.GetComponent<Slider>();
            _camera = Camera.main;
            _startButton.SetActive(false);
            _logo.SetActive(false);
        }

        public void StartSystem(IGroupAccessor group)
        {
            _button.OnClickAsObservable()
                .Subscribe(x =>
                {
                    _stopGlitch();
                    _startButton.SetActive(false);
                    _scene.allowSceneActivation = true;
                })
                .AddTo(_subscriptions);

            this.WaitForScene()
                .Subscribe(x =>
                {
                    MainThreadDispatcher.StartCoroutine(_loadingScene("Game"));
                });
        }

        public void StopSystem(IGroupAccessor group)
        {
            _subscriptions.DisposeAll();
        }

        private void _stopGlitch()
        {
            var analogGlitch = _camera.GetComponent<AnalogGlitch>();

            analogGlitch.enabled = false;
            _subscription.Dispose();
        }

        private IEnumerator _glitch()
        {
            var analogGlitch = _camera.GetComponent<AnalogGlitch>();

            yield return new WaitForSeconds(1);

            while (true)
            {
                analogGlitch.scanLineJitter = UnityEngine.Random.Range(.5f,1);
                analogGlitch.colorDrift = UnityEngine.Random.Range(.5f, 1);

                yield return new WaitForSeconds(UnityEngine.Random.Range(.2f, 1));

                analogGlitch.scanLineJitter = 0;
                analogGlitch.colorDrift = 0;

                yield return new WaitForSeconds(UnityEngine.Random.Range(2.5f, 3.5f));
            }
        }

        private IEnumerator _loadingScene(string sceneName)
        {
            yield return new WaitForSeconds(1);

            _sceneBarSlider.value = 0;
            _scene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            _scene.allowSceneActivation = false;

            while (!_scene.isDone)
            {
                _sceneBarSlider.value = _scene.progress;

                if (_scene.progress >= 0.9f)
                {
                    _sceneBarSlider.value = 1;

                    _startButton.SetActive(true);
                    _logo.SetActive(true);
                    _sceneBar.SetActive(false);
                    _loadingSceneText.SetActive(false);
                    _subscription = Observable.FromCoroutine(_glitch)
                        .Subscribe();

                    yield break;
                }

                yield return null;
            }
        }
    }
}
