using System;
using System.Collections.Generic;
using System.Linq;
using Assets.SimpleLocalization;
using Nekoyume.BlockChain;
using Nekoyume.Game.Controller;
using Nekoyume.Helper;
using Nekoyume.Model;
using Nekoyume.TableData;
using Nekoyume.UI.Module;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class WorldMap : Widget
    {
        [Serializable]
        public struct StageInformation
        {
            [Serializable]
            public struct IconsArea
            {
                public RectTransform root;
                public TextMeshProUGUI text;
                public List<Image> iconImages;
            }

            public TextMeshProUGUI titleText;
            public TextMeshProUGUI descriptionText;
            public IconsArea monstersArea;
            public IconsArea rewardsArea;
            public TextMeshProUGUI expText;
        }

        public class ViewModel
        {
            public readonly ReactiveProperty<bool> IsWorldShown = new ReactiveProperty<bool>(false);
            public readonly ReactiveProperty<int> SelectedWorldId = new ReactiveProperty<int>(1);
            public readonly ReactiveProperty<int> SelectedStageId = new ReactiveProperty<int>(1);

            public WorldInformation WorldInformation;
        }

        public List<WorldMapWorld> worlds = new List<WorldMapWorld>();

        public GameObject worldMapRoot;
        public Button alfheimButton;
        public Button svartalfaheimrButton;
        public Button asgardButton;

        public StageInformation stageInformation;
        public SubmitButton submitButton;

        private readonly List<IDisposable> _disposablesAtShow = new List<IDisposable>();

        public ViewModel SharedViewModel { get; private set; }

        public int SelectedWorldId
        {
            get => SharedViewModel.SelectedWorldId.Value;
            private set => SharedViewModel.SelectedWorldId.SetValueAndForceNotify(value);
        }

        public int SelectedStageId
        {
            get => SharedViewModel.SelectedStageId.Value;
            private set => SharedViewModel.SelectedStageId.SetValueAndForceNotify(value);
        }

        #region Mono

        public override void Initialize()
        {
            base.Initialize();
            var firstStageId = Game.Game.instance.TableSheets.StageSheet.First?.Id ?? 1;
            SharedViewModel = new ViewModel();
            SharedViewModel.SelectedStageId.Value = firstStageId;
            SharedViewModel.IsWorldShown.Subscribe(UpdateWorld).AddTo(gameObject);
            SharedViewModel.SelectedStageId.Subscribe(UpdateStageInformation).AddTo(gameObject);

            var sheet = Game.Game.instance.TableSheets.WorldSheet;
            foreach (var world in worlds)
            {
                if (!sheet.TryGetByName(world.worldName, out var row))
                {
                    throw new SheetRowNotFoundException("WorldSheet", "Name", world.worldName);
                }

                world.Set(row);

                foreach (var stage in world.pages.SelectMany(page => page.stages))
                {
                    stage.onClick.Subscribe(worldMapStage =>
                        {
                            SharedViewModel.SelectedStageId.Value = worldMapStage.SharedViewModel.stageId;
                        })
                        .AddTo(gameObject);
                }
            }

            stageInformation.monstersArea.text.text = LocalizationManager.Localize("UI_WORLD_MAP_MONSTERS");
            stageInformation.rewardsArea.text.text = LocalizationManager.Localize("UI_WORLD_MAP_REWARDS");
            submitButton.submitText.text = LocalizationManager.Localize("UI_WORLD_MAP_ENTER");

            alfheimButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    AudioController.PlayClick();
                    ShowWorld(1);
                }).AddTo(gameObject);
            svartalfaheimrButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    AudioController.PlayClick();
                    ShowWorld(2);
                }).AddTo(gameObject);
            asgardButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    AudioController.PlayClick();
                    ShowWorld(3);
                }).AddTo(gameObject);
            submitButton.OnSubmitClick
                .Subscribe(_ =>
                {
                    AudioController.PlayClick();
                    GoToQuestPreparation();
                }).AddTo(gameObject);
        }

        #endregion

        public void Show(WorldInformation worldInformation)
        {
            SharedViewModel.WorldInformation = worldInformation;
            if (worldInformation is null)
            {
                foreach (var world in worlds)
                {
                    LockWorld(world);
                }

                return;
            }

            foreach (var world in worlds)
            {
                var worldId = world.SharedViewModel.RowData.Id;
                if (!worldInformation.TryGetWorld(worldId, out var worldModel))
                    throw new Exception(nameof(worldId));

                if (worldModel.IsUnlocked)
                {
                    UnlockWorld(world,
                        worldModel.GetNextStageIdForPlay(),
                        worldModel.GetNextStageIdForSelect());
                }
                else
                {
                    LockWorld(world);
                }
            }

            if (!worldInformation.TryGetFirstWorld(out var firstWorld))
                throw new Exception("worldInformation.TryGetFirstWorld() failed!");

            Show(firstWorld.Id, firstWorld.GetNextStageIdForSelect());
            SharedViewModel.IsWorldShown.Value = true;
        }

        public void Show(int worldId, int stageId)
        {
            ShowWorld(worldId, stageId);
            Show();

            var bottomMenu = Find<BottomMenu>();
            bottomMenu.Show(
                UINavigator.NavigationType.Back,
                SubscribeBackButtonClick,
                true,
                BottomMenu.ToggleableType.WorldMap);
            bottomMenu.worldMapButton.button.OnClickAsObservable()
                .Subscribe(_ => SharedViewModel.IsWorldShown.Value = true)
                .AddTo(_disposablesAtShow);
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            _disposablesAtShow.DisposeAllAndClear();
            Find<BottomMenu>().Close(ignoreCloseAnimation);
            base.Close(ignoreCloseAnimation);
        }

        private void LockWorld(WorldMapWorld world)
        {
            world.Set(-1, world.SharedViewModel.RowData.StageBegin);
        }

        private void UnlockWorld(WorldMapWorld world, int openedStageId = -1, int selectedStageId = -1)
        {
            world.Set(openedStageId, selectedStageId);
        }

        private void ShowWorld(int worldId)
        {
            if (!SharedViewModel.WorldInformation.TryGetWorld(worldId, out var world))
                throw new ArgumentException(nameof(worldId));

            ShowWorld(world.Id, world.GetNextStageIdForSelect());
        }

        private void ShowWorld(int worldId, int stageId)
        {
            SharedViewModel.IsWorldShown.Value = false;
            SelectedWorldId = worldId;
            SelectedStageId = stageId;

            foreach (var world in worlds)
            {
                if (world.SharedViewModel.RowData.Id.Equals(SelectedWorldId))
                {
                    world.ShowByStageId(SelectedStageId);
                }
                else
                {
                    world.Hide();
                }
            }
        }

        private void UpdateWorld(bool active)
        {
            if (active)
            {
                Find<BottomMenu>().worldMapButton.Hide();
                worldMapRoot.SetActive(true);
            }
            else
            {
                worldMapRoot.SetActive(false);
                var bottomMenu = Find<BottomMenu>();
                bottomMenu.worldMapButton.Show();
                bottomMenu.ToggleGroup.SetToggledOffAll();
            }
        }

        private void UpdateStageInformation(int stageId)
        {
            var isSubmittable = false;
            if (!(SharedViewModel.WorldInformation is null))
            {
                if (!SharedViewModel.WorldInformation.TryGetWorldByStageId(stageId, out var world))
                    throw new ArgumentException(nameof(stageId));

                isSubmittable = world.IsPlayable(stageId);
            }

            var stageSheet = Game.Game.instance.TableSheets.StageSheet;
            stageSheet.TryGetValue(stageId, out var stageRow, true);
            stageInformation.titleText.text = $"Stage #{SelectedStageId}";

            var monsterCount = stageRow.TotalMonsterIds.Count;
            for (var i = 0; i < stageInformation.monstersArea.iconImages.Count; i++)
            {
                var image = stageInformation.monstersArea.iconImages[i];
                if (i < monsterCount)
                {
                    image.transform.parent.gameObject.SetActive(true);
                    image.sprite = SpriteHelper.GetCharacterIcon(stageRow.TotalMonsterIds[i]);

                    continue;
                }

                image.transform.parent.gameObject.SetActive(false);
            }

            var rewardItemRows = stageRow.GetRewardItemRows();
            for (var i = 0; i < stageInformation.rewardsArea.iconImages.Count; i++)
            {
                var image = stageInformation.rewardsArea.iconImages[i];
                if (i < rewardItemRows.Count)
                {
                    image.transform.parent.gameObject.SetActive(true);
                    image.sprite = SpriteHelper.GetItemIcon(rewardItemRows[i].Id);

                    continue;
                }

                image.transform.parent.gameObject.SetActive(false);
            }

            stageInformation.expText.text = $"EXP +{stageRow.TotalExp}";

            submitButton.SetSubmittable(isSubmittable);
        }

        private void SubscribeBackButtonClick(BottomMenu bottomMenu)
        {
            if (SharedViewModel.IsWorldShown.Value)
            {
                SharedViewModel.IsWorldShown.Value = false;
                bottomMenu.worldMapButton.SetToggledOff();
            }
            else
            {
                Close();
                Find<Menu>().ShowRoom();
            }
        }

        private void GoToQuestPreparation()
        {
            Close();
            Find<QuestPreparation>().ToggleWorldMap();
        }
    }
}
