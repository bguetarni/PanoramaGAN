#!/bin/bash

for sample in $(ls /data/PanoramaGAN/panorama)
do
	rm /data/PanoramaGAN/panorama/"$sample"/16.png
done

