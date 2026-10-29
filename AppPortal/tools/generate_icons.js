const sharp = require('sharp');
const path = require('path');
const fs = require('fs');

const OUT = path.join(__dirname, '..', 'wwwroot', 'icons');

// 直接复用 Pages/Shared/_IconSprite.cshtml 中的 #i-rocket 路径数据
const ROCKET_SVG = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
<path d="M4.5 16.5c-1.5 1.3-2 5-2 5s3.7-.5 5-2c.7-.8.7-2 0-2.8-.8-.7-2-.7-2.8 0Z"/>
<path d="m12 15-3-3a22 22 0 0 1 2-3.9A12.9 12.9 0 0 1 22 2c0 2.7-.8 7.5-6 11a22 22 0 0 1-4 2Z"/>
<path d="M9 12H4s.5-2.8 2-4c1.7-1.3 5 0 5 0"/>
<path d="M12 15v5s2.8-.5 4-2c1.3-1.7 0-5 0-5"/>
</svg>`;

const BLUE = [37, 99, 235];
const INDIGO = [79, 70, 229];

// 原图标视觉比例：火箭白色轮廓约占图块 53%，与应用内 brand 图标一致
const ROCKET_SCALE = 0.6;

// Android 自适应图标把 maskable 画布裁到约 66.7% 可视区，
// 内容在桌面上会被放大约 1.46 倍，因此 maskable 需等比缩小保持观感一致。
const MASKABLE_SCALE = 0.41;

function lerp(a, b, t) {
  return [
    Math.round(a[0] + (b[0] - a[0]) * t),
    Math.round(a[1] + (b[1] - a[1]) * t),
    Math.round(a[2] + (b[2] - a[2]) * t),
  ];
}

// 圆角矩形覆盖度（4x4 超采样，得到抗锯齿 alpha）
function roundRectCoverage(x, y, size, radius) {
  let hits = 0;
  for (let sy = 0; sy < 4; sy++) {
    for (let sx = 0; sx < 4; sx++) {
      const px = x + (sx + 0.5) / 4;
      const py = y + (sy + 0.5) / 4;
      const cx = Math.min(Math.max(px, radius), size - radius);
      const cy = Math.min(Math.max(py, radius), size - radius);
      const dx = px - cx;
      const dy = py - cy;
      if (dx * dx + dy * dy <= radius * radius) hits++;
    }
  }
  return Math.round((hits / 16) * 255);
}

// 渐变底图 raw RGBA；radius 为 null 时全出血（maskable 用）
function gradientBuffer(size, radius) {
  const pixels = Buffer.alloc(size * size * 4);
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const t = (x + y) / (2 * (size - 1));
      const color = lerp(BLUE, INDIGO, t);
      const idx = (y * size + x) * 4;
      pixels[idx] = color[0];
      pixels[idx + 1] = color[1];
      pixels[idx + 2] = color[2];
      pixels[idx + 3] = radius === null ? 255 : roundRectCoverage(x, y, size, radius);
    }
  }
  return pixels;
}

async function renderRocket(size) {
  return sharp(Buffer.from(ROCKET_SVG)).resize(size, size).png().toBuffer();
}

async function buildIcon(size, rocketScale, radius) {
  const rocketSize = Math.round(size * rocketScale);
  const rocket = await renderRocket(rocketSize);
  const offset = Math.floor((size - rocketSize) / 2);

  return sharp(gradientBuffer(size, radius), {
    raw: { width: size, height: size, channels: 4 },
  })
    .composite([{ input: rocket, top: offset, left: offset }])
    .png()
    .toBuffer();
}

async function makeIcon(size, rocketScale, radius, outPath) {
  if (radius === undefined) radius = Math.floor(size * 0.22);
  const png = await buildIcon(size, rocketScale, radius);
  await fs.promises.writeFile(outPath, png);
  console.log(`wrote ${outPath} (${size}x${size}, rocket ${rocketScale})`);
}

// 手写 ICO（内嵌 PNG 帧，Windows Vista+ 支持），保证 16/32/48 都清晰
async function writeIco(frames, outPath) {
  const header = Buffer.alloc(6);
  header.writeUInt16LE(0, 0); // reserved
  header.writeUInt16LE(1, 2); // type: icon
  header.writeUInt16LE(frames.length, 4);

  const dir = Buffer.alloc(16 * frames.length);
  let offset = 6 + dir.length;

  frames.forEach((frame, i) => {
    const entry = i * 16;
    dir.writeUInt8(frame.size >= 256 ? 0 : frame.size, entry);
    dir.writeUInt8(frame.size >= 256 ? 0 : frame.size, entry + 1);
    dir.writeUInt8(0, entry + 2); // palette
    dir.writeUInt8(0, entry + 3); // reserved
    dir.writeUInt16LE(1, entry + 4); // planes
    dir.writeUInt16LE(32, entry + 6); // bit depth
    dir.writeUInt32LE(frame.png.length, entry + 8);
    dir.writeUInt32LE(offset, entry + 12);
    offset += frame.png.length;
  });

  await fs.promises.writeFile(
    outPath,
    Buffer.concat([header, dir, ...frames.map((f) => f.png)])
  );
  console.log(`wrote ${outPath} (${frames.map((f) => f.size).join('/')})`);
}

async function makeFavicon() {
  const size = 512;
  const png = await buildIcon(size, ROCKET_SCALE, Math.floor(size * 0.22));
  const icoPath = path.join(__dirname, '..', 'wwwroot', 'favicon.ico');

  const frames = [];
  for (const s of [16, 32, 48]) {
    frames.push({ size: s, png: await sharp(png).resize(s, s).png().toBuffer() });
  }
  await writeIco(frames, icoPath);
}

async function main() {
  fs.mkdirSync(OUT, { recursive: true });

  await makeIcon(192, ROCKET_SCALE, undefined, path.join(OUT, 'icon-192.png'));
  await makeIcon(512, ROCKET_SCALE, undefined, path.join(OUT, 'icon-512.png'));
  await makeIcon(180, ROCKET_SCALE, Math.floor(180 * 0.18), path.join(OUT, 'apple-touch-icon.png'));

  // maskable：全出血无圆角，缩放补偿 Android 自适应图标裁切
  await makeIcon(512, MASKABLE_SCALE, null, path.join(OUT, 'icon-maskable-512.png'));

  await makeFavicon();
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
