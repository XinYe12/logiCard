"""Link's Awakening-style cloud atlas — varied silhouettes, soft 3D, light edges.

Human Play image copy 12: keep the bulbous direction, but (1) more shape variation,
(2) stronger lobe volume, (3) lighter 边缘 — prior SHADOW (168,190,220) read too deep.
"""
from PIL import Image, ImageFilter
import math

W, H = 1024, 512
COLS, ROWS = 4, 2
CW, CH = W // COLS, H // ROWS

# Lifted LA palette — recesses stay soft blue-grey, never dark outline grey.
HIGHLIGHT = (255, 255, 255)
MID = (244, 248, 252)
SHADOW = (214, 226, 240)  # was ~168 — too deep on edges / void
RECESS = (198, 214, 232)  # soft AO between lobes only (still pale)


def lerp(a, b, t):
    return a + (b - a) * t


def shade_color(n_dot_l, recess=0.0):
    """Bright clay top → pale mid → soft blue underside. recess darkens slightly in lobe joints."""
    t = max(0.0, min(1.0, n_dot_l))
    # Stronger highlight punch for 3D (still soft).
    t = t ** 0.7
    if t > 0.5:
        u = (t - 0.5) / 0.5
        u = u * u * (3 - 2 * u)
        rgb = tuple(int(lerp(MID[i], HIGHLIGHT[i], u)) for i in range(3))
    else:
        u = t / 0.5
        u = u * u * (3 - 2 * u)
        rgb = tuple(int(lerp(SHADOW[i], MID[i], u)) for i in range(3))

    if recess > 0.0:
        rgb = tuple(int(lerp(rgb[i], RECESS[i], recess * 0.55)) for i in range(3))
    return rgb


def stamp_lobe(buf_rgb, buf_a, size, cx, cy, rx, ry, light=(-0.28, -0.82), weight=1.0):
    """One soft sphere lobe. weight scales contribution when stacking."""
    lx, ly = light
    llen = math.sqrt(lx * lx + ly * ly) or 1.0
    lx, ly = lx / llen, ly / llen

    x0 = max(0, int(cx - rx - 3))
    x1 = min(size, int(cx + rx + 4))
    y0 = max(0, int(cy - ry - 3))
    y1 = min(size, int(cy + ry + 4))

    # Opaque pillow body; falloff only at the last ~22% so silhouette stays round.
    soft = 0.78

    for y in range(y0, y1):
        for x in range(x0, x1):
            nx = (x - cx) / max(rx, 1e-5)
            ny = (y - cy) / max(ry, 1e-5)
            d2 = nx * nx + ny * ny
            if d2 >= 1.0:
                continue
            d = math.sqrt(d2)
            nz = math.sqrt(max(0.0, 1.0 - d2))
            # Wrap light — stronger top cheek, soft belly (3D without hard terminator).
            ndl = (-nx * lx - ny * ly) * 0.55 + nz * 0.7
            ndl = max(0.0, min(1.0, ndl * 0.75 + 0.28))

            if d < soft:
                a = 1.0
            else:
                u = (d - soft) / max(1.0 - soft, 1e-5)
                a = 1.0 - (u * u * (3 - 2 * u))
            a *= weight
            a_i = int(a * 255)
            if a_i <= 0:
                continue

            # Recess cue: toward silhouette of this lobe (not outer atlas edge alone).
            recess = max(0.0, (d - 0.35) / 0.65) * 0.35 * (1.0 - ndl)

            r, g, b = shade_color(ndl, recess=recess)

            # Aggressive edge lift — 边缘 must stay near-white (human: 边缘太深).
            lift = 1.0 - a
            lift = lift * lift * (3 - 2 * lift)
            r = min(255, int(r + (255 - r) * lift * 0.92))
            g = min(255, int(g + (255 - g) * lift * 0.88))
            b = min(255, int(b + (252 - b) * lift * 0.75))

            i = y * size + x
            old_a = buf_a[i]
            if a_i >= old_a:
                if old_a > 40:
                    # Soft AO where lobes overlap — pale recess, not dark trench.
                    overlap = old_a / 255.0
                    ao = overlap * 0.22 * (1.0 - ndl)
                    r = int(lerp(r, RECESS[0], ao))
                    g = int(lerp(g, RECESS[1], ao))
                    b = int(lerp(b, RECESS[2], ao))
                    # Keep a little of the previous highlight so stacks feel layered.
                    blend = 0.12
                    br, bg, bb = buf_rgb[i]
                    r = int(r * (1 - blend) + br * blend)
                    g = int(g * (1 - blend) + bg * blend)
                    b = int(b * (1 - blend) + bb * blend)
                buf_rgb[i] = (r, g, b)
                buf_a[i] = max(old_a, a_i)
            elif a_i > 30:
                br, bg, bb = buf_rgb[i]
                k = 0.12
                buf_rgb[i] = (
                    int(br * (1 - k) + r * k),
                    int(bg * (1 - k) + g * k),
                    int(bb * (1 - k) + b * k),
                )


