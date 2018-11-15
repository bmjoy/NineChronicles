using System.Collections;
using DG.Tweening;
using Nekoyume.Data;
using Nekoyume.Data.Table;
using UnityEngine;
using Avatar = Nekoyume.Model.Avatar;


namespace Nekoyume.Game
{
    public class Character : MonoBehaviour
    {
        public Stats Stats;

        public IEnumerator Walk()
        {
            while (true)
            {
                Vector2 position = transform.position;
                position.x += Time.deltaTime * 40 / 160;
                transform.position = position;
                yield return null;
            }
        }

        public IEnumerator Load(GameObject go,  Avatar avatar)
        {
            _Load(go, avatar);
            yield return null;
        }

        public IEnumerator Stop()
        {
            StopCoroutine(Walk());
            yield return null;
        }

        public void _Load(GameObject go, Avatar avatar)
        {
            Vector2 position = go.transform.position;
            position.y = -1;
            go.transform.position = position;
            var render = go.AddComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>($"images/character_{avatar.class_}");
            if (sprite == null)
                sprite = Resources.Load<Sprite>("images/pet");
            render.sprite = sprite;
            render.sortingOrder = 1;
            Material mat = render.material;
            Sequence colorseq = DOTween.Sequence();
            colorseq.Append(mat.DOColor(Color.white, 0.0f));
            var tables = this.GetRootComponent<Tables>();
            var statsTable = tables.Stats;
            Stats = statsTable[avatar.level];
        }
    }
}
