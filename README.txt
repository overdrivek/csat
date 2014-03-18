CSatEng (c) 2008-2014 mjt [matola@sci.fi] 

MIT-license (Licenses/csat-license.txt).

Directories:
 Licenses/                    licenses
 Libraries/                   needed libraries
 csateng/Source/              csat source codes
 csateng/Examples/src/        source codes of examples
 csateng/Examples/data/       3d-models and textures
 csateng/Examples/bin/Debug/  compiled .exe (with OTK release dll)

start  
   csateng/Examples/bin/Debug/Examples.exe  
to test examples.


Compiling:
 Open csateng.sln with MSVC# 2010 (express), it compiles csateng.dll and examples.

NOTES:
  exporter:  http://code.google.com/p/blender2ogre/
  --
  materialDiffuse can be changed with GLExt.Color4()
  --
  2D drawing:
  * screen's 0,0 is left-down corner
  * texture's origo can be setted to center or left-down corner
  --
 
TODO:
* test cloning 
* test transparent objs
* multiple lights
