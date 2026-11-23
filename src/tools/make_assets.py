from PIL import Image, ImageDraw, ImageFilter, ImageFont
import random, os

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets")
os.makedirs(OUT, exist_ok=True)

S = 1024
img = Image.new("RGBA", (S, S), (0, 0, 0, 0))

# squircle mask
mask = Image.new("L", (S, S), 0)
d = ImageDraw.Draw(mask)
d.rounded_rectangle((48, 48, S - 48, S - 48), radius=252, fill=255)

# aurora gradient body
body = Image.new("RGBA", (S, S))
bd = ImageDraw.Draw(body)
for y in range(S):
    for_x = y / (S - 1)
    # diagonal interpolation between violet and cyan
    t = for_x
    r = int(124 + (34 - 124) * t)
    g = int(92 + (211 - 92) * t)
    b = int(246 + (238 - 246) * t)
    bd.line([(0, y), (S, y)], fill=(r, g, b, 255))
# diagonal soft highlight
glow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
gd = ImageDraw.Draw(glow)
gd.ellipse((-180, -300, 760, 640), fill=(255, 255, 255, 92))
glow = glow.filter(ImageFilter.GaussianBlur(130))
body = Image.alpha_composite(body, glow)

img.paste(body, (0, 0), mask)

# inner glass plate
inner = Image.new("RGBA", (S, S), (0, 0, 0, 0))
idraw = ImageDraw.Draw(inner)
idraw.rounded_rectangle((140, 140, S - 140, S - 140), radius=190, fill=(7, 10, 20, 148),
                        outline=(255, 255, 255, 80), width=4)
# top glass reflection
ref = Image.new("RGBA", (S, S), (0, 0, 0, 0))
rd = ImageDraw.Draw(ref)
rd.ellipse((180, 130, 860, 470), fill=(255, 255, 255, 38))
ref = ref.filter(ImageFilter.GaussianBlur(55))
inner = Image.alpha_composite(inner, ref)
img = Image.alpha_composite(img, inner)

# glow ring
ring = Image.new("RGBA", (S, S), (0, 0, 0, 0))
rd = ImageDraw.Draw(ring)
rd.arc((260, 260, 764, 764), start=205, end=500, fill=(255, 255, 255, 220), width=14)
rd.arc((260, 260, 764, 764), start=25, end=150, fill=(34, 211, 238, 230), width=14)
ring = ring.filter(ImageFilter.GaussianBlur(14))
img = Image.alpha_composite(img, ring)

# number 9
font = None
for candidate in [
    r"C:\Windows\Fonts\segoeuib.ttf",
    r"C:\Windows\Fonts\seguisb.ttf",
    r"C:\Windows\Fonts\msyhbd.ttc",
]:
    if os.path.exists(candidate):
        try:
            font = ImageFont.truetype(candidate, 470)
            break
        except Exception:
            pass
if font is None:
    font = ImageFont.load_default()

num = Image.new("RGBA", (S, S), (0, 0, 0, 0))
nd = ImageDraw.Draw(num)
text = "9"
bbox = nd.textbbox((0, 0), text, font=font)
tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
nd.text(((S - tw) / 2 - bbox[0], (S - th) / 2 - bbox[1] - 18), text,
        font=font, fill=(255, 255, 255, 250))
# soft glow behind number
shadow = num.filter(ImageFilter.GaussianBlur(38))
shadow.putalpha(shadow.getchannel("A").point(lambda a: int(a * 0.6)))
img = Image.alpha_composite(img, shadow)
img = Image.alpha_composite(img, num)

# small accent dot (seconds)
dot = Image.new("RGBA", (S, S), (0, 0, 0, 0))
dd = ImageDraw.Draw(dot)
dd.ellipse((714, 714, 806, 806), fill=(255, 255, 255, 235))
dot = dot.filter(ImageFilter.GaussianBlur(2))
img = Image.alpha_composite(img, dot)

# subtle outer ring
border = Image.new("RGBA", (S, S), (0, 0, 0, 0))
bd2 = ImageDraw.Draw(border)
bd2.rounded_rectangle((50, 50, S - 50, S - 50), radius=250, outline=(255, 255, 255, 105), width=5)
img = Image.alpha_composite(img, border)

sizes = [256, 128, 64, 48, 32, 16]
frames = [img.resize((s, s), Image.LANCZOS) for s in sizes]
frames[0].save(os.path.join(OUT, "app.ico"), format="ICO", sizes=[(s, s) for s in sizes],
               append_images=frames[1:])

# noise texture
random.seed(202609)
n = Image.new("RGBA", (190, 190), (0, 0, 0, 0))
px = n.load()
for y in range(190):
    for x in range(190):
        v = random.randint(0, 255)
        a = random.randint(8, 26)
        px[x, y] = (v, v, v, a)
n.save(os.path.join(OUT, "noise.png"))

# preview png
frames[0].save(os.path.join(OUT, "app-preview.png"))
print("assets written", os.path.abspath(OUT))
