#!/usr/bin/env python3
"""
Test script for the new tracker-based GPS endpoint.
Run this to verify the backend accepts GPS data via tracker code.
"""
import json
from urllib.request import Request, urlopen

# Configuration
BASE_URL = "https://if220129.cloud.htl-leonding.ac.at"
TRACKER_CODE = "TEST-TRACKER-001"  # Change this to your test tracker code

def test_gps_endpoint():
    """Test sending GPS data to the new byTracker endpoint."""
    url = f"{BASE_URL}/api/Gps/byTracker"
    
    test_data = {
        "trackerCode": TRACKER_CODE,
        "latitude": 48.2679,
        "longitude": 14.2512,
        "speedKmh": 15.5
    }
    
    print(f"Testing GPS endpoint: {url}")
    print(f"Tracker code: {TRACKER_CODE}")
    print(f"Test data: {json.dumps(test_data, indent=2)}")
    print("-" * 60)
    
    data = json.dumps(test_data).encode("utf-8")
    req = Request(url, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
    
    try:
        with urlopen(req, timeout=15) as resp:
            response_body = resp.read().decode("utf-8")
            print(f"✅ SUCCESS (HTTP {resp.status})")
            print(f"Response: {response_body}")
            return True
    except Exception as e:
        print(f"❌ FAILED")
        print(f"Error: {e}")
        if hasattr(e, 'read'):
            try:
                error_body = e.read().decode('utf-8')
                print(f"Response: {error_body}")
            except:
                pass
        return False

def print_setup_instructions():
    """Print instructions for setting up test data."""
    print("\n" + "=" * 60)
    print("SETUP REQUIRED")
    print("=" * 60)
    print("\nBefore running this test, create a vehicle with tracker code:")
    print("\n1. Via Swagger (https://if220129.cloud.htl-leonding.ac.at/swagger):")
    print("   POST /api/Vehicles")
    print("   {")
    print(f'     "licensePlate": "TEST-001",')
    print(f'     "brand": "TestBrand",')
    print(f'     "model": "TestModel",')
    print(f'     "mileage": 0,')
    print(f'     "fuelConsumption": 7.0,')
    print(f'     "trackerCode": "{TRACKER_CODE}",')
    print(f'     "isClaimed": true')
    print("   }")
    print("\n2. Or via direct DB insert (if you have access):")
    print(f"   INSERT INTO Vehicles (LicensePlate, Brand, Model, Mileage,")
    print(f"     FuelConsumption, TrackerCode, IsClaimed, UserId)")
    print(f"   VALUES ('TEST-001', 'TestBrand', 'TestModel', 0, 7.0,")
    print(f"     '{TRACKER_CODE}', 1, 1);")
    print("\n" + "=" * 60)

def main():
    print("=" * 60)
    print("AutoMind GPS Tracker Test")
    print("=" * 60)
    
    success = test_gps_endpoint()
    
    if not success:
        print_setup_instructions()
        print("\nPossible error causes:")
        print("- Tracker code doesn't exist in database")
        print("- Tracker not marked as claimed (IsClaimed = false)")
        print("- Backend not deployed with new endpoint")
        print("- Network/connectivity issues")
    else:
        print("\n✅ Test passed! GPS endpoint is working correctly.")
        print("\nYou can now:")
        print("1. Deploy this backend version")
        print("2. Configure your Raspberry Pi with the tracker code")
        print("3. Run pi_gps_sender.py on the Pi")

if __name__ == "__main__":
    main()
