#!/usr/bin/env python3
"""
Manual migration: Add TrackerCode and IsClaimed columns to Vehicles table
Run this if EF migrations won't work
"""
import sqlite3
import sys

DB_PATH = "app.db"

def add_tracker_columns():
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        # Check if columns already exist
        cursor.execute("PRAGMA table_info(Vehicles)")
        columns = [col[1] for col in cursor.fetchall()]
        
        if "TrackerCode" in columns and "IsClaimed" in columns:
            print("✓ Columns already exist!")
            conn.close()
            return True
        
        # Add TrackerCode column if missing
        if "TrackerCode" not in columns:
            cursor.execute("ALTER TABLE Vehicles ADD COLUMN TrackerCode TEXT")
            print("✓ Added TrackerCode column")
        
        # Add IsClaimed column if missing
        if "IsClaimed" not in columns:
            cursor.execute("ALTER TABLE Vehicles ADD COLUMN IsClaimed INTEGER DEFAULT 0")
            print("✓ Added IsClaimed column")
        
        conn.commit()
        print("✅ Database updated successfully!")
        return True
        
    except sqlite3.Error as e:
        print(f"❌ Database error: {e}")
        return False
    except FileNotFoundError:
        print(f"❌ Database file not found: {DB_PATH}")
        return False
    finally:
        try:
            conn.close()
        except:
            pass

if __name__ == "__main__":
    success = add_tracker_columns()
    sys.exit(0 if success else 1)
