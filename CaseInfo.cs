using UnityEngine;

public class CaseInfo : MonoBehaviour
{
    public int iWorld = 0;
    public int jWorld = 0;
    public int i = 0;
    public int j = 0;
    public int NbSalle = -1;

    public bool IsBorderEX = false;
    public bool IsBorderIN = false;
                                        // N  , E    , S    ,  O
    public bool[] bMurEX = new bool[4] { false, false, false, false };
    public bool[] bMurIN = new bool[4] { false, false, false, false };
    public bool[] bDoorArray = new bool[4] { false, false, false, false };
    public bool porteEntree = false;

    public bool crawlerMark = false;
}
//phantasma /goria