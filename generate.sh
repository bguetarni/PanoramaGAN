#!/bin/bash
for sample in $(ls data)
do
	echo ${sample}
	ls -d data/$sample/ground_truth/* | xargs ./image-stitching
	mv out.jpg data/$sample/blurred/

done
