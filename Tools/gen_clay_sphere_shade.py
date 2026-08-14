"""ClaySphereShade — soft mass wash, not per-ball lighting.

Unity sphere UVs put V along latitude. A strong crown/belly map therefore paints a
bright cap on EVERY lobe, which from the top-down play camera reads as a bubble
bath of individually lit spheres. Keep contrast very low so volume tint comes
from MaterialPropertyBlock (mass height), not from each pillow's own highlight.
"""
from PIL import Image

W, H = 64, 64
# Near-white crown → barely cooler belly. Enough for painted clay, not ball speculars.
CROWN = (255, 255, 255)
MID = (248, 250, 252)
BELLY = (228, 234, 242)


def lerp(a, b, t):
    return a + (b - a) * t


def lerp3(c0, c1, t):
    return tuple(int(round(lerp(c0[i], c1[i], t))) for i in range(3))


img = Image.new("RGB", (W, H))
px = img.load()
for y in range(H):
    # V increases upward on the sphere UV we authored against — bright at high V.
    t = 1.0 - (y / (H - 1))
    if t > 0.55:
        c = lerp3(MID, CROWN, (t - 0.55) / 0.45)
    else:
        c = lerp3(BELLY, MID, t / 0.55)
    # Tiny U vignette so seams aren't a flat slab, still far below ball-highlight contrast.
    for x in range(W):
        u = abs((x / (W - 1)) - 0.5) * 2.0
        edge = 1.0 - 0.04 * (u * u)
        px[x, y] = tuple(int(round(c[i] * edge)) for i in range(3))

out = "Assets/_Project/Art/Environment/Resources/Weather/ClaySphereShade.png"
img.save(out)
print("wrote", out, img.size)
