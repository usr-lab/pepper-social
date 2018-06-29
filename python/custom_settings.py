import numpy as np
''' -------------------------------------------------'''
''' General settings.                                '''
''' -------------------------------------------------'''
settings = {
            "store_as_int" : True,
            "dir_base" : "data/",
            "project" : "pepperBig_trial3",
           }

''' -----------------------------------------------------------------------------'''
''' These control how the conv-net is made and initialized. Happens in models.py '''
''' -----------------------------------------------------------------------------'''
weight_initializer = {
                        "enabled" : True,
                        "file" : "weights",
                        "avg" : "avg_img",
                        "n_convs" : 3,
                        "conv_depths" : [32, 32, 16],
                        "conv_strides" : [(1,1), (1,1), (1,1)],
                        "conv_sizes" : [(5,5), (5,5), (5,5)],
                        "hidden_size" : 400,
                        "n_dense" : 2,
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
                }
data_gatherer["size"] = (data_gatherer["n"],)+data_gatherer["obs_size"]
data_gatherer["data"] = np.empty(data_gatherer['size'], dtype=np.uint8)
data_gatherer["dir"] = data_gatherer["dir_base"] + data_gatherer["project"] + "/data/"
