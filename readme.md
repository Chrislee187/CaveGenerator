Simple procedure cave floor plan generator, based on [Sebastian Lague's youtube series](https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9]).

The following images show the gradual build up and refinement of the cave as more functionality is added to the generator.

Please note that this images have been generated as I go along and therefore have ended up using different seeds and settings in each example image.

# Basic floorplan generation
Basic floor plan generation from random number samples and some simple smoothing. Drawn using gizmos for testing rather than a Mesh or Gameobjects (mesh coming next).

![alt text][samplev1]

[samplev1]: https://github.com/Chrislee187/CaveGenerator/blob/master/DocImages/samplev1.JPG "Output after the initial random generation and smoothing pass"


# Create a mesh for the floor plan
Using [Marching Squares](https://en.wikipedia.org/wiki/Marching_squares#Basic_algorithm) to create to round out the edges and generate a proper mesh for the wall layout.

![alt text][samplev2]

[samplev2]: https://github.com/Chrislee187/CaveGenerator/blob/master/DocImages/samplev2.JPG "Output after creating a mesh using Marching Squares to round out the edges"


# Create a mesh for the walls
Create a wall mesh for the floor plan from 

![alt text][samplev3]

[samplev3]: https://github.com/Chrislee187/CaveGenerator/blob/master/DocImages/samplev3.JPG "Output after creating adding an optional wall mesh"

# Remove small rooms and wall "islands"

Remove rooms and wall "islands" (a sealed outline of walls inside a room)  whose size fall below a threshold.

## Before

![alt text][samplev4a]

[samplev4a]: https://github.com/Chrislee187/CaveGenerator/blob/master/DocImages/samplev4a.JPG "Output before removing"

## After

![alt text][samplev4b]

[samplev4b]: https://github.com/Chrislee187/CaveGenerator/blob/master/DocImages/samplev4b.JPG "Output after removing"



