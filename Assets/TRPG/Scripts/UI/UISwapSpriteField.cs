using UnityEngine;
using UnityEngine.UI;

namespace TRPG
{
    public class UISwapSpriteField : MonoBehaviour
    {
        [SerializeField] private Image primaryThumbnail;
        [SerializeField] private Image secondaryThumbnail;

        public void SetSprites(Sprite primary, Sprite secondary)
        {
            primaryThumbnail.sprite = primary;
            secondaryThumbnail.sprite = secondary;
        }

        public void Swap()
        {
            Sprite _temp = primaryThumbnail.sprite;
            primaryThumbnail.sprite = secondaryThumbnail.sprite;
            secondaryThumbnail.sprite = _temp;
        }
    }
}