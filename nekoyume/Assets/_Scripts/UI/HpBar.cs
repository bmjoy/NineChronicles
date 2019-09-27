using System.Collections.Generic;
using System.Linq;
using Nekoyume.Game;
using Nekoyume.TableData;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class HpBar : ProgressBar
    {
        public BuffLayout buffLayout;

        public void UpdateBuff(Dictionary<int, Buff> modelBuffs)
        {
            var buffs = modelBuffs.Values.OrderBy(r => r.Data.Id);
            buffLayout.UpdateBuff(buffs);
        }
    }
}
