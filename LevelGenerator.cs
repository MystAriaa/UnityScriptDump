using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    private bool UseSeed = false;
    [SerializeField]
    private int rSeed = 0;

    [SerializeField]
    private int GrilleXInput = 5;
    private int GrilleX;

    [SerializeField]
    private int GrilleYInput = 5;
    private int GrilleY;

    [SerializeField]
    private float CaseSize = 1f;

    [SerializeField]
    private Transform Map;

    [SerializeField]
    private Transform solDefaultPrefab;

    [SerializeField]
    private Transform murDefaultPrefab;

    [SerializeField]
    private Transform porteDefaultPrefab;

    [SerializeField]
    private Transform porteEntreeDefaultPrefab;

    [SerializeField]
    private int DecoupeSalle = 3;

    private float CaseEspacement = 10f;

    private Transform[,] maGrille;
    private List<List<Salle>> maGrilleRoomizer = new List<List<Salle>> { };

    private Transform obj;

    private int rCaseRetirer;
    private int CenterX;
    private int CenterY;

    private int rRemoveSalle;
    private List<int[]> listeDesCaseInfoDeBordure = new List<int[]>();

    void Awake()
    {
        if (UseSeed)
        {
            Random.InitState(rSeed);
        }
        GrilleX = GrilleXInput * DecoupeSalle + 2;
        GrilleY = GrilleYInput * DecoupeSalle + 2;
        maGrille = new Transform[GrilleX, GrilleY];
        CenterX = GrilleX / 2;
        CenterY = GrilleY / 2;
    }

    void Start()
    {
        CreationGrille();
        SuppresionDesBordsInitials();
        AttributionDeLEspace();
        RemoveSomeSalle();
        DeterminationMurEX();
        FusionSalle();
        DeterminationMurIN();
        CleaningMapArray();
        CheckUpMur();
        DeterminationPorte(); //+RegulationSpawnPorte();
        DeterminationPorteDEntre();

        ColorisationCase();
        CrawlerCheck(); //+CreationPorteSiSalleExclu avec CrawlerCheck en appel recursif

        CreationMurEX();
        CreationMurIN();
        CreationPorteIN();
        CreationPorteEntree();

        DeterminationApparenceSalle();

        ColorisationCase();
    }

    void CreationGrille()
    {
        CaseEspacement = 10 * CaseSize;

        for (int i = 0; i < GrilleX; i++)
        {
            for (int j = 0; j < GrilleY; j++)
            {
                obj = Instantiate(solDefaultPrefab, new Vector3(1 * i * CaseEspacement, 0, 1 * j * CaseEspacement), Quaternion.identity, Map);
                obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                obj.gameObject.GetComponent<CaseInfo>().iWorld = i;
                obj.gameObject.GetComponent<CaseInfo>().jWorld = j;
                maGrille[i, j] = obj;
            }
        }
    }

    void SuppresionDesBordsInitials()
    {
        //Suppression des bords
        for (int i = 0; i < GrilleX; i++)
        {
            for (int j = 0; j < GrilleY; j++)
            {
                if (maGrille[i, j] != null)
                {
                    if (i == 0 || i == GrilleX - 1 || j == 0 || j == GrilleY - 1)
                    {
                        Destroy(maGrille[i, j].gameObject);
                        maGrille[i, j] = null;
                    }
                }
            }
        }
    }

    void AttributionDeLEspace()
    {
        int a = 0;
        for (int i = 1; i < GrilleX - DecoupeSalle; i = i + DecoupeSalle)
        {
            List<Salle> tList1 = new List<Salle> { };
            for (int j = 1; j < GrilleY - DecoupeSalle; j = j + DecoupeSalle)
            {
                a++;
                if (maGrille[i, j] != null)
                {
                    List<Transform> tList2 = new List<Transform> { };
                    for (int y = 0; y < DecoupeSalle; y++)
                    {
                        for (int z = 0; z < DecoupeSalle; z++)
                        {
                            maGrille[i + y, j + z].gameObject.GetComponent<CaseInfo>().NbSalle = a;
                            maGrille[i + y, j + z].gameObject.GetComponent<CaseInfo>().i = y;
                            maGrille[i + y, j + z].gameObject.GetComponent<CaseInfo>().j = z;
                            tList2.Add(maGrille[i + y, j + z]);
                        }
                    }
                    Salle room = new Salle();
                    room.CaseList = tList2;
                    tList1.Add(room);
                }
            }
            maGrilleRoomizer.Add(tList1);
        }

        //Determination de l'entrée
        int rR = Random.Range(-1, 2);
        for (int k = 0; k < maGrilleRoomizer[(int)(maGrilleRoomizer[0].Count() / 2) + rR][0].CaseList.Count; k++)
        {
            maGrilleRoomizer[(int)(maGrilleRoomizer[0].Count() / 2) + rR][0].AttribuSalle = Salle.AttributionSalle.Entree;
        }

        for (int i = 0; i < maGrilleRoomizer.Count(); i++)
        {
            for (int j = 0; j < maGrilleRoomizer[i].Count(); j++)
            {
                rR = Random.Range(1, 5);
                if (rR == 1 && maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.NonAttribue)
                {
                    maGrilleRoomizer[i][j].AttribuSalle = Salle.AttributionSalle.Salle1;
                }
                if (rR == 2 && maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.NonAttribue)
                {
                    maGrilleRoomizer[i][j].AttribuSalle = Salle.AttributionSalle.Salle2;
                }
                if (rR == 3 && maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.NonAttribue)
                {
                    maGrilleRoomizer[i][j].AttribuSalle = Salle.AttributionSalle.Salle3;
                }
                if (rR == 4 && maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.NonAttribue)
                {
                    maGrilleRoomizer[i][j].AttribuSalle = Salle.AttributionSalle.Salle4;
                }
            }
        }
    }

    void RemoveSomeSalle()
    {
        rRemoveSalle = Random.Range(1, 4 + (int)((GrilleXInput + GrilleYInput) / 8));
        while (rRemoveSalle != 0)
        {
            int rI = Random.Range(0, maGrilleRoomizer.Count);
            int rJ = Random.Range(0, maGrilleRoomizer[0].Count);
            if (rI == 0 || rI == maGrilleRoomizer.Count || rJ == 0 || rJ == maGrilleRoomizer[0].Count)
            {
                if (maGrilleRoomizer[rI][rJ].AttribuSalle != Salle.AttributionSalle.Entree)
                {
                    maGrilleRoomizer[rI][rJ].AttribuSalle = Salle.AttributionSalle.Deleted;
                    foreach (Transform cases in maGrilleRoomizer[rI][rJ].CaseList) { cases.gameObject.GetComponent<CaseInfo>().NbSalle = -1; Destroy(cases.gameObject); }
                    maGrilleRoomizer[rI][rJ].CaseList.Clear();
                    rRemoveSalle--;
                }
            }
        }
    }

    void DeterminationMurEX()
    {
        for (int i = 0; i < maGrilleRoomizer.Count(); i++)
        {
            for (int j = 0; j < maGrilleRoomizer[i].Count(); j++)
            {
                if (i == 0)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().i == 0)
                        { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[3] = true; } }
                }
                if (i == maGrilleRoomizer.Count() - 1)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().i == DecoupeSalle - 1)
                        { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[1] = true; } }
                }
                if (j == 0)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().j == 0)
                        { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[2] = true; } }
                }
                if (j == maGrilleRoomizer[0].Count() - 1)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().j == DecoupeSalle - 1)
                        { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[0] = true; } }
                }
                // Border
                //-------------
                // EX
                if (i + 1 < maGrilleRoomizer.Count()) { if (maGrilleRoomizer[i + 1][j].AttribuSalle == Salle.AttributionSalle.Deleted)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().i == DecoupeSalle - 1)
                            { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[1] = true; } }
                    }
                }
                if (j + 1 < maGrilleRoomizer[i].Count()) { if (maGrilleRoomizer[i][j + 1].AttribuSalle == Salle.AttributionSalle.Deleted)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().j == DecoupeSalle - 1)
                            { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[0] = true; } }
                    }
                }
                if (i - 1 >= 0) { if (maGrilleRoomizer[i - 1][j].AttribuSalle == Salle.AttributionSalle.Deleted)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().i == 0)
                            { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[3] = true; } }
                    }
                }
                if (j - 1 >= 0) { if (maGrilleRoomizer[i][j - 1].AttribuSalle == Salle.AttributionSalle.Deleted)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { if (cases.gameObject.GetComponent<CaseInfo>().j == 0)
                            { cases.gameObject.GetComponent<CaseInfo>().IsBorderEX = true; cases.gameObject.GetComponent<CaseInfo>().bMurEX[2] = true; } }
                    }
                }
            }
        }
    }

    void FusionSalle()
    {
        for (int i = 0; i < maGrilleRoomizer.Count(); i++)
        {
            for (int j = 0; j < maGrilleRoomizer[i].Count(); j++)
            {

                if (i + 1 < maGrilleRoomizer.Count())
                {
                    if (maGrilleRoomizer[i + 1][j].AttribuSalle == maGrilleRoomizer[i][j].AttribuSalle)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i + 1][j].CaseList)
                        {
                            maGrilleRoomizer[i][j].CaseList.Add(cases);
                            cases.gameObject.GetComponent<CaseInfo>().NbSalle = maGrilleRoomizer[i][j].CaseList[0].gameObject.GetComponent<CaseInfo>().NbSalle;
                        }
                        maGrilleRoomizer[i + 1][j].CaseList.Clear();
                    }
                }
                if (j + 1 < maGrilleRoomizer[i].Count())
                {
                    if (maGrilleRoomizer[i][j + 1].AttribuSalle == maGrilleRoomizer[i][j].AttribuSalle)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i][j + 1].CaseList)
                        {
                            maGrilleRoomizer[i][j].CaseList.Add(cases);
                            cases.gameObject.GetComponent<CaseInfo>().NbSalle = maGrilleRoomizer[i][j].CaseList[0].gameObject.GetComponent<CaseInfo>().NbSalle;
                        }
                        maGrilleRoomizer[i][j + 1].CaseList.Clear();
                    }
                }
                if (i - 1 >= 0)
                {
                    if (maGrilleRoomizer[i - 1][j].AttribuSalle == maGrilleRoomizer[i][j].AttribuSalle)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i - 1][j].CaseList)
                        {
                            maGrilleRoomizer[i][j].CaseList.Add(cases);
                            cases.gameObject.GetComponent<CaseInfo>().NbSalle = maGrilleRoomizer[i][j].CaseList[0].gameObject.GetComponent<CaseInfo>().NbSalle;
                        }
                        maGrilleRoomizer[i - 1][j].CaseList.Clear();
                    }
                }
                if (j - 1 >= 0)
                {
                    if (maGrilleRoomizer[i][j - 1].AttribuSalle == maGrilleRoomizer[i][j].AttribuSalle)
                    {
                        foreach (Transform cases in maGrilleRoomizer[i][j - 1].CaseList)
                        {
                            maGrilleRoomizer[i][j].CaseList.Add(cases);
                            cases.gameObject.GetComponent<CaseInfo>().NbSalle = maGrilleRoomizer[i][j].CaseList[0].gameObject.GetComponent<CaseInfo>().NbSalle;
                        }
                        maGrilleRoomizer[i][j - 1].CaseList.Clear();
                    }
                }
            }
        }
    }

    void DeterminationMurIN()
    {
        for (int i = 0; i < GrilleX; i++)
        {
            for (int j = 0; j < GrilleY; j++)
            {
                if (maGrille[i, j] != null)
                {
                    if (maGrille[i + 1, j] != null) //vers droite
                    {
                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle != maGrille[i + 1, j].gameObject.GetComponent<CaseInfo>().NbSalle && maGrille[i + 1, j].gameObject.GetComponent<CaseInfo>().NbSalle != -1)
                        {
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN = true;
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[1] = true;
                        }
                    }
                    if (maGrille[i - 1, j] != null) //vers gauche
                    {
                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle != maGrille[i - 1, j].gameObject.GetComponent<CaseInfo>().NbSalle && maGrille[i - 1, j].gameObject.GetComponent<CaseInfo>().NbSalle != -1)
                        {
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN = true;
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[3] = true;
                        }
                    }
                    if (maGrille[i, j + 1] != null) //vers haut
                    {
                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle != maGrille[i, j + 1].gameObject.GetComponent<CaseInfo>().NbSalle && maGrille[i, j + 1].gameObject.GetComponent<CaseInfo>().NbSalle != -1)
                        {
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN = true;
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[0] = true;
                        }
                    }
                    if (maGrille[i, j - 1] != null) // vers bas
                    {
                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle != maGrille[i, j - 1].gameObject.GetComponent<CaseInfo>().NbSalle && maGrille[i, j - 1].gameObject.GetComponent<CaseInfo>().NbSalle != -1)
                        {
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN = true;
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[2] = true;
                        }
                    }
                }
            }
        }
    }

    void CheckUpMur()
    {
        for (int i = 0; i < maGrilleRoomizer.Count(); i++)
        {
            for (int j = 0; j < maGrilleRoomizer[i].Count(); j++)
            {
                foreach (Transform cases in maGrilleRoomizer[i][j].CaseList)
                {
                    int flagCount = 0;
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[0]) { flagCount++; }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[2]) { flagCount++; }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[3]) { flagCount++; }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[1]) { flagCount++; }
                    if (flagCount > 2) { Debug.LogError("Error Mur, plus de 3 mur sur une case INTERIEUR"); }
                    flagCount = 0;
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurEX[0]) { flagCount++; }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurEX[2]) { flagCount++; }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurEX[3]) { flagCount++; }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurEX[1]) { flagCount++; }
                    if (flagCount > 2) { Debug.LogError("Error Mur, plus de 3 mur sur une case EXTERIEUR"); }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[0] == cases.gameObject.GetComponent<CaseInfo>().bMurEX[0] && cases.gameObject.GetComponent<CaseInfo>().bMurIN[0] == true) { Debug.LogError("Error Mur, Superposition de mur EX/IN direction N"); }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[2] == cases.gameObject.GetComponent<CaseInfo>().bMurEX[2] && cases.gameObject.GetComponent<CaseInfo>().bMurIN[2] == true) { Debug.LogError("Error Mur, Superposition de mur EX/IN direction S"); }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[3] == cases.gameObject.GetComponent<CaseInfo>().bMurEX[3] && cases.gameObject.GetComponent<CaseInfo>().bMurIN[3] == true) { Debug.LogError("Error Mur, Superposition de mur EX/IN direction O"); }
                    if (cases.gameObject.GetComponent<CaseInfo>().bMurIN[1] == cases.gameObject.GetComponent<CaseInfo>().bMurEX[1] && cases.gameObject.GetComponent<CaseInfo>().bMurIN[1] == true) { Debug.LogError("Error Mur, Superposition de mur EX/IN direction E"); }
                }
            }
        }
    }

    void CleaningMapArray()
    {
        for (int i = 0; i < maGrilleRoomizer.Count(); i++)
        {
            for (int j = 0; j < maGrilleRoomizer[i].Count(); j++)
            {
                if (maGrilleRoomizer[i][j].CaseList.Count == 0)
                {
                    maGrilleRoomizer[i].Remove(maGrilleRoomizer[i][j]);
                }
            }
        }
    }

    void DeterminationPorte()
    {
        int rPorteIndex = 0;
        List<Transform> ListeDesCasesMurals = new List<Transform> { };
        for (int i = 0; i < maGrilleRoomizer.Count(); i++)
        {
            for (int j = 0; j < maGrilleRoomizer[i].Count(); j++)
            {
                ListeDesCasesMurals.Clear();
                foreach (Transform cases in maGrilleRoomizer[i][j].CaseList)
                {
                    if (cases.gameObject.GetComponent<CaseInfo>().IsBorderIN)
                    {
                        ListeDesCasesMurals.Add(cases);
                    }
                }
                rPorteIndex = Random.Range(1, ListeDesCasesMurals.Count()-1);

                if (ListeDesCasesMurals.Count() != 0)
                {
                    if (ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[0])
                    {
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[0] = true;
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[0] = false;
                    }
                    if (ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[2])
                    {
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[2] = true;
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[2] = false;
                    }
                    if (ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[3])
                    {
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[3] = true;
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[3] = false;
                    }
                    if (ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[1])
                    {
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[1] = true;
                        ListeDesCasesMurals[rPorteIndex].gameObject.GetComponent<CaseInfo>().bMurIN[1] = false;
                    }
                }
            }
        }

        //
        RegulationSpawnPorte();
        //-------------------------------------
        //symetrie de la porte

        for (int i = 0; i < GrilleX; i++)
        {
            for (int j = 0; j < GrilleY; j++)
            {
                if (maGrille[i, j] != null)
                {
                    if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[1])
                    {
                        if (maGrille[i + 1, j] != null)
                        {
                            maGrille[i + 1, j].gameObject.GetComponent<CaseInfo>().bDoorArray[3] = true;
                            maGrille[i + 1, j].gameObject.GetComponent<CaseInfo>().bMurIN[3] = false;
                        }
                    }
                    if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[3])
                    {
                        if (maGrille[i - 1, j] != null)
                        {
                            maGrille[i - 1, j].gameObject.GetComponent<CaseInfo>().bDoorArray[1] = true;
                            maGrille[i - 1, j].gameObject.GetComponent<CaseInfo>().bMurIN[1] = false;
                        }
                    }
                    if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[0])
                    {
                        if (maGrille[i, j + 1] != null)
                        {
                            maGrille[i, j + 1].gameObject.GetComponent<CaseInfo>().bDoorArray[2] = true;
                            maGrille[i, j + 1].gameObject.GetComponent<CaseInfo>().bMurIN[2] = false;
                        }
                    }
                    if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[2])
                    {
                        if (maGrille[i, j - 1] != null)
                        {
                            maGrille[i, j - 1].gameObject.GetComponent<CaseInfo>().bDoorArray[0] = true;
                            maGrille[i, j - 1].gameObject.GetComponent<CaseInfo>().bMurIN[0] = false;
                        }
                    }
                }
            }
        }
    }

    void DeterminationPorteDEntre() //fct uniquement pour une porte au sud car entree uniquement sud
    {
        int countNbCasePotentielPourPorteEntree = 0;
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                if (Salles.AttribuSalle == Salle.AttributionSalle.Entree)
                {
                    foreach (Transform Cases in Salles.CaseList)
                    {
                        if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderEX)
                        {
                            countNbCasePotentielPourPorteEntree++;
                        }
                    }
                }
            }
        }

        int countdown = 0;
        if (countNbCasePotentielPourPorteEntree == DecoupeSalle)
        {
            countdown = (int)(countNbCasePotentielPourPorteEntree / 2);
        }
        else if (countNbCasePotentielPourPorteEntree == DecoupeSalle*2-1)
        {
            countdown = (int)(countNbCasePotentielPourPorteEntree / 4);
        }
        else if (countNbCasePotentielPourPorteEntree == DecoupeSalle * 3 - 2)
        {
            countdown = (int)(countNbCasePotentielPourPorteEntree / 2);
        }

        bool flagdone = false;
        bool flagDecalagePorte = false;
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                if (Salles.AttribuSalle == Salle.AttributionSalle.Entree)
                {
                    foreach (Transform Cases in Salles.CaseList)
                    {
                        if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderEX)
                        {
                            if (countdown == 0 && flagdone == false)
                            {
                                if (Cases.gameObject.GetComponent<CaseInfo>().bMurEX[2])
                                {
                                    Cases.gameObject.GetComponent<CaseInfo>().bMurEX[2] = false;
                                    Cases.gameObject.GetComponent<CaseInfo>().porteEntree = true;
                                }
                                else if (Cases.gameObject.GetComponent<CaseInfo>().bMurEX[3])
                                { // attention on est a gauche
                                    flagDecalagePorte = true;
                                }
                                flagdone = true;
                            }
                            else if (countdown != 0)
                            {
                                countdown--;
                            }
                        }
                    }
                }
            }
        }
        if (flagDecalagePorte)
        {
            flagdone = false;
            countdown = (int)(countNbCasePotentielPourPorteEntree / 4) + (int)(countNbCasePotentielPourPorteEntree / 2);
            foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
            {
                foreach (Salle Salles in ListOfSalle)
                {
                    if (Salles.AttribuSalle == Salle.AttributionSalle.Entree)
                    {
                        foreach (Transform Cases in Salles.CaseList)
                        {
                            if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderEX)
                            {
                                if (countdown == 0 && flagdone == false)
                                {
                                    if (Cases.gameObject.GetComponent<CaseInfo>().bMurEX[2])
                                    {
                                        Cases.gameObject.GetComponent<CaseInfo>().bMurEX[2] = false;
                                        Cases.gameObject.GetComponent<CaseInfo>().porteEntree = true;
                                    }
                                    flagdone = true;
                                }
                                else if (countdown != 0)
                                {
                                    countdown--;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void RegulationSpawnPorte()
    {
        int countNbPorte = 0;
        int countNbMurIN = 0;
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                if (Salles.AttribuSalle == Salle.AttributionSalle.Entree)
                {
                    foreach (Transform Cases in Salles.CaseList)
                    {
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[0])
                        {
                            countNbPorte++;
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[1])
                        {
                            countNbPorte++;
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[2])
                        {
                            countNbPorte++;
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[3])
                        {
                            countNbPorte++;
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderIN)
                        {
                            countNbMurIN++;
                        }
                    }
                }
            }
        }
        int rPorteEnPlusIndex = Random.Range(1, countNbMurIN - 1);
        bool onlyOnce = true;
        if (countNbPorte <= 1)
        {
            foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
            {
                foreach (Salle Salles in ListOfSalle)
                {
                    if (Salles.AttribuSalle == Salle.AttributionSalle.Entree)
                    {
                        for (int z = 0; z < Salles.CaseList.Count(); z++)
                        {
                            if (Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().IsBorderIN)
                            {
                                rPorteEnPlusIndex--;
                            }
                            if (rPorteEnPlusIndex == 0)
                            {
                                if (Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().IsBorderIN)
                                {
                                    if (Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[0])
                                    {
                                        if (onlyOnce)
                                        {
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bDoorArray[0] = true;
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[0] = false;
                                            onlyOnce = false;
                                        }
                                    }
                                    if (Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[1])
                                    {
                                        if (onlyOnce)
                                        {
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bDoorArray[1] = true;
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[1] = false;
                                            onlyOnce = false;
                                        }
                                    }
                                    if (Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[2])
                                    {
                                        if (onlyOnce)
                                        {
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bDoorArray[2] = true;
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[2] = false;
                                            onlyOnce = false;
                                        }
                                    }
                                    if (Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[3])
                                    {
                                        if (onlyOnce)
                                        {
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bDoorArray[3] = true;
                                            Salles.CaseList[z].gameObject.GetComponent<CaseInfo>().bMurIN[3] = false;
                                            onlyOnce = false;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        // Ajout d'une porte vers une autre salle à la salle d'entree si elle n'en possede  qu'une
        //----------------------------------------------------------------
        // Separation des portes spawnant cote à cote redondants

        int noteNbSalle = -5;
        int Iindex = 0;
        int Jindex = 0;

        for (int i = 0; i < GrilleX; i++)
        {
            for (int j = 0; j < GrilleY; j++)
            {
                if (maGrille[i, j] != null)
                {
                    if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[0] || maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[1] || maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[2] || maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[3])
                    {
                        noteNbSalle = maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle;

                        Iindex = i;
                        Jindex = j + 1;
                        if (maGrille[Iindex, Jindex] != null)
                        {
                            if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().NbSalle == noteNbSalle)
                            {
                                if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().bDoorArray[0])
                                {
                                    for (int k = 0; k <= 3; k++)
                                    {
                                        maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[k] = false;
                                    }
                                    maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[0] = true;
                                    Debug.Log("Suppresion d'une porte double en ("+ maGrille[i, j].gameObject.GetComponent<CaseInfo>().iWorld+ ","+ maGrille[i, j].gameObject.GetComponent<CaseInfo>().jWorld + ")");
                                }
                            }
                        }
                        Iindex = i;
                        Jindex = j - 1;
                        if (maGrille[Iindex, Jindex] != null)
                        {
                            if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().NbSalle == noteNbSalle)
                            {
                                if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().bDoorArray[2])
                                {
                                    for (int k = 0; k <= 3; k++)
                                    {
                                        maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[k] = false;
                                    }
                                    maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[2] = true;
                                    Debug.Log("Suppresion d'une porte double en (" + maGrille[i, j].gameObject.GetComponent<CaseInfo>().iWorld + "," + maGrille[i, j].gameObject.GetComponent<CaseInfo>().jWorld + ")");
                                }
                            }
                        }
                        Iindex = i + 1;
                        Jindex = j;
                        if (maGrille[Iindex, Jindex] != null)
                        {
                            if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().NbSalle == noteNbSalle)
                            {
                                if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().bDoorArray[1])
                                {
                                    for (int k = 0; k <= 3; k++)
                                    {
                                        maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[k] = false;
                                    }
                                    maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[1] = true;
                                    Debug.Log("Suppresion d'une porte double en (" + maGrille[i, j].gameObject.GetComponent<CaseInfo>().iWorld + "," + maGrille[i, j].gameObject.GetComponent<CaseInfo>().jWorld + ")");
                                }
                            }
                        }
                        Iindex = i - 1;
                        Jindex = j;
                        if (maGrille[Iindex, Jindex] != null)
                        {
                            if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().NbSalle == noteNbSalle)
                            {
                                if (maGrille[Iindex, Jindex].gameObject.GetComponent<CaseInfo>().bDoorArray[3])
                                {
                                    for (int k = 0; k <= 3; k++)
                                    {
                                        maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[k] = false;
                                    }
                                    maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[3] = true;
                                    Debug.Log("Suppresion d'une porte double en (" + maGrille[i, j].gameObject.GetComponent<CaseInfo>().iWorld + "," + maGrille[i, j].gameObject.GetComponent<CaseInfo>().jWorld + ")");
                                }
                            }
                        }
                    }

                }
            }
        }

    }

    void CrawlerCheck() //crawler salle qui check si toute sont relier en passant par les portes en checkant si toute les cases on ete check si nb checked = nb cases
    {
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                if (Salles.AttribuSalle == Salle.AttributionSalle.Entree)
                {
                    foreach (Transform Cases in Salles.CaseList)
                    {
                        if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderEX)
                        {
                            if (Cases.gameObject.GetComponent<CaseInfo>().porteEntree)
                            {
                                Cases.gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                            }
                        }
                    }
                }
            }
        }

        bool flagBoucle = true;
        int IIndex = 0;
        int JIndex = 0;

        while (flagBoucle)
        {
            flagBoucle = false;
            for (int i = 0; i < GrilleX; i++)
            {
                for (int j = 0; j < GrilleY; j++)
                {
                    if (maGrille[i, j] != null)
                    {
                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().crawlerMark == true)
                        {
                            IIndex = i;
                            JIndex = j + 1;
                            if (maGrille[IIndex, JIndex] != null)
                            {
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == false)
                                {
                                    //De toute a pas mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }
                                    //De pas mur a mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }

                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == true)
                                    {
                                        //A travers les portes
                                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[0] == true)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                        //Si meme nb de salle
                                        if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().NbSalle == maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                    }
                                }
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == true)
                                {
                                    maGrille[IIndex, JIndex].gameObject.GetComponent<Renderer>().material.color = Color.gray;
                                }
                            }
                            IIndex = i;
                            JIndex = j - 1;
                            if (maGrille[IIndex, JIndex] != null)
                            {
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == false)
                                {
                                    //De toute a pas mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }
                                    //De pas mur a mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }

                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == true)
                                    {
                                        //A travers les portes
                                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[2] == true)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                        //Si meme nb de salle
                                        if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().NbSalle == maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                    }
                                }
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == true)
                                {
                                    maGrille[IIndex, JIndex].gameObject.GetComponent<Renderer>().material.color = Color.gray;
                                }
                            }
                            IIndex = i + 1;
                            JIndex = j;
                            if (maGrille[IIndex, JIndex] != null)
                            {
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == false)
                                {
                                    //De toute a pas mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }
                                    //De pas mur a mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }

                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == true)
                                    {
                                        //A travers les portes
                                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[1] == true)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                        //Si meme nb de salle
                                        if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().NbSalle == maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                    }
                                }
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == true)
                                {
                                    maGrille[IIndex, JIndex].gameObject.GetComponent<Renderer>().material.color = Color.gray;
                                }
                            }
                            IIndex = i - 1;
                            JIndex = j;
                            if (maGrille[IIndex, JIndex] != null)
                            {
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == false)
                                {
                                    //De toute a pas mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }
                                    //De pas mur a mur
                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == false)
                                    {
                                        maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                        flagBoucle = true;
                                    }

                                    if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().IsBorderIN == true && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN == true)
                                    {
                                        //A travers les portes
                                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[3] == true)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                        //Si meme nb de salle
                                        if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().NbSalle == maGrille[i, j].gameObject.GetComponent<CaseInfo>().NbSalle)
                                        {
                                            maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark = true;
                                            flagBoucle = true;
                                        }
                                    }
                                }
                                if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark == true)
                                {
                                    maGrille[IIndex, JIndex].gameObject.GetComponent<Renderer>().material.color = Color.gray;
                                }
                            }
                        }
                    }
                }
            }
        }

        //Contage des cases non mark et mark
        int countCases = 0;
        int countCasesMarked = 0;
        for (int i = 0; i < GrilleX; i++)
        {
            for (int j = 0; j < GrilleY; j++)
            {
                if (maGrille[i, j] != null)
                {
                    countCases++;
                    if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().crawlerMark)
                    {
                        countCasesMarked++;
                    }
                }
            }
        }


        if (countCases != countCasesMarked)
        {
            Debug.LogError("! Des salles ne sont pas atteignable !");
            Debug.LogError("Nb de cases total: " + countCases + ", Nb de cases atteignable: " + countCasesMarked);
            CreationPorteSiSalleExclu();
            for (int i = 0; i < GrilleX; i++)
            {
                for (int j = 0; j < GrilleY; j++)
                {
                    if (maGrille[i, j] != null)
                    {
                        if (maGrille[i, j].gameObject.GetComponent<CaseInfo>().crawlerMark)
                        {
                            maGrille[i, j].gameObject.GetComponent<CaseInfo>().crawlerMark = false; //On demark pour retest si jamais
                        }
                    }
                }
            }
            CrawlerCheck();
        }
        else
        {
            Debug.Log("Map Validée");
        }
    }

    void CreationPorteSiSalleExclu()
    {
        int IIndex = 0;
        int JIndex = 0;
        bool flag = true;
        for (int i = 0; i < GrilleX; i++)
        {
            for (int j = 0; j < GrilleY; j++)
            {
                if (maGrille[i, j] != null && flag)
                {
                    if (!maGrille[i, j].gameObject.GetComponent<CaseInfo>().crawlerMark && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN)
                    {
                        //On cree une unique porte
                        IIndex = i;
                        JIndex = j + 1;
                        if (maGrille[IIndex, JIndex] != null)
                        {
                            if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN)
                            {
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[0] = true;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[2] = true;
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[0] = false;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bMurIN[2] = false;
                                flag = false;
                                Debug.Log("Creation d'une porte pour joindre 2 salles en (" + i + "," + j + ")");
                            }
                        }
                        IIndex = i;
                        JIndex = j - 1;
                        if (maGrille[IIndex, JIndex] != null)
                        {
                            if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN)
                            {
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[2] = true;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[0] = true;
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[2] = false;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bMurIN[0] = false;
                                flag = false;
                                Debug.Log("Creation d'une porte pour joindre 2 salles en (" + i + "," + j + ")");
                            }
                        }
                        IIndex = i + 1;
                        JIndex = j;
                        if (maGrille[IIndex, JIndex] != null)
                        {
                            if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN)
                            {
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[1] = true;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[3] = true;
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[1] = false;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bMurIN[3] = false;
                                flag = false;
                                Debug.Log("Creation d'une porte pour joindre 2 salles en (" + i + "," + j + ")");
                            }
                        }
                        IIndex = i - 1;
                        JIndex = j;
                        if (maGrille[IIndex, JIndex] != null)
                        {
                            if (maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().crawlerMark && maGrille[i, j].gameObject.GetComponent<CaseInfo>().IsBorderIN)
                            {
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bDoorArray[3] = true;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bDoorArray[1] = true;
                                maGrille[i, j].gameObject.GetComponent<CaseInfo>().bMurIN[3] = false;
                                maGrille[IIndex, JIndex].gameObject.GetComponent<CaseInfo>().bMurIN[1] = false;
                                flag = false;
                                Debug.Log("Creation d'une porte pour joindre 2 salles en (" + i + "," + j + ")");
                            }
                        }

                    }
                }
            }
        }
    }

    void CreationMurEX()
    {
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                foreach (Transform Cases in Salles.CaseList)
                {
                    if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderEX)
                    {
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurEX[0]) // N
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(0, 5f, 5f) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(-90, 0, 0);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                            //-------------------------------------------------
                            murRot = Quaternion.Euler(90, 0, 0);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurEXList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurEX[2]) // S
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(0, 5f, -5f) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(90, 0, 0);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                            //-------------------------------------------------
                            murRot = Quaternion.Euler(-90, 0, 0);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurEXList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurEX[1]) // E
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(5f, 5f, 0) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, 0, 90);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                            //-------------------------------------------------
                            murRot = Quaternion.Euler(0, 0, -90);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurEXList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurEX[3]) // O
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(-5f, 5f, 0) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, 0, -90);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                            //-------------------------------------------------
                            murRot = Quaternion.Euler(0, 0, 90);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurEXList.Add(obj);
                        }
                    }
                }
            }
        }
    }

    void CreationMurIN()
    {
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                foreach (Transform Cases in Salles.CaseList)
                {
                    if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderIN)
                    {
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurIN[0]) // N
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(0, 5f, 5f) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(-90, 0, 0);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurIN[2]) // S
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(0, 5f, -5f) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(90, 0, 0);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurIN[1]) // E
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(5f, 5f, 0) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, 0, 90);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bMurIN[3]) // O
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(-5f, 5f, 0) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, 0, -90);

                            obj = Instantiate(murDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, 1, CaseSize);
                            Salles.MurINList.Add(obj);
                        }
                    }
                }

            }
        }
    } 

    void CreationPorteIN()
    {
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                foreach (Transform Cases in Salles.CaseList)
                {
                    if (Cases.gameObject.GetComponent<CaseInfo>().IsBorderIN)
                    {
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[0]) // N
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(0, 5f, 5f) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, 0, 0);

                            obj = Instantiate(porteDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, CaseSize, 1);
                            Salles.DoorList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[2]) // S
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(0, 5f, -5f) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, 180, 0);

                            obj = Instantiate(porteDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, CaseSize, 1);
                            Salles.DoorList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[1]) // E
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(5f, 5f, 0) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, 90, 0);

                            obj = Instantiate(porteDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, CaseSize, 1);
                            Salles.DoorList.Add(obj);
                        }
                        if (Cases.gameObject.GetComponent<CaseInfo>().bDoorArray[3]) // O
                        {
                            Vector3 murPos = (Cases.localPosition + new Vector3(-5f, 5f, 0) * CaseSize);
                            Quaternion murRot = Quaternion.Euler(0, -90, 0);

                            obj = Instantiate(porteDefaultPrefab, murPos, murRot, Map);
                            obj.transform.localScale = new Vector3(CaseSize, CaseSize, 1);
                            Salles.DoorList.Add(obj);
                        }
                    }
                }

            }
        }
    }

    void CreationPorteEntree()
    {
        foreach (List<Salle> ListOfSalle in maGrilleRoomizer)
        {
            foreach (Salle Salles in ListOfSalle)
            {
                foreach (Transform Cases in Salles.CaseList)
                {
                    if (Cases.gameObject.GetComponent<CaseInfo>().porteEntree && Cases.gameObject.GetComponent<CaseInfo>().IsBorderEX)
                    {
                        Vector3 murPos = (Cases.localPosition + new Vector3(0, 5f, -5f) * CaseSize);
                        Quaternion murRot = Quaternion.Euler(0, 0, 0);

                        obj = Instantiate(porteEntreeDefaultPrefab, murPos, murRot, Map);
                        obj.transform.localScale = new Vector3(CaseSize, CaseSize, 1);
                        Salles.DoorList.Add(obj);
                        //---------------
                        murPos = (Cases.localPosition + new Vector3(0, 5f, -5f) * CaseSize);
                        murRot = Quaternion.Euler(0, 180, 0);

                        obj = Instantiate(porteDefaultPrefab, murPos, murRot, Map);
                        obj.transform.localScale = new Vector3(CaseSize, CaseSize, 1);
                        Salles.DoorList.Add(obj);
                    }
                }
            }
        }
    }






    void DeterminationApparenceSalle()
    {
        
    }

    void ColorisationCase()
    {
        for (int i = 0; i < maGrilleRoomizer.Count(); i++)
        {
            for (int j = 0; j < maGrilleRoomizer[i].Count(); j++)
            {
                if (maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.Salle1)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { cases.gameObject.GetComponent<Renderer>().material.color = Color.green; }
                }
                if (maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.Salle2)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { cases.gameObject.GetComponent<Renderer>().material.color = Color.blue; }
                }
                if (maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.Salle3)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { cases.gameObject.GetComponent<Renderer>().material.color = Color.yellow; }
                }
                if (maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.Salle4)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { cases.gameObject.GetComponent<Renderer>().material.color = Color.magenta; }
                }
                if (maGrilleRoomizer[i][j].AttribuSalle == Salle.AttributionSalle.Entree)
                {
                    foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) { cases.gameObject.GetComponent<Renderer>().material.color = Color.red; }
                }

                foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) //Mur IN
                {
                    if (cases.gameObject.GetComponent<CaseInfo>().IsBorderIN)
                    {
                        cases.gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                    }
                }
                /*foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) //Crawler
                {
                    if (cases.gameObject.GetComponent<CaseInfo>().crawlerMark)
                    {
                        cases.gameObject.GetComponent<Renderer>().material.color = Color.grey;
                    }
                }*/
                foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) //Mur EX
                {
                    if (cases.gameObject.GetComponent<CaseInfo>().IsBorderEX)
                    {
                        cases.gameObject.GetComponent<Renderer>().material.color = Color.white;
                    }
                }
                foreach (Transform cases in maGrilleRoomizer[i][j].CaseList) // Portes
                {
                    if (cases.gameObject.GetComponent<CaseInfo>().bDoorArray[0] || cases.gameObject.GetComponent<CaseInfo>().bDoorArray[2] || cases.gameObject.GetComponent<CaseInfo>().bDoorArray[1] || cases.gameObject.GetComponent<CaseInfo>().bDoorArray[3])
                    {
                        cases.gameObject.GetComponent<Renderer>().material.color = Color.black;
                    }
                }

                foreach (Transform murs in maGrilleRoomizer[i][j].MurINList) //Mur
                {
                    murs.gameObject.GetComponent<Renderer>().material.color = new Color(0.5f,0.5f,0.5f,1f);
                }
                foreach (Transform murs in maGrilleRoomizer[i][j].MurEXList) //Mur
                {
                    murs.gameObject.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                }
            }
        }
    }

}


/* ESPACE
 * 
 * Entree 1x1 :                            x1 max
 * 
 * Salle type 1 : Carre                    x2 mini
 * 
 * Salle type 2 : Rectangle 2x3 mini       x1 mini
 * 
 * Salle type 3 : 1x1 closets              x3 mini
 * 
 * Salle type 4 : L 2x2+2x1 mini           x1 mini
 * 
 * Reste type couloir (filler)
 * 
 * 
 */