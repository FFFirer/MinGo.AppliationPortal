const sharp = require('sharp');
const path = require('path');
const fs = require('fs');

const OUT = path.join(__dirname, '..', 'wwwroot', 'icons');

const ROCKET_SVG = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
<path d="M4.5 16.5c-1.5 1.3-2 5-2 5s3.7-.5 5-2c.7-.8.7-2 0-2.8-.8-.7-2-.7-2.8 0Z"/>
<path d="m12 15-3-3a22 22 0 0 1 2-3.9A12.9 12.9 0 0 1 22 2c0 2.7-.8 7.5-6 11a22 22 0 0 1-4 2Z"/>
<path d="M9 12H4s.5-2.8 2-4c1.7-1.3 5 0 5 0"/>
<path d="M12 15v5s2.8-.5 4-2c1.3-1.7 0-5 0-5"/>
</svg>`;

function lerp(a, b, t) {
  return a.map((v, i) => Math.round(v + (b[i] - v) * t));
}

function createGradient(size) {
  const BLUE = [37, 99, 235];
  const INDIGO = [79, 70, 229];
  const pixels = Buffer.alloc(size * size * 4);
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const t = (x + y) / (2 * (size - 1));
      const color = lerp(BLUE, INDIGO, t);
      const idx = (y * size + x) * 4;
      pixels[idx] = color[0];
      pixels[idx + 1] = color[1];
      pixels[idx + 2] = color[2];
      pixels[idx + 3] = 255;
    }
  }
  return sharp(pixels, { raw: { width: size, height: size, channels: 4 } });
}

async function renderRocket(size) {
  return sharp(Buffer.from(ROCKET_SVG)).resize(size, size).png().toBuffer();
}

function createRoundedRectSvg(size, radius) {
  return `<svg><rect x="0" y="0" width="${size}" height="${size}" rx="${radius}" fill="white"/></svg>`;
}

async function makeIcon(size, rocketScale, cornerRadius, outPath) {
  if (cornerRadius === null) cornerRadius = Math.floor(size * 0.22);

  const rocketSize = Math.floor(size * rocketScale);
  const rocket = await renderRocket(rocketSize);
  const offset = Math.floor((size - rocketSize) / 2);

  // Create gradient background with rounded corners
  const roundedBg = await sharp({
    create: { width: size, height: size, channels: 4, background: '#00000000' }
  }).composite([{
    input: Buffer.from(createRoundedRectSvg(size, cornerRadius)),
    blend: 'over'
  }]).png().toBuffer();

  // Create gradient pixels
  const gradientPixels = Buffer.alloc(size * size * 4);
  const BLUE = [37, 99, 235];
  const INDIGO = [79, 70, 229];
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const t = (x + y) / (2 * (size - 1));
      const color = lerp(BLUE, INDIGO, t);
      const idx = (y * size + x) * 4;
      gradientPixels[idx] = color[0];
      gradientPixels[idx + 1] = color[1];
      gradientPixels[idx + 2] = color[2];
      gradientPixels[idx + 3] = 255;
    }
  }

  await sharp(gradientPixels, { raw: { width: size, height: size, channels: 4 } })
    .composite([{
      input: roundedBg,
      blend: 'dest-in'
    }])
    .composite([{ input: rocket, top: offset, left: offset }])
    .png()
    .toFile(outPath);

  console.log(`wrote ${outPath} (${size}x${size})`);
}

async function makeFavicon() {
  const size = 512;
  const cornerRadius = Math.floor(size * 0.22);
  const rocketSize = Math.floor(size * 0.60);
  const rocket = await renderRocket(rocketSize);
  const offset = Math.floor((size - rocketSize) / 2);

  const roundedBg = await sharp({
    create: { width: size, height: size, channels: 4, background: '#00000000' }
  }).composite([{
    input: Buffer.from(createRoundedRectSvg(size, cornerRadius)),
    blend: 'over'
  }]).png().toBuffer();

  const gradientPixels = Buffer.alloc(size * size * 4);
  const BLUE = [37, 99, 235];
  const INDIGO = [79, 70, 229];
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const t = (x + y) / (2 * (size - 1));
      const color = lerp(BLUE, INDIGO, t);
      const idx = (y * size + x) * 4;
      gradientPixels[idx] = color[0];
      gradientPixels[idx + 1] = color[1];
      gradientPixels[idx + 2] = color[2];
      gradientPixels[idx + 3] = 255;
    }
  }

  const canvas = await sharp(gradientPixels, { raw: { width: size, height: size, channels: 4 } })
    .composite([{ input: roundedBg, blend: 'dest-in' }])
    .composite([{ input: rocket, top: offset, left: offset }])
    .png()
    .toBuffer();

  // Create ICO with multiple sizes - use 32x32 as base
  const icoPath = path.join(__dirname, '..', 'wwwroot', 'favicon.ico');
  await sharp(canvas).resize(32, 32).toFile(icoPath);
  console.log(`wrote ${icoPath} (32x32)`);
}

async function main() {
  fs.mkdirSync(OUT, { recursive: true });

  await makeIcon(192, 0.60, null, path.join(OUT, 'icon-192.png'));
  await makeIcon(512, 0.60, null, path.join(OUT, 'icon-512.png'));
  await makeIcon(180, 0.60, Math.floor(180 * 0.18), path.join(OUT, 'apple-touch-icon.png'));

  // Maskable: full bleed, no rounded corners
  const rocketSize = Math.floor(512 * 0.60);
  const rocket = await renderRocket(rocketSize);
  const offset = Math.floor((512 - rocketSize) / 2);

  const gradientPixels = Buffer.alloc(512 * 512 * 4);
  const BLUE = [37, 99, 235];
  const INDIGO = [79, 70, 229];
  for (let y = 0; y < 512; y++) {
    for (let x = 0; x < 512; x++) {
      const t = (x + y) / (2 * 511);
      const color = lerp(BLUE, INDIGO, t);
      const idx = (y * 512 + x) * 4;
      gradientPixels[idx] = color[0];
      gradientPixels[idx + 1] = color[1];
      gradientPixels[idx + 2] = color[2];
      gradientPixels[idx + 3] = 255;
    }
  }

  await sharp(gradientPixels, { raw: { width: 512, height: 512, channels: 4 } })
    .composite([{ input: rocket, top: offset, left: offset }])
    .png()
    .toFile(path.join(OUT, 'icon-maskable-512.png'));
  console.log(`wrote ${path.join(OUT, 'icon-maskable-512.png')} (512x512, maskable)`);

  await makeFavicon();
}

main().catch(console.error);
