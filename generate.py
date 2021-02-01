import os

n = 3
path = 'data/'
in_path = '/data/panorama_{}/inputs/'.format(n)
out_path = '/data/panorama_{}/outputs/'.format(n)
dirs = os.listdir(path + 'unblurred/')
dirs.sort()
dirs = dirs[900:]
failed = []
for directory in dirs:
	images_list = os.listdir(path + 'unblurred/' + directory)
	images_list.sort()
	for i in range(len(images_list) - 2*(n-1)):
		sample_directory = directory + '_' + str(i+1).zfill(2)
		sample_not_in_inputs = sample_directory not in os.listdir(in_path)
		sample_not_in_outputs = sample_directory + '.jpg' not in os.listdir(out_path)
		if sample_not_in_inputs or sample_not_in_outputs:
			images = images_list[i:i+2*n:2]
			unblurred_images_path = path + 'unblurred/' + directory + '/'
			cmd = './image-stitching'
			for img in images:
				cmd += ' {}'.format(unblurred_images_path + img)
			os.system(cmd)
			if 'out.jpg' in os.listdir():
				os.system('mv out.jpg {}'.format(out_path + sample_directory + '.jpg'))
				try:
					os.mkdir(in_path + sample_directory)
				except FileExistsError:
					pass
				blurred_images_path = path + 'blurred/' + directory + '/'
				for img in images:
					os.system('cp {} {}'.format(blurred_images_path + img, in_path + sample_directory + '/' + img))
			else:
				failed.append(sample_directory)
print(failed)
