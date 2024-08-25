using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NeatUtilities : MonoBehaviour
{
    public static float GetCompatabilityDistance(NeatGenome firstGenome, NeatGenome secondGenome, float c1, float c2, float c3)
    {
        (int, int, float) DistjointExcessAWD = GetDisjointAndExcess(firstGenome.connectionGenes, secondGenome.connectionGenes);
        
        int N = (firstGenome.connectionGenes.Count >= secondGenome.connectionGenes.Count) ? firstGenome.connectionGenes.Count : secondGenome.connectionGenes.Count;

        int disjoint = DistjointExcessAWD.Item1;
        int excess = DistjointExcessAWD.Item2;
        float aWD = DistjointExcessAWD.Item3;

        float compDis = ((c1 * excess)) + ((c2 * disjoint) / N) + (c3 * aWD);
        
        return compDis;
    }

    // Compatability distance related
    public static (int, int, float) GetDisjointAndExcess(List<ConnectionGene> firstCons, List<ConnectionGene> secondCons)
    {
        int disjoint = 0;
        int excess = 0;
        int breakPoint = 0;

        int firstGenomeEnd = 0;
        int secondGenomeEnd = 0;

        float aWD = 0; // Average weight distance
        int matchingCount = 0;

        if (firstCons.Count != 0)
        {
            firstGenomeEnd = firstCons[firstCons.Count - 1].innovNum;
        }

        if (secondCons.Count != 0)
        {
            secondGenomeEnd = secondCons[secondCons.Count - 1].innovNum;
        }
            
        if (firstGenomeEnd >= secondGenomeEnd)
        {
            breakPoint = secondGenomeEnd;
        }

        else if (secondGenomeEnd > firstGenomeEnd)
        {
            breakPoint = firstGenomeEnd;
        }

        foreach (ConnectionGene con in firstCons)
        {
            if (con.innovNum <= breakPoint)
            {
                (bool, float, float) isNotMatching = isDisjoint(secondCons, con.innovNum, firstCons);
                
                if (isNotMatching.Item1 == true)
                {
                    disjoint += 1;
                }

                else 
                {
                    // Genes are matching
                    aWD += System.Math.Abs(isNotMatching.Item2 - isNotMatching.Item3);
                    matchingCount += 1;
                }
            }

            else
            {
                excess += HasInnov(secondCons, con.innovNum) ? 0 : 1;
            }
        }

        if (matchingCount > 0)
        {
            aWD = aWD / matchingCount;
        }

        return (disjoint, excess, aWD);
    }

    private static (bool, float, float) isDisjoint(List<ConnectionGene> compConnections, int compInnov, List<ConnectionGene> baseConnections)
    {
        if (HasInnov(compConnections, compInnov))
        {
            ConnectionGene compCon = null;
            ConnectionGene baseCon = null;

            foreach (ConnectionGene con in compConnections)
            {
                if (con.innovNum == compInnov)
                {
                    compCon = con;
                    break;
                }
            }

            foreach (ConnectionGene con in baseConnections)
            {
                if (con.innovNum == compInnov)
                {
                    baseCon = con;
                    break;
                }
            }

            if (compCon.inputNode == baseCon.inputNode && compCon.outputNode == baseCon.outputNode)
            {
                return (false, baseCon.weight, compCon.weight);
            }

            else
            {
                return (true, 0, 0);
            }
        }

        else 
        {
            return (true, 0, 0);
        }
    }

    private static bool HasInnov(List<ConnectionGene> connections, int searchInnov)
    {
        foreach (ConnectionGene con in connections)
        {
            if (con.innovNum == searchInnov)
            {
                return true;
            }
        }

        return false;
    }

    // Save and load related
   public static void SaveGenome(NeatGenome genome)
   {
        NeatGenomeJson genomeJson = new NeatGenomeJson();
        foreach (NodeGene node in genome.nodeGenes)
        {
            NodeGeneJson nodeJson = new NodeGeneJson();
            nodeJson.id = node.id;
            nodeJson.type = (NodeGeneJson.TYPE) node.type;
            genomeJson.nodeGenes.Add(nodeJson);
        }

        foreach (ConnectionGene con in genome.connectionGenes)
        {
            ConnectionGeneJson conJson = new ConnectionGeneJson();
            conJson.inputNode = con.inputNode;
            conJson.outputNode = con.outputNode;
            conJson.weight = con.weight;
            conJson.isActive = con.isActive;
            conJson.innovNum = con.innovNum;
            genomeJson.connectionGenes.Add(conJson);
        }

        string json = JsonUtility.ToJson(genomeJson);
        File.WriteAllText(Application.dataPath + "/save.txt", json);
        // print(json);
   }

   public static NeatGenome LoadGenome()
   {
        string genomeString = File.ReadAllText(Application.dataPath + "/save.txt");
        NeatGenomeJson savedGenome = JsonUtility.FromJson<NeatGenomeJson>(genomeString);
        NeatGenome loadedGenome = new NeatGenome();
        foreach (NodeGeneJson savedNode in savedGenome.nodeGenes)
        {
            NodeGene newNode = new NodeGene(savedNode.id, (NodeGene.TYPE) savedNode.type);
            loadedGenome.nodeGenes.Add(newNode);
        }

        foreach (ConnectionGeneJson savedCon in savedGenome.connectionGenes)
        {
            ConnectionGene newConnection = new ConnectionGene(savedCon.inputNode, savedCon.outputNode, savedCon.weight, savedCon.isActive, savedCon.innovNum);
            loadedGenome.connectionGenes.Add(newConnection);
        }

        return loadedGenome;
   }
}

[System.Serializable]
public class NeatGenomeJson
{
    public List<NodeGeneJson> nodeGenes = new List<NodeGeneJson>();
    public List<ConnectionGeneJson> connectionGenes = new List<ConnectionGeneJson>();
}

[System.Serializable]
public class NodeGeneJson
{
    public  int id;
    public enum TYPE
    {
        Input, Output, Hidden
    };

    public TYPE type;
}

[System.Serializable]
public class ConnectionGeneJson
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isActive;
    public int innovNum;
}
