#!/usr/bin/env python3
import json
import os
import sys
import time
import subprocess
from urllib.parse import urlencode
from urllib.request import Request, urlopen

BASE_URL = os.getenv("AUTOMIND_BASE_URL", "https://if220129.cloud.htl-leonding.ac.at")
REALM = os.getenv("AUTOMIND_REALM", "automind-realm")
CLIENT_ID = os.getenv("AUTOMIND_CLIENT_ID", "automind-backend")
USERNAME = os.getenv("AUTOMIND_USERNAME", "trackertest")
PASSWORD = os.getenv("AUTOMIND_PASSWORD", "admin")
VEHICLE_ID = int(os.getenv("AUTOMIND_VEHICLE_ID", "1"))
SEND_INTERVAL = float(os.getenv("AUTOMIND_SEND_INTERVAL", "30"))
GPS_SOURCE = os.getenv("GPS_SOURCE", "gpspipe")

TOKEN_URL = f"{BASE_URL}/keycloak/realms/{REALM}/protocol/openid-connect/token"
GPS_ENDPOINT = f"{BASE_URL}/api/Gps"

def get_token():
    if not USERNAME or not PASSWORD:
        raise RuntimeError("USERNAME / PASSWORD fehlen")
    data = urlencode({
        "grant_type": "password",
        "client_id": CLIENT_ID,
        "username": USERNAME,
        "password": PASSWORD,
        "scope": "openid"
    }).encode("utf-8")
    req = Request(TOKEN_URL, data=data, method="POST")
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urlopen(req, timeout=15) as resp:
        payload = json.loads(resp.read().decode("utf-8"))
    return payload["access_token"]

def post_gps(token, lat, lon, speed_kmh):
    body = {
        "vehicleId": VEHICLE_ID,
        "latitude": lat,
        "longitude": lon,
        "speedKmh": speed_kmh
    }
    data = json.dumps(body).encode("utf-8")
    req = Request(GPS_ENDPOINT, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
    req.add_header("Authorization", f"Bearer {token}")
    try:
        with urlopen(req, timeout=15) as resp:
            _ = resp.read()
    except Exception as e:
        print(f"\n[ERROR] GPS POST fehlgeschlagen!")
        print(f"[ERROR] URL: {GPS_ENDPOINT}")
        print(f"[ERROR] Body: {json.dumps(body, indent=2)}")
        print(f"[ERROR] {e}")
        if hasattr(e, 'read'):
            try:
                error_body = e.read().decode('utf-8')
                print(f"[ERROR] Response: {error_body}")
            except:
                pass
        raise

def gpspipe_stream():
    proc = subprocess.Popen(
        ["gpspipe", "-w"],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True
    )
    if proc.stdout is None:
        raise RuntimeError("gpspipe error")
    for line in proc.stdout:
        line = line.strip()
        if not line:
            continue
        try:
            msg = json.loads(line)
        except json.JSONDecodeError:
            continue
        if msg.get("class") != "TPV":
            continue
        lat = msg.get("lat")
        lon = msg.get("lon")
        speed = msg.get("speed")
        if lat is None or lon is None:
            continue
        speed_kmh = None
        if speed is not None:
            speed_kmh = speed * 3.6
        yield float(lat), float(lon), speed_kmh

def stdin_stream():
    for line in sys.stdin:
        line = line.strip()
        if not line:
            continue
        parts = [p.strip() for p in line.split(",")]
        if len(parts) < 2:
            continue
        lat = float(parts[0])
        lon = float(parts[1])
        speed_kmh = float(parts[2]) if len(parts) >= 3 else None
        yield lat, lon, speed_kmh

def simulate_stream():
    lat, lon = 48.2679, 14.2512
    while True:
        yield lat, lon, 10.0
        lat += 0.00005
        lon += 0.00005
        time.sleep(SEND_INTERVAL)

def main():
    token = get_token()
    print("Token OK. Sende GPS...")
    if GPS_SOURCE == "gpspipe":
        stream = gpspipe_stream()
    elif GPS_SOURCE == "stdin":
        stream = stdin_stream()
    else:
        stream = simulate_stream()
    last_send = 0.0
    for lat, lon, speed_kmh in stream:
        now = time.time()
        if now - last_send < SEND_INTERVAL:
            continue
        post_gps(token, lat, lon, speed_kmh)
        print(f"Sent: {lat:.6f}, {lon:.6f}, {speed_kmh if speed_kmh is not None else 'n/a'}")
        last_send = now

if __name__ == "__main__":
    main()
