using System.Collections.Generic;
using System.Linq;
using Nekoyume.Action;
using Nekoyume.Game.Controller;
using Nekoyume.Game.Item;
using UnityEngine;

namespace Nekoyume.UI
{
    public class Inventory : Widget
    {
        private const float DisableAlpha = 0.3f;

        public Transform grid;
        public GameObject slotBase;

        private List<InventorySlot> _slots;
        private const int maxSlot = 42;

        protected override void Awake()
        {
            base.Awake();

            _slots = new List<InventorySlot>();
            slotBase.SetActive(true);
            for (int i = 0; i < maxSlot; ++i)
            {
                GameObject newSlot = Instantiate(slotBase, grid);
                InventorySlot slot = newSlot.GetComponent<InventorySlot>();
                slot.Item = null;
                _slots.Add(slot);
            }

            slotBase.SetActive(false);
            Game.Event.OnUpdateEquipment.AddListener(UpdateEquipment);
            Game.Event.OnSlotClick.AddListener(ToggleSlot);
        }

        private void ToggleSlot(InventorySlot selected, bool toggled)
        {
            foreach (var slot in _slots)
            {
                if (slot != selected && slot.outLine != null)
                {
                    slot.toggled = false;
                    slot.outLine.SetActive(false);
                }
            }
        }

        private void UpdateEquipment(Equipment equipment)
        {
            foreach (var slot in _slots)
            {
                var item = slot.Item;
                if (item != null && item is Weapon)
                {
                    slot.LabelEquip.text = "";
                    if (item == equipment)
                    {
                        slot.LabelEquip.text = equipment.equipped ? "E" : "";
                    }
                }
            }
        }

        public override void Show()
        {
            List<Game.Item.Inventory.InventoryItem> items = ActionManager.instance.Avatar.Items;
            for (int i = 0; i < maxSlot; ++i)
            {
                InventorySlot slot = _slots[i];
                slot.SetAlpha(1f);
                if (items != null && items.Count > i)
                {
                    var inventoryItem = items[i];
                    slot.Set(inventoryItem.Item, inventoryItem.Count);
                    if (inventoryItem.Item is Weapon)
                    {
                        var weapon = (Weapon) inventoryItem.Item;
                        slot.LabelEquip.text = weapon.equipped ? "E" : "";
                    }
                }
                else
                {
                    slot.Clear();
                }
            }

            base.Show();
        }

        public void CloseClick()
        {
            var status = Find<Status>();
            if (status)
            {
                status.BtnInventory.group.SetAllTogglesOff();
            }
            
            AudioController.PlayClick();
        }

        public void SetItemTypesToDisable(params ItemBase.ItemType[] targetTypes)
        {
            for (int i = 0; i < maxSlot; i++)
            {
                var slot = _slots[i];
                if (slot.Item == null)
                {
                    break;
                }

                if (targetTypes.Contains(slot.Item.Data.cls.ToEnumItemType()))
                {
                    slot.SetAlpha(DisableAlpha);
                }
                else
                {
                    slot.SetAlpha(1.0f);
                }
            }
        }

        public void SetOtherItemTypesToDisable(params ItemBase.ItemType[] targetTypes)
        {
            for (int i = 0; i < maxSlot; i++)
            {
                var slot = _slots[i];
                if (slot.Item == null)
                {
                    break;
                }

                if (!targetTypes.Contains(slot.Item.Data.cls.ToEnumItemType()))
                {
                    slot.SetAlpha(DisableAlpha);
                }
                else
                {
                    slot.SetAlpha(1.0f);
                }
            }
        }

        public void SetItemTypesToGlow(params ItemBase.ItemType[] targetTypes)
        {
            for (int i = 0; i < maxSlot; i++)
            {
                var slot = _slots[i];
                if (slot.Item == null)
                {
                    break;
                }

                slot.glow.SetActive(targetTypes.Contains(slot.Item.Data.cls.ToEnumItemType()));
            }
        }
    }
}
