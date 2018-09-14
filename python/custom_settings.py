import numpy as np
from initialization.spatial_softmax import *
''' -------------------------------------------------'''
''' General settings.                                '''
''' -------------------------------------------------'''
settings = {
            "store_as_int" : True,
            "dir_base" : "python/data/",
            "project" : "AE_comp_4/",
           }

''' -----------------------------------------------------------------------------'''
''' These control how the conv-net is made and initialized. Happens in models.py '''
''' -----------------------------------------------------------------------------'''
weight_initializer = {
                        "enabled" : True,
                        "disable_visual_processing" : False, #This if true, makes the visual process thing return a constant zero-tensor. EVEN IF enabled IS SET TO FALSE!
                        "batch_normalization" : True,
                        "use_avg" : False,
                        "spatial_AE" : True,
                        "pretrained_dense_encoder" : None, #Set to None to deactivate!
                        "softargmax_layer" : spatial_soft_argmax,
                        "trainable_convs" : False,
                        "file" : "weights",
                        "avg" : "avg_img",
                        "n_convs" : 3,
                        "conv_depths" : [64, 32, 16],
                        "conv_strides" : [(1,1), (1,1), (1,1)],
                        "conv_sizes" : [(7,7), (5,5), (5,5)],
                        "conv_padding" : "valid",
                        "hidden_size" : 2048,
                        "n_dense" : 3,
                     }
weight_initializer['init_dir'] = settings['dir_base'] + settings['project'] + "/init/"

''' ------------------------------------------------------------------'''
''' This controls the data-gatherer. Happens in trainer_controller.py '''
''' ------------------------------------------------------------------'''
data_gatherer = {
                "dir_base" : settings['dir_base'],
                "project" : settings['project'],
                "file_base" : "devEnv_",
                "n" : 1000,
                "obs_size" : (96,96,3),
                "n_chunks" : 100,
                "idx" : 0,
                "firstRun" : True,
                "reset_after_each_frame" : False,
                }
data_gatherer["size"] = (data_gatherer["n"],)+data_gatherer["obs_size"]
data_gatherer["data"] = np.empty(data_gatherer['size'], dtype=np.uint8)
data_gatherer["dir"] = data_gatherer["dir_base"] + data_gatherer["project"] + "/data/"
