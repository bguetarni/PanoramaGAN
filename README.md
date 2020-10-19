# PanoramaGAN

### Folders
- The blurred images (*inputs*) images are located in *Pictures/blurred_images*
- The panorama (*ground-truth*) is located in *./out.png*
- To produce the *ground-truth* we use a non-blurred version if the blurred images and a stitching-algorithm. These non-blurred iages are located in *Pictures/ground_truth*

The stitching algorithm is the executable *image-stitching* that has to be in the same folder as the file *config.cfg* to be used.
To produce a panorama from images, call the executable wit the list of images as arguments. This will produce the *out.png* panorama.

**Tip**: In order to produce these images, the *Fixed Timestep* in Unity must be set to **0.05** and the angle between the frames to **18**.