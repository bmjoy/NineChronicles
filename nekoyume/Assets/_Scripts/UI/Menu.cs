using System.Collections;
using Assets.SimpleLocalization;
using Nekoyume.Game;
using Nekoyume.Game.Controller;
using Nekoyume.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class Menu : Widget
    {
        public BottomMenu bottomMenu;
        public GameObject btnQuest;
        public Text btnQuestText;
        public GameObject btnCombination;
        public Text btnCombinationText;
        public GameObject btnShop;
        public Text btnShopText;
        public GameObject btnRanking;
        public Text btnRankingText;
        public Text LabelInfo;
        public SpeechBubble[] SpeechBubbles;

        public Stage Stage;

        protected override void Awake()
        {
            base.Awake();

            btnQuestText.text = LocalizationManager.Localize("UI_QUEST");
            btnCombinationText.text = LocalizationManager.Localize("UI_COMBINATION");
            btnShopText.text = LocalizationManager.Localize("UI_SHOP");
            btnRankingText.text = LocalizationManager.Localize("UI_RANKING");

            Stage = GameObject.Find("Stage").GetComponent<Stage>();
            SpeechBubbles = GetComponentsInChildren<SpeechBubble>();
        }

        public void ShowButtons(bool value)
        {
            btnQuest.SetActive(value);
            btnCombination.SetActive(value);
            btnShop.SetActive(value);
            btnRanking.SetActive(value);
        }

        public void ShowRoom()
        {
            var stage = Game.Game.instance.stage;
            stage.LoadBackground("room");
            stage.GetPlayer(stage.roomPosition);

            var player = stage.GetPlayer();
            player.gameObject.SetActive(true);

            Show();
            ShowButtons(true);
            StartCoroutine(ShowSpeeches());

            LabelInfo.text = "";

            AudioController.instance.PlayMusic(AudioController.MusicCode.Main);
        }

        public void ShowWorld()
        {
            Show();
            ShowButtons(false);

            LabelInfo.text = "";
        }

        public void QuestClick()
        {
            Close();
            Find<WorldMap>().Show();
            AudioController.PlayClick();
            AnalyticsManager.Instance.OnEvent(AnalyticsManager.EventName.ClickMainBattle);
        }

        public void ShopClick()
        {
            Close();
            Find<Shop>().Show();
            AudioController.PlayClick();
            AnalyticsManager.Instance.OnEvent(AnalyticsManager.EventName.ClickMainShop);
        }

        public void CombinationClick()
        {
            Close();
            Find<Combination>().Show();
            AudioController.PlayClick();
            AnalyticsManager.Instance.OnEvent(AnalyticsManager.EventName.ClickMainCombination);
        }

        public void RankingClick()
        {
            Close();
            Find<RankingBoard>().Show();
            AudioController.PlayClick();
        }

        public override void Initialize()
        {
            base.Initialize();

            var status = Find<Status>();
            bottomMenu.inventoryButton.onClick.AddListener(status.ToggleInventory);
            bottomMenu.questButton.onClick.AddListener(status.ToggleQuest);
            bottomMenu.infoAndEquipButton.onClick.AddListener(status.ToggleStatus);
        }

        public override void Show()
        {
            base.Show();
            Find<Status>().Show();
        }

        public override void Close()
        {
            StopCoroutine(ShowSpeeches());
            foreach (var speechBubble in SpeechBubbles)
            {
                speechBubble.Hide();
            }
            
            Find<Inventory>().Close();
            Find<StatusDetail>().Close();
            Find<Quest>().Close();
            Find<Status>().Close();
            base.Close();
        }

        private IEnumerator ShowSpeeches()
        {
            foreach (var speechBubble in SpeechBubbles)
            {
                speechBubble.Init();
            }

            yield return new WaitForSeconds(2.0f);

            while (true)
            {
                var n = SpeechBubbles.Length;
                while (n > 1)
                {
                    n--;
                    var k = Mathf.FloorToInt(Random.value * (n + 1));
                    var value = SpeechBubbles[k];
                    SpeechBubbles[k] = SpeechBubbles[n];
                    SpeechBubbles[n] = value;
                }

                foreach (var speechBubble in SpeechBubbles)
                {
                    yield return StartCoroutine(speechBubble.CoShowText());
                    yield return new WaitForSeconds(Random.Range(2.0f, 4.0f));
                }
            }
        }
    }
}
