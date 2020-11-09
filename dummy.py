import os
import cv2
import numpy as np

width, height = [], []
for sample in os.listdir('data/'):
    pano = cv2.imread('data/' + sample + '/blurred/out.jpg')
    width.append(pano.shape[1])
    height.append(pano.shape[0])
width = np.array(width)
height = np.array(height)
print('percentage of panoramas with height less than 480 {}'.format(np.count_nonzero(height < 480)/len(height)))
print('width')
print(' min : {}'.format(width.min()))
print(' max : {}'.format(width.max()))
print(' mean : {}'.format(width.mean()))
print(' median : {}'.format(np.median(width)))
print(' std : {}'.format(width.std()))
print('height')
print(' min : {}'.format(height.min()))
print(' max : {}'.format(height.max()))
print(' mean : {}'.format(height.mean()))
print(' median : {}'.format(np.median(height)))
print(' std : {}'.format(height.std()))
