"""Ceiling spider (#174): fixes the TRELLIS spider and exports the game model.

TRELLIS fused the 4th pair of legs (6 legs and a loose piece). This makes the mesh symmetric, copies the
rearmost pair swung 38 deg backwards, decimates to ~3,000 faces, scales to a 1 m leg span and exports a
static FBX plus preview renders. Run:
    blender -b --python Tools/Blender/arreglar_arana.py -- Tools/Modelos/arana_trellis.glb <out.fbx> <preview.png>
"""
import bpy, sys, math, bmesh
from mathutils import Vector, Matrix
src, out, png = sys.argv[-3:]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
ob=[o for o in bpy.data.objects if o.type=='MESH'][0]
for o in list(bpy.data.objects):
    if o.type=='EMPTY': bpy.data.objects.remove(o)
ob.parent=None; bpy.context.view_layer.objects.active=ob; ob.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
# Center on the body's mid-line (X), then mirror the complete +X half over the half missing a leg.
xs=[v.co.x for v in ob.data.vertices]; cx=(min(xs)+max(xs))/2
for v in ob.data.vertices: v.co.x-=cx
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.symmetrize(direction='POSITIVE_X', threshold=0.002)
bpy.ops.mesh.remove_doubles(threshold=0.001)
bpy.ops.object.mode_set(mode='OBJECT')
# TRELLIS fused the 4th pair of legs: copy the rearmost existing pair and swing each copy ~38 deg backwards
# around the point where it leaves the body, so the spider has 8 legs.
bm=bmesh.new(); bm.from_mesh(ob.data)
for side in (1,-1):
    sel=[f for f in bm.faces if all(side*v.co.x>0.18 and v.co.y>-0.005 for v in f.verts)]
    res=bmesh.ops.duplicate(bm, geom=sel)
    nv=[e for e in res['geom'] if isinstance(e,bmesh.types.BMVert)]
    pivot=Vector((side*0.18,0.05,sum(v.co.z for v in nv)/len(nv)))
    rot=Matrix.Rotation(math.radians(38*side),3,'Z')
    for v in nv: v.co=pivot+rot@(v.co-pivot)+Vector((0,0.02,0))
bm.to_mesh(ob.data); bm.free()
m=ob.modifiers.new('dec','DECIMATE'); m.ratio=min(1,3000/len(ob.data.polygons)); bpy.ops.object.modifier_apply(modifier='dec')
ws=[Vector(v.co) for v in ob.data.vertices]
mn=Vector([min(w[i] for w in ws) for i in range(3)]); mx=Vector([max(w[i] for w in ws) for i in range(3)])
s=1.0/(mx.x-mn.x)   # 1 m leg span, as wide as a big dog
for v in ob.data.vertices: v.co=Vector(((v.co.x-(mn.x+mx.x)/2)*s,(v.co.y-(mn.y+mx.y)/2)*s,(v.co.z-mn.z)*s))
ob.name='Arana'; print('faces', len(ob.data.polygons), 'dims', [round(d,2) for d in ob.dimensions])
bpy.ops.export_scene.fbx(filepath=out, use_selection=True, path_mode='COPY', embed_textures=True, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y', bake_space_transform=True)
scn=bpy.context.scene
l=bpy.data.objects.new('l', bpy.data.lights.new('l','SUN')); scn.collection.objects.link(l); l.rotation_euler=(math.radians(30),0,math.radians(30)); l.data.energy=3
w=bpy.data.worlds.new('w'); scn.world=w; w.color=(0.5,0.5,0.55)
cam=bpy.data.objects.new('cam', bpy.data.cameras.new('cam')); scn.collection.objects.link(cam); scn.camera=cam; cam.data.type='ORTHO'; cam.data.ortho_scale=1.15
scn.render.resolution_x=500; scn.render.resolution_y=500
for k,(loc,rot) in {'top':((0,0,4),(0,0,0)),'bottom':((0,0,-4),(180,0,0)),'persp':((1.8,-1.8,1.5),(60,0,45))}.items():
    cam.location=loc; cam.rotation_euler=[math.radians(r) for r in rot]
    scn.render.filepath=png.replace('.png','_'+k+'.png'); bpy.ops.render.render(write_still=True)
