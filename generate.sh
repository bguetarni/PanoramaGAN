#!/bin/bash
if [ "$#" -eq 0 ]
then
	echo "Enter number of images to consider"
	read nb
else
	nb="$1"
fi
if ! [[ "$nb" =~ ^[0-9]+$ ]]
then
	echo "Input passed not a number"
else
	echo "begin panorama cration" > log.txt
	for sample in $(ls data)
	do
		if [ -e data/ground_truth/"$sample"/out.jpg ]
		then
			echo "$sample skipped"
		else
			echo "$sample"
			ls -d data/unblurred/"$sample"/* | head -n "$nb" | xargs ./image-stitching
			if [ -e "out.jpg" ]
			then
				mv out.jpg data/ground_truth/"$sample"/
			else
				echo "$sample panorama failed" >> log.txt
			fi
		fi
	done
fi
