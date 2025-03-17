using UnityEngine;

public class Gobbomation : MonoBehaviour
{
    public SpriteRenderer earRenderer;
    public SpriteRenderer hairRenderer;
    public SpriteRenderer clothesRenderer;
    public SpriteRenderer baseRenderer;

    public Sprite[] selectedBaseSprites;
    public Sprite[] selectedEarSprites;
    public Sprite[] selectedHairSprites;
    public Sprite[] selectedClothesSprites;

    public void SetFrame(int frameIndex)
    {
        baseRenderer.sprite = selectedBaseSprites[frameIndex];
        earRenderer.sprite = selectedEarSprites[frameIndex];
        hairRenderer.sprite = selectedHairSprites[frameIndex];
        clothesRenderer.sprite = selectedClothesSprites[frameIndex];
    }
}
