using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Gobborandomizer : MonoBehaviour
{
    public static Gobborandomizer Instance;

    public Base[] bases;
    public Hair[] hairs;
    public Clothes[] clothes;
    public Ear[] ears;

    public List<Color> baseColours;
    public List<Color> hairColours;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Randomize(Gobbomation gobbomation) {

        /*if (Random.Range(1, 100) == 69)
        {
            SetSpecialGobbo();
            return;
        }*/

        int randBase = Random.Range(0, bases.Length);
        int randEar = Random.Range(0, ears.Length);
        int randHair = Random.Range(0, hairs.Length);
        int randClothes = Random.Range(0, clothes.Length);
        int randBaseColour = Random.Range(0, baseColours.Count);
        int ranHairColour = Random.Range(0, hairColours.Count);

        gobbomation.selectedBaseSprites = bases[randBase].sprite;
        gobbomation.selectedEarSprites = ears[randEar].sprite;
        gobbomation.selectedHairSprites = hairs[randHair].sprite;
        gobbomation.selectedClothesSprites = clothes[randClothes].sprite;

        gobbomation.baseRenderer.color = baseColours[randBaseColour];
        gobbomation.earRenderer.color = baseColours[randBaseColour];
        gobbomation.hairRenderer.color = hairColours[ranHairColour];
    }



    public void SetSpecialGobbo()
    {
        //set special gobbo
    }
    
    [System.Serializable]
    public class Base
    {
        public Sprite[] sprite;
    }
    [System.Serializable]
    public class Hair
    {
        public Sprite[] sprite;
    }
    [System.Serializable]
    public class Clothes
    {
        public Sprite[] sprite;
    }
    [System.Serializable]
    public class Ear
    {
        public Sprite[] sprite;
    }
}
