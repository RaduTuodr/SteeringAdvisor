import base64

import cv2
import numpy as np
import socketio

from werkzeug.serving import run_simple

sio = socketio.Server(
    async_mode="threading",
    cors_allowed_origins="*",
    logger=True,
    engineio_logger=True
)

@sio.on("connect")
def connect(sid, environ):
    print("Simulator connected: ", sid)

@sio.on("telemetry")
def telemetry(sid, data):

    image_data = data["image"]

    if "," in image_data:
        image_data = image_data.split(",", 1)[1]

    image_bytes = base64.b64decode(image_data)
    image_array = np.frombuffer(image_bytes, dtype=np.uint8)
    image = cv2.imdecode(image_array, cv2.IMREAD_COLOR)

    if image is None:
        print("Couldn't load image.")
        return

    print("Image dimensions: ", image.shape)
    cv2.imshow("image", image)
    cv2.waitKey(1)

app = socketio.WSGIApp(sio)

if __name__ == "__main__":
    print("Starting server on port 4567")

    run_simple("0.0.0.0", 4567, app, threaded=True)
