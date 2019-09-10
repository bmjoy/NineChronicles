using System.Collections.Generic;
using System.Linq;
using System.Text;
using Libplanet.Action;
using Nekoyume.Battle;
using Nekoyume.BlockChain;
using Nekoyume.Game;
using Nekoyume.Game.Character;
using Nekoyume.Game.Item;
using Nekoyume.Model;
using Nekoyume.TableData;
using Nekoyume.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume
{
    public class Cheat : ScreenWidget
    {
        private static Cheat Instance;

        public Text log;
        public Button BtnOpen;
        public Button buttonBase;
        public ScrollRect list;
        public ScrollRect skillList;
        public HorizontalLayoutGroup skillPanel;

        private Transform _modal;
        private float _updateTime = 0.0f;
        private StringBuilder _logString = new StringBuilder();
        private BattleLog.Result _result;
        private int[,] _stageRange;
        private Game.Skill[] _skills;
        private Game.Skill _selectedSkill;

        public class DebugRandom : IRandom
        {
            private readonly System.Random _random = new System.Random();
            public int Next()
            {
                return _random.Next();
            }

            public int Next(int maxValue)
            {
                return _random.Next(maxValue);
            }

            public int Next(int minValue, int maxValue)
            {
                return _random.Next(minValue, maxValue);
            }

            public void NextBytes(byte[] buffer)
            {
                _random.NextBytes(buffer);
            }

            public double NextDouble()
            {
                return _random.NextDouble();
            }
        }

        public static void Display(string text)
        {
            Instance.log.text = text;
        }
        
        static void Log(string text)
        {
            Instance._logString.Insert(0, $"> {text}\n");
            Instance.log.text += Instance._logString.ToString();
        }

        protected override void Awake()
        {
            base.Awake();

            Instance = this;
        }

        private void Start()
        {
            _modal = transform.Find("Modal");
            _modal.gameObject.SetActive(false);
#if DEBUG
#else
            Transform btn = transform.Find("Btn");
            btn.gameObject.SetActive(false);
#endif
        }

        private void Update()
        {
            _updateTime += Time.deltaTime;
        }

        public override void Show()
        {
            _modal.gameObject.SetActive(true);
            BtnOpen.gameObject.SetActive(false);
            foreach (var i in Enumerable.Range(1, Game.Game.instance.TableSheets.StageSheet.Keys.Last()))
            {
                Button newButton = Instantiate(buttonBase, list.content);
                newButton.GetComponentInChildren<Text>().text = i.ToString();
                newButton.onClick.AddListener(() => DummyBattle(i));
                newButton.gameObject.SetActive(true);
            }

            var skills = new List<Game.Skill>();
            foreach (var skillRow in Game.Game.instance.TableSheets.SkillSheet)
            {
                var skill = SkillFactory.Get(skillRow, 50, 1m);
                skills.Add(skill);
                Button newButton = Instantiate(buttonBase, skillList.content);
                newButton.GetComponentInChildren<Text>().text = $"{skillRow.GetLocalizedName()}_{skillRow.ElementalType}";
                newButton.onClick.AddListener(() => SelectSkill(skill));
                newButton.gameObject.SetActive(true);
            }

            _skills = skills.ToArray();

            base.Show();
        }

        public override void Close()
        {
            foreach (Transform child in list.content.transform)
            {
                Destroy(child.gameObject);
            }
            list.gameObject.SetActive(false);
            skillPanel.gameObject.SetActive(false);

            _modal.gameObject.SetActive(false);
            BtnOpen.gameObject.SetActive(true);
        }

        public override bool IsActive()
        {
            return _modal.gameObject.activeSelf;
        }

        public void HandleClick(GameObject sender)
        {
#if DEBUG
            Invoke(sender.name, 0.0f);
#endif
        }

        private void LevelUp()
        {
            GameObject enemyObj = GameObject.Find("Enemy");
            if (enemyObj == null)
            {
                Log("Need Enemy.");
                return;
            }
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                var player = playerObj.GetComponent<Game.Character.Player>();
                player.Level += 1;
                Log($"Level Up to {player.Level}");
            }
            var enemy = enemyObj.GetComponent<Enemy>();
            Game.Event.OnEnemyDead.Invoke(enemy);
        }

        private void SpeedUp()
        {
            Time.timeScale = 2.0f;
            Log($"Speed Up to {Time.timeScale}");
        }

        private void DummyBattleWin()
        {
            _result = BattleLog.Result.Win;
            list.gameObject.SetActive(true);
        }

        private void DummyBattleLose()
        {
            _result = BattleLog.Result.Lose;
            list.gameObject.SetActive(true);
        }

        private void DummyBattle(int stageId)
        {
            Find<BattleResult>()?.Close();
            Find<Menu>()?.Close();
            Find<Menu>()?.ShowWorld();
            
            var simulator = new Simulator(new DebugRandom(), States.Instance.currentAvatarState.Value, new List<Food>(), stageId, _selectedSkill);
            simulator.Simulate();
            simulator.Log.result = _result;
            
            var stage = Game.Game.instance.stage;
            stage.Play(simulator.Log);
            
            Close();
        }

        private void DummySkill()
        {
            skillPanel.gameObject.SetActive(true);
        }

        private void SelectSkill(Game.Skill skill)
        {
            _selectedSkill = skill;
            DummyBattle(1);
        }
    }
}
