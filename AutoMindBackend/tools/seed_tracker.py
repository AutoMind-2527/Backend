#!/usr/bin/env python3
"""
Script to add a test tracker code to the database
Run this ONCE to seed a test tracker
"""
import sqlite3
import secrets
import string

DB_PATH = "app.db"  # SQLite database file

def generate_tracker_code():
    """Generate a unique tracker code"""
    chars = string.ascii_uppercase + string.digits
    return ''.join(secrets.choice(chars) for _ in range(12))

def add_test_tracker():
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        # Check if tracker already exists
        cursor.execute("SELECT COUNT(*) FROM Vehicles WHERE LicensePlate = 'TEST-TRACKER'")
        if cursor.fetchone()[0] > 0:
            print("✓ Test tracker already exists")
            conn.close()
            return
        
        # Generate unique tracker code
        tracker_code = generate_tracker_code()
        
        # Insert test vehicle WITH tracker code
        cursor.execute("""
            INSERT INTO Vehicles 
            (LicensePlate, Brand, Model, Mileage, FuelConsumption, UserId, TrackerCode, IsClaimed)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        """, (
            'TEST-TRACKER',
            'Raspberry Pi',
            'GPS Device',
            0,
            0.0,
            1,  # Admin user
            tracker_code,
            0  # Not claimed yet
        ))
        
        conn.commit()
        print(f"✅ Test tracker added!")
        print(f"📍 Tracker Code: {tracker_code}")
        print(f"   Use this code in the UI to test claiming")
        
    except Exception as e:
        print(f"❌ Error: {e}")
    finally:
        conn.close()

if __name__ == "__main__":
    add_test_tracker()
