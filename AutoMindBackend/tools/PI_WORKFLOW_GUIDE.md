# Raspberry Pi Setup - Complete Workflow

## Overview

Your system has **two ways** to create vehicles:

### Option 1: Direct Vehicle Creation (Normal Users)
User creates vehicle via frontend/API → Immediately owned by that user → No tracker code

### Option 2: Raspberry Pi Tracker (IoT Devices)  
Admin creates vehicle with TrackerCode → Pi sends GPS data → User claims tracker → Vehicle assigned to user

## Workflow for Raspberry Pi Integration

### Step 1: Create Vehicle with Tracker Code (Admin)

**Via Swagger** (https://if220129.cloud.htl-leonding.ac.at/swagger):

Login as Admin, then POST `/api/Vehicles`:

```json
{
  "licensePlate": "RPI-001",
  "brand": "RaspberryPi",
  "model": "GPS Tracker v1",
  "mileage": 0,
  "fuelConsumption": 0,
  "trackerCode": "RPI-TRACKER-12345"
}
```

**Important:** The vehicle is created with:
- `IsClaimed = false` (because trackerCode is provided)
- `UserId = your admin user id` (temporary, will be reassigned when claimed)

### Step 2: Configure Raspberry Pi

On your Raspberry Pi, set environment variables:

```bash
# Required: Your unique tracker code (must match DB)
export AUTOMIND_TRACKER_CODE="RPI-TRACKER-12345"

# Optional: Backend URL (default shown)
export AUTOMIND_BASE_URL="https://if220129.cloud.htl-leonding.ac.at"

# Optional: Send interval in seconds (default: 30)
export AUTOMIND_SEND_INTERVAL="30"

# Optional: GPS source (gpspipe, stdin, simulate)
export GPS_SOURCE="gpspipe"
```

Make it permanent:
```bash
echo 'export AUTOMIND_TRACKER_CODE="RPI-TRACKER-12345"' >> ~/.bashrc
source ~/.bashrc
```

### Step 3: Run GPS Sender on Pi

```bash
cd /path/to/tools
python3 pi_gps_sender.py
```

**The Pi will:**
- Send GPS data to `POST /api/Gps/byTracker`
- No authentication needed (uses tracker code)
- Backend looks up vehicle by tracker code
- GPS data stored and trips created automatically

### Step 4: User Claims Tracker (Frontend)

1. User logs in to https://if210096.cloud.htl-leonding.ac.at/
2. Goes to Dashboard
3. Clicks **"+ Add Tracker"**
4. Enters tracker code: `RPI-TRACKER-12345`
5. Frontend calls `POST /api/Vehicles/claim` with the code

**Backend does:**
```csharp
// Find unclaimed vehicle with this tracker code
var vehicle = vehicles.FirstOrDefault(v => v.TrackerCode == "RPI-TRACKER-12345" && !v.IsClaimed);

// Assign to user
vehicle.UserId = currentUser.Id;
vehicle.IsClaimed = true;
```

**Result:** Vehicle now shows in user's dashboard with all GPS data and trips!

## Quick Test Guide

### Test 1: Create Test Vehicle
```bash
# Via Swagger as admin
POST /api/Vehicles
{
  "licensePlate": "TEST-001",
  "brand": "TestBrand", 
  "model": "TestModel",
  "mileage": 0,
  "fuelConsumption": 7.0,
  "trackerCode": "TEST-TRACKER-001"
}
```

### Test 2: Simulate GPS Data
```bash
# On your dev machine (not Pi)
export AUTOMIND_TRACKER_CODE="TEST-TRACKER-001"
export GPS_SOURCE="simulate"
cd Backend/AutoMindBackend/tools
python3 pi_gps_sender.py
```

Should see:
```
Starting GPS sender with tracker code: TEST-TRACKER-001
Sending to: https://if220129.cloud.htl-leonding.ac.at/api/Gps/byTracker
Interval: 30.0s
Sent: 48.267900, 14.251200, 10.0
Sent: 48.267950, 14.251250, 10.0
```

### Test 3: Verify GPS Data Received
```bash
# Via Swagger (admin only)
GET /api/Gps
```

Should see GPS points with your test vehicle ID.

### Test 4: Claim the Tracker
1. Login to frontend as regular user
2. Dashboard → "+ Add Tracker"
3. Enter: `TEST-TRACKER-001`
4. Should see success message
5. Vehicle appears in your vehicles list

### Test 5: Check Vehicle Ownership
```bash
# Via Swagger
GET /api/Vehicles
```

The vehicle should now show your user's ID as UserId and `IsClaimed = true`.

## Pi Script (Already Updated)

Your `pi_gps_sender.py` is already updated and correct. It:

✅ Uses tracker code instead of username/password  
✅ Sends to `/api/Gps/byTracker` endpoint  
✅ No authentication headers needed  
✅ Backend looks up vehicle automatically  

**No changes needed to the Pi script!**

## API Endpoints Reference

| Endpoint | Auth | Purpose |
|----------|------|---------|
| `POST /api/Vehicles` | User/Admin | Create vehicle (with optional trackerCode) |
| `POST /api/Vehicles/claim` | User/Admin | Claim unclaimed tracker |
| `POST /api/Gps/byTracker` | None | Pi sends GPS data (tracker code in body) |
| `GET /api/Gps` | Admin | View all GPS data (debug) |
| `GET /api/Vehicles` | User/Admin | List user's vehicles |

## Example: Complete Setup Flow

```bash
# 1. Admin creates vehicle with tracker code (via Swagger)
POST /api/Vehicles
{
  "licensePlate": "MY-PI-001",
  "brand": "RaspberryPi",
  "model": "4B with GPS",
  "mileage": 0,
  "fuelConsumption": 0,
  "trackerCode": "MY-HOME-TRACKER"
}

# 2. Configure Pi
ssh pi@raspberrypi.local
export AUTOMIND_TRACKER_CODE="MY-HOME-TRACKER"
export GPS_SOURCE="gpspipe"  # or "simulate" for testing
cd ~/automind
python3 pi_gps_sender.py

# 3. Pi starts sending GPS data
# Backend receives and stores it automatically

# 4. User claims tracker (via frontend)
# POST /api/Vehicles/claim { "trackerCode": "MY-HOME-TRACKER" }

# 5. Done! Vehicle appears in user's dashboard
```

## Differences from Old System

| Old System | New System |
|------------|------------|
| Pi has username/password | Pi has tracker code |
| Pi needs Keycloak token | No authentication needed |
| Pi sends to `/api/Gps` | Pi sends to `/api/Gps/byTracker` |
| Vehicle ID hardcoded | Vehicle looked up by tracker code |
| Security risk | More secure (no credentials on device) |
| One Pi per user account | Multiple Pis per user |

## Troubleshooting

### Pi Error: "Tracker code not found"
→ Create vehicle in backend first with that exact tracker code

### Pi Error: "Tracker not yet claimed by a user"  
→ This is normal! Vehicle exists but not claimed yet. Pi can still send GPS data. User should claim it via frontend.

### Swagger: Can't see trackerCode field
→ Redeploy backend (new image with updated VehicleCreateDto)

### User can't claim tracker
→ Check vehicle has `IsClaimed = false` in database

### GPS data not appearing in dashboard
→ Verify vehicle is claimed by checking `IsClaimed = true` and `UserId` matches current user

## Security Notes

**Current Implementation:**
- Tracker code is sufficient to send GPS data
- Suitable for school project/prototype
- Easy to test and debug

**Production Recommendations:**
1. Add API key authentication
2. Implement rate limiting per tracker code
3. Use UUID tracker codes (not simple strings)
4. Add tracker revocation system
5. Log all GPS submissions
6. HTTPS only (already done ✅)

## Files Updated

- ✅ `Models/VehilcleCreateDTO.cs` - Added `TrackerCode` field
- ✅ `Controller/VehiclesController.cs` - Handle tracker code in Create endpoint
- ✅ `Controller/GPSController.cs` - Added `/byTracker` endpoint
- ✅ `tools/pi_gps_sender.py` - Simplified to use tracker codes
- ✅ `Migrations/20260227100000_AddTrackerCodeToVehicle.cs` - DB migration

## Next Steps

1. **Rebuild and redeploy backend** (with updated VehicleCreateDto)
2. **Create test vehicle** with tracker code via Swagger
3. **Test Pi script** with simulated GPS
4. **Claim tracker** via frontend
5. **Deploy to real Pi** with actual GPS hardware

## Support

See also:
- [README_PI_SETUP.md](./README_PI_SETUP.md) - Detailed Pi setup
- [PI_INTEGRATION_SUMMARY.md](./PI_INTEGRATION_SUMMARY.md) - Quick reference
- [test_tracker_endpoint.py](./test_tracker_endpoint.py) - Endpoint test script
