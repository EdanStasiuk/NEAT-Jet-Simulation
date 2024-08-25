using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatNetwork
{
   public NeatGenome myGenome;
   public List<Node> nodes;
   public List<Node> inputNodes;
   public List<Node> outputNodes;
   public List<Node> hiddenNodes;
   public List<Connection> connections;
   public float fitness;

   public NeatNetwork(int input, int output, int hidden)
   {
        myGenome = CreateInitialGenome(input, output, hidden);
        nodes = new List<Node>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();
        hiddenNodes = new List<Node>();
        connections = new List<Connection>();
        CreateNetwork();
   }

   public NeatNetwork(NeatGenome genome)
   {
        myGenome = genome;
        nodes = new List<Node>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();
        hiddenNodes = new List<Node>();
        connections = new List<Connection>();
        CreateNetwork();
   }

   private NeatGenome CreateInitialGenome(int input, int output, int hidden)
   {
        List<NodeGene> newNodeGenes = new List<NodeGene>();
        List<ConnectionGene> newConnectionGenes = new List<ConnectionGene>();
        int nodeId = 0;

        for (int i = 0; i < input; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeId, NodeGene.TYPE.Input);
            newNodeGenes.Add(newNodeGene);
            nodeId += 1;
        }

        for (int i = 0; i < output; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeId, NodeGene.TYPE.Output);
            newNodeGenes.Add(newNodeGene);
            nodeId += 1;
        }

        for (int i = 0; i < hidden; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeId, NodeGene.TYPE.Hidden);
            newNodeGenes.Add(newNodeGene);
            nodeId += 1;
        }
 
        NeatGenome newGenome = new NeatGenome(newNodeGenes, newConnectionGenes);

        return newGenome;
   }

   private void CreateNetwork()
   {
        ResetNetwork();

        // Creation of network structure: nodes
        foreach(NodeGene nodeGene in myGenome.nodeGenes)
        {
            Node newNode = new Node(nodeGene.id);
            nodes.Add(newNode);
            if (nodeGene.type == NodeGene.TYPE.Input)
            {
                inputNodes.Add(newNode);
            }
            else if (nodeGene.type == NodeGene.TYPE.Hidden)
            {
                hiddenNodes.Add(newNode);
            }
            else if (nodeGene.type == NodeGene.TYPE.Output)
            {
                outputNodes.Add(newNode);
            }
        }

        // Creation of network structure: edges
        foreach(ConnectionGene connectionGene in myGenome.connectionGenes)
        {
            if (connectionGene.isActive)
            {
                Connection newConnection = new Connection(connectionGene.inputNode, connectionGene.outputNode, connectionGene.weight, connectionGene.isActive);
                connections.Add(newConnection);
            }
        }

        // Creation of network structure: node neighbours
        foreach(Node node in nodes)
        {
            foreach(Connection connection in connections)
            {
                if (connection.inputNode == node.id)
                {
                    node.outputConnections.Add(connection);
                }
                else if (connection.outputNode == node.id)
                {
                    node.inputConnections.Add(connection);
                }
            }
        }
   }

   private void ResetNetwork()
   {
        nodes.Clear();
        inputNodes.Clear();
        outputNodes.Clear();
        hiddenNodes.Clear();
        connections.Clear();
   }

   public void MutateNetwork()
   {
        myGenome.MutateGenome();
        CreateNetwork();
   }

   // Main driver function for the neural network
   public float[] FeedForwardNetwork(float[] inputs)
   {
        float[] outputs = new float[outputNodes.Count];

        for (int i = 0; i < inputNodes.Count; i++)
        {
            inputNodes[i].SetInputNodeValue(inputs[i]);
            inputNodes[i].FeedForwardValue();
            inputNodes[i].value = 0;
        }

        for  (int i = 0; i < hiddenNodes.Count; i++)
        {
            hiddenNodes[i].SetHiddenNodeValue();
            hiddenNodes[i].FeedForwardValue();
            hiddenNodes[i].value = 0;
        }

        for (int i = 0; i < outputNodes.Count; i++)
        {
            outputNodes[i].SetOutputNodeValue();
            outputs[i] = outputNodes[i].value;
            outputNodes[i].value = 0;
        }

        return outputs;
   }
}

public class Node
{
    public int id;
    public float value;
    public List<Connection> inputConnections;
    public List<Connection> outputConnections;

    public Node(int id)
    {
        this.id = id;
        inputConnections = new List<Connection>();
        outputConnections = new List<Connection>();
    }

    public void SetInputNodeValue(float value)
    {
        this.value = Sigmoid(value);
    }

    public void SetHiddenNodeValue()
    {
        float value = 0;
        foreach (Connection connection in inputConnections)
        {
            value += (connection.weight * connection.inputNodeValue);
        }

        this.value = TanH(value);
    }
    
    public void SetOutputNodeValue()
    {
        float value = 0;
        foreach (Connection connection in inputConnections)
        {
            value += (connection.weight * connection.inputNodeValue);
        }

        this.value = TanH(value);
    }

    public void FeedForwardValue()
    {
        foreach(Connection connection in outputConnections)
        {
            connection.inputNodeValue = value;
        }
    }

    // Activate functions
    private float Sigmoid(float x)
    {
        return (1 / (1 + Mathf.Exp(-x)));
    }

    private float TanH(float x)
    {
        return (2 / (1 + Mathf.Exp(-2 * x)) - 1);
    }

    private float TanHMod1(float x)
    {
        return ((2 / (1 + Mathf.Exp(-4 * x))) - 1);
    }
}

public class Connection
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isActive;
    public float inputNodeValue;

    public Connection(int inputNode, int outputNode, float weight, bool isActive)
    {
        this.inputNode = inputNode;
        this.outputNode = outputNode;
        this.weight = weight;
        this.isActive = isActive;
    }
}