using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatGenome
{
    public List<NodeGene> nodeGenes;
    public List<ConnectionGene> connectionGenes;

    public NeatGenome()
    {
        nodeGenes = new List<NodeGene>();
        connectionGenes = new List<ConnectionGene>();
    }

    public NeatGenome(List<NodeGene> nodeGenes, List<ConnectionGene> connectionGenes)
    {
        this.nodeGenes = nodeGenes;
        this.connectionGenes = connectionGenes;
    }

    public void MutateGenome()
    {
        float createEdgeChance = 90f;
        float createNodeChance = 10f;
        float chanceEdge = Random.Range(0f, 100f);
        float chanceNode = Random.Range(0f, 100f);

        if (chanceNode <= createNodeChance)
        {
            // Create random new node
            AddRandomNode();
        }

        if (chanceEdge <= createEdgeChance)
        {
            // Create random new edge
            AddRandomConnection();
        }

        MutateWeights();
    }

    private void AddRandomNode()
    {
        if (connectionGenes.Count != 0)
        {
            int randomConnection = Random.Range(0, connectionGenes.Count);
            ConnectionGene mutatingConnection = connectionGenes[randomConnection];
            int firstNode = mutatingConnection.inputNode;
            int secondNode = mutatingConnection.outputNode;

            // Disable the mutating connection
            mutatingConnection.isActive = false;

            int newId = GetNextNodeId();

            NodeGene newNode = new NodeGene(newId, NodeGene.TYPE.Hidden);
            nodeGenes.Add(newNode);

            int nextInnovNum = GetNextInnovationNumber();
            ConnectionGene firstNewConnection = new ConnectionGene(firstNode, newNode.id, 1f, true, nextInnovNum);
            connectionGenes.Add(firstNewConnection);

            nextInnovNum = GetNextInnovationNumber();
            ConnectionGene secondNewConnection = new ConnectionGene(newNode.id, secondNode,  mutatingConnection.weight, true, nextInnovNum);
            connectionGenes.Add(secondNewConnection);
        }
    }

    private int GetNextNodeId()
    {
        int nextId = 0;
        foreach (NodeGene node in nodeGenes)
        {
            if (nextId <= node.id)
            {
                nextId = node.id;
            }
        }

        nextId += 1;
        
        return nextId;
    }

    private bool AddRandomConnection()
    {
        int firstNode = Random.Range(0, nodeGenes.Count);
        int secondNode = Random.Range(0, nodeGenes.Count);
        NodeGene.TYPE firstType = nodeGenes[firstNode].type;
        NodeGene.TYPE secondType = nodeGenes[secondNode].type;

        if (firstType == secondType && firstType != NodeGene.TYPE.Hidden) // No nodes in the input layer should connect to each other, same for output layer
        {
            return AddRandomConnection();
        }

        foreach (ConnectionGene connection in connectionGenes)
        {
            // Already an edge between the nodes
            if ((firstNode == connection.inputNode && secondNode == connection.outputNode) 
                || (secondNode == connection.inputNode && firstNode == connection.outputNode))
            {
                return false;
            }
        }

        if (firstType == NodeGene.TYPE.Output || (firstType == NodeGene.TYPE.Hidden
            && secondType == NodeGene.TYPE.Input))
        {
            int temp = firstNode;
            firstNode = secondNode;
            secondNode = temp;

            firstType = nodeGenes[firstNode].type;
            secondType = nodeGenes[secondNode].type;
        }

        int innov = GetNextInnovationNumber();
        float weight = Random.Range(-1f, 1f);
        bool active = true;
        ConnectionGene newConnection = new ConnectionGene(firstNode, secondNode, weight, active, innov);
        connectionGenes.Add(newConnection);
        
        return true;
    }

    private int GetNextInnovationNumber()
    {
        int nextInnov = 0;
        foreach (ConnectionGene connection in connectionGenes)
        {
            if (nextInnov <= connection.innovNum)
            {
                nextInnov = connection.innovNum;
            }
        }

        nextInnov += 1;
        
        return nextInnov;
    }

    private void MutateWeights()
    { 
        float randomWeightChance = 5f;
        float perturbWeightChance = 95f;
        float chanceRandom = Random.Range(0f, 100f);
        float chancePerturb = Random.Range(0f, 100f);

        if (chanceRandom <= randomWeightChance)
        {
            RandomizeSingleWeight();
        }

        if (chancePerturb <= perturbWeightChance)
        {
            PerturbWeights();
        }
    }

    private void RandomizeSingleWeight()
    {
        if (connectionGenes.Count != 0)
        {
            int randomConnectionIndex = Random.Range(0, connectionGenes.Count);
            ConnectionGene connection = connectionGenes[randomConnectionIndex];
            connection.weight = Random.Range(-1f, 1f);
        }
    }

    private void PerturbWeights()
    {
        foreach (ConnectionGene connection in connectionGenes)
        {
            connection.weight = connection.weight + (Random.Range(-0.5f, 0.5f) * 0.5f);
        }
    }
    
}

public class NodeGene
{
    public  int id;
    public enum TYPE
    {
        Input, Output, Hidden
    };

    public TYPE type;

    public NodeGene(int givenID, TYPE givenType)
    {
        id = givenID;
        type = givenType;
    }
}

public class ConnectionGene
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isActive;
    public int innovNum;

    public ConnectionGene(int inputNode, int outputNode, float weight, bool isActive, int innovNum)
    {
        this.inputNode = inputNode;
        this.outputNode = outputNode;
        this.weight = weight;
        this.isActive = isActive;
        this.innovNum = innovNum;
    }
}