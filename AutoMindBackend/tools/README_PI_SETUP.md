# Raspberry Pi GPS Sender Setup

## Overview
The Raspberry Pi GPS sender now uses **tracker codes** instead of user authentication. This is more secure and scalable - each Pi has its own unique tracker code that identifies the vehicle.

## Changes from Old System

### Old System (User Authentication)
- Required username/password for each Pi
- Used Keycloak token authentication
- Security risk: user credentials on device
- Required `VEHICLE_ID` environment variable

### New System (Tracker Code)
- Each Pi has a unique tracker code
- No user authentication needed
- Backend looks up vehicle by tracker code
- More secure and easier to manage multiple devices

## Setup Steps

### 1. Create Vehicle with Tracker Code

First, create a vehicle in the backend with a unique tracker code. You can do this via:

**Option A: Swagger API** (recommended for testing)
```bash
# Navigate to: https://if220129.cloud.htl-leonding.ac.at/swagger
# POST /api/Vehicles
# Body:
{
  "licensePlate": "PI-001",
  "brand": "RaspberryPi",
  "model": "GPS Tracker",
  "mileage": 0,
  "fuelConsumption": 0,
  "trackerCode": "RPI-TRACKER-12345",
  "isClaimed": false
}
```

**Option B: SQLite Direct** (if you have DB access)
```sql
INSERT INTO Vehicles (LicensePlate, Brand, Model, Mileage, FuelConsumption, TrackerCode, IsClaimed, UserId)
VALUES ('PI-001', 'RaspberryPi', 'GPS Tracker', 0, 0, 'RPI-TRACKER-12345', 0, 1);
```

### 2. Configure Raspberry Pi

Set environment variables on your Raspberry Pi:

```bash
# Required: Your unique tracker code
export AUTOMIND_TRACKER_CODE="RPI-TRACKER-12345"

# Optional: Backend URL (default shown)
export AUTOMIND_BASE_URL="https://if220129.cloud.htl-leonding.ac.at"

# Optional: Send interval in seconds (default: 30)
export AUTOMIND_SEND_INTERVAL="30"

# Optional: GPS source (gpspipe, stdin, simulate)
export GPS_SOURCE="gpspipe"
```

Add these to `~/.bashrc` or `~/.profile` to make them permanent:
```bash
echo 'export AUTOMIND_TRACKER_CODE="RPI-TRACKER-12345"' >> ~/.bashrc
source ~/.bashrc
```

### 3. Run the GPS Sender

```bash
cd /path/to/tools
python3 pi_gps_sender.py
```

Expected output:
```
Starting GPS sender with tracker code: RPI-TRACKER-12345
Sending to: https://if220129.cloud.htl-leonding.ac.at/api/Gps/byTracker
Interval: 30.0s
Sent: 48.267900, 14.251200, 10.0
Sent: 48.267950, 14.251250, 10.0
...
```

### 4. Claim the Tracker in Frontend

Once the tracker is sending data:

1. Login to AutoMind frontend
2. Go to Dashboard
3. Click "+ Add Tracker"
4. Enter tracker code: `RPI-TRACKER-12345`
5. Vehicle now appears in your dashboard

## GPS Source Options

### gpspipe (Real GPS Hardware)
```bash
export GPS_SOURCE="gpspipe"
```
Requires GPS hardware connected via gpsd.

### stdin (Manual Testing)
```bash
export GPS_SOURCE="stdin"
echo "48.2679,14.2512,10.0" | python3 pi_gps_sender.py
```
Send GPS coordinates as CSV: `latitude,longitude,speed_kmh`

### simulate (Testing without GPS)
```bash
export GPS_SOURCE="simulate"
```
Generates fake GPS data moving around Linz.

## Testing

### Test 1: Simulate GPS Data
```bash
export AUTOMIND_TRACKER_CODE="TEST-TRACKER-999"
export GPS_SOURCE="simulate"
python3 pi_gps_sender.py
```

### Test 2: Manual GPS via stdin
```bash
export AUTOMIND_TRACKER_CODE="TEST-TRACKER-999"
export GPS_SOURCE="stdin"
python3 pi_gps_sender.py
# Then type: 48.2679,14.2512,15.5
```

