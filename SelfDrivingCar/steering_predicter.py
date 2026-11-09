import os
from PIL import Image

import cv2
import numpy as np
import pandas as pd
import kagglehub

path = kagglehub.dataset_download("zaynena/selfdriving-car-simulator")
dataset_path = os.path.join(path, "dataset")
csv_path = None
img_path = None

for root, dirs, files in os.walk(path):
    if "driving_log.csv" in files:
        csv_path = os.path.join(root, "driving_log.csv")
        break
for root, dirs, files in os.walk(dataset_path):
    if any(file.endswith(".jpg") for file in files):
        img_path = root
        break

if csv_path is None:
    raise FileNotFoundError("driving_log.csv not found")

columns = [
    "center",
    "left",
    "right",
    "steering",
    "throttle",
    "brake",
    "speed"
]

driving_log = pd.read_csv(csv_path, header=None, names=columns)

X_files = driving_log["center"].values
y = driving_log["steering"].values

# For this project's purpose, only the center camera will be used
X_files = list(filter(lambda x: "center" in x, X_files))

def pad_path(file_path: str) -> str:
    return os.path.join(img_path, os.path.basename(file_path))

def load_img_from_path(path: str) -> np.ndarray:
    with Image.open(path) as img:
        return np.array(img.convert("RGB"))

cv2.imshow("image", cv2.cvtColor(load_img_from_path(pad_path(X_files[0])), cv2.COLOR_RGB2BGR))

cv2.waitKey(0)
cv2.destroyAllWindows()