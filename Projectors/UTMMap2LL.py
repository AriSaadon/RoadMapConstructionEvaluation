from pyproj import Proj 
import sys
import os

projector = Proj("+proj=utm +zone=31, +north +ellps=WGS84 +datum=WGS84 +units=m +no_defs")
#Proj("epsg:2100") Greek grid
# Proj("+proj=utm +zone=31, +north +ellps=WGS84 +datum=WGS84 +units=m +no_defs") Utrecht

f = open("Vertices", 'r+')
lines = [line.rstrip('\n') for line in f]
f.seek(0)

for line in lines:
    t = line.split(',')
    lon, lat = projector(t[1], t[2], inverse=True)
    f.write(t[0] + "," + str(lat) + "," + str(lon) + "\n")     

f.truncate()