# Distinct silhouettes — not the same hex cluster rotated 8 times.
# Each entry: list of (cx, cy, rx, ry) in 0-1 cell space.
SHAPES = [
    # 0 wide raft
    [
        (0.50, 0.55, 0.36, 0.28),
        (0.28, 0.52, 0.24, 0.22),
        (0.72, 0.52, 0.24, 0.22),
        (0.42, 0.38, 0.20, 0.18),
        (0.60, 0.40, 0.18, 0.16),
    ],
    # 1 tall stack
    [
        (0.50, 0.62, 0.30, 0.26),
        (0.50, 0.42, 0.28, 0.24),
        (0.38, 0.50, 0.20, 0.18),
        (0.62, 0.48, 0.20, 0.18),
        (0.50, 0.28, 0.16, 0.14),
    ],
    # 2 comma / kidney
    [
        (0.42, 0.55, 0.32, 0.28),
        (0.62, 0.48, 0.26, 0.24),
        (0.70, 0.36, 0.18, 0.16),
        (0.34, 0.40, 0.18, 0.16),
        (0.52, 0.66, 0.20, 0.16),
    ],
    # 3 twin peaks
    [
        (0.34, 0.50, 0.28, 0.26),
        (0.66, 0.50, 0.28, 0.26),
        (0.50, 0.58, 0.22, 0.18),
        (0.28, 0.36, 0.16, 0.14),
        (0.72, 0.36, 0.16, 0.14),
    ],
    # 4 plump single with small buds
    [
        (0.50, 0.52, 0.38, 0.34),
        (0.32, 0.42, 0.16, 0.14),
        (0.68, 0.44, 0.16, 0.14),
        (0.50, 0.32, 0.18, 0.15),
        (0.58, 0.64, 0.14, 0.12),
    ],
    # 5 long horizontal train
    [
        (0.22, 0.52, 0.20, 0.20),
        (0.40, 0.50, 0.22, 0.22),
        (0.58, 0.50, 0.22, 0.22),
        (0.76, 0.52, 0.20, 0.20),
        (0.50, 0.38, 0.18, 0.14),
    ],
    # 6 asymmetric left-heavy
    [
        (0.38, 0.52, 0.34, 0.30),
        (0.58, 0.48, 0.24, 0.22),
        (0.70, 0.56, 0.18, 0.16),
        (0.30, 0.38, 0.18, 0.16),
        (0.48, 0.34, 0.16, 0.14),
    ],
    # 7 fluffy crown
    [
        (0.50, 0.58, 0.32, 0.26),
        (0.36, 0.44, 0.22, 0.20),
        (0.64, 0.44, 0.22, 0.20),
        (0.50, 0.34, 0.24, 0.20),
        (0.42, 0.64, 0.16, 0.14),
        (0.58, 0.64, 0.16, 0.14),
    ],
]


def make_cloud_frame(idx):
    size = CW
    buf_rgb = [(0, 0, 0)] * (size * size)
    buf_a = [0] * (size * size)

    lobes = SHAPES[idx % len(SHAPES)]
    # Slight per-frame light tilt so shading direction also varies.
    light_ang = -0.55 + (idx % 4) * 0.12
    light = (math.sin(light_ang) * 0.45, -0.85)

    for i, (ux, uy, urx, ury) in enumerate(lobes):
        # Tiny jitter unique per index — keeps shape family readable but not identical clones.
        jx = 0.012 * math.sin(idx * 1.7 + i * 2.1)
        jy = 0.010 * math.cos(idx * 1.3 + i * 1.9)
        cx = size * (ux + jx)
        cy = size * (uy + jy)
        s = 0.97 + 0.04 * ((idx + i) % 3) / 2
        rx = size * urx * s
        ry = size * ury * s
        # Front-ish lobes slightly stronger.
        weight = 0.92 + 0.08 * (i / max(len(lobes) - 1, 1))
        stamp_lobe(buf_rgb, buf_a, size, cx, cy, rx, ry, light=light, weight=weight)

    out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = out.load()
    for y in range(size):
        for x in range(size):
            i = y * size + x
            a = buf_a[i]
            if a == 0:
                continue
            r, g, b = buf_rgb[i]
            # Final edge pass: any low-alpha fringe forced brighter.
            if a < 200:
                t = a / 200.0
                lift = 1.0 - t
                r = min(255, int(r + (255 - r) * lift * 0.85))
                g = min(255, int(g + (255 - g) * lift * 0.8))
                b = min(255, int(b + (250 - b) * lift * 0.65))
            px[x, y] = (r, g, b, a)

    rgb = out.convert("RGB")
    a_img = out.getchannel("A")
    # Mild blur — keep lobe volume, soften only the fringe.
    rgb = rgb.filter(ImageFilter.GaussianBlur(radius=0.9))
    a_img = a_img.filter(ImageFilter.GaussianBlur(radius=1.4))
    return Image.merge("RGBA", (*rgb.split(), a_img))


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
    print("wrote", out_path, atlas.size, "shapes", len(SHAPES))


if __name__ == "__main__":
    main()
