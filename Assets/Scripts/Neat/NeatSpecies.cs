using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatSpecies
{
    public NeatGenome mascot;
    public List<NeatGenome> members;
    public Color speciesColor;
    public float speciesFitness;

    public NeatSpecies(NeatGenome startingMember)
    {
        mascot = startingMember;
        members = new List<NeatGenome>();
        members.Add(startingMember);
        speciesColor = new Color(Random.Range(0,255)/255f, Random.Range(0,255)/255f, Random.Range(0,255)/255f);
        speciesFitness = 0;
    }

}
