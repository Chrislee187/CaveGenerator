Simple procedure floor plan generator, based on [Sebastian Lague's youtube series](https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9]).

# First Pass
Basic floor plan generation from random number samples and some simple smoothing. Drawn using gizmos for testing rather than a Mesh or Gameobjects (mesh coming next).

![alt text][samplev1]

[samplev1]: https://github.com/Chrislee187/CaveGenerator/blob/master/DocImages/samplev1.JPG "Output after the initial random generation and smoothing pass"


# Second Pass
Using [Marching Squares](https://en.wikipedia.org/wiki/Marching_squares#Basic_algorithm) to create to round out the edges and generate a proper mesh for the wall layout.

![alt text][samplev2]

[samplev2]: https://github.com/Chrislee187/CaveGenerator/blob/master/DocImages/samplev2.JPG "Output after creating a mesh using Marching Squares to round out the edges"