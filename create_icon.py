from PIL import Image, ImageDraw

def create_app_icon():
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Outer rounded container
    draw.rounded_rectangle(
        [10, 10, 246, 246],
        radius=50,
        fill=(15, 23, 42, 255),       # Slate 900
        outline=(16, 185, 129, 255),  # Emerald 500
        width=7
    )

    # Draw book pages
    # Left page
    draw.rounded_rectangle([44, 52, 122, 178], radius=8, fill=(244, 244, 245, 255))
    # Right page
    draw.rounded_rectangle([134, 52, 212, 178], radius=8, fill=(228, 228, 231, 255))

    # Text lines on left page
    line_col = (148, 163, 184, 255)
    for y in [76, 98, 120, 142]:
        draw.line([56, y, 110, y], fill=line_col, width=4)

    # Text lines on right page
    for y in [76, 98, 120, 142]:
        draw.line([146, y, 200, y], fill=line_col, width=4)

    # Emerald center down badge
    cx, cy, r = 128, 196, 34
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(16, 185, 129, 255), outline=(255, 255, 255, 230), width=3)

    # White downward arrow
    draw.polygon([(cx, cy + 16), (cx - 14, cy - 2), (cx + 14, cy - 2)], fill=(255, 255, 255, 255))
    draw.rectangle([cx - 5, cy - 16, cx + 5, cy - 2], fill=(255, 255, 255, 255))

    img.save("app.ico", format="ICO", sizes=[(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])
    print("app.ico generated successfully with full resolutions!")

if __name__ == "__main__":
    create_app_icon()
