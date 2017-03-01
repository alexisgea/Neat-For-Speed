# Neat-For-Speed
Small car racing game where the aim is to race against neural networks. This is something I programmed for fun as a way to improve my skill in Unity and learn about neural networks and genetic algorythms. It is under the MIT license attached with this project.

Though the game has NEAT in it's name, the current neural network implementation is not the actual NEAT design but my goal is to implement it at some point.

Currently the neural network and genetic algorythm is of my own design after reading a little bit on how it works.

In the current version of the game the genetic algorythm will train a population of 100 continuously. It takes about 15 generations to get a working autonomous car. Eventually it will be possible to save the network, and then race against it. As well as visualize the working networks during the training.

## The neural network
It is a fully connected layered neural network in the sense that each neuron of each layer is alway connected to every neuron in the next layer (no more no less). The activation function used on the neurons is TanH. The network can currently dynamically change the number of hidden layers and the number of neurons in the hidden layers. Finally there is only one Bias neuron set to 1 on the input layer. To process the network I use matrix math that I wrote.

## The genetic algorythm
At the end of each cycle, the algorythm selects the fittest neural network and, makes mutated copies and insert them back in all the population. There is no breeding between pairs of network. Their is a very small chance that the algorythm will add/remove a neuron or a full layer. If adding/removing a layer, it will always be the last one directly connected to the output and will be the same size as the output. The weight will be mutated (if they are at all) in one of five way (mutliplication, addition, opposite, random replacement, set to 0). The algorythm also change race tracks randomly to avoid overfitting.
