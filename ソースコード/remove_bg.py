import sys
import os

try:
    from PIL import Image  # type: ignore
except ImportError:
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "Pillow"])
    from PIL import Image  # type: ignore

def process_transparent(img_path):
    if not os.path.exists(img_path):
        return
        
    img = Image.open(img_path).convert("RGBA")
    width, height = img.size
    pixels = img.load() # Use load() to access pixels instead of getdata() to avoid DeprecationWarning
    
    # 完全に白、または非常に明るいグレーを特定するためのしきい値
    # 服などの中身を消さないよう、ピクセルが「端から繋がっているか」を判定するFlood Fill
    visited = set()
    queue = []
    
    # 四隅のピクセルからスタート
    corners = [(0, 0), (width - 1, 0), (0, height - 1), (width - 1, height - 1)]
    for start_p in corners:
        queue.append(start_p)
        visited.add(start_p)
        
    bg_color = pixels[0, 0]

    # Flood Fillによる背景画素の抽出
    while queue:
        x, y = queue.pop(0)
        
        x_int, y_int = int(x), int(y)
        
        # 範囲外チェック
        if x_int < 0 or x_int >= width or y_int < 0 or y_int >= height:
            continue
            
        r, g, b, a = pixels[x_int, y_int]
        r_int, g_int, b_int = int(r), int(g), int(b)
        
        # キャラクターの細かい影まで透過しないようにしつつ、エッジを消す調整
        if r_int > 180 and g_int > 180 and b_int > 180 and abs(r_int - b_int) < 25 and abs(g_int - b_int) < 25:
            # 透明化
            pixels[x_int, y_int] = (0, 0, 0, 0)
            
            # 周辺ピクセルをキューに追加
            for dx, dy in [(0, -1), (0, 1), (-1, 0), (1, 0)]:
                nx, ny = x_int + dx, y_int + dy
                if 0 <= nx < width and 0 <= ny < height and (nx, ny) not in visited:
                    visited.add((nx, ny))
                    queue.append((nx, ny))

    # 保存
    new_path = img_path.replace(".png", "_transparent.png")
    img.save(new_path, "PNG")
    print(f"Saved {new_path}")

content_dir = r"E:\GAME\NovelGame2D\Content\Images"
process_transparent(os.path.join(content_dir, "char.png"))
process_transparent(os.path.join(content_dir, "char_smile.png"))
process_transparent(os.path.join(content_dir, "char_sad.png"))
process_transparent(os.path.join(content_dir, "char_surprised.png"))
process_transparent(os.path.join(content_dir, "char_thinking.png"))
