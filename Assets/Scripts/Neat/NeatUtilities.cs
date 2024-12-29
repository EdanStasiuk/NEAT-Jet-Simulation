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
        // TODO: There is probably a better way of doing so that the program can run with WebGL, but
        // this simple fix is good enough for now since the save data seems to be about as good as the
        // network is going to get as it is currently set up.
        
        // Use this for natively running within Unity:
        // string genomeString = File.ReadAllText(Application.dataPath + "/save.txt");

        // Use this for WebGL builds:
        string genomeString = @"{
            ""nodeGenes"": [
                {""id"":0,""type"":0}, {""id"":1,""type"":0}, {""id"":2,""type"":0},
                {""id"":3,""type"":0}, {""id"":4,""type"":0}, {""id"":5,""type"":0},
                {""id"":6,""type"":1}, {""id"":7,""type"":1}, {""id"":8,""type"":1},
                {""id"":9,""type"":1}, {""id"":10,""type"":2}, {""id"":11,""type"":2},
                {""id"":12,""type"":2}
            ],
            ""connectionGenes"": [
                {""inputNode"":2,""outputNode"":9,""weight"":-0.20087483525276185,""isActive"":false,""innovNum"":1},
                {""inputNode"":0,""outputNode"":8,""weight"":-0.855819582939148,""isActive"":true,""innovNum"":2},
                {""inputNode"":4,""outputNode"":8,""weight"":0.47371989488601687,""isActive"":true,""innovNum"":3},
                {""inputNode"":3,""outputNode"":8,""weight"":-1.2001075744628907,""isActive"":true,""innovNum"":4},
                {""inputNode"":2,""outputNode"":10,""weight"":0.2648876905441284,""isActive"":true,""innovNum"":5},
                {""inputNode"":10,""outputNode"":9,""weight"":0.7483364939689636,""isActive"":false,""innovNum"":6},
                {""inputNode"":3,""outputNode"":10,""weight"":-0.6453731060028076,""isActive"":true,""innovNum"":7},
                {""inputNode"":4,""outputNode"":9,""weight"":-1.4624354839324952,""isActive"":false,""innovNum"":8},
                {""inputNode"":10,""outputNode"":11,""weight"":0.9492435455322266,""isActive"":true,""innovNum"":9},
                {""inputNode"":11,""outputNode"":9,""weight"":0.5852257013320923,""isActive"":true,""innovNum"":10},
                {""inputNode"":10,""outputNode"":6,""weight"":-0.33515089750289919,""isActive"":true,""innovNum"":11},
                {""inputNode"":5,""outputNode"":7,""weight"":-0.6649156808853149,""isActive"":true,""innovNum"":12},
                {""inputNode"":4,""outputNode"":11,""weight"":0.652118980884552,""isActive"":true,""innovNum"":13},
                {""inputNode"":2,""outputNode"":11,""weight"":-0.5490822196006775,""isActive"":true,""innovNum"":14},
                {""inputNode"":1,""outputNode"":10,""weight"":-0.44497519731521609,""isActive"":true,""innovNum"":15},
                {""inputNode"":11,""outputNode"":7,""weight"":0.7888258695602417,""isActive"":true,""innovNum"":16},
                {""inputNode"":4,""outputNode"":10,""weight"":0.6561046242713928,""isActive"":true,""innovNum"":17},
                {""inputNode"":4,""outputNode"":12,""weight"":0.677369236946106,""isActive"":true,""innovNum"":18},
                {""inputNode"":12,""outputNode"":9,""weight"":-0.9692157506942749,""isActive"":true,""innovNum"":19},
                {""inputNode"":5,""outputNode"":8,""weight"":-0.5081489682197571,""isActive"":true,""innovNum"":20},
                {""inputNode"":3,""outputNode"":12,""weight"":-0.8913705945014954,""isActive"":true,""innovNum"":21},
                {""inputNode"":2,""outputNode"":12,""weight"":0.46772655844688418,""isActive"":true,""innovNum"":22}
            ]
        }";

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
