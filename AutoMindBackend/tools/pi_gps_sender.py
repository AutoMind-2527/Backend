#!/usr/bin/env python3
import json
import os
import sys
import time
import subprocess
from urllib.request import Request, urlopen

BASE_URL = os.getenv("AUTOMIND_BASE_URL", "https://if220129.cloud.htl-leonding.ac.at")
TRACKER_CODE = os.getenv("AUTOMIND_TRACKER_CODE", "DEMO-TRACKER-001")
SEND_INTERVAL = float(os.getenv("AUTOMIND_SEND_INTERVAL", "30"))
GPS_SOURCE = os.getenv("GPS_SOURCE", "gpspipe")

GPS_ENDPOINT = f"{BASE_URL}/api/Gps/byTracker"

def post_gps(lat, lon, speed_kmh):
    """Send GPS data to backend using tracker code (no authentication required)."""
    body = {
        "trackerCode": TRACKER_CODE,
        "latitude": lat,
        "longitude": lon,
        "speedKmh": speed_kmh
    }
    data = json.dumps(body).encode("utf-8")
    req = Request(GPS_ENDPOINT, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
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
    if not TRACKER_CODE or TRACKER_CODE == "DEMO-TRACKER-001":
        print("[WARNING] Using demo tracker code. Set AUTOMIND_TRACKER_CODE environment variable.")
    
    print(f"Starting GPS sender with tracker code: {TRACKER_CODE}")
    print(f"Sending to: {GPS_ENDPOINT}")
    print(f"Interval: {SEND_INTERVAL}s")
    
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
        post_gps(lat, lon, speed_kmh)
        print(f"Sent: {lat:.6f}, {lon:.6f}, {speed_kmh if speed_kmh is not None else 'n/a'}")
        last_send = now

if __name__ == "__main__":
    main()
