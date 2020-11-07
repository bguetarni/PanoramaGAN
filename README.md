# PanoramaGAN

The stitching algorithm is the executable *image-stitching* that has to be in the same folder as the file *config.cfg* to be used.
To produce a panorama from images, call the executable wit the list of images as arguments. This will produce the *out.png* panorama.\
A `generate.sh` script is given to automatically generate the panoramas for the dataset from the blurred images using the stitching algorithm.\
Notice that the dataset is located in the *data* folder.

**Tip**: In order to produce these images, the *Fixed Timestep* in Unity must be set to **0.05** and the angle between the frames to **18**.


### Examples of images
- The blurred images (*inputs*) images are located in *Pictures/blurred_images*
- The panorama (*ground-truth*) is located in *./out.png*
- To produce the *ground-truth* we use a non-blurred version if the blurred images and a stitching-algorithm. These non-blurred iages are located in *Pictures/ground_truth*


### Links
[OpenPano github](https://github.com/ppwwyyxx/OpenPano)

[OpenPano blog](http://ppwwyyxx.com/blog/2016/How-to-Write-a-Panorama-Stitcher/)


### Papers
[Automatic Panoramic Image Stitching using Invariant Features](http://matthewalunbrown.com/papers/ijcv2007.pdf)