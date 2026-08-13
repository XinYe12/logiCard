"""Link's Awakening-style cloud atlas: glued bulbous lobes with soft 3D shading.

White lit tops + soft pale blue-grey undersides. Soft alpha edges (near-white RGB) —
no dark Kenney-style smoke rim. Reference: screenshots/image copy 11.png.
"""
from PIL import Image
import math

W, H = 1024, 512
COLS, ROWS = 4, 2
CW, CH = W // COLS, H // ROWS

# LA palette (against blue sky in ref; still reads on dark void via soft pale shade)
HIGHLIGHT = (255, 255, 255)
MID = (236, 242, 250)
SHADOW = (168, 190, 220)  # soft blue-grey underside — volume, not outline


def shade_color(n_dot_l):
    """Smooth highlight → mid → soft blue-grey shadow."""
    t = max(0.0, min(1.0, n_dot_l))
    # Bias toward bright clay top; reserved soft shade in recesses only.
    t = t ** 0.85
    if t > 0.55:
        u = (t - 0.55) / 0.45
        return tuple(int(MID[i] + (HIGHLIGHT[i] - MID[i]) * u) for i in range(3))
    u = t / 0.55
    return tuple(int(SHADOW[i] + (MID[i] - SHADOW[i]) * u) for i in range(3))


def stamp_lobe(buf_rgb, buf_a, size, cx, cy, rx, ry, light=(-0.35, -0.75)):
    """One soft sphere lobe with fake lighting. light = (lx, ly) in image space (y+ down)."""
    lx, ly = light
    llen = math.sqrt(lx * lx + ly * ly) or 1.0
    lx, ly = lx / llen, ly / llen

    x0 = max(0, int(cx - rx - 2))
    x1 = min(size, int(cx + rx + 3))
    y0 = max(0, int(cy - ry - 2))
    y1 = min(size, int(cy + ry + 3))

    soft = 0.72  # solid clay body until late falloff — LA clouds are opaque pillows

    for y in range(y0, y1):
        for x in range(x0, x1):
            nx = (x - cx) / max(rx, 1e-5)
            ny = (y - cy) / max(ry, 1e-5)
            d2 = nx * nx + ny * ny
            if d2 >= 1.0:
                continue
            d = math.sqrt(d2)
            # Sphere normal z from x,y on unit disc
            nz = math.sqrt(max(0.0, 1.0 - d2))
            # Image-space light from upper-left (LA overhead sun read)
            ndl = max(0.0, -nx * lx - ny * ly + nz * 0.55)
            ndl = max(0.0, min(1.0, ndl * 0.85 + 0.15))

            if d < soft:
                a = 1.0
            else:
                u = (d - soft) / max(1.0 - soft, 1e-5)
                a = 1.0 - (u * u * (3 - 2 * u))

            a_i = int(a * 255)
            if a_i <= 0:
                continue

            r, g, b = shade_color(ndl)
            # Lift RGB toward white as alpha falls so fringe never reads as a dark stroke.
            lift = 1.0 - a
            r = min(255, int(r + (255 - r) * lift * 0.65))
            g = min(255, int(g + (255 - g) * lift * 0.55))
            b = min(255, int(b + (248 - b) * lift * 0.35))

            i = y * size + x
            # Overwrite with brighter / more opaque lobe (front-ish clay stack)
            old_a = buf_a[i]
            if a_i >= old_a:
                # Soft over: keep a touch of previous shade in recesses
                if old_a > 0 and a_i < 250:
                    blend = 0.18
                    r = int(r * (1 - blend) + buf_rgb[i][0] * blend)
                    g = int(g * (1 - blend) + buf_rgb[i][1] * blend)
                    b = int(b * (1 - blend) + buf_rgb[i][2] * blend)
                buf_rgb[i] = (r, g, b)
                buf_a[i] = a_i
            elif a_i > old_a * 0.55:
                # Side lobe peeking — deepen recess slightly
                br, bg, bb = buf_rgb[i]
                buf_rgb[i] = (
                    int(br * 0.92 + r * 0.08),
                    int(bg * 0.92 + g * 0.08),
                    int(bb * 0.92 + b * 0.08),
                )


def make_cloud_frame(idx):
    size = CW
    buf_rgb = [(0, 0, 0)] * (size * size)
    buf_a = [0] * (size * size)

    # Glued bulbous cluster — 4–6 lobes sharing one pillow silhouette (LA Evil Eagle sky).
    lobes = [
        # (cx, cy, rx, ry) relative 0-1
        (0.50, 0.55, 0.34, 0.30),
        (0.34, 0.48, 0.26, 0.24),
        (0.66, 0.50, 0.27, 0.25),
        (0.48, 0.36, 0.24, 0.22),
        (0.58, 0.62, 0.22, 0.20),
        (0.40, 0.62, 0.20, 0.18),
    ]
    # Per-frame slight rearrange so atlas tiles aren't identical
    rot = idx * 0.4
    n = 4 + (idx % 3)
    for i, (ux, uy, urx, ury) in enumerate(lobes[:n]):
        ang = rot + i * 0.15
        cx = size * (ux + math.cos(ang) * 0.01 * (idx % 3))
        cy = size * (uy + math.sin(ang) * 0.01 * ((idx + 1) % 3))
        rx = size * (urx * (0.96 + 0.04 * ((idx + i) % 2)))
        ry = size * (ury * (0.96 + 0.04 * ((idx * 2 + i) % 2)))
        stamp_lobe(buf_rgb, buf_a, size, cx, cy, rx, ry)

    out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = out.load()
    for y in range(size):
        for x in range(size):
            i = y * size + x
            a = buf_a[i]
            if a == 0:
                continue
            r, g, b = buf_rgb[i]
            px[x, y] = (r, g, b, a)

    # Very mild blur — soften lobe seams without killing the clay shading.
    from PIL import ImageFilter

    rgb = out.convert("RGB")
    a_img = out.getchannel("A")
    rgb = rgb.filter(ImageFilter.GaussianBlur(radius=1.2))
    a_img = a_img.filter(ImageFilter.GaussianBlur(radius=1.6))
    out = Image.merge("RGBA", (*rgb.split(), a_img))
    return out


def main():
    atlas = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    for i in range(COLS * ROWS):
        col = i % COLS
        row = i // COLS
        frame = make_cloud_frame(i)
        atlas.paste(frame, (col * CW, row * CH), frame)

    out_path = (
        r"D:\projects\Game\logiCard-atmosphere-stylized"
        r"\Assets\_Project\Art\Environment\Resources\Weather\CloudAtlas.png"
    )
    atlas.save(out_path, "PNG")
    print("wrote", out_path, atlas.size)


if __name__ == "__main__":
    main()
