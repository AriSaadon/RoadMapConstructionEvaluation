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
    utmX, utmY = projector(t[2], t[1])
    f.write(t[0] + "," + str(utmX) + "," + str(utmY) + "\n")     

f.truncate()