### Test 3: Check Backend Logs
Look for GPS points in backend:
```bash
# Via API (requires admin login)
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://if220129.cloud.htl-leonding.ac.at/api/Gps
```

## Troubleshooting

### Error: "Tracker code not found"
- Vehicle with this tracker code doesn't exist in database
- Create vehicle first (see Step 1)

### Error: "Tracker not yet claimed by a user"
- Vehicle exists but `IsClaimed = false`
- Either claim it via frontend, or manually set `IsClaimed = true` in DB

### Error: Connection refused
- Check `AUTOMIND_BASE_URL` environment variable
- Verify backend is running at that URL

### Warning: "Using demo tracker code"
- You didn't set `AUTOMIND_TRACKER_CODE`
- Set it to your actual tracker code

## Security Considerations

### Current Implementation
- Tracker code is sufficient to send GPS data
- No authentication required for simplicity
- Suitable for school project/prototype

### Production Recommendations
For production deployment, consider:
1. **API Key Authentication**: Add header validation
2. **Rate Limiting**: Prevent spam/abuse
3. **HTTPS Only**: Enforce encrypted communication
4. **Tracker Code Complexity**: Use UUIDs instead of simple strings
5. **Revocation**: Allow admins to disable tracker codes

### Example API Key Enhancement
```python
# Add to pi_gps_sender.py
API_KEY = os.getenv("AUTOMIND_API_KEY", "")
req.add_header("X-API-Key", API_KEY)

# Backend would validate:
if request.headers.get("X-API-Key") != expected_api_key:
    return Unauthorized()
```

## Multiple Raspberry Pis

Each Pi needs a unique tracker code:

```bash
# Pi 1
export AUTOMIND_TRACKER_CODE="RPI-HOME-001"

# Pi 2
export AUTOMIND_TRACKER_CODE="RPI-WORK-002"

# Pi 3
export AUTOMIND_TRACKER_CODE="RPI-TEST-003"
```

All can belong to the same user account once claimed via frontend.

## Systemd Service (Auto-start on Boot)

Create `/etc/systemd/system/automind-gps.service`:
```ini
[Unit]
Description=AutoMind GPS Sender
After=network.target gpsd.service

[Service]
Type=simple
User=pi
Environment="AUTOMIND_TRACKER_CODE=RPI-TRACKER-12345"
Environment="GPS_SOURCE=gpspipe"
ExecStart=/usr/bin/python3 /home/pi/automind/pi_gps_sender.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable automind-gps
sudo systemctl start automind-gps
sudo systemctl status automind-gps
```

## API Reference

### POST /api/Gps/byTracker

**Endpoint:** `https://if220129.cloud.htl-leonding.ac.at/api/Gps/byTracker`

**Authentication:** None required (AllowAnonymous)

**Request Body:**
```json
{
  "trackerCode": "RPI-TRACKER-12345",
  "latitude": 48.2679,
  "longitude": 14.2512,
  "speedKmh": 15.5
}
```

**Success Response (200 OK):**
```json
{
  "message": "GPS point stored",
  "vehicleId": 5
}
```

**Error Responses:**

- `404 Not Found`: Tracker code doesn't exist
  ```json
  { "error": "Tracker code not found" }
  ```

- `400 Bad Request`: Tracker not claimed
  ```json
  { "error": "Tracker not yet claimed by a user" }
  ```

## Migration from Old System

If you have existing Pi setups using the old authentication system:

1. **Create tracker codes** for existing vehicles
2. **Update environment variables** on each Pi:
   - Remove: `AUTOMIND_USERNAME`, `AUTOMIND_PASSWORD`, `AUTOMIND_REALM`, `AUTOMIND_CLIENT_ID`, `AUTOMIND_VEHICLE_ID`
   - Add: `AUTOMIND_TRACKER_CODE`
3. **Update pi_gps_sender.py** to new version
4. **Test** with simulate mode first
5. **Deploy** to production Pis

Old vehicles without tracker codes continue to work via the authenticated `/api/Gps` endpoint (requires user login).
