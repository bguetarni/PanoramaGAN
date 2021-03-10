#!/bin/bash
for sample in $(ls data/unblurred/)
do
	if [ -e /data/panorama_15/inputs/"$sample" ]
	then
		echo "$sample" skipped
	else
		echo "$sample"
		ls -d data/unblurred/"$sample"/* | xargs ./image-stitching
		if [ -e "out.jpg" ]
		then
			mkdir /data/panorama_15/inputs/"$sample"
			cp data/blurred/"$sample"/* /data/panorama_15/inputs/"$sample"/
			mv out.jpg /data/panorama_15/outputs/"$sample".jpg
		else
			echo "$sample" panorama failed
		fi
	fi
done
