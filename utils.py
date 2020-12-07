import os
import cv2
import numpy as np


mean = [0.3852, 0.3761, 0.3258]
std = [0.0372, 0.0275, 0.0280]


def get_statistics():
    images_path = 'data/'
    width, height = [], []
    for sample in os.listdir(images_path):
        if os.path.isfile(images_path + sample + '/blurred/out.jpg'):
            pano = cv2.imread(images_path + sample + '/blurred/out.jpg')
            width.append(pano.shape[1])
            height.append(pano.shape[0])
    width = np.array(width)
    height = np.array(height)
    print('{} samples'.format(width.shape))
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
    print('percentage of panoramas with height less than 480 {}%'.format(100*round(np.count_nonzero(height < 480) / len(height), 2)))


def get_n_statistics():
    images_path = 'data/'
    n = []
    for sample in os.listdir(images_path):
        if os.path.isfile(images_path + sample + '/blurred/out.jpg'):
            pano = cv2.imread(images_path + sample + '/blurred/out.jpg')
            w, h = pano.shape[1], pano.shape[0]
            nb_frames = (w/(h/96))/128
            n.append(nb_frames)
    n = np.array(n)
    print('min : {}'.format(n.min()))
    print('max : {}'.format(n.max()))
    print('median : {}'.format(np.median(n)))


def resize_and_fill_pano(x):
    h, w = x.shape[0], x.shape[1]
    w_resize = round(w/(h/96))
    x = cv2.resize(x, (w_resize, 96))
    x = x/255
    out = np.ones((96, 6*128, 3))
    diff = 6*128 - x.shape[1]
    out[:, round(diff/2):x.shape[1] + round(diff/2), :] = x
    return out


def create_dataset(path='/data/PanoramaGAN/panorama/'):
    data = os.listdir(path)
    X, Y = None, None
    for i, sample in enumerate(data):
        if i % 100 == 0:
            print(i)
        x = None
        for img_name in os.listdir(path + sample):
            if img_name != 'out.jpg':
                img = cv2.imread(path + sample + '/' + img_name)
                img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
                img = cv2.resize(img, (128, 96))

                # normalization
                img = (img/255 - mean)/std
                img = np.asarray(img, dtype='float32')

                if x is None:
                    x = img
                else:
                    x = np.concatenate((x, img), axis=0)
            else:
                img = cv2.imread(path + sample + '/' + img_name)
                img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

                # normalization
                img = (img/255 - mean)/std

                # reshape the pano to fit the 96*(6*128)
                img = resize_and_fill_pano(img)
                img = np.asarray(img, dtype='float32')

                y = np.empty((3*6, 96, 128))
                for i in range(6):
                    y[i*3:(i+1)*3] = img[:, :, 128*i:128*(i+1)]
        x = np.expand_dims(x, axis=0)
        y = np.expand_dims(y, axis=0)
        if X is None:
            X = x
            Y = y
        else:
            X = np.concatenate((X, x), axis=0)
            Y = np.concatenate((Y, y), axis=0)
    np.savez('dataset', X, Y)


if __name__ == '__main__':
    create_dataset()
