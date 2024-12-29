# NEAT-Jet-Simulation

## Jet AI Navigation with NEAT in Unity

This project implements a NEAT (NeuroEvolution of Augmenting Topologies) neural network to control an AI-driven jet in Unity. The AI navigates through waypoints while optimizing its path using sensor inputs and rewards for alignment and efficiency. This project explores the application of evolutionary algorithms to complex navigation tasks.

You can view the project online [here](https://edanstasiuk.github.io/NEAT-Jet-Simulation/).

### Network Structure

The neural network used for controlling the jet consists of the following layers:

1. **Input Nodes**:  
   The input layer of the network receives several sensory inputs that help guide the jet's navigation:
   
   - **Sensor 1**: Downward sensor that detects obstacles below the jet. This is based on a raycast, and the distance to the first obstacle encountered (if any) is used as input.
   - **Sensor 2**: Horizontal direction to the waypoint.
   - **Sensor 3**: Vertical direction to the waypoint.
   - **Sensor 4**: Forward direction to the waypoint.
   - **Sensor 5**: The angle between the jet's forward direction and the direction to the waypoint.

   These sensors work together to provide the jet with crucial information about its environment, its position relative to the waypoint, and any obstacles in its path.

2. **Output Nodes**:  
   The output layer of the network controls the jet’s movement, such as steering, acceleration, and other flight parameters.

### Findings and Future Work

While the current design has been successful in achieving waypoint traversal, the movement of the jet appears rigid and lacks a natural, fluid behavior. This limitation arises from using a single network per jet to control all aspects of the jet's movement.

I was inspired to do this project after watching this video [here](https://www.youtube.com/watch?v=hWOSx_9fpaQ) and plan on redesigning the network to use 3 networks per jet, as in the video, to emulate jet movement better. This will involve having two networks for controlling altitude, and a third for controlling the throttle.

By distributing responsibilities across multiple networks, the overall movement of the jet will likely become more organic and responsive to changes in the environment. This redesign is expected to improve the realism of the simulation and create more lifelike jet behaviors.
