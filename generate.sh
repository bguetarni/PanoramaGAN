#!/bin/bash
echo "Enter number of images to consider"
read nb
if ! [[ "$nb" =~ ^[0-9]+$ ]]
then
	echo "Input passed not a number"
else
	for sample in $(ls data)
	do
		echo ${sample}
		ls -d data/$sample/ground_truth/* | head -n "$nb" | xargs ./image-stitching
		mv out.jpg data/$sample/blurred/
	done
fi
