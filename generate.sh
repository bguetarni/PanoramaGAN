#!/bin/bash
echo "Enter number of images to consider"
read nb
if ! [[ "$nb" =~ ^[0-9]+$ ]]
then
	echo "Input passed not a number"
else
	echo "begin panorama cration" > log.txt
	for sample in $(ls data)
	do
		if [ -e "data/$sample/blurred/out.jpg" ]
		then
			echo "$sample skipped"
		else
			echo "$sample"
			ls -d data/$sample/ground_truth/* | head -n "$nb" | xargs ./image-stitching
			if [ -e "out.jpg" ]
			then
				mv out.jpg data/$sample/blurred/
			else
				echo "$sample panorama failed" >> log.txt
			fi
		fi
	done
fi
