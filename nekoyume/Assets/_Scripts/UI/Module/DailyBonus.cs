using System;
using System.Collections.Generic;
using Assets.SimpleLocalization;
using Nekoyume.BlockChain;
using Nekoyume.Model;
using Nekoyume.State;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class DailyBonus : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public Slider slider;
        public Button button;
        public CanvasGroup canvasGroup;
        public RectTransform tooltipArea;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private bool _updateEnable;
        private bool _isFull;
        private Animation _animation;
        private long _receivedIndex;

        private VanilaTooltip _tooltip;

        #region Mono

        private void Awake()
        {
            slider.maxValue = GameConfig.DailyRewardInterval;
            text.text = $"0 / {GameConfig.DailyRewardInterval}";
            button.interactable = false;
            _animation = GetComponent<Animation>();
            _updateEnable = true;
        }

        private void OnEnable()
        {
            Game.Game.instance.Agent.blockIndex.ObserveOnMainThread().Subscribe(SetIndex).AddTo(_disposables);
            ReactiveCurrentAvatarState.DailyRewardReceivedIndex.Subscribe(SetReceivedIndex).AddTo(_disposables);
            canvasGroup.alpha = 0;
        }

        private void OnDisable()
        {
            _disposables.DisposeAllAndClear();
        }

        #endregion

        private void SetIndex(long index)
        {
            if(!_updateEnable)
            {
                return;
            }

            var min = Math.Max(index - _receivedIndex, 0);
            var value = Math.Min(min, GameConfig.DailyRewardInterval);
            _isFull = value >= GameConfig.DailyRewardInterval;
            
            button.interactable = _isFull;
            canvasGroup.interactable = _isFull;
            if (_isFull)
            {
                _animation.Play();
            }

            text.text = $"{value} / {GameConfig.DailyRewardInterval}";
            slider.value = value;
        }

        public void GetReward()
        {
            ActionManager.instance.DailyReward().Subscribe(_ =>
            {
                _updateEnable = true;
                Notification.Push(Game.Mail.MailType.System, LocalizationManager.Localize("UI_RECEIVED_DAILY_REWARD"));
            });
            Notification.Push(Game.Mail.MailType.System, LocalizationManager.Localize("UI_RECEIVING_DAILY_REWARD"));
            _animation.Stop();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            button.interactable = false;
            _isFull = false;
            SetIndex(0);
            _updateEnable = false;
        }

        public void ShowTooltip()
        {
            _tooltip = Widget.Find<VanilaTooltip>();
            _tooltip?.Show("UI_PROSPERITY_DEGREE", "UI_PROSPERITY_DEGREE_DESCRIPTION", tooltipArea.position);
        }

        public void HideTooltip()
        {
            _tooltip?.Close();
            _tooltip = null;
        }

        private void SetReceivedIndex(long index)
        {
            if (index != _receivedIndex)
            {
                _receivedIndex = index;
            }
        }
    }
}
