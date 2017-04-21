# Neat-For-Speed
A small neural network simulation project by Alexis and Seb. It was made for fun as a way to understand how neural networks work as well as a programming challenge. Though the project name bears NEAT, it does not currently include the NEAT implementation but  it would be neat ;) to have it at some point.

## The neural network
It is a fully connected layered neural network in the sense that each neuron of each layer is alway connected to every neuron in the next layer (no more no less). The activation function used on the neurons is TanH. The network can currently dynamically change the number of hidden layers and the number of neurons in the hidden layers. Finally there is only one Bias neuron set to 1 on the input layer. To process the network I use matrix math that I wrote.

## The genetic algorythm
At the end of each cycle, the algorythm selects the fittest neural network and, makes mutated copies and insert them back in all the population. There is no breeding between pairs of network. Their is a very small chance that the algorythm will add/remove a neuron or a full layer. If adding/removing a layer, it will always be the last one directly connected to the output and will be the same size as the output. The weight will be mutated (if they are at all) in one of five way (mutliplication, addition, opposite, random replacement, set to 0).
