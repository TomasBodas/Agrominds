import time
import random
import json
import requests
from datetime import datetime, timezone

# ================== CONFIG ==================
NGROK_BASE = "http://localhost:412"
ROBOT_ID = 2
API_KEY = "dev-telemetry-key"

HEADERS = {
    "Content-Type": "application/json",
    "X-Api-Key": API_KEY,
}

# ================== HELPERS ==================
def iso_utc() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


# ================== API CALLS ==================
def send_telemetry(battery: int, temp: float, state: str):
    url = f"{NGROK_BASE}/Robot/RegisterTelemetry"

    body = {
        "IdRobot": ROBOT_ID,
        "FechaHora": iso_utc(),
        "NivelBateria": battery,
        "Temperatura": round(temp, 1),
        "Estado": state,
        # Si tu API espera string JSON, dejalo así.
        # Si espera objeto, avisame y lo cambio.
       "DatosJSON": json.dumps({
    "sim": "python",
    "device": "AgroBot-R1",
    "firmware": "1.0.3",
    "irrigation": {
        "soilMoisture": {
            "depth10cm": round(random.uniform(18.0, 32.0), 1),
            "depth30cm": round(random.uniform(22.0, 38.0), 1),
            "depth60cm": round(random.uniform(28.0, 45.0), 1)
        },
        "ambient": {
            "airTemp": round(random.uniform(18.0, 35.0), 1),
            "airHumidity": round(random.uniform(35.0, 85.0), 1)
        },
        "flow": {
            "litersPerMinute": round(random.uniform(8.0, 22.0), 1),
            "pressureBar": round(random.uniform(1.2, 3.5), 2)
        },
        "valve": {
            "id": "V-12",
            "status": "OPEN",
            "openMinutes": random.randint(5, 45)
        },
        "waterBalance": {
            "etcMm": round(random.uniform(2.0, 6.5), 2),
            "deficitMm": round(random.uniform(0.0, 15.0), 1)
        },
        "recommendation": {
            "shouldIrrigate": True,
            "recommendedMinutes": random.randint(15, 40),
            "reason": "Déficit hídrico detectado en capa 30–60cm"
        }
    },
    "alerts": [
        {
            "code": "LOW_MOISTURE",
            "severity": "medium",
            "message": "Humedad baja en suelo profundo"
        }
    ]
}),
    }

    response = requests.post(
        url,
        headers=HEADERS,
        data=json.dumps(body),
        timeout=15,
    )

    print("Telemetry:", response.status_code, response.text)


def change_state(new_state: int):
    url = f"{NGROK_BASE}/Robot/ChangeState"

    body = {
        "id": ROBOT_ID,
        "newState": new_state,
    }

    response = requests.post(
        url,
        headers=HEADERS,
        data=json.dumps(body),
        timeout=15,
    )

    print("ChangeState:", response.status_code, response.text)


# ================== MAIN LOOP ==================
def main():
    battery = 100
    temp = 25.0

    # Opcional: activar robot (estado 1)
    change_state(1)

    while True:
        battery = max(0, battery - 1)
        temp += 0.2

        state = "Activo" if battery > 20 else "BateriaBaja"
        send_telemetry(battery, temp, state)

        if battery == 20:
            # Inactivo cuando batería baja
            change_state(3)

        time.sleep(5)


if __name__ == "__main__":
    main()
