import os

path = 'data/'
in_path = '/data/panorama/inputs/'
out_path = '/data/panorama/outputs/'
dirs = os.listdir(path + 'unblurred/')
dirs.sort()
failed = []
for directory in dirs:
	images_list = os.listdir(path + 'unblurred/' + directory)
	images_list.sort()
	for i in range(len(images_list) - 1):
		sample_directory = directory + '_' + str(i+1).zfill(2)
		sample_not_in_inputs = sample_directory not in os.listdir(in_path)
		sample_not_in_outputs = sample_directory + '.jpg' not in os.listdir(out_path)
		if sample_not_in_inputs or sample_not_in_outputs:
			img1 = images_list[i]
			img2 = images_list[i + 1]
			unblurred_images_path = path + 'unblurred/' + directory + '/'
			os.system('./image-stitching {} {}'.format(unblurred_images_path + img1, unblurred_images_path + img2))
			if 'out.jpg' in os.listdir():
				os.system('mv out.jpg {}'.format(out_path + sample_directory + '.jpg'))
				try:
					os.mkdir(in_path + sample_directory)
				except FileExistsError:
					pass
				blurred_images_path = path + 'blurred/' + directory + '/'
				os.system('cp {} {}'.format(blurred_images_path + img1, in_path + sample_directory + '/' + img1))
				os.system('cp {} {}'.format(blurred_images_path + img2, in_path + sample_directory + '/' + img2))
			else:
				failed.append(sample_directory)
print(failed)
