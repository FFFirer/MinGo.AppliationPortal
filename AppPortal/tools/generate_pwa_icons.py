"""Generate PWA icon set from the brand rocket gradient mark."""
from PIL import Image, ImageDraw
import math
import os

OUT = os.path.join(os.path.dirname(__file__), "..", "wwwroot", "icons")
os.makedirs(OUT, exist_ok=True)

BLUE = (37, 99, 235)      # blue-600 #2563eb
INDIGO = (79, 70, 229)    # indigo-600 #4f46e5
WHITE = (255, 255, 255)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def gradient(size):
    img = Image.new("RGB", (size, size))
    px = img.load()
    for y in range(size):
        for x in range(size):
            t = (x + y) / (2 * (size - 1) if size > 1 else 1)
            px[x, y] = lerp(BLUE, INDIGO, t)
    return img


def rounded_mask(size, radius):
    m = Image.new("L", (size, size), 0)
    d = ImageDraw.Draw(m)
    d.rounded_rectangle([0, 0, size - 1, size - 1], radius=radius, fill=255)
    return m


def draw_rocket(img, cx, cy, scale, color=WHITE):
    """Geometric rocket pointing up-right (lucide-like silhouette)."""
    d = ImageDraw.Draw(img)
    s = scale

    def P(x, y):
        return (cx + x * s, cy + y * s)

    # Main body: elongated diamond/rocket pointing toward upper-right
    body = [
        P(0.55, -0.62),   # nose
        P(0.78, -0.18),
        P(0.38, 0.42),    # tail
        P(-0.18, -0.08),
    ]
    d.polygon(body, fill=color)

    # Left fin
    fin_l = [P(-0.18, -0.08), P(0.12, 0.22), P(-0.28, 0.32), P(-0.42, 0.05)]
    d.polygon(fin_l, fill=color)

    # Right / lower fin
    fin_r = [P(0.38, 0.42), P(0.28, 0.72), P(-0.02, 0.42), P(0.12, 0.22)]
    d.polygon(fin_r, fill=color)

    # Exhaust flame
    flame = [P(-0.28, 0.32), P(-0.12, 0.48), P(-0.38, 0.72), P(-0.52, 0.48), P(-0.42, 0.22)]
    d.polygon(flame, fill=color)

    # Window
    r = 0.12 * s
    d.ellipse([cx + 0.28 * s - r, cy - 0.28 * s - r, cx + 0.28 * s + r, cy - 0.28 * s + r], fill=BLUE)
    # subtle inner ring
    r2 = 0.055 * s
    d.ellipse([cx + 0.28 * s - r2, cy - 0.28 * s - r2, cx + 0.28 * s + r2, cy - 0.28 * s + r2], fill=color)


def make_icon(size, rocket_scale, corner_radius=None, path="icon.png"):
    img = gradient(size).convert("RGBA")
    if corner_radius is None:
        corner_radius = int(size * 0.22)  # squircle-ish for 'any'
    # Soft rounded square mask
    mask = rounded_mask(size, corner_radius)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(img, (0, 0), mask)
    cx = size / 2
    cy = size / 2
    draw_rocket(canvas, cx, cy, size * rocket_scale)
    out = os.path.join(OUT, path)
    canvas.save(out, "PNG")
    print(f"wrote {out} ({size}x{size})")


def make_maskable(size, path="icon-maskable-512.png"):
    # Full-bleed background (no rounded corners — OS applies mask)
    img = gradient(size).convert("RGBA")
    # Safe zone: content within center ~80% => scale rocket smaller
    draw_rocket(img, size / 2, size / 2, size * 0.22)
    out = os.path.join(OUT, path)
    img.save(out, "PNG")
    print(f"wrote {out} ({size}x{size}, maskable safe zone)")


def make_favicon(base_size=512, path="../favicon.ico"):
    """Generate favicon.ico (16×16 + 32×32 + 48×48) from the same rocket art."""
    img = gradient(base_size).convert("RGBA")
    mask = rounded_mask(base_size, int(base_size * 0.22))
    canvas = Image.new("RGBA", (base_size, base_size), (0, 0, 0, 0))
    canvas.paste(img, (0, 0), mask)
    draw_rocket(canvas, base_size / 2, base_size / 2, base_size * 0.30)

    out = os.path.join(os.path.dirname(__file__), path)
    ico_sizes = [(16, 16), (32, 32), (48, 48)]
    frames = [canvas.resize(s, Image.LANCZOS) for s in ico_sizes]
    frames[0].save(out, format="ICO", sizes=ico_sizes, append_images=frames[1:])
    print(f"wrote {out} (16/32/48)")


if __name__ == "__main__":
    make_icon(192, rocket_scale=0.45, path="icon-192.png")
    make_icon(512, rocket_scale=0.45, path="icon-512.png")
    make_icon(180, rocket_scale=0.45, corner_radius=int(180 * 0.18), path="apple-touch-icon.png")
    make_maskable(512)
    make_favicon()
