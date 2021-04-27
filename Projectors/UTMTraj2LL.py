from pyproj import Proj 
import sys
import os

projector = Proj("+proj=utm +zone=16, +north +ellps=WGS84 +datum=WGS84 +units=m +no_defs")
#Proj("epsg:2100") Greek grid
# Proj("+proj=utm +zone=31, +north +ellps=WGS84 +datum=WGS84 +units=m +no_defs") Utrecht

files = os.listdir("trajectories")

for fileName in files:
    with open("trajectories/" + fileName, 'r+') as f:
        lines = [line.rstrip('\n') for line in f]
        f.seek(0)

        for line in lines:
            t = line.split()
            lon, lat = projector(t[0], t[1], inverse=True)
            f.write(str(lat) + "," + str(lon) + "," + t[2] + "\n")    
        
        f.truncate()
