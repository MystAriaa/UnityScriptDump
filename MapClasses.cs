using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapClasses : MonoBehaviour
{
    
}
public class Salle
{
    public List<Transform> CaseList = new List<Transform> { };
    public List<Transform> MurEXList = new List<Transform> { };
    public List<Transform> MurINList = new List<Transform> { };
    public List<Transform> DoorList = new List<Transform> { };

    public int nbDoor = 0;

    //public int[] SalleNb = { 0, 0 };

    public enum AttributionSalle { NonAttribue, Entree, Salle1, Salle2, Salle3, Salle4, Filler, Deleted }
    public enum ApparenceSalle { NonAttribue, Cuisine, Chambre, SalleDeBain, Coridor, Hall, Bureau, Debarras }
    public AttributionSalle AttribuSalle = AttributionSalle.NonAttribue;
    public ApparenceSalle ApparaSalle = ApparenceSalle.NonAttribue;
}

