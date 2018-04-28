using Assets.Game.Components;
using Assets.Game.Config;
using Assets.Game.Events;
using CnControls;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Systems;
using EcsRx.Unity.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Systems
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

        private IEventSystem _eventSystem;
        private List<IDisposable> _subscriptions;
        private Text _playerHealthPanelCount;
        private Text _scoreCount;
        private int _totalScore;
        private GameObject _pauseTextGo;
        private Image _curtainImage;
        private GameObject _curtainGo;
        private Image _pauseImage;
        private Image _playImage;
        private Toggle _pauseToggle;
        private Button _resetBtnButton;
        private GameObject _directionJoystick;
        private GameObject _rotationJoystick;
        private GameObject _restartPanelGo;
        private GameObject _pauseButtonGo;
        private Slider _playerHealthBarSlider;
        private IGroupAccessor _playersAccessor;
        private GameObject _dashButtonGo;
        private Button _dashButton;
        private Text _restartPanelScoreCount;
        private float _playerHealth;
        private float _playerHealthMax;

        public UISystem(IEventSystem eventSystem, IPoolManager poolManager)
        {
            var playerHealthPanelCountGo = GameObject.Find("PlayerHealthPanel/Count");
            _playerHealthPanelCount = playerHealthPanelCountGo.GetComponent<Text>();
            var scoreCountGo = GameObject.Find("ScorePanel/Count");
            _scoreCount = scoreCountGo.GetComponent<Text>();
            _totalScore = 0;
            _pauseButtonGo = GameObject.Find("PauseButton");
            _pauseToggle = _pauseButtonGo.GetComponent<Toggle>();
            var resetButtonGo = GameObject.Find("ResetButton");
            _resetBtnButton = resetButtonGo.GetComponent<Button>();
            var pauseGo = GameObject.Find("PauseButton/Pause");
            _pauseImage = pauseGo.GetComponent<Image>();
            var playGo = GameObject.Find("PauseButton/Play");
            _playImage = playGo.GetComponent<Image>();
            _directionJoystick = GameObject.Find("DirectionJoystick");
            _rotationJoystick = GameObject.Find("RotationJoystick");
            _restartPanelGo = GameObject.Find("RestartPanel");
            _pauseTextGo = GameObject.Find("PauseText");
            var playerHealthBarGo = GameObject.Find("PlayerHealthPanel/Bar");
            _playerHealthBarSlider = playerHealthBarGo.GetComponent<Slider>();
            _curtainGo = GameObject.Find("Curtain");
            _curtainImage = _curtainGo.GetComponent<Image>();
            _dashButtonGo = GameObject.Find("DashButton");
            var restartPanelScoreCountGo = GameObject.Find("RestartPanel/ScoreCount");
            _restartPanelScoreCount = restartPanelScoreCountGo.GetComponent<Text>();
            _eventSystem = eventSystem;
            _subscriptions = new List<IDisposable>();
            _playersAccessor = poolManager
                .CreateGroupAccessor(new Group(
                    typeof(PlayerComponent),
                    typeof(ViewComponent)
                ));
        }

        public void StartSystem(IGroupAccessor group)
        {
            _eventSystem.Receive<KillEvent>()
                .Where(x => _isEnemy(x.target))
                .Subscribe(x => _totalScore += 100)
                .AddTo(_subscriptions);

            _eventSystem.Receive<ScoreEvent>()
                .Subscribe(x => {
                    var coinComponent = x.target.GetComponent<CoinComponent>();
                    _totalScore += coinComponent.score;
                })
                .AddTo(_subscriptions);

            _eventSystem.Receive<KillEvent>()
                .Where(x => _isPlayer(x.target))
                .Subscribe(x => _showRestartPanel(true))
                .AddTo(_subscriptions);

            _pauseToggle.OnValueChangedAsObservable()
                .Subscribe(x => _pause(x))
                .AddTo(_subscriptions);

            _resetBtnButton.OnClickAsObservable()
                .Subscribe(x =>
                {
                    _showRestartPanel(false);
                    _restartLevel();
                })
                .AddTo(_subscriptions);

            _eventSystem.Receive<LevelEvent>()
                .Where(x => x.action == LevelActions.START)
                .Subscribe(x => _resetAll())
                .AddTo(_subscriptions);
    
            Observable.Interval(TimeSpan.FromMilliseconds(30))
                .Subscribe(x => {
                    _updatePlayerHealth();
                    _updateScore();
                })
                .AddTo(_subscriptions);

            this.WaitForScene()
                .Subscribe(x =>
                {
                    _resetAll();
                });
        }

        public void StopSystem(IGroupAccessor group)
        {
            _subscriptions.DisposeAll();
        }

        private Boolean _isPlayer(IEntity entity)
        {
            return entity.HasComponent<PlayerComponent>();
        }

        private Boolean _isEnemy(IEntity entity)
        {
            return entity.HasComponent<EnemyComponent>();
        }

        private void _restartLevel()
        {
            var levelEvent = new LevelEvent()
            {
                action = LevelActions.RESTART
            };

            _eventSystem.Publish(levelEvent);
        }

        private void _updatePlayerHealth()
        {
            var playerHealth = 0f; 

            if (_playersAccessor.Entities.Any())
            {
                var player = _playersAccessor.Entities.First();
                var actotComponent = player.GetComponent<ActorComponent>();

                playerHealth = actotComponent.health;
            }

            if (_playerHealth < playerHealth)
            {
                _playerHealth += 5;
            } else if (_playerHealth > playerHealth)
            {
                _playerHealth -= 5;
            } else {
                _playerHealth = playerHealth;
            }

            _playerHealthPanelCount.text = string.Format("{0}/{1}", _playerHealth, _playerHealthMax);
            _playerHealthBarSlider.value = _playerHealth / _playerHealthMax;
        }

        private void _updateScore()
        {
            var totalScore = Int32.Parse(_scoreCount.text);

            if (totalScore < _totalScore)
            {
                totalScore += 5;
            } else if (totalScore > _totalScore)
            {
                totalScore -= 5;
            } else {
                totalScore = _totalScore;
            }

            _scoreCount.text = (totalScore).ToString();
            _restartPanelScoreCount.text = string.Format("Score: {0}", _totalScore);
        }

        private void _resetScore()
        {
            _totalScore = 0;
            _scoreCount.text = (_totalScore).ToString();
        }

        private void _resetPlayerHealth()
        {
            var player = _playersAccessor.Entities.First();
            var actotComponent = player.GetComponent<ActorComponent>();

            _playerHealthMax = actotComponent.maxHealth;
            _playerHealthPanelCount.text = string.Format("{0}/{1}", actotComponent.health, _playerHealthMax);
            _playerHealthBarSlider.value = 100;
        }

        private void _resetAll()
        {
            _showCurtain();
            _showJoystick(true);
            _showRestartPanel(false);
            _updatePlayerHealth();
            _resetPlayerHealth();
            _resetScore();
        }

        private void _pause(Boolean isPaused)
        {
            var levelEvent = new LevelEvent();

            _pauseImage.enabled = isPaused;
            _playImage.enabled = !isPaused;
            _pauseTextGo.SetActive(!isPaused);
            _dashButtonGo.SetActive(isPaused);
            levelEvent.action = isPaused
                ? LevelActions.RESUME
                : LevelActions.PAUSE;
            _eventSystem.Publish(levelEvent);
            _showJoystick(isPaused);
        }

        private void _showRestartPanel(Boolean show)
        {
            _restartPanelGo.SetActive(show);
            _pauseButtonGo.SetActive(!show);
            _dashButtonGo.SetActive(!show);
            _showJoystick(!show);
        }

        private void _showJoystick(Boolean show)
        {
            // Reset axis and position.
            if (!show)
            {
                var rotationSimpleJoystick = _rotationJoystick.GetComponent<SimpleJoystick>();
                rotationSimpleJoystick.OnPointerUp(null);

                var directionSimpleJoystick = _directionJoystick.GetComponent<SimpleJoystick>();
                directionSimpleJoystick.OnPointerUp(null);
            }

            _directionJoystick.SetActive(show);
            _rotationJoystick.SetActive(show);
        }

        private void _showCurtain()
        {
            var color = _curtainImage.color;

            MainThreadDispatcher.StartCoroutine(_fadeOutCurtain(color));
        }

        private IEnumerator _fadeOutCurtain(Color color)
        {
            var elapsedTime = 0.0f;
            var totalTime = 0.8f;

            _curtainGo.SetActive(true);
            _curtainImage.color = color;

            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.SmoothStep(1, 0, elapsedTime / totalTime);
                _curtainImage.color = color;

                yield return null;
            }

            color.a = 0;
            _curtainImage.color = color;
            _curtainGo.SetActive(false);
        }
    }
}
